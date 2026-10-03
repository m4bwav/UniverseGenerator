using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The belt level (ai-docs/notes/2026-10-03-moon-and-belt-level-design.md): addresses, composition and resources.</summary>
    public class BeltTests
    {
        private const int SystemSeeds = 10000;
        private static List<StarSystem>? s_systems;

        private static List<StarSystem> Systems => s_systems ??= SystemJson.Seeds(SystemSeeds).Select(s => StarSystem.Generate(s)).ToList();

        [Test]
        public void The_suns_asteroid_belt_is_mostly_carbonaceous_and_cold()
        {
            for (var i = 0; i < 200; i++)
            {
                var b = BeltDetail.Apply(new Belt { Kind = BeltKind.Asteroid, Inner = 2.2, Outer = 3.3 }, 0, new Address(1, "sol" + i.ToString(CultureInfo.InvariantCulture), "system"), (ulong)i, "Sol", 1, 2.7);
                var c = b.Composition;
                Assert.That(c.Carbonaceous, Is.GreaterThan(c.Silicate).And.GreaterThan(c.Metal), b.Summary);
                Assert.That(c.Ice, Is.EqualTo(0));
                Assert.That(b.Temperature, Is.InRange(150.0, 180.0), "the main belt about 165 K");
                Assert.That(b.LargestBody, Is.InRange(100.0, 500.0), "Ceres 470 km");
                Assert.That(b.Summary, Does.StartWith(c.Carbonaceous.ToString(CultureInfo.InvariantCulture) + "% carbonaceous rock"));
                Assert.That(b.Name, Is.EqualTo("Sol Belt"));
                Assert.That(b.Address, Is.EqualTo("v1-sol" + i.ToString(CultureInfo.InvariantCulture) + "/system/belt/0"));
            }
        }

        [Test]
        public void Every_belt_is_consistent()
        {
            var problems = new List<string>();
            void Check(bool ok, Belt b, string what)
            {
                if (!ok && problems.Count < 30)
                {
                    problems.Add($"{b.Address} ({b.Kind}): {what}");
                }
            }

            foreach (var s in Systems)
            {
                for (var k = 0; k < s.Belts.Count; k++)
                {
                    var b = s.Belts[k];
                    var c = b.Composition;
                    Check(b.Index == k && b.Address == s.Address + "/belt/" + k.ToString(CultureInfo.InvariantCulture), b, "address or index");
                    Check(b.Name == s.Name + (b.Kind == BeltKind.Asteroid ? " Belt" : " Outer Belt"), b, $"name {b.Name}");
                    Check(c.Silicate + c.Carbonaceous + c.Metal + c.Ice == 100, b, "composition does not sum to 100");
                    Check(c.Silicate >= 5 && c.Carbonaceous >= 0 && c.Metal >= 0 && c.Ice >= 0, b, "a negative share or almost no silicate");
                    var middle = Math.Sqrt(b.Inner * b.Outer);
                    Check(b.Kind == BeltKind.Ice ? c.Ice >= 55 || b.Temperature > 150 : middle / s.FrostLine > 1.2 || c.Ice == 0, b, $"{c.Ice}% ice at {b.Temperature} K");
                    Check(c.Ice == 0 || b.Temperature < 250, b, $"ice at {b.Temperature} K");
                    Check(b.Kind == BeltKind.Asteroid ? b.Mass >= 0.0001 && b.Mass <= 0.003 : b.Mass >= 0.005 && b.Mass <= 0.2, b, $"mass {b.Mass}");
                    Check(b.Temperature > 0 && !double.IsNaN(b.Temperature), b, "temperature");
                    var r = b.Resources;
                    Check(new[] { r.Metals, r.RareElements, r.Ices, r.Gases, r.Organics }.All(x => x >= 0 && x <= 5), b, "a resource grade outside 0 to 5");
                    Check(r.Metals >= 1 && r.RareElements >= 1, b, "a belt with no metal");
                    Check((r.Gases > 0) == (c.Ice >= 30), b, "gases without ices, or ices without gases");
                    Check(b.Summary.Contains("; largest body ") && b.Summary.Contains(" °C; richest in "), b, "summary");
                }

                if (s.Belts.Count == 2)
                {
                    Check(s.Belts[0].Temperature > s.Belts[1].Temperature, s.Belts[1], "the outer belt is warmer than the asteroid belt");
                }
            }

            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Belts_far_out_hold_more_carbon_and_ice()
        {
            var asteroid = Systems.SelectMany(s => s.Belts.Where(b => b.Kind == BeltKind.Asteroid).Select(b => (b, x: Math.Sqrt(b.Inner * b.Outer) / s.FrostLine))).ToList();
            var inner = asteroid.Where(t => t.x < 0.6).Select(t => t.b).ToList();
            var outer = asteroid.Where(t => t.x > 1.5).Select(t => t.b).ToList();
            Assert.That(inner.Count, Is.GreaterThan(100));
            Assert.That(outer.Count, Is.GreaterThan(100));
            Assert.That(outer.Average(b => b.Composition.Carbonaceous), Is.GreaterThan(inner.Average(b => b.Composition.Carbonaceous) + 15));
            Assert.That(outer.Average(b => b.Resources.Ices), Is.GreaterThan(inner.Average(b => b.Resources.Ices)));
        }

        [Test]
        public void Prints_the_statistics_for_review()
        {
            var belts = Systems.SelectMany(s => s.Belts).ToList();
            TestContext.Out.WriteLine($"{belts.Count} belts in {SystemSeeds} systems");
            foreach (var kind in new[] { BeltKind.Asteroid, BeltKind.Ice })
            {
                var bs = belts.Where(b => b.Kind == kind).ToList();
                TestContext.Out.WriteLine($"{kind} ({bs.Count}): silicate {bs.Average(b => b.Composition.Silicate):0}%, carbonaceous {bs.Average(b => b.Composition.Carbonaceous):0}%, metal {bs.Average(b => b.Composition.Metal):0}%, ice {bs.Average(b => b.Composition.Ice):0}%; T {bs.Min(b => b.Temperature)} to {bs.Max(b => b.Temperature)} K; metals {bs.Average(b => b.Resources.Metals):0.0}, rare {bs.Average(b => b.Resources.RareElements):0.0}, ices {bs.Average(b => b.Resources.Ices):0.0}, gases {bs.Average(b => b.Resources.Gases):0.0}, organics {bs.Average(b => b.Resources.Organics):0.0}");
            }

            TestContext.Out.WriteLine($"icy belts over 170 K, too warm for ice: {100.0 * belts.Count(b => b.Kind == BeltKind.Ice && b.Temperature > 170) / belts.Count(b => b.Kind == BeltKind.Ice):0.#}%; asteroid belts over 1,000 K: {100.0 * belts.Count(b => b.Kind == BeltKind.Asteroid && b.Temperature > 1000) / belts.Count(b => b.Kind == BeltKind.Asteroid):0.#}%");
            foreach (var b in belts.Take(8))
            {
                TestContext.Out.WriteLine($"  {b.Name}: {b.Summary}");
            }

            Assert.Pass();
        }
    }
}
