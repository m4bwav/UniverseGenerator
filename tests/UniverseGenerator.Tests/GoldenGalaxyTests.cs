using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy level's golden file: fixed seeds and options, the fields of <see cref="GalaxyJson"/>, and two systems
    /// of each galaxy in full (the first and the core), which proves the context a galaxy gives its systems.
    /// </summary>
    public class GoldenGalaxyTests
    {
        [Test]
        public void Galaxies_match_the_golden_file()
        {
            var cases = new (string Seed, GeneratorOptions Options)[]
            {
                ("my-seed", Preset.Default),
                ("42", Preset.Default),
                ("Andromeda 7", Preset.Default),
                ("spiral", Preset.Default with { Systems = 120 }),
                ("bar", Preset.Default with { Systems = 80, Shape = GalaxyShape.Barred }),
                ("ring", Preset.Plausible with { Systems = 30, Shape = GalaxyShape.Ring }),
                ("one", Preset.Default with { Systems = 1 }),
                ("two", Preset.Default with { Systems = 2 }),
            };

            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("galaxies").BeginArray();
            foreach (var (seed, options) in cases)
            {
                var g = Galaxy.Generate(seed, options);
                w.BeginObject().Name("galaxy");
                GalaxyJson.Write(w, g);
                w.Name("systems").BeginArray();
                SystemJson.Write(w, g.System(0));
                if (g.Core != 0)
                {
                    SystemJson.Write(w, g.System(g.Core));
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/galaxy.json", w.ToString());
        }
    }
}
