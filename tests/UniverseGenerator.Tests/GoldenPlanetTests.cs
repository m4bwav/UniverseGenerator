using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The planet level's golden file: lone planets in full (fixed seeds and presets), and the planet-level fields of
    /// the planets of some systems and of a galaxy, whose other fields are in system.json and galaxy.json.
    /// </summary>
    public class GoldenPlanetTests
    {
        [Test]
        public void Planets_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("alone").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "-42", "", "Andromeda 7", "a/b c", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8" })
            {
                PlanetJson.Lone(w, Planet.Generate(seed));
            }

            w.EndArray();
            w.Name("plausible").BeginArray();
            foreach (var seed in new[] { "p1", "p2", "p3" })
            {
                PlanetJson.Lone(w, Planet.Generate(seed, Preset.Plausible));
            }

            w.EndArray();
            w.Name("weird").BeginArray();
            foreach (var seed in new[] { "w1", "w2", "w3" })
            {
                PlanetJson.Lone(w, Planet.Generate(seed, Preset.Default with { Weirdness = 100 }));
            }

            w.EndArray();
            w.Name("inSystems").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2", "s3", "s4" })
            {
                foreach (var p in StarSystem.Generate(seed).Planets)
                {
                    PlanetJson.Detail(w, p);
                }
            }

            w.EndArray();
            w.Name("inGalaxy").BeginArray();
            var galaxy = Galaxy.Generate("my-seed");
            foreach (var index in new[] { 0, galaxy.Core })
            {
                foreach (var p in galaxy.System(index).Planets)
                {
                    PlanetJson.Detail(w, p);
                }
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/planet.json", w.ToString());
        }
    }
}
