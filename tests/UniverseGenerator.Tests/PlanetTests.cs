using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The planet level (ai-docs/notes/2026-10-02-planet-level-design.md): analogues, properties and distributions.</summary>
    public class PlanetTests
    {
        private const int SystemSeeds = 10000;
        private const int LoneSeeds = 2000;
        private static List<Planet>? s_inSystems;
        private static List<Planet>? s_lone;

        private static List<Planet> InSystems => s_inSystems ??= SystemJson.Seeds(SystemSeeds).SelectMany(s => StarSystem.Generate(s).Planets).ToList();

        private static List<Planet> Lone => s_lone ??= SystemJson.Seeds(LoneSeeds).Select(s => Planet.Generate(s)).ToList();

        private static readonly Star s_sun = new Star { Class = StarClass.G, SpectralType = "G2V", Mass = 1, Luminosity = 1, Radius = 1, Temperature = 5778, Description = "yellow star" };

        private static Planet Analogue(PlanetKind kind, OrbitZone zone, double orbit, double mass, double radius, ulong seed) =>
            PlanetDetail.Apply(
                new Planet { Address = "test", Kind = kind, Zone = zone, Orbit = orbit, Mass = mass, Radius = radius, Period = Math.Round(365.25 * Math.Sqrt(orbit * orbit * orbit), 2) },
                new PlanetHost(s_sun, 1, 1, StellarAge.Mature), seed, 5);

        [Test]
        public void The_readme_example_runs()
        {
            var planet = Planet.Generate("my-seed");
            Assert.That(planet.Address, Is.EqualTo("v1-my-seed/planet"));
            TestContext.Out.WriteLine($"{planet.Name}: {planet.Descriptor}");
            TestContext.Out.WriteLine(planet.Summary);
            foreach (var p in Galaxy.Generate("my-seed").System(0).Planets)
            {
                TestContext.Out.WriteLine($"  {p.Name}: {p.Descriptor}; {p.Summary}");
            }
        }

        [Test]
        public void The_same_seed_gives_the_same_planet_and_a_number_equals_its_digits()
        {
            Assert.That(PlanetJson.LoneText(Planet.Generate("abc")), Is.EqualTo(PlanetJson.LoneText(Planet.Generate("abc"))));
            Assert.That(PlanetJson.LoneText(Planet.Generate(42)), Is.EqualTo(PlanetJson.LoneText(Planet.Generate("42"))));
            Assert.That(PlanetJson.LoneText(Planet.Generate(42)), Is.Not.EqualTo(PlanetJson.LoneText(Planet.Generate(-42))));
        }

        [Test]
        public void Invalid_input_is_refused_with_a_reason()
        {
            var e = Assert.Throws<ArgumentException>(() => Planet.Generate("x", Preset.Default with { Weirdness = -1 }));
            Assert.That(e!.Message, Does.StartWith("Weirdness must be 0 to 100; you asked for -1."));
            Assert.Throws<ArgumentNullException>(() => Planet.Generate(null!));
            Assert.Throws<ArgumentException>(() => Planet.Generate(new string('x', 201)));
            Assert.Throws<ArgumentNullException>(() => Planet.Generate("x").HabitabilityFor(null!));
        }

        [Test]
        public void An_earth_analogue_looks_like_earth()
        {
            for (ulong seed = 0; seed < 200; seed++)
            {
                var p = Analogue(PlanetKind.Garden, OrbitZone.Temperate, 1, 1, 1, seed);
                Assert.That(p.Gravity, Is.EqualTo(1));
                Assert.That(p.EscapeVelocity, Is.EqualTo(11.19));
                Assert.That(p.Density, Is.EqualTo(5.51));
                Assert.That(p.Atmosphere.LightestGasKept, Is.InRange(6.5, 8.0), "keeps water and nitrogen, loses helium");
                Assert.That(p.Atmosphere.Gases, Is.EqualTo(new[] { Gas.Nitrogen, Gas.Oxygen }));
                Assert.That(p.Atmosphere.Breathable, Is.True);
                Assert.That(p.Temperature, Is.InRange(274.0, 301.0));
                Assert.That(p.Life, Is.EqualTo(LifeLevel.Complex));
                Assert.That(p.Similarity, Is.GreaterThanOrEqualTo(0.9));
                Assert.That(p.Habitability, Is.EqualTo(Habitability.Ideal));
                Assert.That(p.Bands[0].Temperature, Is.GreaterThan(p.Bands[5].Temperature), "the equator is warmer than the poles");
                Assert.That(p.Biomes.Sum(b => b.Share), Is.InRange(0.97, 1.03));
                Assert.That(p.Hazard, Is.InRange(1, 2));
            }
        }

        [Test]
        public void A_venus_analogue_is_a_crushing_oven()
        {
            for (ulong seed = 0; seed < 50; seed++)
            {
                var p = Analogue(PlanetKind.Greenhouse, OrbitZone.Warm, 0.723, 0.815, 0.95, seed);
                Assert.That(p.Temperature, Is.GreaterThan(p.Atmosphere.Pressure >= 80 ? 700 : 500), p.Summary);
                Assert.That(p.Atmosphere.Class, Is.EqualTo(AtmosphereClass.Crushing));
                Assert.That(p.Atmosphere.Gases[0], Is.EqualTo(Gas.CarbonDioxide));
                Assert.That(p.Spin, Is.EqualTo(Spin.Resonant), "Venus turns very slowly but is not locked");
                Assert.That(p.Hazard, Is.EqualTo(5));
                Assert.That(p.Habitability, Is.EqualTo(Habitability.Hostile));
            }
        }

        [Test]
        public void A_mars_analogue_keeps_carbon_dioxide_and_loses_nitrogen()
        {
            for (ulong seed = 0; seed < 50; seed++)
            {
                var p = Analogue(PlanetKind.Desert, OrbitZone.Cold, 1.52, 0.107, 0.532, seed);
                Assert.That(p.Atmosphere.LightestGasKept, Is.InRange(28.5, 44.0));
                Assert.That(p.Atmosphere.Gases, Is.EqualTo(new[] { Gas.CarbonDioxide }));
                Assert.That(p.Atmosphere.Why, Does.Contain("lets nitrogen escape"));
                Assert.That(p.Temperature, Is.LessThan(273));
            }
        }

        [Test]
        public void Every_planet_is_consistent()
        {
            var problems = new List<string>();
            void Check(bool ok, Planet p, string what)
            {
                if (!ok && problems.Count < 30)
                {
                    problems.Add($"{p.Address} ({p.Kind}): {what}");
                }
            }

            foreach (var p in InSystems.Concat(Lone))
            {
                var giant = PlanetDetail.IsGiant(p.Kind);
                var a = p.Atmosphere;
                foreach (var x in new[] { p.Gravity, p.EscapeVelocity, p.Density, p.Insolation, p.Albedo, p.Temperature, p.DayTemperature, p.NightTemperature, p.Rotation, p.Tilt, p.Water, p.Ice, p.Similarity, a.LightestGasKept })
                {
                    Check(!double.IsNaN(x) && !double.IsInfinity(x) && x >= 0, p, "a value is NaN, infinite or negative");
                }

                Check(p.Temperature > 0 && p.NightTemperature <= p.Temperature && p.Temperature <= p.DayTemperature, p, $"temperatures {p.NightTemperature} {p.Temperature} {p.DayTemperature}");
                Check(p.Albedo > 0 && p.Albedo < 1, p, "albedo");
                Check((a.Class == AtmosphereClass.Envelope) == giant && (a.Pressure is null) == giant, p, "envelope exactly for giants");
                if (!giant)
                {
                    var pr = a.Pressure!.Value;
                    var expected = pr == 0 ? AtmosphereClass.None : pr < 0.01 ? AtmosphereClass.Trace : pr < 0.5 ? AtmosphereClass.Thin
                        : pr < 2 ? AtmosphereClass.Standard : pr < 10 ? AtmosphereClass.Dense : AtmosphereClass.Crushing;
                    Check(a.Class == expected, p, $"class {a.Class} for {pr} bar");
                    Check((pr == 0) == (a.Gases.Count == 0), p, "gases without air or air without gases");
                    Check(p.Bands.Count == 6 && Math.Abs(p.Bands.Sum(b => b.Share) - 1) < 0.001, p, "bands");
                    Check(Math.Abs(p.Biomes.Sum(b => b.Share) - 1) <= 0.03, p, $"biomes sum to {p.Biomes.Sum(b => b.Share)}");
                    Check(p.Water + p.Ice <= 1.01, p, "water and ice over the whole surface");
                    if (p.Kind != PlanetKind.Lava)
                    {
                        // The Jeans rule: every gas in the air is one the planet can keep, unless it is a trace leaking away.
                        var jeans = new Dictionary<Gas, double> { [Gas.Hydrogen] = 2, [Gas.Helium] = 4, [Gas.Methane] = 16, [Gas.WaterVapour] = 18, [Gas.Nitrogen] = 28, [Gas.Oxygen] = 32, [Gas.CarbonDioxide] = 44, [Gas.SulphurDioxide] = 64 };
                        Check(a.Class == AtmosphereClass.Trace || a.Gases.All(g => jeans[g] >= a.LightestGasKept), p, $"keeps a gas lighter than {a.LightestGasKept}");
                    }
                }
                else
                {
                    Check(p.Bands.Count == 0 && p.Biomes.Count == 0 && p.Water == 0 && p.Ice == 0, p, "a giant with a surface");
                }

                Check(!a.Breathable || (a.Gases.Contains(Gas.Oxygen) && a.Pressure >= 0.5 && a.Pressure <= 3), p, "breathable without oxygen");
                Check(p.Kind != PlanetKind.Garden || (p.Life == LifeLevel.Complex && p.Flora >= 2 && p.Fauna >= 1), p, "a garden without complex life");
                Check(p.Kind != PlanetKind.Garden || (p.Temperature >= 260 && p.Temperature <= 335), p, $"a garden at {p.Temperature} K");
                Check(p.Kind != PlanetKind.Ocean || p.Water + p.Ice >= 0.9, p, "an ocean world without its ocean");
                Check(p.Kind != PlanetKind.Ocean || p.Temperature < 373, p, $"a boiling ocean world at {p.Temperature} K");
                Check(p.Zone != OrbitZone.Temperate || !(p.Kind == PlanetKind.Rocky || p.Kind == PlanetKind.Desert) || p.Temperature <= 340, p, $"a temperate world at {p.Temperature} K");
                var dead = p.Kind == PlanetKind.Barren || p.Kind == PlanetKind.Iron || p.Kind == PlanetKind.Lava || p.Kind == PlanetKind.Dwarf || p.Kind == PlanetKind.HotJupiter;
                Check(!dead || p.Life == LifeLevel.None, p, "life on a dead world");
                Check(p.Flora == 0 || p.Life >= LifeLevel.Simple, p, "plants without life");
                Check(p.Biomes.All(b => b.Biome < Biome.Tundra || b.Biome > Biome.Rainforest) || p.Flora >= 1, p, "forests without flora");
                Check(p.Kind != PlanetKind.HotJupiter || p.Spin == Spin.Locked, p, "a hot Jupiter that is not locked");
                Check(p.Spin != Spin.Locked || Math.Abs(p.Rotation - Math.Round(p.Period * 24, 1)) < 0.11, p, "locked but the day is not the year");
                Check(p.Tilt >= 0 && p.Tilt <= 180, p, "tilt");
                Check(p.Traits.Count >= 1 && p.Traits.Count <= 2 && p.Traits.Distinct().Count() == p.Traits.Count, p, "traits");
                var r = p.Resources;
                Check(new[] { r.Metals, r.RareElements, r.Ices, r.Gases, r.Organics }.All(x => x >= 0 && x <= 5), p, "a resource grade outside 0 to 5");
                Check((r.Organics > 0) == (p.Life > LifeLevel.None || (a.Gases.Contains(Gas.Methane) && !giant)), p, "organics without life or methane");
                Check(p.Hazard >= 1 && p.Hazard <= 5 && (p.Hazard == 1) == (p.Hazards.Count == 0), p, $"hazard {p.Hazard} with {p.Hazards.Count} hazards");
                Check(!giant || p.Hazard == 5, p, "a giant that is safe to land on");
                Check(p.Similarity >= 0 && p.Similarity <= 1, p, "similarity");
                Check(p.Habitability == p.HabitabilityFor(Species.Human), p, "habitability is not the human one");
                Check(p.Habitability != Habitability.Ideal || p.Atmosphere.Breathable, p, "ideal without breathable air");
                Check(p.Summary.EndsWith("; hazard " + p.Hazard.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal), p, "summary");
            }

            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void A_lone_planet_draws_its_zone_and_agrees_with_its_star()
        {
            var shares = Lone.GroupBy(p => p.Zone).ToDictionary(g => g.Key, g => 100.0 * g.Count() / Lone.Count);
            Assert.That(shares[OrbitZone.Temperate], Is.GreaterThan(25), "temperate worlds lead");
            Assert.That(Lone.All(p => p.Name.EndsWith(" b", StringComparison.Ordinal) && p.Index == 0));
            Assert.That(Lone.Count(p => p.Kind == PlanetKind.Garden), Is.GreaterThan(LoneSeeds / 40));
            foreach (var p in Lone)
            {
                foreach (var m in p.Moons)
                {
                    Assert.That(m.Address, Does.StartWith(p.Address + "/moon/"));
                }
            }
        }

        [Test]
        public void Weirdness_sets_the_anomaly_share()
        {
            var share = 100.0 * InSystems.Count(p => p.Anomaly != null) / InSystems.Count;
            Assert.That(share, Is.InRange(4.0, 6.0));
            Assert.That(SystemJson.Seeds(300).Select(s => Planet.Generate(s, Preset.Default with { Weirdness = 0 })).Count(p => p.Anomaly != null), Is.EqualTo(0));
            Assert.That(SystemJson.Seeds(300).Select(s => Planet.Generate(s, Preset.Default with { Weirdness = 100 })).All(p => p.Anomaly != null));
        }

        [Test]
        public void Weirdness_changes_only_the_anomaly()
        {
            foreach (var seed in SystemJson.Seeds(100))
            {
                var calm = Planet.Generate(seed, Preset.Default with { Weirdness = 0 });
                var weird = Planet.Generate(seed, Preset.Default with { Weirdness = 100 });
                Assert.That(PlanetJson.LoneText(weird with { Anomaly = null }), Is.EqualTo(PlanetJson.LoneText(calm)));
            }
        }

        [Test]
        public void Gardens_are_mostly_livable_and_most_planets_are_not()
        {
            var gardens = InSystems.Where(p => p.Kind == PlanetKind.Garden).ToList();
            Assert.That(100.0 * gardens.Count(p => p.Habitability >= Habitability.Habitable) / gardens.Count, Is.GreaterThan(80));
            Assert.That(100.0 * InSystems.Count(p => p.Habitability == Habitability.Hostile) / InSystems.Count, Is.GreaterThan(60));
            var hardy = Species.Human with { Name = "hardy", MinTemperature = 200, MaxTemperature = 400, MaxGravity = 3, MinPressure = 0.01, MaxPressure = 20, BreathesOxygen = false };
            Assert.That(InSystems.Count(p => p.HabitabilityFor(hardy) == Habitability.Ideal), Is.GreaterThan(InSystems.Count(p => p.Habitability == Habitability.Ideal)));
        }

        [Test]
        public void Prints_the_statistics_for_review()
        {
            void Line(string title, IEnumerable<string> keys)
            {
                var list = keys.ToList();
                var groups = list.GroupBy(k => k).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {100.0 * g.Count() / Math.Max(1, list.Count):0.#}%");
                TestContext.Out.WriteLine(title + ": " + string.Join(", ", groups));
            }

            var all = InSystems;
            var surfaced = all.Where(p => !PlanetDetail.IsGiant(p.Kind)).ToList();
            TestContext.Out.WriteLine($"{all.Count} planets in {SystemSeeds} systems; {surfaced.Count} with a surface; {Lone.Count} lone planets");
            Line("atmosphere (surfaced)", surfaced.Select(p => p.Atmosphere.Class.ToString()));
            Line("first gas (surfaced)", surfaced.Where(p => p.Atmosphere.Gases.Count > 0).Select(p => p.Atmosphere.Gases[0].ToString()));
            Line("spin", all.Select(p => p.Spin.ToString()));
            Line("life", all.Select(p => p.Life.ToString()));
            Line("habitability", all.Select(p => p.Habitability.ToString()));
            Line("hazard", all.Select(p => p.Hazard.ToString(CultureInfo.InvariantCulture)));
            Line("hazards", all.SelectMany(p => p.Hazards));
            Line("traits", all.SelectMany(p => p.Traits));
            Line("dominant biome (surfaced)", surfaced.Select(p => p.Biomes[0].Biome.ToString()));
            Line("temperature by 50 K (surfaced)", surfaced.Select(p => ((int)p.Temperature / 50 * 50).ToString("D4", CultureInfo.InvariantCulture)));
            foreach (var kind in new[] { PlanetKind.Garden, PlanetKind.Ocean, PlanetKind.Rocky, PlanetKind.Desert, PlanetKind.Ice })
            {
                var ps = all.Where(p => p.Kind == kind).ToList();
                TestContext.Out.WriteLine($"{kind}: T {ps.Min(p => p.Temperature)} to {ps.Max(p => p.Temperature)} K (mean {ps.Average(p => p.Temperature):0}), pressure {ps.Average(p => p.Atmosphere.Pressure ?? 0):0.00} bar mean, water {ps.Average(p => p.Water):0.00}, ice {ps.Average(p => p.Ice):0.00}, ESI {ps.Average(p => p.Similarity):0.00}");
            }

            var systems = SystemJson.Seeds(SystemSeeds).Select(s => StarSystem.Generate(s)).ToList();
            TestContext.Out.WriteLine($"systems with an ideal world {100.0 * systems.Count(s => s.Planets.Any(p => p.Habitability == Habitability.Ideal)) / systems.Count:0.#}%, with a habitable or better world {100.0 * systems.Count(s => s.Planets.Any(p => p.Habitability >= Habitability.Habitable)) / systems.Count:0.#}%");
            Line("lone zones", Lone.Select(p => p.Zone.ToString()));
            Line("lone kinds", Lone.Select(p => p.Kind.ToString()));
            foreach (var p in Lone.Take(12))
            {
                TestContext.Out.WriteLine($"  {p.Name}: {p.Descriptor}; {p.Summary}; {string.Join(", ", p.Traits)}");
            }

            Assert.Pass();
        }
    }
}
