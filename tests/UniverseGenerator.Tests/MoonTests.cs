using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The moon level (ai-docs/notes/2026-10-03-moon-and-belt-level-design.md): analogues, properties and distributions.</summary>
    public class MoonTests
    {
        private const int SystemSeeds = 10000;
        private const int LoneSeeds = 2000;
        private static List<(Moon Moon, Planet Planet)>? s_all;

        private static List<(Moon Moon, Planet Planet)> All => s_all ??= SystemJson.Seeds(SystemSeeds).SelectMany(s => StarSystem.Generate(s).Planets)
            .Concat(SystemJson.Seeds(LoneSeeds).Select(s => Planet.Generate(s)))
            .SelectMany(p => p.Moons.Select(m => (m, p))).ToList();

        private static readonly Star s_sun = new Star { Class = StarClass.G, SpectralType = "G2V", Mass = 1, Luminosity = 1, Radius = 1, Temperature = 5778, Description = "yellow star" };
        private static readonly Planet s_jupiter = new Planet { Kind = PlanetKind.GasGiant, Zone = OrbitZone.Outer, Orbit = 5.2, Period = 4332.6, Mass = 317.8, Radius = 11.21, Tilt = 3.1 };
        private static readonly Planet s_saturn = new Planet { Kind = PlanetKind.GasGiant, Zone = OrbitZone.Outer, Orbit = 9.54, Period = 10759, Mass = 95.2, Radius = 9.45, Tilt = 26.7, Rings = true };
        private static readonly Planet s_earth = new Planet { Kind = PlanetKind.Garden, Zone = OrbitZone.Temperate, Orbit = 1, Period = 365.25, Mass = 1, Radius = 1, Tilt = 23.4 };

        private static ulong Seed(string address)
        {
            Assert.That(Address.TryParse(address, out var parsed, out _), Is.True, address);
            return parsed!.ObjectSeed;
        }

        private static Moon Analogue(MoonKind kind, double radiusKm, double orbitRadii, Planet parent, ulong seed) =>
            MoonDetail.Apply(new Moon { Address = "test", Name = "test", Kind = kind, Orbit = orbitRadii, Radius = radiusKm }, parent, new PlanetHost(s_sun, 1, 1, StellarAge.Mature), seed, 5);

        [Test]
        public void An_io_analogue_is_a_volcanic_hell_in_the_giants_radiation()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                var m = Analogue(MoonKind.Volcanic, 1822, 5.9, s_jupiter, seed);
                Assert.That(m.Period, Is.EqualTo(1.77).Within(0.02), "Io circles Jupiter in 1.77 days");
                Assert.That(m.Rotation, Is.EqualTo(Math.Round(m.Period * 24, 1)));
                Assert.That(m.Gravity, Is.InRange(0.17, 0.2), "Io 0.183 g");
                Assert.That(m.EscapeVelocity, Is.InRange(2.4, 2.7), "Io 2.56 km/s");
                Assert.That(m.TidalHeating, Is.InRange(1.0, 10.0));
                Assert.That(m.Atmosphere.Gases, Is.EqualTo(new[] { Gas.SulphurDioxide }));
                Assert.That(m.Atmosphere.Class, Is.EqualTo(AtmosphereClass.Trace));
                Assert.That(m.Hazards, Does.Contain("the giant's radiation belts").And.Contain("eruptions"));
                Assert.That(m.Hazard, Is.EqualTo(5));
                Assert.That(m.Life, Is.EqualTo(LifeLevel.None));
                Assert.That(m.Descriptor, Is.EqualTo("a volcanic moon of a gas giant"));
            }
        }

        [Test]
        public void A_europa_analogue_hides_an_ocean_under_its_ice()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                var m = Analogue(MoonKind.Ocean, 1561, 9.4, s_jupiter, seed);
                Assert.That(m.Period, Is.EqualTo(3.55).Within(0.03), "Europa circles Jupiter in 3.55 days");
                Assert.That(m.SubsurfaceOcean, Is.True);
                Assert.That(m.Ice, Is.EqualTo(1));
                Assert.That(m.Water, Is.EqualTo(0));
                Assert.That(m.Temperature, Is.LessThan(130), "Europa about 102 K");
                Assert.That(m.TidalHeating, Is.InRange(0.02, 0.5), "Europa about 0.1 W/m²");
                Assert.That(m.Life, Is.LessThanOrEqualTo(LifeLevel.Simple));
                Assert.That(m.Flora, Is.EqualTo(0));
                Assert.That(m.Summary, Does.Contain("an ocean under the ice"));
            }
        }

        [Test]
        public void A_titan_analogue_keeps_a_cold_nitrogen_and_methane_sky()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                var m = Analogue(MoonKind.Hazy, 2575, 20.3, s_saturn, seed);
                Assert.That(m.Period, Is.EqualTo(15.95).Within(0.15), "Titan circles Saturn in 15.95 days");
                Assert.That(m.Gravity, Is.InRange(0.12, 0.15), "Titan 0.138 g");
                Assert.That(m.Atmosphere.Gases, Is.EqualTo(new[] { Gas.Nitrogen, Gas.Methane }));
                Assert.That(m.Atmosphere.Pressure, Is.InRange(0.5, 5.0), "Titan 1.5 bar");
                Assert.That(m.Temperature, Is.InRange(85.0, 120.0), m.Summary + " (Titan 94 K)");
                Assert.That(m.SubsurfaceOcean, Is.True);
                Assert.That(m.Resources.Organics, Is.GreaterThanOrEqualTo(2));
                Assert.That(m.Descriptor, Is.EqualTo("a hazy moon of a ringed gas giant"));
            }
        }

        [Test]
        public void A_moon_analogue_is_an_airless_rock()
        {
            for (ulong seed = 0; seed < 100; seed++)
            {
                var m = Analogue(MoonKind.Barren, 1737, 60.3, s_earth, seed);
                Assert.That(m.Period, Is.EqualTo(27.4).Within(0.2), "the Moon circles the Earth in 27.3 days (the Earth alone, 27.4)");
                Assert.That(m.Gravity, Is.InRange(0.13, 0.18), "the Moon 0.165 g");
                Assert.That(m.Mass, Is.InRange(0.009, 0.014), "the Moon 0.0123 Earths");
                Assert.That(m.Atmosphere.Class, Is.EqualTo(AtmosphereClass.None));
                Assert.That(m.TidalHeating, Is.LessThan(0.001));
                Assert.That(m.Hazards, Does.Contain("vacuum"));
                Assert.That(m.Life, Is.EqualTo(LifeLevel.None));
            }
        }

        [Test]
        public void Every_moon_is_consistent()
        {
            var problems = new List<string>();
            void Check(bool ok, Moon m, string what)
            {
                if (!ok && problems.Count < 30)
                {
                    problems.Add($"{m.Address} ({m.Kind}): {what}");
                }
            }

            var jeans = new Dictionary<Gas, double> { [Gas.Hydrogen] = 2, [Gas.Helium] = 4, [Gas.Methane] = 16, [Gas.WaterVapour] = 18, [Gas.Nitrogen] = 28, [Gas.Oxygen] = 32, [Gas.CarbonDioxide] = 44, [Gas.SulphurDioxide] = 64 };
            foreach (var (m, p) in All)
            {
                var a = m.Atmosphere;
                foreach (var x in new[] { m.Distance, m.Period, m.Density, m.Mass, m.Gravity, m.EscapeVelocity, m.Insolation, m.Albedo, m.TidalHeating, m.Temperature, m.DayTemperature, m.NightTemperature, m.Water, m.Ice, m.Rotation, m.Tilt, m.Similarity, a.LightestGasKept })
                {
                    Check(!double.IsNaN(x) && !double.IsInfinity(x) && x >= 0, m, "a value is NaN, infinite or negative");
                }

                Check(m.Gravity > 0 && m.EscapeVelocity > 0 && m.Period > 0 && m.Distance > 0, m, "zero gravity, escape velocity, period or distance");
                Check(Math.Abs(m.Mass - m.Density / 5.514 * Math.Pow(m.Radius / 6371, 3)) <= 0.000002, m, "mass, density and radius disagree");
                Check(Math.Abs(m.Rotation - Math.Round(m.Period * 24, 1)) < 0.11, m, "it does not keep one face to its planet");
                Check(m.Tilt == p.Tilt && m.Insolation == p.Insolation, m, "its tilt or light differs from its planet's");
                Check(m.NightTemperature <= m.Temperature && m.Temperature <= m.DayTemperature, m, $"temperatures {m.NightTemperature} {m.Temperature} {m.DayTemperature}");
                Check(m.Albedo > 0 && m.Albedo < 1, m, "albedo");
                var pr = a.Pressure;
                Check(pr != null && a.Class == PlanetDetail.Classify(pr.Value), m, $"class {a.Class} for {pr} bar");
                Check((pr == 0) == (a.Gases.Count == 0), m, "gases without air or air without gases");
                var renewed = a.Why.Contains("renew");
                Check(!renewed || m.Kind == MoonKind.Hazy || m.Kind == MoonKind.Garden, m, "air renewed on a kind that does not promise it");
                Check(a.Class == AtmosphereClass.Trace || renewed || a.Gases.All(g => jeans[g] >= a.LightestGasKept), m, $"keeps a gas lighter than {a.LightestGasKept}");
                Check(!a.Breathable || (a.Gases.Contains(Gas.Oxygen) && pr >= 0.5 && pr <= 3), m, "breathable without oxygen");
                Check(m.Kind != MoonKind.Hazy || (pr >= 0.5 && a.Gases.Contains(Gas.Nitrogen) && a.Gases.Contains(Gas.Methane)), m, "a hazy moon without its haze");
                Check(m.Kind != MoonKind.Garden || (a.Breathable && m.Life == LifeLevel.Complex && m.Flora >= 2 && m.Fauna >= 1), m, "a garden moon without air or life");
                Check((m.Kind != MoonKind.Barren && m.Kind != MoonKind.Volcanic) || m.Life == LifeLevel.None, m, "life on a barren or volcanic moon");
                Check(m.Flora == 0 || m.Life >= LifeLevel.Simple, m, "plants without life");
                Check(m.Kind != MoonKind.Volcanic || m.TidalHeating >= 1, m, $"a volcanic moon heated {m.TidalHeating} W/m²");
                Check(m.Kind != MoonKind.Ocean || (m.TidalHeating >= 0.02 && m.SubsurfaceOcean && m.Ice == 1), m, "an ocean moon without its hidden ocean");
                Check(m.Kind == MoonKind.Volcanic || m.TidalHeating <= 0.5, m, $"a {m.Kind} moon heated {m.TidalHeating} W/m²");
                Check(m.Bands.Count == 6 && Math.Abs(m.Bands.Sum(b => b.Share) - 1) < 0.001, m, "bands");
                Check(Math.Abs(m.Biomes.Sum(b => b.Share) - 1) <= 0.03, m, $"biomes sum to {m.Biomes.Sum(b => b.Share)}");
                Check(m.Biomes.All(b => b.Biome < Biome.Tundra || b.Biome > Biome.Rainforest) || m.Flora >= 1, m, "forests without flora");
                Check(m.Traits.Count >= 1 && m.Traits.Count <= 2 && m.Traits.Distinct().Count() == m.Traits.Count, m, "traits");
                var r = m.Resources;
                Check(new[] { r.Metals, r.RareElements, r.Ices, r.Gases, r.Organics }.All(x => x >= 0 && x <= 5), m, "a resource grade outside 0 to 5");
                Check(m.Hazard >= 1 && m.Hazard <= 5 && (m.Hazard == 1) == (m.Hazards.Count == 0), m, $"hazard {m.Hazard} with {m.Hazards.Count} hazards");
                Check(m.Similarity >= 0 && m.Similarity <= 1, m, "similarity");
                Check(m.Habitability == m.HabitabilityFor(Species.Human), m, "habitability is not the human one");
                Check(m.Habitability != Habitability.Ideal || a.Breathable, m, "ideal without breathable air");
                Check(m.Descriptor.EndsWith(Story.PlanetNoun(p.Kind), StringComparison.Ordinal), m, "descriptor");
                Check(m.Summary.EndsWith("; hazard " + m.Hazard.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal), m, "summary");
            }

            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Inner_moons_circle_faster()
        {
            foreach (var group in All.GroupBy(x => x.Planet.Address))
            {
                var moons = group.Select(x => x.Moon).ToList();
                for (var j = 1; j < moons.Count; j++)
                {
                    Assert.That(moons[j].Period, Is.GreaterThanOrEqualTo(moons[j - 1].Period), moons[j].Address);
                    Assert.That(moons[j].Distance, Is.GreaterThan(moons[j - 1].Distance), moons[j].Address);
                }
            }
        }

        [Test]
        public void Garden_moons_are_mostly_livable_and_most_moons_are_not()
        {
            // People need 0.2 g for a world to count as habitable (Species.Human), and most garden moons are small: the
            // rest are marginal for people, though a game's own species may thrive there.
            var gardens = All.Where(x => x.Moon.Kind == MoonKind.Garden).Select(x => x.Moon).ToList();
            Assert.That(gardens.Count, Is.GreaterThan(30));
            Assert.That(gardens.All(m => m.Habitability >= Habitability.Marginal && m.Hazard <= 3), Is.True);
            var heavy = gardens.Where(m => m.Gravity >= 0.2).ToList();
            Assert.That(100.0 * heavy.Count(m => m.Habitability >= Habitability.Habitable) / heavy.Count, Is.GreaterThan(80));
            var lowGravity = Species.Human with { Name = "low-gravity", MinGravity = 0.02 };
            Assert.That(100.0 * gardens.Count(m => m.HabitabilityFor(lowGravity) >= Habitability.Habitable) / gardens.Count, Is.GreaterThan(80));
            Assert.That(100.0 * All.Count(x => x.Moon.Habitability == Habitability.Hostile) / All.Count, Is.GreaterThan(70));
        }

        [Test]
        public void Weirdness_sets_the_anomaly_share()
        {
            var share = 100.0 * All.Count(x => x.Moon.Anomaly != null) / All.Count;
            Assert.That(share, Is.InRange(4.0, 6.0));
            Assert.That(SystemJson.Seeds(300).SelectMany(s => Planet.Generate(s, Preset.Default with { Weirdness = 0 }).Moons).Count(m => m.Anomaly != null), Is.EqualTo(0));
            Assert.That(SystemJson.Seeds(300).SelectMany(s => Planet.Generate(s, Preset.Default with { Weirdness = 100 }).Moons).All(m => m.Anomaly != null));
        }

        [Test]
        public void Moon_detail_leaves_its_planet_as_it_was()
        {
            // The moon level reads its planet; it never feeds back into it.
            foreach (var seed in SystemJson.Seeds(200))
            {
                var p = Planet.Generate(seed);
                Assert.That(PlanetJson.LoneText(p with { Moons = p.Moons.Select(m => new Moon { Address = m.Address, Index = m.Index, Name = m.Name, Kind = m.Kind, Orbit = m.Orbit, Radius = m.Radius }).ToArray() }), Is.EqualTo(PlanetJson.LoneText(p)));
            }
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

            var moons = All.Select(x => x.Moon).ToList();
            TestContext.Out.WriteLine($"{moons.Count} moons around the planets of {SystemSeeds} systems and {LoneSeeds} lone planets");
            Line("kind", moons.Select(m => m.Kind.ToString()));
            Line("atmosphere", moons.Select(m => m.Atmosphere.Class.ToString()));
            Line("life", moons.Select(m => m.Life.ToString()));
            Line("habitability", moons.Select(m => m.Habitability.ToString()));
            Line("hazard", moons.Select(m => m.Hazard.ToString(CultureInfo.InvariantCulture)));
            Line("hazards", moons.SelectMany(m => m.Hazards));
            Line("traits", moons.SelectMany(m => m.Traits));
            foreach (var kind in new[] { MoonKind.Volcanic, MoonKind.Ocean })
            {
                var parts = All.Where(x => x.Moon.Kind == kind).Select(x => MoonDetail.Tides(Seed(x.Moon.Address), x.Moon, x.Planet, x.Moon.Distance)).ToList();
                TestContext.Out.WriteLine($"{kind} moons heated by the floor, not the tides formula: {100.0 * parts.Count(t => t.Floor > t.Flow) / parts.Count:0.#}%");
            }

            TestContext.Out.WriteLine($"hidden oceans {100.0 * moons.Count(m => m.SubsurfaceOcean) / moons.Count:0.#}%; air renewed {moons.Count(m => m.Atmosphere.Why.Contains("renew"))} of {moons.Count(m => m.Kind == MoonKind.Hazy || m.Kind == MoonKind.Garden)} hazy and garden moons");
            foreach (var kind in new[] { MoonKind.Barren, MoonKind.Ice, MoonKind.Volcanic, MoonKind.Ocean, MoonKind.Hazy, MoonKind.Garden })
            {
                var ms = moons.Where(m => m.Kind == kind).OrderBy(m => m.TidalHeating).ToList();
                TestContext.Out.WriteLine($"{kind} ({ms.Count}): radius {ms.Min(m => m.Radius)} to {ms.Max(m => m.Radius)} km, g {ms.Average(m => m.Gravity):0.000} mean, T {ms.Min(m => m.Temperature)} to {ms.Max(m => m.Temperature)} K (mean {ms.Average(m => m.Temperature):0}), heat median {ms[ms.Count / 2].TidalHeating} W/m² (at 0.5: {100.0 * ms.Count(m => m.TidalHeating == 0.5) / ms.Count:0.#}%), over 273 K {100.0 * ms.Count(m => m.Temperature > 273) / ms.Count:0.#}%, ESI {ms.Average(m => m.Similarity):0.00}");
            }

            foreach (var m in moons.Where(m => m.Kind == MoonKind.Garden).Take(4).Concat(moons.Take(12)))
            {
                TestContext.Out.WriteLine($"  {m.Name}: {m.Descriptor}; {m.Summary}; {string.Join(", ", m.Traits)}");
            }

            Assert.Pass();
        }
    }
}
