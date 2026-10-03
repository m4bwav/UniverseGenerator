using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    public class StarSystemTests
    {
        private const int PropertySeeds = 10000;
        private static List<StarSystem>? s_many;

        private static List<StarSystem> Many => s_many ??= SystemJson.Seeds(PropertySeeds).Select(s => StarSystem.Generate(s)).ToList();

        [Test]
        public void The_readme_example_runs()
        {
            var system = StarSystem.Generate("my-seed");
            Assert.That(system.Name, Is.Not.Empty);
            Assert.That(system.Address, Is.EqualTo("v1-my-seed/system"));
            TestContext.Out.WriteLine($"{system.Name}: {system.Star.SpectralType} {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
            TestContext.Out.WriteLine(system.Descriptor);
            foreach (var p in system.Planets)
            {
                TestContext.Out.WriteLine($"  {p.Name}: {p.Descriptor}, {p.Orbit} au, {p.Mass} Earths, {p.Moons.Count} moons");
            }
        }

        [Test]
        public void The_same_seed_gives_the_same_system_and_a_number_equals_its_digits()
        {
            Assert.That(SystemJson.Text(StarSystem.Generate("abc")), Is.EqualTo(SystemJson.Text(StarSystem.Generate("abc"))));
            Assert.That(SystemJson.Text(StarSystem.Generate(42)), Is.EqualTo(SystemJson.Text(StarSystem.Generate("42"))));
            Assert.That(SystemJson.Text(StarSystem.Generate(42)), Is.Not.EqualTo(SystemJson.Text(StarSystem.Generate(-42))));
        }

        [Test]
        public void Context_from_a_galaxy_changes_only_what_it_sets()
        {
            var alone = StarSystem.Generate("ctx");
            var address = new Address(GeneratorVersion.Current, "ctx", "system");
            var placed = StarSystemGenerator.Generate(address, new SystemContext(alone.Name, alone.Age, alone.Danger), Preset.Default);
            Assert.That(SystemJson.Text(placed), Is.EqualTo(SystemJson.Text(alone)));
        }

        [Test]
        public void Invalid_input_is_refused_with_a_reason()
        {
            var e = Assert.Throws<ArgumentException>(() => StarSystem.Generate("x", Preset.Default with { Weirdness = 101 }));
            Assert.That(e!.Message, Does.StartWith("Weirdness must be 0 to 100; you asked for 101."));
            e = Assert.Throws<ArgumentException>(() => StarSystem.Generate("x", Preset.Default with { MaxPlanetsPerSystem = -1 }));
            Assert.That(e!.Message, Does.StartWith("MaxPlanetsPerSystem must be 0 to 20; you asked for -1."));
            Assert.Throws<ArgumentNullException>(() => StarSystem.Generate(null!));
            Assert.Throws<ArgumentException>(() => StarSystem.Generate(new string('x', 201)));
        }

        [Test]
        public void MaxPlanetsPerSystem_caps_the_count_and_keeps_the_inner_planets()
        {
            foreach (var seed in SystemJson.Seeds(200))
            {
                var full = StarSystem.Generate(seed);
                var capped = StarSystem.Generate(seed, Preset.Default with { MaxPlanetsPerSystem = 2 });
                Assert.That(capped.Planets.Count, Is.EqualTo(Math.Min(2, full.Planets.Count)));
                for (var i = 0; i < capped.Planets.Count; i++)
                {
                    Assert.That(capped.Planets[i].Orbit, Is.EqualTo(full.Planets[i].Orbit));
                    Assert.That(capped.Planets[i].Kind, Is.EqualTo(full.Planets[i].Kind));
                }
            }
        }

        [Test]
        public void Every_system_is_consistent()
        {
            var problems = new List<string>();
            void Check(bool ok, StarSystem s, string what)
            {
                if (!ok && problems.Count < 30)
                {
                    problems.Add($"{s.Address}: {what}");
                }
            }

            foreach (var s in Many)
            {
                Check(s.Danger >= 1 && s.Danger <= 10, s, "danger out of range");
                Check(s.Landmarks.Count >= 1, s, "no landmark");
                Check(s.Tags.Count >= 1 && s.Tags.Count <= 2 && s.Tags.Distinct().Count() == s.Tags.Count, s, "tags");
                Check(s.Planets.Count <= 12, s, "too many planets");
                Check(s.Star.Mass > 0 && !double.IsNaN(s.Star.Temperature), s, "star values");
                Check(s.HabitableZoneInner < s.HabitableZoneOuter && s.HabitableZoneOuter < s.FrostLine, s, "zones out of order");
                Check(s.Planets.Select(p => p.Name).Distinct().Count() == s.Planets.Count, s, "planet names repeat");
                if (s.Companion != null)
                {
                    Check(s.Companion.Star.Mass <= s.Star.Mass, s, "companion heavier than the primary");
                }

                double previous = 0;
                foreach (var p in s.Planets)
                {
                    Check(p.Orbit > previous, s, $"{p.Name} orbit not increasing");
                    previous = p.Orbit;
                    Check(p.Mass > 0 && p.Radius > 0 && p.Period > 0, s, $"{p.Name} values");
                    Check(!(p.Radius > 1.34 && p.Radius < 1.99), s, $"{p.Name} radius {p.Radius} in the radius valley");
                    Check(!(p.Kind == PlanetKind.SubNeptune && p.Period < 10), s, $"{p.Name} sub-Neptune in the hot Neptune desert");
                    Check(p.Kind != PlanetKind.HotJupiter || p.Zone == OrbitZone.Hot, s, $"{p.Name} hot Jupiter outside the hot zone");
                    Check(p.Descriptor.Length > 0 && p.Descriptor[0] == 'a', s, $"{p.Name} descriptor");
                    double moonOrbit = 0;
                    foreach (var m in p.Moons)
                    {
                        Check(m.Orbit > moonOrbit && m.Radius > 0, s, $"{m.Name} orbit or radius");
                        moonOrbit = m.Orbit;
                    }
                }

                foreach (var b in s.Belts)
                {
                    Check(b.Inner < b.Outer, s, "belt edges");
                    Check(s.Planets.All(p => p.Orbit < b.Inner || p.Orbit > b.Outer), s, "a planet inside a belt");
                }

                foreach (var st in s.Stations)
                {
                    Check(st.Planet == null || (st.Planet >= 0 && st.Planet < s.Planets.Count), s, $"{st.Name} orbits a planet that does not exist");
                }

                Check(s.Stations.Select(st => st.Name).Distinct().Count() == s.Stations.Count, s, "station names repeat");
            }

            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Spectral_types_agree_with_the_star_class()
        {
            foreach (var s in Many.Select(x => x.Star))
            {
                var c = s.Class;
                if (c <= StarClass.M)
                {
                    Assert.That(s.SpectralType[0].ToString(), Is.EqualTo(c.ToString()), $"{s.SpectralType} for a {c} star at {s.Temperature} K");
                    Assert.That(s.SpectralType, Does.EndWith("V"));
                }
            }

            Assert.That(Many.Count(x => x.Star.Class == StarClass.G && x.Star.SpectralType == "G2V"), Is.GreaterThan(0));
        }

        [Test]
        public void The_game_mix_meets_its_own_targets()
        {
            // Plan D18: M 31%, K 21%, G 14%, F 10%, white dwarf 7%, A 6%, giant 5%, B 3%; tolerance from the age blend.
            var share = Many.GroupBy(s => s.Star.Class).ToDictionary(g => g.Key, g => 100.0 * g.Count() / Many.Count);
            double Share(StarClass c) => share.TryGetValue(c, out var v) ? v : 0;
            Assert.That(Share(StarClass.M), Is.InRange(27.0, 35.0));
            Assert.That(Share(StarClass.K), Is.InRange(17.0, 25.0));
            Assert.That(Share(StarClass.G), Is.InRange(12.0, 19.0));
            Assert.That(Share(StarClass.F), Is.InRange(7.0, 13.0));
            Assert.That(Share(StarClass.WhiteDwarf), Is.InRange(4.0, 9.0));
            Assert.That(Share(StarClass.B), Is.InRange(1.0, 4.0));
            Assert.That(Share(StarClass.BlackHole), Is.GreaterThan(0));
        }

        [Test]
        public void Plausible_is_mostly_red_dwarfs()
        {
            var systems = SystemJson.Seeds(3000).Select(s => StarSystem.Generate(s, Preset.Plausible)).ToList();
            Assert.That(100.0 * systems.Count(s => s.Star.Class == StarClass.M) / systems.Count, Is.InRange(68.0, 77.0));
            Assert.That(systems.Count(s => s.Star.Class == StarClass.O), Is.EqualTo(0));
        }

        [Test]
        public void Weirdness_sets_the_outlier_share()
        {
            var share = 100.0 * Many.Count(s => s.Landmarks.Any(l => l.Outlier)) / Many.Count;
            Assert.That(share, Is.InRange(4.0, 6.0));
            Assert.That(SystemJson.Seeds(500).Select(s => StarSystem.Generate(s, Preset.Default with { Weirdness = 0 })).Count(s => s.Landmarks.Any(l => l.Outlier)), Is.EqualTo(0));
        }

        [Test]
        public void Prints_the_statistics_for_review()
        {
            var planets = Many.SelectMany(s => s.Planets).ToList();
            void Line(string title, IEnumerable<string> keys)
            {
                var groups = keys.GroupBy(k => k).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {100.0 * g.Count() / Math.Max(1, keys.Count()):0.#}%");
                TestContext.Out.WriteLine(title + ": " + string.Join(", ", groups));
            }

            TestContext.Out.WriteLine($"{Many.Count} systems, {planets.Count} planets ({(double)planets.Count / Many.Count:0.00} per system), {planets.Sum(p => p.Moons.Count)} moons");
            Line("stars", Many.Select(s => s.Star.Class.ToString()));
            Line("planets", planets.Select(p => p.Kind.ToString()));
            Line("planet count", Many.Select(s => s.Planets.Count.ToString(CultureInfo.InvariantCulture)));
            Line("landmarks", Many.SelectMany(s => s.Landmarks).Select(l => l.Kind.ToString()));
            Line("tags", Many.SelectMany(s => s.Tags));
            TestContext.Out.WriteLine($"binaries {100.0 * Many.Count(s => s.Companion != null) / Many.Count:0.#}%, belts {(double)Many.Sum(s => s.Belts.Count) / Many.Count:0.00} per system, stations {(double)Many.Sum(s => s.Stations.Count) / Many.Count:0.00} per system, living worlds {100.0 * Many.Count(s => s.Landmarks.Any(l => l.Kind == LandmarkKind.LivingWorld)) / Many.Count:0.#}%");
            TestContext.Out.WriteLine("distinct names " + Many.Select(s => s.Name).Distinct().Count());
            Assert.Pass();
        }
    }
}
