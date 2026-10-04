using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// presets.json: the galaxies of the presets and options added after 1.0.0-beta.1 (Pocket, Roguelike, Cozy, Epic,
    /// Arms, ExtraLanes, DangerShift), with the fields of <see cref="GalaxyJson"/> and the first system in full, and a lone
    /// system with a danger shift. A new file, so galaxy.json and system.json never change.
    /// </summary>
    public class GoldenPresetTests
    {
        private static readonly (string Seed, GeneratorOptions Options)[] Cases =
        {
            ("my-seed", Preset.Pocket),
            ("my-seed", Preset.Roguelike),
            ("my-seed", Preset.Cozy),
            ("my-seed", Preset.Epic),
            ("spiral", Preset.Default with { Systems = 120, Arms = 4 }),
            ("bar", Preset.Default with { Systems = 80, Shape = GalaxyShape.Barred, Arms = 2 }),
            ("42", Preset.Default with { ExtraLanes = 0 }),
            ("42", Preset.Default with { ExtraLanes = 100 }),
            ("42", Preset.Default with { DangerShift = 5 }),
            ("42", Preset.Default with { DangerShift = -5 }),
        };

        [Test]
        public void Presets_and_options_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("galaxies").BeginArray();
            foreach (var (seed, options) in Cases)
            {
                var g = Galaxy.Generate(seed, options);
                w.BeginObject().Name("options").String(options.ToCode()).Name("galaxy");
                GalaxyJson.Write(w, g);
                w.Name("systems").BeginArray();
                SystemJson.Write(w, g.System(0));
                w.EndArray().EndObject();
            }

            w.EndArray();
            w.Name("systems").BeginArray();
            foreach (var shift in new[] { -3, 2 })
            {
                SystemJson.Write(w, StarSystem.Generate("my-seed", Preset.Default with { DangerShift = shift }));
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/presets.json", w.ToString());
        }
    }
}
