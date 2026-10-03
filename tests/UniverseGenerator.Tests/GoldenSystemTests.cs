using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The star system level's golden file: fixed seeds and presets, the fields of <see cref="SystemJson"/>.</summary>
    public class GoldenSystemTests
    {
        [Test]
        public void Systems_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("default").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "-42", "", "Andromeda 7", "a/b c", "s1", "s2", "s3", "s4", "s5", "s6" })
            {
                SystemJson.Write(w, StarSystem.Generate(seed));
            }

            w.EndArray();
            w.Name("plausible").BeginArray();
            foreach (var seed in new[] { "p1", "p2", "p3" })
            {
                SystemJson.Write(w, StarSystem.Generate(seed, Preset.Plausible));
            }

            w.EndArray();
            w.Name("weird").BeginArray();
            foreach (var seed in new[] { "w1", "w2", "w3" })
            {
                SystemJson.Write(w, StarSystem.Generate(seed, Preset.Default with { Weirdness = 100, MaxPlanetsPerSystem = 3 }));
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/system.json", w.ToString());
        }
    }
}
