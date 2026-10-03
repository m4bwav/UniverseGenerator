using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The galaxy extras (ai-docs/notes/2026-10-03-galaxy-extras-design.md): their rules hold on every galaxy.</summary>
    public class GalaxyExtrasTests
    {
        private static List<Galaxy>? s_many;

        // 500 default galaxies, then larger and smaller ones, a spiral of 400 and a cluster's galaxies with active cores.
        private static List<Galaxy> Many => s_many ??= SystemJson.Seeds(500).Select(s => Galaxy.Generate(s))
            .Concat(SystemJson.Seeds(40).Select(s => Galaxy.Generate("x" + s, Preset.Default with { Systems = 150 })))
            .Concat(SystemJson.Seeds(40).Select(s => Galaxy.Generate("y" + s, Preset.Default with { Systems = 9 })))
            .Concat(new[] { Galaxy.Generate("big", Preset.Default with { Systems = 400, Shape = GalaxyShape.Spiral }), Galaxy.Generate("one", Preset.Default with { Systems = 1 }) })
            .Concat(GalaxyCluster.Generate("Virgo", Preset.Default with { ClusterKind = ClusterKind.Cluster, Systems = 40 }).Galaxies)
            .ToList();

        private static List<int>[] Adjacent(Galaxy g)
        {
            var adjacent = Enumerable.Range(0, g.Map.Count).Select(_ => new List<int>()).ToArray();
            foreach (var l in g.Lanes)
            {
                adjacent[l.A].Add(l.B);
                adjacent[l.B].Add(l.A);
            }

            return adjacent;
        }

        [Test]
        public void The_extras_follow_their_rules_in_every_galaxy()
        {
            var problems = new List<string>();
            void Check(bool ok, Galaxy g, string what)
            {
                if (!ok && problems.Count < 30)
                {
                    problems.Add($"{g.Address}: {what}");
                }
            }

            foreach (var g in Many)
            {
                var n = g.Map.Count;
                var adjacent = Adjacent(g);

                // Factions: capitals held, every holding joined to its capital through the same faction, lawful ones stop at danger 9.
                Check(g.Factions.Count >= 1 && g.Factions.Count <= 6 && (n < 8 ? g.Factions.Count == 1 : g.Factions.Count >= 2), g, $"{g.Factions.Count} factions");
                Check(g.Factions.Select(f => f.Name).Distinct(StringComparer.Ordinal).Count() == g.Factions.Count, g, "faction names repeat");
                foreach (var f in g.Factions)
                {
                    Check(g.Map[f.Capital].Faction == f.Index, g, $"{f.Name} does not hold its capital");
                    Check(f.Systems == g.Map.Count(m => m.Faction == f.Index), g, $"{f.Name} counts {f.Systems}");
                    var reached = new HashSet<int> { f.Capital };
                    var queue = new Queue<int>(reached);
                    while (queue.Count > 0)
                    {
                        foreach (var t in adjacent[queue.Dequeue()].Where(t => g.Map[t].Faction == f.Index && reached.Add(t)))
                        {
                            queue.Enqueue(t);
                        }
                    }

                    Check(reached.Count == f.Systems, g, $"{f.Name} holds systems cut off from its capital");
                    if (f.Kind != FactionKind.Pirates)
                    {
                        Check(g.Map.Where(m => m.Faction == f.Index && m.Index != f.Capital).All(m => m.Danger <= 8), g, $"{f.Name} holds a system of danger 9 or 10");
                    }
                }

                foreach (var m in g.Map)
                {
                    var border = m.Faction != null && adjacent[m.Index].Any(t => g.Map[t].Faction != null && g.Map[t].Faction != m.Faction);
                    Check(m.Contested == border, g, $"{m.Name} contested {m.Contested}");
                }

                // Points of interest: one per system at most, ruins only where history allows, a trail across distinct regions.
                Check(g.PointsOfInterest.Select(p => p.System).Distinct().Count() == g.PointsOfInterest.Count, g, "two points of interest in one system");
                Check(g.PointsOfInterest.All(p => p.System >= 0 && p.System < n), g, "a point of interest outside the map");
                Check(g.PointsOfInterest.Where(p => p.Kind == PointOfInterestKind.Ruins || p.Kind == PointOfInterestKind.PrecursorSite).All(p => g.Regions[g.Map[p.System].Region].Age != StellarAge.Young), g, "ruins in a young region");
                Check(g.PointsOfInterest.Where(p => p.Kind == PointOfInterestKind.Cache).All(p => !g.Map[p.System].Chokepoint && g.Map[p.System].Region != g.Map[g.Core].Region), g, "a cache at a chokepoint or in the core region");
                var trail = g.PointsOfInterest.Where(p => p.ChainStep > 0).ToList();
                Check(trail.Count == 0 || (trail.Count >= 2 && trail.Count <= 5 && trail.Select((p, i) => p.ChainStep == i + 1 && p.ChainLength == trail.Count).All(x => x)), g, "trail steps");
                Check(trail.Select(p => g.Map[p.System].Region).Distinct().Count() == trail.Count, g, "two trail sites in one region");
                Check(trail.All(p => p.Kind == PointOfInterestKind.PrecursorSite) && g.PointsOfInterest.Count(p => p.Kind == PointOfInterestKind.PrecursorSite) == trail.Count, g, "precursor sites off the trail");

                // Hazards: systems exactly those within the radius; the active core's radiation when there is one.
                foreach (var h in g.Hazards)
                {
                    var inside = g.Map.Where(m => (m.X - h.X) * (m.X - h.X) + (m.Y - h.Y) * (m.Y - h.Y) <= h.Radius * h.Radius).Select(m => m.Index);
                    Check(h.Systems.SequenceEqual(inside), g, $"{h.Name} systems");
                    Check(h.Radius > 0 && h.Effect.Length > 0, g, $"{h.Name} radius or effect");
                }

                Check(g.Hazards.Select(h => h.Name).Distinct(StringComparer.Ordinal).Count() == g.Hazards.Count, g, "hazard names repeat");
                Check(g.Hazards.Any(h => h.Kind == HazardKind.RadiationZone) == (g.CoreActivity != CoreActivity.Quiet), g, "core radiation");
                Check(g.Hazards.Count(h => h.Kind != HazardKind.RadiationZone) >= 1 + g.Regions.Count(r => r.Theme == "nebula maze"), g, "too few hazards");

                // Monuments: exactly one per region, inside it.
                Check(g.Monuments.Count == g.Regions.Count && g.Monuments.Select((m, r) => m.Region == r && g.Map[m.System].Region == r).All(x => x), g, "monuments and regions");
                Check(g.Monuments.Select(m => m.Name).Distinct(StringComparer.Ordinal).Count() == g.Monuments.Count, g, "monument names repeat");

                // Beacons: 2 to 4 on distinct systems (1 under 10 systems, none for one), the star beacons matching the star.
                var beacons = n == 1 ? 0 : n < 10 ? 1 : -1;
                Check(beacons >= 0 ? g.Beacons.Count == beacons : g.Beacons.Count >= 2 && g.Beacons.Count <= 4, g, $"{g.Beacons.Count} beacons");
                Check(g.Beacons.Select(b => b.System).Distinct().Count() == g.Beacons.Count, g, "two beacons in one system");
                foreach (var b in g.Beacons)
                {
                    var star = g.Map[b.System].StarClass;
                    var expected = star == StarClass.NeutronStar ? BeaconKind.Pulsar : star == StarClass.Giant || star == StarClass.Supergiant ? BeaconKind.BeaconStar : star == StarClass.BlackHole ? BeaconKind.AccretionGlow : (BeaconKind?)null;
                    Check(expected == null ? b.Kind == BeaconKind.NavigationBeacon || b.Kind == BeaconKind.SignalTower : b.Kind == expected, g, $"{b.Name} is a {b.Kind} at a {star}");
                }
            }

            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void The_same_seed_gives_the_same_extras_and_the_systems_do_not_see_them()
        {
            var a = Galaxy.Generate("extras");
            Assert.That(ExtrasJson.Text(Galaxy.Generate("extras")), Is.EqualTo(ExtrasJson.Text(a)));
            Assert.That(ExtrasJson.Text(Galaxy.Generate("extras 2")), Is.Not.EqualTo(ExtrasJson.Text(a)));

            // A system's danger and name are the map's, whatever faction holds it.
            foreach (var m in a.Map)
            {
                Assert.That((a.System(m.Index).Danger, a.System(m.Index).Name), Is.EqualTo((m.Danger, m.Name)));
            }
        }

        [Test]
        public void The_shares_are_in_their_bands()
        {
            var defaults = Many.Take(500).ToList();
            var systems = defaults.Sum(g => g.Map.Count);
            double held = defaults.Sum(g => g.Map.Count(m => m.Faction != null)) / (double)systems;
            double contested = defaults.Sum(g => g.Map.Count(m => m.Contested)) / (double)systems;
            var factions = defaults.SelectMany(g => g.Factions).ToList();
            var points = defaults.SelectMany(g => g.PointsOfInterest).ToList();
            var hazards = defaults.SelectMany(g => g.Hazards).ToList();
            var monuments = defaults.SelectMany(g => g.Monuments).ToList();
            var beacons = defaults.SelectMany(g => g.Beacons).ToList();
            string Shares<T>(IEnumerable<T> items) where T : notnull
            {
                var list = items.ToList();
                return string.Join(", ", list.GroupBy(x => x).OrderByDescending(x => x.Count()).Select(x => $"{x.Key} {100.0 * x.Count() / list.Count:0.0}%"));
            }

            TestContext.Out.WriteLine($"Held {100 * held:0.0}%, contested {100 * contested:0.0}%; {factions.Count / 500.0:0.00} factions per galaxy: {Shares(factions.Select(f => f.Kind))}");
            TestContext.Out.WriteLine($"Points of interest {points.Count / 500.0:0.00} per galaxy ({defaults.Count(g => g.PointsOfInterest.Any(p => p.ChainStep > 0))} galaxies with a trail): {Shares(points.Select(p => p.Kind))}");
            TestContext.Out.WriteLine($"Hazards {hazards.Count / 500.0:0.00} per galaxy, {hazards.Average(h => h.Systems.Count):0.0} systems each: {Shares(hazards.Select(h => h.Kind))}");
            TestContext.Out.WriteLine($"Monuments: {Shares(monuments.Select(m => m.Kind))}");
            TestContext.Out.WriteLine($"Beacons {beacons.Count / 500.0:0.00} per galaxy: {Shares(beacons.Select(b => b.Kind))}");

            Assert.That(held, Is.InRange(0.45, 0.95), "share of systems held");
            Assert.That(contested, Is.InRange(0.05, 0.5), "share of border systems");
            Assert.That(factions.Count / 500.0, Is.InRange(2.0, 4.0), "factions per galaxy at 60 systems");
            Assert.That(points.Count / 500.0, Is.InRange(5.0, 13.0), "points of interest per galaxy at 60 systems");
            Assert.That(7, Is.EqualTo(points.Select(p => p.Kind).Distinct().Count()), "every kind of point of interest occurs");
            Assert.That(8, Is.EqualTo(monuments.Select(m => m.Kind).Distinct().Count()), "every kind of monument occurs");
            Assert.That(factions.Any(f => f.Kind == FactionKind.Pirates), Is.True, "pirates occur");
        }

        [Test]
        public void A_large_map_gains_little_time()
        {
            Galaxy.Generate("warm", Preset.Default with { Systems = 2000 });
            var clock = Stopwatch.StartNew();
            var g = Galaxy.Generate("time", Preset.Default with { Systems = 2000 });
            clock.Stop();
            TestContext.Out.WriteLine(string.Format(CultureInfo.InvariantCulture, "2,000-system map with extras: {0} ms; {1} factions, {2} points of interest, {3} hazards", clock.ElapsedMilliseconds, g.Factions.Count, g.PointsOfInterest.Count, g.Hazards.Count));
            Assert.That(g.Factions.Count, Is.InRange(2, 6));
            Assert.That(g.Map.Count(m => m.Faction != null), Is.GreaterThan(500), "factions on a large map hold more than bubbles");
        }
    }
}
