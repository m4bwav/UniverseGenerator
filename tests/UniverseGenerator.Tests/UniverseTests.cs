using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The universe level: properties over many seeds, distributions, the statistics for review, and its addresses.</summary>
    public class UniverseTests
    {
        private static readonly GalaxyShape[] s_armed = { GalaxyShape.Spiral, GalaxyShape.Barred };
        private static readonly Epoch[] s_epochs = { Epoch.Young, Epoch.Mature, Epoch.Old };

        private static List<Universe>? s_many;

        private static List<Universe> Many =>
            s_many ??= Enumerable.Range(0, 1000).Select(i => Universe.Generate("u" + i.ToString(CultureInfo.InvariantCulture))).ToList();

        [Test]
        public void A_universe_has_an_address_and_a_map()
        {
            var u = Universe.Generate("my-seed");
            Assert.That(u.Address, Is.EqualTo("v1-my-seed/universe"));
            Assert.That(Universe.Generate(42).Address, Is.EqualTo("v1-42/universe"));
            Assert.That(u.Nodes.Count, Is.InRange(4, 7));
            Assert.That(u.Clusters.Count, Is.EqualTo(u.Nodes.Count));
            Assert.That(u.Cluster(2).Address, Is.EqualTo("v1-my-seed/universe/cluster/2"));
            Assert.That(u.Cluster(2).Galaxy(1).Address, Is.EqualTo("v1-my-seed/universe/cluster/2/galaxy/1"));
            Assert.That(u.Name, Does.Match("^the [A-Z][a-z]+ Reach$"));
            var e = Assert.Throws<ArgumentOutOfRangeException>(() => u.Cluster(u.Nodes.Count));
            Assert.That(e!.Message, Does.StartWith($"This universe has {u.Nodes.Count} clusters, numbered 0 to {u.Nodes.Count - 1}; you asked for {u.Nodes.Count}."));
        }

        [Test]
        public void The_epoch_option_is_obeyed_and_checked()
        {
            foreach (var epoch in s_epochs)
            {
                var age = epoch == Epoch.Young ? StellarAge.Young : epoch == Epoch.Old ? StellarAge.Old : StellarAge.Mature;
                for (var i = 0; i < 20; i++)
                {
                    var seed = "e" + i.ToString(CultureInfo.InvariantCulture);
                    var u = Universe.Generate(seed, Preset.Default with { Epoch = epoch });
                    Assert.That(u.Age, Is.EqualTo(age));
                    Assert.That(u.Clusters.Select(c => c.Age), Is.All.EqualTo(age));
                    Assert.That(GalaxyCluster.Generate(seed, Preset.Default with { Epoch = epoch }).Age, Is.EqualTo(age));
                }
            }

            var x = Assert.Throws<ArgumentException>(() => Universe.Generate("x", Preset.Default with { Epoch = (Epoch)9 }));
            Assert.That(x!.Message, Does.StartWith("Epoch must be Auto, Young, Mature or Old; you asked for 9."));
        }

        [Test]
        public void The_web_reaches_every_cluster_and_keeps_them_apart()
        {
            foreach (var u in Many)
            {
                var n = u.Nodes.Count;
                var seen = new bool[n];
                var queue = new Queue<int>();
                seen[0] = true;
                queue.Enqueue(0);
                while (queue.Count > 0)
                {
                    var a = queue.Dequeue();
                    foreach (var f in u.Filaments.Where(f => f.A == a || f.B == a))
                    {
                        var b = f.A == a ? f.B : f.A;
                        if (!seen[b])
                        {
                            seen[b] = true;
                            queue.Enqueue(b);
                        }
                    }
                }

                Assert.That(seen, Is.All.True, u.Address);
                for (var a = 0; a < n; a++)
                {
                    Assert.That(Math.Sqrt(u.Nodes[a].X * u.Nodes[a].X + u.Nodes[a].Y * u.Nodes[a].Y), Is.LessThanOrEqualTo((a == 0 ? 0.3 : 0.9) * u.Radius + 0.001), u.Address);
                    for (var b = a + 1; b < n; b++)
                    {
                        Assert.That(Distance(u.Nodes[a].X, u.Nodes[a].Y, u.Nodes[b].X, u.Nodes[b].Y), Is.GreaterThanOrEqualTo(250 * 0.9), $"{u.Address} {a}-{b}");
                    }
                }

                foreach (var f in u.Filaments)
                {
                    Assert.That(f.A, Is.LessThan(f.B));
                    Assert.That(u.Cluster(f.A).Map[f.GalaxyA].Role, Is.Not.EqualTo(GalaxyRole.Satellite));
                    Assert.That(u.Cluster(f.B).Map[f.GalaxyB].Role, Is.Not.EqualTo(GalaxyRole.Satellite));
                }

                Assert.That(u.Nodes.Select(e => e.Name).Distinct().Count(), Is.EqualTo(n), u.Address);
                Assert.That(u.Nodes.Select(e => e.Galaxies), Is.EqualTo(u.Clusters.Select(c => c.Map.Count)));
            }
        }

        [Test]
        public void A_filament_opens_in_both_galaxies_it_joins()
        {
            foreach (var u in Many.Take(40))
            {
                foreach (var f in u.Filaments)
                {
                    var a = u.Cluster(f.A).Galaxy(f.GalaxyA);
                    var b = u.Cluster(f.B).Galaxy(f.GalaxyB);
                    Assert.That(a.Gates.Count(g => g.Tier == LinkTier.Filament && g.Cluster == f.B && g.Galaxy == f.GalaxyB && g.Name == b.Name), Is.EqualTo(1), $"{u.Address} {f.A}-{f.B}");
                    Assert.That(b.Gates.Count(g => g.Tier == LinkTier.Filament && g.Cluster == f.A && g.Galaxy == f.GalaxyA && g.Name == a.Name), Is.EqualTo(1), $"{u.Address} {f.A}-{f.B}");
                }

                foreach (var gate in u.Clusters.SelectMany(c => c.Galaxies).SelectMany(g => g.Gates))
                {
                    Assert.That(gate.Cluster >= 0, Is.EqualTo(gate.Tier == LinkTier.Filament));
                }
            }
        }

        [Test]
        public void Voids_keep_clear_of_clusters_filaments_and_each_other()
        {
            foreach (var u in Many)
            {
                Assert.That(u.Voids.Count, Is.InRange(0, 3), u.Address);
                foreach (var v in u.Voids)
                {
                    Assert.That(v.Address, Is.EqualTo(u.Address + "/void/" + v.Index.ToString(CultureInfo.InvariantCulture)));
                    Assert.That(v.Radius, Is.GreaterThanOrEqualTo(50));
                    Assert.That(v.Name, Does.Match("^the [A-Z][a-z]+ Void$"));
                    Assert.That(Math.Sqrt(v.X * v.X + v.Y * v.Y) + v.Radius, Is.LessThanOrEqualTo(u.Radius + 0.001), v.Address);
                    foreach (var n in u.Nodes)
                    {
                        Assert.That(Distance(v.X, v.Y, n.X, n.Y), Is.GreaterThanOrEqualTo(v.Radius + 20 - 0.001), v.Address);
                    }

                    foreach (var f in u.Filaments)
                    {
                        Assert.That(Segment(v.X, v.Y, u.Nodes[f.A], u.Nodes[f.B]), Is.GreaterThanOrEqualTo(v.Radius - 0.001), v.Address);
                    }

                    foreach (var w in u.Voids.Where(w => w.Index < v.Index))
                    {
                        Assert.That(Distance(v.X, v.Y, w.X, w.Y), Is.GreaterThanOrEqualTo(v.Radius + w.Radius - 0.001), v.Address);
                    }
                }

                Assert.That(u.Voids.Select(v => v.Name).Distinct().Count(), Is.EqualTo(u.Voids.Count));
            }

            foreach (var v in Many.Take(100).SelectMany(u => u.Voids))
            {
                Assert.That(v.Systems.Count, Is.InRange(1, 3));
                foreach (var s in v.Systems)
                {
                    Assert.That(s.Age, Is.EqualTo(StellarAge.Old), s.Address);
                    Assert.That(s.Danger, Is.InRange(1, 3), s.Address);
                    Assert.That(s.Address, Does.StartWith(v.Address + "/system/"));
                }
            }
        }

        [Test]
        public void Every_landmark_slot_holds_what_it_promises()
        {
            foreach (var u in Many)
            {
                Assert.That(u.Landmarks.Select(l => l.Kind), Is.EqualTo(new[] { UniverseLandmarkKind.Home, UniverseLandmarkKind.DyingGiant, UniverseLandmarkKind.RingGalaxy, UniverseLandmarkKind.Quasar, UniverseLandmarkKind.Merger }));
                Assert.That(u.Nodes.Take(3).Select(n => n.Role), Is.EqualTo(new[] { NodeRole.Home, NodeRole.GreatCluster, NodeRole.Merger }));
                Assert.That(u.Nodes.Skip(3).Select(n => n.Role), Is.All.EqualTo(NodeRole.Field));
                Assert.That(u.Nodes[0].Kind, Is.EqualTo(ClusterKind.Group));
                Assert.That(u.Nodes[1].Kind, Is.EqualTo(ClusterKind.Cluster));
                Assert.That(u.Nodes[2].Kind, Is.EqualTo(ClusterKind.Group));

                var home = Entry(u, u.Landmarks[0].Address);
                Assert.That(home.CoreActivity, Is.EqualTo(CoreActivity.Quiet));
                Assert.That(s_armed, Does.Contain(home.Shape), u.Address);
                Assert.That(home.Address, Is.EqualTo(u.Cluster(0).Map[0].Address));

                var dying = Entry(u, u.Landmarks[1].Address);
                Assert.That((dying.Type, dying.Age, dying.CoreActivity), Is.EqualTo(("cD", StellarAge.Old, CoreActivity.Quiet)), u.Address);

                var ring = Entry(u, u.Landmarks[2].Address);
                Assert.That((ring.Type, ring.Shape, ring.Role), Is.EqualTo(("Ring", GalaxyShape.Ring, GalaxyRole.Member)), u.Address);

                var quasar = Entry(u, u.Landmarks[3].Address);
                Assert.That(quasar.CoreActivity, Is.EqualTo(CoreActivity.Quasar));
                var node = u.Nodes.Single(n => quasar.Address.StartsWith(n.Address + "/", StringComparison.Ordinal));
                Assert.That(node.Index, Is.GreaterThanOrEqualTo(3));
                var farthest = u.Nodes.Skip(3).OrderByDescending(n => Distance(n.X, n.Y, u.Nodes[0].X, u.Nodes[0].Y)).ThenBy(n => n.Index).First();
                Assert.That(node.Index, Is.EqualTo(farthest.Index), u.Address);
                Assert.That(quasar.Index, Is.EqualTo(1));
            }
        }

        [Test]
        public void The_merging_pair_touches_and_shares_a_tidal_frontier()
        {
            foreach (var u in Many)
            {
                var c = u.Cluster(u.Merger.Cluster);
                Assert.That(u.Merger.Cluster, Is.EqualTo(2));
                var (a, b) = (c.Map[0], c.Map[1]);
                Assert.That((u.Merger.GalaxyA, u.Merger.GalaxyB), Is.EqualTo((a.Address, b.Address)));
                Assert.That(Distance(a.X, a.Y, b.X, b.Y), Is.EqualTo(a.Size + b.Size).Within(0.005), u.Address);
                Assert.That(c.Links.Single(l => l.A == 0 && l.B == 1).Tier, Is.EqualTo(LinkTier.Tidal));
                Assert.That(c.Links.Count(l => l.Tier == LinkTier.Tidal), Is.EqualTo(1));
                Assert.That((a.Age, b.Age), Is.EqualTo((StellarAge.Young, StellarAge.Young)));
                Assert.That(u.Merger.Name, Does.Match("^the [A-Z][a-z]+-[A-Z][a-z]+ Collision$"));
                Assert.That(u.Merger.Hook, Does.EndWith("."));
            }

            // Inside the pair: 2 more danger within 300 units of the tidal link, the same elsewhere, and a frontier region.
            foreach (var u in Many.Take(60))
            {
                var c = u.Cluster(2);
                var contexts = ((LazyGalaxies)c.Galaxies).Contexts;
                for (var i = 0; i < 2; i++)
                {
                    var g = c.Galaxy(i);
                    Assert.That(Address.TryParse(g.Address, out var address, out _), Is.True);
                    var plain = GalaxyGenerator.Generate(address!, Preset.Default, contexts[i] with { Frontier = false });
                    var gate = g.System(g.Gates.Single(x => x.Tier == LinkTier.Tidal).System);
                    var at = g.Map.Single(e => e.Address == gate.Address);
                    Assert.That(g.Regions[at.Region].Theme, Is.EqualTo("tidal frontier"), g.Address);
                    foreach (var e in g.Map)
                    {
                        var near = Distance(e.X, e.Y, at.X, at.Y) <= 300;
                        var before = plain.Map[e.Index].Danger;
                        Assert.That(e.Danger, Is.EqualTo(near ? Math.Min(10, before + 2) : before), e.Address);
                        Assert.That(g.System(e.Index).Danger, Is.EqualTo(e.Danger), e.Address);
                    }
                }
            }
        }

        [Test]
        public void Every_galaxy_but_the_quasar_sees_it_in_the_sky()
        {
            foreach (var u in Many.Take(30))
            {
                var quasar = u.Landmarks[3];
                foreach (var g in u.Clusters.SelectMany(c => c.Galaxies))
                {
                    if (g.Address == quasar.Address)
                    {
                        Assert.That(g.Landmark, Is.Null);
                        continue;
                    }

                    Assert.That(g.Landmark, Is.Not.Null, g.Address);
                    Assert.That((g.Landmark!.Name, g.Landmark.Address), Is.EqualTo((quasar.Name, quasar.Address)));
                    Assert.That(g.Landmark.Bearing, Is.InRange(0, 359));
                    Assert.That(g.Landmark.LightYears % 1000, Is.EqualTo(0));
                    Assert.That(g.Landmark.LightYears, Is.LessThanOrEqualTo(2 * u.Radius * Distances.UniverseUnitLightYears * 1.1));
                }
            }

            Assert.That(Galaxy.Generate("alone").Landmark, Is.Null);
            Assert.That(GalaxyCluster.Generate("alone").Galaxies.Select(g => g.Landmark), Is.All.Null);
        }

        [Test]
        public void Distances_convert_units_and_travel_times()
        {
            Assert.That(Distances.LightYears(MapLevel.Galaxy, 1000), Is.EqualTo(50000));
            Assert.That(Distances.LightYears(MapLevel.Cluster, 1000), Is.EqualTo(2000000));
            Assert.That(Distances.LightYears(MapLevel.Universe, 1000), Is.EqualTo(100000000));
            Assert.That(Distances.LaneDays(0), Is.EqualTo(1));
            Assert.That(Distances.LaneDays(100), Is.EqualTo(1));
            Assert.That(Distances.LaneDays(100.5), Is.EqualTo(2));
            Assert.That(Distances.TravelDays(LinkTier.Gate, 500), Is.EqualTo(7));
            Assert.That(Distances.TravelDays(LinkTier.Wormhole, 500), Is.EqualTo(3));
            Assert.That(Distances.TravelDays(LinkTier.Tether, 500), Is.EqualTo(2));
            Assert.That(Distances.TravelDays(LinkTier.Tidal, 500), Is.EqualTo(1));
            Assert.That(Distances.TravelDays(LinkTier.Filament, 400), Is.EqualTo(38));
            Assert.That(Distances.TravelDays(LinkTier.Filament, 401), Is.EqualTo(39));
            // A cluster on the universe map is its 1,000-unit radius at the cluster scale.
            Assert.That(Distances.LightYears(MapLevel.Cluster, 1000), Is.EqualTo(Distances.LightYears(MapLevel.Universe, 1000 * ClusterGenerator.UniverseScale)));
        }

        [Test]
        public void A_cluster_in_a_universe_and_the_same_seed_alone_are_different_places()
        {
            var u = Universe.Generate("place");
            var lone = GalaxyCluster.Generate("place");
            Assert.That(u.Cluster(0).Address, Is.Not.EqualTo(lone.Address));
            Assert.That(ClusterJson.Text(u.Cluster(0)), Is.Not.EqualTo(ClusterJson.Text(lone)));
        }

        [Test]
        public void Epochs_and_kinds_follow_their_weights()
        {
            var ages = Many.GroupBy(u => u.Age).ToDictionary(g => g.Key, g => 100.0 * g.Count() / Many.Count);
            Assert.That(ages[StellarAge.Young], Is.EqualTo(30).Within(5));
            Assert.That(ages[StellarAge.Mature], Is.EqualTo(45).Within(5));
            Assert.That(ages[StellarAge.Old], Is.EqualTo(25).Within(5));

            double ClusterShare(StellarAge age)
            {
                var field = Many.Where(u => u.Age == age).SelectMany(u => u.Nodes.Skip(3)).ToList();
                return 100.0 * field.Count(n => n.Kind == ClusterKind.Cluster) / field.Count;
            }

            Assert.That(ClusterShare(StellarAge.Young), Is.EqualTo(25).Within(7));
            Assert.That(ClusterShare(StellarAge.Mature), Is.EqualTo(40).Within(7));
            Assert.That(ClusterShare(StellarAge.Old), Is.EqualTo(55).Within(7));

            double Quasars(StellarAge age) =>
                Many.Where(u => u.Age == age).SelectMany(u => u.Clusters).SelectMany(c => c.Map).Count(e => e.CoreActivity == CoreActivity.Quasar)
                / (double)Many.Count(u => u.Age == age);
            Assert.That(Quasars(StellarAge.Young), Is.GreaterThan(Quasars(StellarAge.Old)));
        }

        [Test]
        public void Prints_the_statistics_and_timing_for_review()
        {
            void Line(string title, IEnumerable<string> keys)
            {
                var list = keys.ToList();
                var groups = list.GroupBy(k => k).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {100.0 * g.Count() / Math.Max(1, list.Count):0.#}%");
                TestContext.Out.WriteLine(title + ": " + string.Join(", ", groups));
            }

            var entries = Many.SelectMany(u => u.Clusters).SelectMany(c => c.Map).ToList();
            TestContext.Out.WriteLine($"{Many.Count} universes, {Many.Sum(u => u.Nodes.Count)} clusters, {entries.Count} galaxies, {entries.Sum(e => e.Systems)} systems");
            Line("epochs", Many.Select(u => u.Age.ToString()));
            Line("nodes", Many.Select(u => u.Nodes.Count.ToString(CultureInfo.InvariantCulture)));
            Line("field kinds", Many.SelectMany(u => u.Nodes.Skip(3)).Select(n => n.Kind.ToString()));
            Line("filaments per universe", Many.Select(u => u.Filaments.Count.ToString(CultureInfo.InvariantCulture)));
            Line("voids per universe", Many.Select(u => u.Voids.Count.ToString(CultureInfo.InvariantCulture)));
            TestContext.Out.WriteLine($"galaxies per universe {Many.Average(u => u.Nodes.Sum(n => n.Galaxies)):0.0} ({Many.Min(u => u.Nodes.Sum(n => n.Galaxies))} to {Many.Max(u => u.Nodes.Sum(n => n.Galaxies))}); systems {Many.Average(u => u.Clusters.Sum(c => c.Map.Sum(e => e.Systems))):0}");
            TestContext.Out.WriteLine($"filament length {Many.SelectMany(u => u.Filaments).Average(f => f.Length):0} units; void radius {Many.SelectMany(u => u.Voids).DefaultIfEmpty().Average(v => v?.Radius ?? 0):0} units; void systems {Many.SelectMany(u => u.Voids).Sum(v => v.Systems.Count)}");
            Line("cores", entries.Select(e => e.CoreActivity.ToString()));
            Line("hooks", Many.Select(u => u.Merger.Hook.Substring(0, 12)));

            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 20; i++)
            {
                Universe.Generate("timing-" + i.ToString(CultureInfo.InvariantCulture));
            }

            var map = watch.Elapsed.TotalMilliseconds / 20;
            watch.Restart();
            for (var i = 0; i < 3; i++)
            {
                _ = Universe.Generate("timing-maps-" + i.ToString(CultureInfo.InvariantCulture)).Clusters.SelectMany(c => c.Galaxies).Sum(g => g.Map.Count);
            }

            var maps = watch.Elapsed.TotalMilliseconds / 3;
            TestContext.Out.WriteLine($"timing (warm): universe map {map:0.00} ms; with every galaxy map {maps:0} ms");
            Assert.That(map, Is.LessThan(20));
        }

        private static ClusterEntry Entry(Universe u, string address) =>
            u.Clusters.SelectMany(c => c.Map).Single(e => e.Address == address);

        private static double Distance(double ax, double ay, double bx, double by) =>
            Math.Sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

        private static double Segment(double px, double py, UniverseNode a, UniverseNode b)
        {
            double vx = b.X - a.X, vy = b.Y - a.Y;
            var t = Math.Max(0, Math.Min(1, ((px - a.X) * vx + (py - a.Y) * vy) / (vx * vx + vy * vy)));
            return Distance(px, py, a.X + t * vx, a.Y + t * vy);
        }
    }
}
