using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorOptions.Check and Galaxy.Warnings (diagnostics.json): soft problems reported, never thrown.</summary>
    public class DiagnosticsTests
    {
        private static readonly (string Seed, GeneratorOptions Options)[] Cases =
        {
            ("my-seed", Preset.Default),
            ("small-spiral", Preset.Default with { Systems = 40, Shape = GalaxyShape.Spiral }),
            ("ring-arms", Preset.Default with { Shape = GalaxyShape.Ring, Arms = 3 }),
            ("auto-arms", Preset.Default with { Arms = 2 }),
            ("auto-big-arms", Preset.Default with { Systems = 120, Arms = 4 }),
            ("crowded", Preset.Default with { Systems = 2000, Shape = GalaxyShape.Ring }),
        };

        [Test]
        public void Warnings_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("cases").BeginArray();
            foreach (var (seed, options) in Cases)
            {
                w.BeginObject().Name("seed").String(seed).Name("options").String(options.ToCode());
                w.Name("check").BeginArray();
                foreach (var warning in options.Check())
                {
                    w.BeginArray().String(warning.Code.ToString()).String(warning.Message).EndArray();
                }

                var galaxy = Galaxy.Generate(seed, options);
                w.EndArray().Name("systems").Int(galaxy.Map.Count).Name("warnings").BeginArray();
                foreach (var warning in galaxy.Warnings)
                {
                    w.BeginArray().String(warning.Code.ToString()).String(warning.Message).EndArray();
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/diagnostics.json", w.ToString());
        }

        [Test]
        public void Default_galaxies_have_no_warnings()
        {
            foreach (var seed in new[] { "my-seed", "42", "Andromeda 7", "a", "b", "c" })
            {
                Assert.That(Galaxy.Generate(seed).Warnings, Is.Empty, seed);
            }

            Assert.That(Preset.Default.Check(), Is.Empty);
            Assert.That(Preset.Epic.Check(), Is.Empty);
        }

        [Test]
        public void Check_reports_settings_that_change_nothing_or_read_badly()
        {
            var codes = (Preset.Default with { Systems = 40, Shape = GalaxyShape.Barred }).Check().Select(x => x.Code);
            Assert.That(codes, Is.EqualTo(new[] { WarningCode.ShapeTooSmall }));
            codes = (Preset.Default with { Shape = GalaxyShape.Ring, Arms = 3 }).Check().Select(x => x.Code);
            Assert.That(codes, Is.EqualTo(new[] { WarningCode.ArmsIgnored }));
            codes = (Preset.Cozy with { Arms = 3 }).Check().Select(x => x.Code);
            Assert.That(codes, Is.EqualTo(new[] { WarningCode.ArmsIgnored }));
            Assert.That((Preset.Default with { Systems = 120, Shape = GalaxyShape.Spiral, Arms = 3 }).Check(), Is.Empty);
            Assert.That(() => (Preset.Default with { Systems = 0 }).Check(), Throws.ArgumentException);
        }

        [Test]
        public void A_galaxy_reports_a_trim_and_the_warnings_of_its_options()
        {
            foreach (var (seed, options) in Cases)
            {
                var galaxy = Galaxy.Generate(seed, options);
                var trimmed = galaxy.Warnings.Any(x => x.Code == WarningCode.SystemsTrimmed);
                Assert.That(trimmed, Is.EqualTo(galaxy.Map.Count < options.Systems), seed);
                foreach (var warning in options.Check())
                {
                    Assert.That(galaxy.Warnings, Does.Contain(warning), seed);
                }
            }
        }

        [Test]
        public void Auto_reports_arms_when_it_draws_a_shape_without_them()
        {
            for (var i = 0; i < 40; i++)
            {
                var galaxy = Galaxy.Generate("auto" + i, Preset.Default with { Systems = 120, Arms = 3 });
                var armless = galaxy.Shape != GalaxyShape.Spiral && galaxy.Shape != GalaxyShape.Barred;
                Assert.That(galaxy.Warnings.Any(x => x.Code == WarningCode.ArmsIgnored), Is.EqualTo(armless), galaxy.Address);
            }
        }
    }
}
