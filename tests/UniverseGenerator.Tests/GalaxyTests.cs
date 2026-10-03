using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    public class GalaxyTests
    {
        private const int PropertySeeds = 500;
        private static List<Galaxy>? s_many;

        private static List<Galaxy> Many => s_many ??= SystemJson.Seeds(PropertySeeds).Select(s => Galaxy.Generate(s)).ToList();

        [Test]
        public void The_readme_example_runs()
        {
            var galaxy = Galaxy.Generate("my-seed");
            Assert.That(galaxy.Address, Is.EqualTo("v1-my-seed/galaxy"));
            Assert.That(galaxy.Systems.Count, Is.EqualTo(60));
            TestContext.Out.WriteLine($"{galaxy.Shape}, {galaxy.Regions.Count} regions, {galaxy.Lanes.Count} lanes, core {galaxy.Map[galaxy.Core].Name}");
            foreach (var system in galaxy.Systems)
            {
                TestContext.Out.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
            }
        }

        [Test]
        public void The_same_seed_gives_the_same_galaxy_and_a_number_equals_its_digits()
        {
            Assert.That(GalaxyJson.Text(Galaxy.Generate("abc")), Is.EqualTo(GalaxyJson.Text(Galaxy.Generate("abc"))));
            Assert.That(GalaxyJson.Text(Galaxy.Generate(42)), Is.EqualTo(GalaxyJson.Text(Galaxy.Generate("42"))));
            Assert.That(GalaxyJson.Text(Galaxy.Generate(42)), Is.Not.EqualTo(GalaxyJson.Text(Galaxy.Generate(-42))));
        }

        [Test]
        public void Every_galaxy_is_consistent()
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
                Check(n == 60 && g.Systems.Count == n, g, $"{n} systems");
                Check(g.Shape != GalaxyShape.Spiral && g.Shape != GalaxyShape.Barred && g.Shape != GalaxyShape.Auto, g, $"shape {g.Shape} at 60 systems");
                Check(g.Map.Select(m => m.Name).Distinct(StringComparer.Ordinal).Count() == n, g, "names repeat");
                Check(g.Map.Select((m, i) => m.Index == i && m.Address == g.Address + "/system/" + i.ToString(CultureInfo.InvariantCulture)).All(x => x), g, "indexes or addresses");
                Check(g.Map.All(m => m.X * m.X + m.Y * m.Y <= g.Radius * g.Radius), g, "a system outside the radius");
                Check(g.Map.All(m => m.Danger >= 1 && m.Danger <= 10), g, "danger out of range");
                Check(g.Map.All(m => m.Region >= 0 && m.Region < g.Regions.Count), g, "region out of range");
                Check(g.Regions.Count >= 2 && g.Regions.Count <= 8, g, $"{g.Regions.Count} regions");
                Check(g.Regions.Select(r => r.Name).Distinct().Count() == g.Regions.Count, g, "region names repeat");
                Check(g.Regions.All(r => g.Map[r.Centre].Region == r.Index), g, "a region centre in another region");
                Check(g.Regions[0].Centre == g.Core && g.Map[g.Core].Hops == 0, g, "core");
                Check(g.Lanes.All(l => l.A < l.B && l.B < n), g, "lane order");
                Check(g.Lanes.Select(l => (l.A, l.B)).Distinct().Count() == g.Lanes.Count, g, "lanes repeat");
                Check(g.Map.All(m => m.Hops < int.MaxValue), g, "lanes not connected");

                var nearest = g.Map.Min(a => g.Map.Where(b => b != a).Min(b => (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y)));
                Check(nearest >= 30 * 30, g, $"systems {Math.Sqrt(nearest):0.0} units apart");

                // Hops are shortest routes: neighbours differ by at most one.
                Check(g.Lanes.All(l => Math.Abs(g.Map[l.A].Hops - g.Map[l.B].Hops) <= 1), g, "hops");

                // A bridge lane splits the map when removed; any other lane does not.
                foreach (var lane in g.Lanes.Take(20))
                {
                    Check(lane.Bridge == !Connected(g, lane), g, $"lane {lane.A}-{lane.B} bridge {lane.Bridge}");
                }

                // A chokepoint splits the map when removed.
                foreach (var m in g.Map.Take(20))
                {
                    Check(m.Chokepoint == !Connected(g, m.Index), g, $"system {m.Index} chokepoint {m.Chokepoint}");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Placed_systems_get_their_map_context()
        {
            foreach (var g in Many.Take(50))
            {
                for (var i = 0; i < g.Map.Count; i++)
                {
                    var m = g.Map[i];
                    var s = g.System(i);
                    Assert.That(s.Address, Is.EqualTo(m.Address));
                    Assert.That(s.Name, Is.EqualTo(m.Name));
                    Assert.That(s.Danger, Is.EqualTo(m.Danger));
                    Assert.That(s.Star.Class, Is.EqualTo(m.StarClass));
                    Assert.That(s.Age, Is.EqualTo(g.Regions[m.Region].Age));
                    Assert.That(ReferenceEquals(s, g.Systems[i]), Is.True, "a system is generated once and kept");
                }
            }
        }

        [Test]
        public void The_lanes_are_the_prototype_lanes()
        {
            // The grid search must give exactly the prototype's all-pairs result: the relative neighbourhood graph plus
            // 25% of the other Gabriel edges, one roll per Gabriel pair in index order from the "lanes" stream.
            foreach (var (seed, options) in new[] { ("l1", Preset.Default), ("l2", Preset.Default with { Systems = 300 }), ("l3", Preset.Default with { Systems = 120, Shape = GalaxyShape.Ring }), ("l4", Preset.Default with { Systems = 2 }) })
            {
                var g = Galaxy.Generate(seed, options);
                var rng = Seeds.Stream(new Address(GeneratorVersion.Current, seed, "galaxy").ObjectSeed, "lanes");
                var n = g.Map.Count;
                double D(int a, int b) => (g.Map[a].X - g.Map[b].X) * (g.Map[a].X - g.Map[b].X) + (g.Map[a].Y - g.Map[b].Y) * (g.Map[a].Y - g.Map[b].Y);
                var expected = new List<(int, int)>();
                for (var a = 0; a < n; a++)
                {
                    for (var b = a + 1; b < n; b++)
                    {
                        var dab = D(a, b);
                        bool rngEdge = true, gabriel = true;
                        double mx = (g.Map[a].X + g.Map[b].X) / 2, my = (g.Map[a].Y + g.Map[b].Y) / 2;
                        for (var c = 0; c < n && (rngEdge || gabriel); c++)
                        {
                            if (c == a || c == b)
                            {
                                continue;
                            }

                            if (Math.Max(D(a, c), D(b, c)) < dab)
                            {
                                rngEdge = false;
                            }

                            double dx = g.Map[c].X - mx, dy = g.Map[c].Y - my;
                            if (dx * dx + dy * dy < dab / 4)
                            {
                                gabriel = false;
                            }
                        }

                        var extra = gabriel && rng.NextInt(100) < 25;
                        if (rngEdge || extra)
                        {
                            expected.Add((a, b));
                        }
                    }
                }

                Assert.That(g.Lanes.Select(l => (l.A, l.B)).ToList(), Is.EqualTo(expected), seed);
            }
        }

        [Test]
        public void Shapes_follow_the_count_and_the_option()
        {
            var big = SystemJson.Seeds(200).Select(s => Galaxy.Generate(s, Preset.Default with { Systems = 80 }).Shape).ToList();
            foreach (var shape in new[] { GalaxyShape.Spiral, GalaxyShape.Barred, GalaxyShape.Elliptical, GalaxyShape.Ring, GalaxyShape.Irregular })
            {
                Assert.That(big, Has.Member(shape));
            }

            Assert.That(100.0 * big.Count(s => s == GalaxyShape.Spiral) / big.Count, Is.InRange(30.0, 50.0));
            foreach (var shape in new[] { GalaxyShape.Spiral, GalaxyShape.Barred, GalaxyShape.Ring })
            {
                var g = Galaxy.Generate("shape", Preset.Default with { Shape = shape });
                Assert.That(g.Shape, Is.EqualTo(shape));
                Assert.That(g.Map.Count, Is.EqualTo(60));
            }

            Assert.That(Galaxy.Generate("arms", Preset.Default with { Shape = GalaxyShape.Spiral }).Arms, Is.InRange(2, 4));
            Assert.That(Galaxy.Generate("arms", Preset.Default with { Shape = GalaxyShape.Elliptical }).Arms, Is.Zero);
        }

        [Test]
        public void Danger_covers_one_to_ten()
        {
            var danger = Many.SelectMany(g => g.Map).GroupBy(m => m.Danger).ToDictionary(x => x.Key, x => x.Count());
            for (var d = 1; d <= 10; d++)
            {
                Assert.That(danger.ContainsKey(d), Is.True, $"danger {d} never occurs");
            }

            // The core starts at 1; regional noise adds at most 2 and the system's own at most 1.
            Assert.That(Many.All(g => g.Map[g.Core].Danger <= 4), Is.True, "the core is safe");
        }

        [Test]
        public void Small_and_large_galaxies_work()
        {
            var one = Galaxy.Generate("one", Preset.Default with { Systems = 1 });
            Assert.That(one.Map.Count, Is.EqualTo(1));
            Assert.That(one.Lanes, Is.Empty);
            Assert.That(one.Regions.Count, Is.EqualTo(1));
            Assert.That(one.System(0).Name, Is.EqualTo(one.Map[0].Name));

            var two = Galaxy.Generate("two", Preset.Default with { Systems = 2 });
            Assert.That(two.Lanes.Count, Is.EqualTo(1));
            Assert.That(two.Lanes[0].Bridge, Is.True);

            foreach (var shape in new[] { GalaxyShape.Spiral, GalaxyShape.Barred, GalaxyShape.Elliptical, GalaxyShape.Ring, GalaxyShape.Irregular })
            {
                var big = Galaxy.Generate("big", Preset.Default with { Systems = GeneratorOptions.MaxSystems, Shape = shape });
                Assert.That(big.Map.Count, Is.EqualTo(GeneratorOptions.MaxSystems), shape.ToString());
                Assert.That(big.Map.Select(m => m.Name).Distinct().Count(), Is.EqualTo(big.Map.Count));
                Assert.That(big.Map.All(m => m.Hops < int.MaxValue), Is.True);
            }
        }

        [Test]
        public void Invalid_input_is_refused_with_a_reason()
        {
            var e = Assert.Throws<ArgumentException>(() => Galaxy.Generate("x", Preset.Default with { Systems = 0 }));
            Assert.That(e!.Message, Does.StartWith("Systems must be 1 to 2000; you asked for 0."));
            e = Assert.Throws<ArgumentException>(() => Galaxy.Generate("x", Preset.Default with { Shape = (GalaxyShape)9 }));
            Assert.That(e!.Message, Does.StartWith("Shape must be a GalaxyShape"));
            var r = Assert.Throws<ArgumentOutOfRangeException>(() => Galaxy.Generate("x").System(60));
            Assert.That(r!.Message, Does.StartWith("This galaxy has 60 systems, numbered 0 to 59; you asked for 60."));
            Assert.Throws<ArgumentNullException>(() => Galaxy.Generate(null!));
        }

        [Test]
        public void Prints_the_statistics_and_timing_for_review()
        {
            var systems = Many.SelectMany(g => g.Map).ToList();
            void Line(string title, IEnumerable<string> keys)
            {
                var list = keys.ToList();
                var groups = list.GroupBy(k => k).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {100.0 * g.Count() / Math.Max(1, list.Count):0.#}%");
                TestContext.Out.WriteLine(title + ": " + string.Join(", ", groups));
            }

            TestContext.Out.WriteLine($"{Many.Count} galaxies, {systems.Count} systems");
            Line("shapes", Many.Select(g => g.Shape.ToString()));
            Line("stars", systems.Select(m => m.StarClass.ToString()));
            TestContext.Out.WriteLine("danger: " + string.Join(" ", Enumerable.Range(1, 10).Select(d => $"{d}:{systems.Count(m => m.Danger == d)}")));
            TestContext.Out.WriteLine($"lanes per system {Many.Sum(g => g.Lanes.Count) * 2.0 / systems.Count:0.00}; bridges per galaxy {Many.Average(g => g.Lanes.Count(l => l.Bridge)):0.0}; chokepoints per galaxy {Many.Average(g => g.Map.Count(m => m.Chokepoint)):0.0}; regions per galaxy {Many.Average(g => g.Regions.Count):0.0}");
            Line("region ages", Many.SelectMany(g => g.Regions).Select(r => r.Age.ToString()));

            var watch = Stopwatch.StartNew();
            for (var i = 0; i < 20; i++)
            {
                Galaxy.Generate("timing-" + i.ToString(CultureInfo.InvariantCulture));
            }

            var map = watch.Elapsed.TotalMilliseconds / 20;
            watch.Restart();
            for (var i = 0; i < 20; i++)
            {
                _ = Galaxy.Generate("timing-full-" + i.ToString(CultureInfo.InvariantCulture)).Systems.Sum(s => s.Planets.Count);
            }

            var full = watch.Elapsed.TotalMilliseconds / 20;
            watch.Restart();
            Galaxy.Generate("timing-big", Preset.Default with { Systems = GeneratorOptions.MaxSystems });
            TestContext.Out.WriteLine($"timing (warm): default galaxy map {map:0.00} ms, with every system in full {full:0.00} ms; 2,000-system map {watch.Elapsed.TotalMilliseconds:0} ms");
            Assert.That(full, Is.LessThan(50), "plan D19: a default galaxy under 50 ms");
        }

        private static bool Connected(Galaxy g, Lane without)
        {
            return Reach(g, -1, without) == g.Map.Count;
        }

        private static bool Connected(Galaxy g, int withoutSystem)
        {
            return Reach(g, withoutSystem, null) == g.Map.Count - 1;
        }

        private static int Reach(Galaxy g, int skip, Lane? skipLane)
        {
            var start = skip == 0 ? 1 : 0;
            if (start >= g.Map.Count)
            {
                return 0;
            }

            var seen = new HashSet<int> { start };
            var queue = new Queue<int>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                foreach (var l in g.Lanes)
                {
                    if (ReferenceEquals(l, skipLane) || (l.A != u && l.B != u))
                    {
                        continue;
                    }

                    var v = l.A == u ? l.B : l.A;
                    if (v != skip && seen.Add(v))
                    {
                        queue.Enqueue(v);
                    }
                }
            }

            return seen.Count;
        }
    }
}
