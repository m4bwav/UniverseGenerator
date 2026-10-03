using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The galaxy cluster level: properties over many seeds, distributions, and the statistics for review.</summary>
    public class ClusterTests
    {
        private static readonly GalaxyShape[] s_armed = { GalaxyShape.Spiral, GalaxyShape.Barred };

        private static List<GalaxyCluster>? s_many;

        private static List<GalaxyCluster> Many =>
            s_many ??= Enumerable.Range(0, 2000).Select(i => GalaxyCluster.Generate("c" + i.ToString(CultureInfo.InvariantCulture))).ToList();

        [Test]
        public void A_cluster_has_an_address_and_a_map()
        {
            var c = GalaxyCluster.Generate("my-seed");
            Assert.That(c.Address, Is.EqualTo("v1-my-seed/cluster"));
            Assert.That(c.Kind, Is.Not.EqualTo(ClusterKind.Auto));
            Assert.That(c.Galaxies.Count, Is.EqualTo(c.Map.Count));
            Assert.That(c.Galaxy(1).Address, Is.EqualTo("v1-my-seed/cluster/galaxy/1"));
            Assert.That(c.Map[1].Address, Is.EqualTo(c.Galaxy(1).Address));
            Assert.That(GalaxyCluster.Generate(42).Address, Is.EqualTo("v1-42/cluster"));
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => c.Galaxy(c.Map.Count));
            Assert.That(e!.Message, Does.StartWith($"This cluster has {c.Map.Count} galaxies, numbered 0 to {c.Map.Count - 1}; you asked for {c.Map.Count}."));
        }

        [Test]
        public void The_kind_option_is_obeyed_and_checked()
        {
            for (var i = 0; i < 50; i++)
            {
                var seed = "k" + i.ToString(CultureInfo.InvariantCulture);
                Assert.That(GalaxyCluster.Generate(seed, Preset.Default with { ClusterKind = ClusterKind.Group }).Kind, Is.EqualTo(ClusterKind.Group));
                Assert.That(GalaxyCluster.Generate(seed, Preset.Default with { ClusterKind = ClusterKind.Cluster }).Kind, Is.EqualTo(ClusterKind.Cluster));
            }

            var e = Assert.Throws<ArgumentException>(() => GalaxyCluster.Generate("x", Preset.Default with { ClusterKind = (ClusterKind)7 }));
            Assert.That(e!.Message, Does.StartWith("ClusterKind must be Auto, Group or Cluster; you asked for 7."));
        }

        [Test]
        public void Every_galaxy_is_reachable_and_only_tethers_are_bridges()
        {
            foreach (var c in Many)
            {
                Assert.That(Reach(c, _ => true), Is.EqualTo(c.Map.Count), c.Address);
                // The wormhole ring (or the gates where they share a pair with it) leaves no single link whose loss cuts the map.
                foreach (var cut in c.Links.Where(l => l.Tier != LinkTier.Tether && c.Map.Count(e => e.Role != GalaxyRole.Satellite) > 2))
                {
                    Assert.That(Reach(c, l => l != cut), Is.EqualTo(c.Map.Count), $"{c.Address} without {cut.A}-{cut.B}");
                }

                foreach (var l in c.Links)
                {
                    Assert.That(l.A, Is.LessThan(l.B));
                    Assert.That(l.Length, Is.GreaterThan(0));
                }

                Assert.That(c.Links.Select(l => (l.A, l.B)).Distinct().Count(), Is.EqualTo(c.Links.Count), c.Address);
            }
        }

        [Test]
        public void Members_roles_sizes_and_types_follow_the_kind()
        {
            foreach (var c in Many)
            {
                var m = c.Map;
                if (c.Kind == ClusterKind.Group)
                {
                    Assert.That(m.Count, Is.InRange(3, 13), c.Address);
                    Assert.That(m.Count(e => e.Role == GalaxyRole.Major), Is.EqualTo(2), c.Address);
                    Assert.That(m.All(e => e.Type != "cD"), c.Address);
                }
                else
                {
                    Assert.That(m.Count, Is.InRange(12, 28), c.Address);
                    Assert.That(m[0].Type, Is.EqualTo("cD"), c.Address);
                    Assert.That((m[0].X, m[0].Y), Is.EqualTo((0.0, 0.0)), c.Address);
                    Assert.That(m.Skip(1).All(e => e.Type != "cD"), c.Address);
                    Assert.That(m.Count(e => e.Role == GalaxyRole.Major), Is.InRange(2, 3), c.Address);
                    Assert.That(m.Count(e => e.Host == 0), Is.Zero, c.Address);
                }

                Assert.That(m.Select(e => e.Name).Distinct().Count(), Is.EqualTo(m.Count), c.Address);
                for (var i = 0; i < m.Count; i++)
                {
                    var e = m[i];
                    Assert.That(e.Index, Is.EqualTo(i));
                    Assert.That(e.Shape, Is.EqualTo(Expected(e.Type)), e.Address + " " + e.Type);
                    if (s_armed.Contains(e.Shape))
                    {
                        Assert.That(e.Systems, Is.GreaterThanOrEqualTo(80), e.Address + ": arms need 80 systems (plan D16)");
                    }

                    var small = e.Role == GalaxyRole.Satellite || e.Role == GalaxyRole.Dwarf;
                    Assert.That(e.Type[0] == 'd', Is.EqualTo(small), e.Address + " " + e.Type);
                    if (small)
                    {
                        Assert.That(e.CoreActivity, Is.EqualTo(CoreActivity.Quiet), e.Address);
                    }

                    if (e.Role == GalaxyRole.Satellite)
                    {
                        var host = m[e.Host];
                        Assert.That(host.Role, Is.EqualTo(GalaxyRole.Major), e.Address);
                        Assert.That(e.Name, Does.StartWith(host.Name.Split(' ')[0] + " "), e.Address);
                        Assert.That(Distance(e, host), Is.LessThanOrEqualTo(2.2 * host.Size + e.Size + 0.01), e.Address);
                    }
                    else
                    {
                        Assert.That(e.Host, Is.EqualTo(-1));
                        Assert.That(Math.Sqrt(e.X * e.X + e.Y * e.Y), Is.LessThanOrEqualTo(c.Radius * 0.9 + 400.01), e.Address);
                    }
                }
            }
        }

        [Test]
        public void Galaxies_never_overlap_on_the_cluster_map()
        {
            var worst = double.MaxValue;
            foreach (var c in Many)
            {
                foreach (var a in c.Map)
                {
                    foreach (var b in c.Map.Where(b => b.Index > a.Index))
                    {
                        worst = Math.Min(worst, Distance(a, b) / (a.Size + b.Size));
                    }
                }
            }

            TestContext.Out.WriteLine($"closest pair: centres {worst:0.000} of the summed sizes apart");
            Assert.That(worst, Is.GreaterThanOrEqualTo(1.0));
        }

        [Test]
        public void A_galaxy_in_a_cluster_is_what_its_map_entry_says()
        {
            foreach (var c in Many.Take(150))
            {
                foreach (var g in c.Galaxies)
                {
                    var e = c.Map.Single(x => x.Address == g.Address);
                    Assert.That((g.Name, g.Type, g.Shape, g.Age, g.Richness, g.CoreActivity), Is.EqualTo((e.Name, e.Type, e.Shape, e.Age, e.Richness, e.CoreActivity)), g.Address);
                    Assert.That(g.Map.Count, Is.EqualTo(e.Systems), g.Address);
                    Assert.That(g.CoreHazardRadius, Is.EqualTo(e.CoreActivity == CoreActivity.Quasar ? 300 : e.CoreActivity == CoreActivity.Seyfert ? 150 : 0));
                    Assert.That(g.Descriptor, Does.Match("^an? "), g.Address);
                    if (e.Role == GalaxyRole.Satellite)
                    {
                        Assert.That(g.Descriptor, Does.EndWith(", a satellite of " + c.Map[e.Host].Name), g.Address);
                    }

                    var mine = c.Links.Where(l => l.A == e.Index || l.B == e.Index).ToList();
                    Assert.That(g.Gates.Count, Is.EqualTo(mine.Count), g.Address);
                    for (var k = 0; k < mine.Count; k++)
                    {
                        var gate = g.Gates[k];
                        var other = mine[k].A == e.Index ? mine[k].B : mine[k].A;
                        Assert.That((gate.Galaxy, gate.Name, gate.Tier), Is.EqualTo((other, c.Map[other].Name, mine[k].Tier)), g.Address);
                        if (gate.Tier == LinkTier.Gate)
                        {
                            Assert.That(gate.System, Is.EqualTo(g.Core), g.Address);
                        }
                        else
                        {
                            // The farthest system towards the other galaxy.
                            double dx = c.Map[other].X - e.X, dy = c.Map[other].Y - e.Y;
                            var reach = g.Map[gate.System].X * dx + g.Map[gate.System].Y * dy;
                            Assert.That(g.Map.All(s => s.X * dx + s.Y * dy <= reach), g.Address);
                        }
                    }
                }
            }
        }

        [Test]
        public void An_active_core_raises_danger_only_inside_its_radius()
        {
            var cores = 0;
            var raised = 0;
            foreach (var c in Many.Take(400))
            {
                foreach (var e in c.Map.Where(x => x.CoreActivity != CoreActivity.Quiet))
                {
                    var g = c.Galaxy(e.Index);
                    var context = ((LazyGalaxies)c.Galaxies).Contexts[e.Index];
                    Assert.That(Address.TryParse(g.Address, out var address, out _), Is.True);
                    var quiet = GalaxyGenerator.Generate(address!, Preset.Default, context with
                    {
                        CoreActivity = CoreActivity.Quiet,
                        Tuning = context.Tuning with { CoreDanger = 0, CoreHazardRadius = 0 },
                    });
                    var boost = e.CoreActivity == CoreActivity.Quasar ? 3 : 2;
                    var r = g.CoreHazardRadius;
                    for (var i = 0; i < g.Map.Count; i++)
                    {
                        var s = g.Map[i];
                        var q = quiet.Map[i];
                        Assert.That((s.X, s.Y, s.Region), Is.EqualTo((q.X, q.Y, q.Region)), s.Address);
                        if (s.X * s.X + s.Y * s.Y <= r * r)
                        {
                            Assert.That(s.Danger, Is.InRange(q.Danger, Math.Min(10, q.Danger + boost)), s.Address);
                            raised += s.Danger > q.Danger ? 1 : 0;
                        }
                        else
                        {
                            Assert.That(s.Danger, Is.EqualTo(q.Danger), s.Address);
                        }
                    }

                    cores++;
                }
            }

            TestContext.Out.WriteLine($"{cores} active cores, {raised} systems made more dangerous");
            Assert.That(cores, Is.GreaterThan(50));
            Assert.That(raised, Is.GreaterThan(cores));
        }

        [Test]
        public void A_galaxy_alone_reads_its_type_from_its_shape()
        {
            foreach (var i in Enumerable.Range(0, 300))
            {
                var g = Galaxy.Generate("lone" + i.ToString(CultureInfo.InvariantCulture), Preset.Default with { Systems = i % 2 == 0 ? 60 : 90 });
                Assert.That(Expected(g.Type), Is.EqualTo(g.Shape), g.Type);
                Assert.That(g.Name, Does.EndWith(" Galaxy"));
                Assert.That((g.Age, g.Richness, g.CoreActivity, g.CoreHazardRadius, g.Gates.Count), Is.EqualTo((StellarAge.Mature, GalaxyRichness.Normal, CoreActivity.Quiet, 0.0, 0)));
                Assert.That(g.Descriptor, Is.EqualTo((Noun(g.Type)[0] is 'a' or 'e' or 'i' or 'o' or 'u' ? "an " : "a ") + Noun(g.Type)));
            }

            Assert.That(Galaxy.Generate("bar", Preset.Default with { Systems = 80, Shape = GalaxyShape.Barred }).Type, Does.Match("^SB[abc]$"));
            Assert.That(Galaxy.Generate("ring", Preset.Default with { Shape = GalaxyShape.Ring }).Type, Is.EqualTo("Ring"));
        }

        [Test]
        public void A_galaxy_alone_and_the_same_seed_in_a_cluster_are_different_places()
        {
            var alone = Galaxy.Generate("my-seed");
            var inCluster = GalaxyCluster.Generate("my-seed").Galaxy(0);
            Assert.That(alone.Address, Is.EqualTo("v1-my-seed/galaxy"));
            Assert.That(inCluster.Address, Is.EqualTo("v1-my-seed/cluster/galaxy/0"));
            Assert.That(inCluster.System(0).Address, Is.EqualTo("v1-my-seed/cluster/galaxy/0/system/0"));
            Assert.That(GalaxyJson.Text(inCluster), Is.Not.EqualTo(GalaxyJson.Text(alone)));
        }

        [Test]
        public void Kinds_morphology_and_cores_follow_their_weights()
        {
            var groups = Many.Count(c => c.Kind == ClusterKind.Group) / (double)Many.Count;
            Assert.That(groups, Is.InRange(0.56, 0.64), "Auto draws a group 60%");
            var members = Many.Where(c => c.Kind == ClusterKind.Cluster).SelectMany(c => c.Map).Where(e => e.Role == GalaxyRole.Member).ToList();
            double EarlyShare(IEnumerable<ClusterEntry> list)
            {
                var l = list.ToList();
                return l.Count(e => e.Type[0] == 'E' || e.Type == "S0") / (double)Math.Max(1, l.Count);
            }

            var inner = EarlyShare(members.Where(e => Math.Sqrt(e.X * e.X + e.Y * e.Y) < 300));
            var outer = EarlyShare(members.Where(e => Math.Sqrt(e.X * e.X + e.Y * e.Y) >= 600));
            TestContext.Out.WriteLine($"ellipticals and lenticulars: inner {inner:P0}, outer {outer:P0}");
            Assert.That(inner, Is.GreaterThan(outer + 0.2), "the morphology and density relation");
            var majors = Many.SelectMany(c => c.Map).Where(e => e.Role == GalaxyRole.Major).ToList();
            var active = majors.Count(e => e.CoreActivity != CoreActivity.Quiet) / (double)majors.Count;
            Assert.That(active, Is.InRange(0.35, 0.5));
            var allDwarfsPoorOrNormal = Many.SelectMany(c => c.Map).Where(e => e.Role == GalaxyRole.Dwarf || e.Role == GalaxyRole.Satellite).All(e => e.Richness != GalaxyRichness.Rich);
            Assert.That(allDwarfsPoorOrNormal);
        }

        [Test]
        public void Old_galaxies_hold_old_regions_and_rich_galaxies_more_giant_planets()
        {
            var galaxies = Many.Take(120).SelectMany(c => c.Galaxies).ToList();
            double OldShare(StellarAge age)
            {
                var regions = galaxies.Where(g => g.Age == age).SelectMany(g => g.Regions).ToList();
                return regions.Count(r => r.Age == StellarAge.Old) / (double)regions.Count;
            }

            double GiantShare(GalaxyRichness richness)
            {
                var planets = galaxies.Where(g => g.Richness == richness).SelectMany(g => g.Systems.Take(15)).SelectMany(s => s.Planets).ToList();
                return planets.Count(p => p.Kind == PlanetKind.GasGiant || p.Kind == PlanetKind.HotJupiter) / (double)planets.Count;
            }

            TestContext.Out.WriteLine($"old regions: young galaxies {OldShare(StellarAge.Young):P0}, old galaxies {OldShare(StellarAge.Old):P0}");
            TestContext.Out.WriteLine($"gas giants among planets: poor {GiantShare(GalaxyRichness.Poor):P1}, normal {GiantShare(GalaxyRichness.Normal):P1}, rich {GiantShare(GalaxyRichness.Rich):P1}");
            Assert.That(OldShare(StellarAge.Old), Is.GreaterThan(OldShare(StellarAge.Young) + 0.3));
            Assert.That(GiantShare(GalaxyRichness.Rich), Is.GreaterThan(GiantShare(GalaxyRichness.Normal)));
            Assert.That(GiantShare(GalaxyRichness.Normal), Is.GreaterThan(GiantShare(GalaxyRichness.Poor)));
        }

        [Test]
        public void Prints_the_statistics_and_timing_for_review()
        {
            var entries = Many.SelectMany(c => c.Map).ToList();
            void Line(string title, IEnumerable<string> keys)
            {
                var list = keys.ToList();
                var groups = list.GroupBy(k => k).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {100.0 * g.Count() / Math.Max(1, list.Count):0.#}%");
                TestContext.Out.WriteLine(title + ": " + string.Join(", ", groups));
            }

            TestContext.Out.WriteLine($"{Many.Count} clusters, {entries.Count} galaxies");
            Line("kinds", Many.Select(c => c.Kind.ToString()));
            foreach (var kind in new[] { ClusterKind.Group, ClusterKind.Cluster })
            {
                var of = Many.Where(c => c.Kind == kind).ToList();
                TestContext.Out.WriteLine($"{kind}: {of.Average(c => c.Map.Count):0.0} galaxies ({of.Min(c => c.Map.Count)} to {of.Max(c => c.Map.Count)}), {of.Average(c => c.Map.Sum(e => e.Systems)):0} systems, {of.Average(c => c.Links.Count):0.0} links");
                Line($"  {kind} types", of.SelectMany(c => c.Map).Select(e => e.Type[0] == 'E' ? "E" : e.Type.StartsWith("SB", StringComparison.Ordinal) ? "SB" : e.Type[0] == 'S' && e.Type != "S0" ? "S" : e.Type));
            }

            Line("roles", entries.Select(e => e.Role.ToString()));
            Line("ages", entries.Select(e => e.Age.ToString()));
            Line("richness", entries.Select(e => e.Richness.ToString()));
            Line("cores", entries.Select(e => e.CoreActivity.ToString()));
            Line("links", Many.SelectMany(c => c.Links).Select(l => l.Tier.ToString()));
            Line("systems per galaxy", entries.Select(e => e.Systems < 30 ? "under 30" : e.Systems < 80 ? "30 to 79" : e.Systems < 150 ? "80 to 149" : "150+"));

            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 50; i++)
            {
                GalaxyCluster.Generate("timing-" + i.ToString(CultureInfo.InvariantCulture));
            }

            var map = watch.Elapsed.TotalMilliseconds / 50;
            watch.Restart();
            for (var i = 0; i < 10; i++)
            {
                _ = GalaxyCluster.Generate("timing-maps-" + i.ToString(CultureInfo.InvariantCulture)).Galaxies.Sum(g => g.Map.Count);
            }

            var maps = watch.Elapsed.TotalMilliseconds / 10;
            watch.Restart();
            for (var i = 0; i < 3; i++)
            {
                _ = GalaxyCluster.Generate("timing-full-" + i.ToString(CultureInfo.InvariantCulture)).Galaxies.SelectMany(g => g.Systems).Sum(s => s.Planets.Count);
            }

            var full = watch.Elapsed.TotalMilliseconds / 3;
            TestContext.Out.WriteLine($"timing (warm): cluster map {map:0.000} ms; with every galaxy map {maps:0.0} ms; with every system in full {full:0} ms");
            Assert.That(map, Is.LessThan(5));
        }

        internal static int Reach(GalaxyCluster c, Func<ClusterLink, bool> use)
        {
            var seen = new bool[c.Map.Count];
            var queue = new Queue<int>();
            seen[0] = true;
            queue.Enqueue(0);
            var count = 1;
            while (queue.Count > 0)
            {
                var a = queue.Dequeue();
                foreach (var l in c.Links.Where(use))
                {
                    var b = l.A == a ? l.B : l.B == a ? l.A : -1;
                    if (b >= 0 && !seen[b])
                    {
                        seen[b] = true;
                        count++;
                        queue.Enqueue(b);
                    }
                }
            }

            return count;
        }

        private static double Distance(ClusterEntry a, ClusterEntry b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static GalaxyShape Expected(string type) =>
            type.StartsWith("SB", StringComparison.Ordinal) ? GalaxyShape.Barred
            : type == "Sa" || type == "Sb" || type == "Sc" ? GalaxyShape.Spiral
            : type == "Ring" ? GalaxyShape.Ring
            : type == "Irr" || type == "dIrr" ? GalaxyShape.Irregular
            : GalaxyShape.Elliptical;

        private static string Noun(string type) =>
            type.StartsWith("SB", StringComparison.Ordinal) ? "barred spiral galaxy"
            : type[0] == 'S' ? "spiral galaxy"
            : type == "Ring" ? "ring galaxy"
            : type == "Irr" ? "irregular galaxy"
            : "elliptical galaxy";
    }
}
