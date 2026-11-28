using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>Surface radiation and magnetic fields: derived from values already drawn, so no seed moves (radiation.json).</summary>
    public class RadiationTests
    {
        [Test]
        public void Radiation_matches_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("systems").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2", "s3", "s4", "k0", "k1" })
            {
                var s = StarSystem.Generate(seed);
                w.BeginObject().Name("address").String(s.Address).Name("planets").BeginArray();
                foreach (var p in s.Planets)
                {
                    w.BeginObject().Name("address").String(p.Address).Name("magnetosphere").Bool(p.Magnetosphere);
                    w.Name("radiation").String(p.Radiation.ToString()).Name("moons").BeginArray();
                    foreach (var m in p.Moons)
                    {
                        w.String(m.Radiation.ToString());
                    }

                    w.EndArray().EndObject();
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/radiation.json", w.ToString());
        }

        private static PlanetHost Host(StarClass c, StellarAge age) =>
            new PlanetHost(new Star { Class = c }, 1, 1, age);

        [Test]
        public void Earth_Mars_and_Venus_read_as_expected()
        {
            var sun = Host(StarClass.G, StellarAge.Mature);
            Assert.That(PlanetDetail.SurfaceRadiation(sun, true, 1), Is.EqualTo(Radiation.Low), "Earth");
            Assert.That(PlanetDetail.SurfaceRadiation(sun, false, 0.006), Is.EqualTo(Radiation.High), "Mars");
            Assert.That(PlanetDetail.SurfaceRadiation(sun, false, 92), Is.EqualTo(Radiation.Low), "Venus");
            Assert.That(PlanetDetail.SurfaceRadiation(sun, false, 0.1), Is.EqualTo(Radiation.Moderate), "thin air, no field");
        }

        [Test]
        public void Red_dwarfs_and_young_stars_raise_the_dose()
        {
            Assert.That(PlanetDetail.SurfaceRadiation(Host(StarClass.M, StellarAge.Mature), true, 1), Is.EqualTo(Radiation.Moderate));
            Assert.That(PlanetDetail.SurfaceRadiation(Host(StarClass.G, StellarAge.Young), true, 0.1), Is.EqualTo(Radiation.Moderate));
            Assert.That(PlanetDetail.SurfaceRadiation(Host(StarClass.M, StellarAge.Young), true, 1), Is.EqualTo(Radiation.High));
        }

        [Test]
        public void A_field_needs_iron_mass_and_a_fast_free_turn()
        {
            Assert.That(PlanetDetail.HasMagnetosphere(false, 1, 5.5, Spin.Free, 24), Is.True, "Earth");
            Assert.That(PlanetDetail.HasMagnetosphere(false, 0.11, 3.9, Spin.Free, 25), Is.False, "Mars");
            Assert.That(PlanetDetail.HasMagnetosphere(false, 0.8, 5.2, Spin.Free, 5832), Is.False, "Venus turns too slowly");
            Assert.That(PlanetDetail.HasMagnetosphere(false, 1, 5.5, Spin.Locked, 50), Is.False, "locked");
            Assert.That(PlanetDetail.HasMagnetosphere(true, 300, 1.3, Spin.Free, 10), Is.True, "giant");
        }
    }
}
