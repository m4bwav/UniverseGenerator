using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorOptions.Require (constraints.json): every galaxy holds what it guarantees, and nothing else moves.</summary>
    public class ConstraintTests
    {
        private const Guarantee Stars = Guarantee.BlueStar | Guarantee.Giant | Guarantee.WhiteDwarf | Guarantee.NeutronStar | Guarantee.BlackHole | Guarantee.SunLikeStar;
        private const Guarantee Every = Stars | Guarantee.GardenWorld | Guarantee.OceanWorld | Guarantee.PrecursorSite;

        private static readonly (string Seed, GeneratorOptions Options)[] Cases =
        {
            ("my-seed", Preset.Default with { Require = Every }),
            ("pocket", Preset.Pocket with { Require = Guarantee.GardenWorld | Guarantee.BlackHole }),
            ("tiny", Preset.Default with { Systems = 3, Require = Every }),
            ("bare", Preset.Default with { Systems = 10, MaxPlanetsPerSystem = 0, Require = Guarantee.GardenWorld }),
        };

        [Test]
        public void Guaranteed_galaxies_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("cases").BeginArray();
            foreach (var (seed, options) in Cases)
            {
                var plain = Galaxy.Generate(seed, options with { Require = Guarantee.None });
                var galaxy = Galaxy.Generate(seed, options);
                w.BeginObject().Name("seed").String(seed).Name("options").String(options.ToCode()).Name("warnings").BeginArray();
                foreach (var warning in galaxy.Warnings)
                {
                    w.BeginArray().String(warning.Code.ToString()).String(warning.Message).EndArray();
                }

                w.EndArray().Name("galaxy");
                GalaxyJson.Write(w, galaxy);
                w.Name("extras");
                ExtrasJson.Write(w, galaxy);
                w.Name("changedSystems").BeginArray();
                for (var i = 0; i < galaxy.Systems.Count; i++)
                {
                    if (SystemJson.Text(galaxy.Systems[i]) != SystemJson.Text(plain.Systems[i]))
                    {
                        SystemJson.Write(w, galaxy.Systems[i]);
                    }
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/constraints.json", w.ToString());
        }

        [Test]
        public void Every_guarantee_is_met_in_every_galaxy()
        {
            foreach (var seed in Enumerable.Range(0, 40).Select(i => "req" + i))
            {
                var galaxy = Galaxy.Generate(seed, Preset.Pocket with { Require = Every });
                Assert.That(galaxy.Warnings, Is.Empty, seed);
                var classes = galaxy.Map.Select(m => m.StarClass).ToList();
                Assert.That(classes, Has.Some.EqualTo(StarClass.B).Or.Some.EqualTo(StarClass.O), seed);
                Assert.That(classes, Has.Some.EqualTo(StarClass.Giant).Or.Some.EqualTo(StarClass.Supergiant), seed);
                Assert.That(classes, Has.Some.EqualTo(StarClass.WhiteDwarf).And.Some.EqualTo(StarClass.NeutronStar).And.Some.EqualTo(StarClass.BlackHole).And.Some.EqualTo(StarClass.G), seed);
                var kinds = galaxy.Systems.SelectMany(s => s.Planets).Select(p => p.Kind).ToList();
                Assert.That(kinds, Has.Some.EqualTo(PlanetKind.Garden).And.Some.EqualTo(PlanetKind.Ocean), seed);
                Assert.That(galaxy.PointsOfInterest.Select(p => p.Kind), Has.Some.EqualTo(PointOfInterestKind.PrecursorSite), seed);
                for (var i = 0; i < galaxy.Map.Count; i++)
                {
                    Assert.That(galaxy.Systems[i].Star.Class, Is.EqualTo(galaxy.Map[i].StarClass), $"{seed} system {i}: map and system agree");
                    Assert.That(galaxy.Systems[i].Name, Is.EqualTo(galaxy.Map[i].Name));
                }

                Assert.That(galaxy.Map.Select(m => m.Name).Distinct().Count(), Is.EqualTo(galaxy.Map.Count), "names stay unique");
            }
        }

        [Test]
        public void A_repair_changes_only_the_systems_it_picks()
        {
            foreach (var seed in Enumerable.Range(0, 20).Select(i => "only" + i))
            {
                var options = Preset.Pocket with { Require = Every };
                var plain = Galaxy.Generate(seed, Preset.Pocket);
                var galaxy = Galaxy.Generate(seed, options);
                var changed = Enumerable.Range(0, plain.Map.Count).Count(i => SystemJson.Text(plain.Systems[i]) != SystemJson.Text(galaxy.Systems[i]));
                Assert.That(changed, Is.LessThanOrEqualTo(8), "at most one system per star or planet guarantee");
                Assert.That(galaxy.Lanes, Is.EqualTo(plain.Lanes).Using<Lane>((a, b) => a == b));
                Assert.That(galaxy.Map.Select(m => (m.X, m.Y, m.Danger, m.Region)), Is.EqualTo(plain.Map.Select(m => (m.X, m.Y, m.Danger, m.Region))));
            }
        }

        [Test]
        public void A_guarantee_already_met_changes_nothing()
        {
            var plain = Galaxy.Generate("my-seed");
            var classes = plain.Map.Select(m => m.StarClass).ToList();
            Assume.That(classes, Has.Some.EqualTo(StarClass.G).And.Some.EqualTo(StarClass.WhiteDwarf));
            var same = Galaxy.Generate("my-seed", Preset.Default with { Require = Guarantee.SunLikeStar | Guarantee.WhiteDwarf });
            Assert.That(GalaxyJson.Text(same), Is.EqualTo(GalaxyJson.Text(plain)));
            Assert.That(ExtrasJson.Text(same), Is.EqualTo(ExtrasJson.Text(plain)));
            for (var i = 0; i < plain.Systems.Count; i++)
            {
                Assert.That(SystemJson.Text(same.Systems[i]), Is.EqualTo(SystemJson.Text(plain.Systems[i])));
            }
        }

        [Test]
        public void Repaired_systems_regenerate_from_their_addresses_and_links()
        {
            var options = Preset.Default with { Systems = 30, Require = Every };
            foreach (var seed in new[] { "link0", "link1", "link2" })
            {
                var galaxy = Galaxy.Generate(seed, options);
                foreach (var system in galaxy.Systems)
                {
                    Assert.That(SystemJson.Text((StarSystem)Universe.At(system.Address, options)), Is.EqualTo(SystemJson.Text(system)), system.Address);
                    Assert.That(SystemJson.Text((StarSystem)Universe.At(Universe.Link(system.Address, options))), Is.EqualTo(SystemJson.Text(system)));
                }
            }

            var cluster = GalaxyCluster.Generate("link-cluster", Preset.Default with { Require = Guarantee.GardenWorld | Guarantee.BlackHole });
            foreach (var member in cluster.Galaxies)
            {
                Assert.That(member.Map.Select(m => m.StarClass), Has.Some.EqualTo(StarClass.BlackHole), member.Address);
            }
        }

        [Test]
        public void Unmet_guarantees_are_warnings_not_errors()
        {
            var bare = Preset.Default with { MaxPlanetsPerSystem = 0, Require = Guarantee.OceanWorld };
            Assert.That(bare.Check().Select(x => x.Code), Is.EqualTo(new[] { WarningCode.GuaranteeUnmet }));
            Assert.That(Galaxy.Generate("bare", bare).Warnings.Select(x => x.Code), Has.Some.EqualTo(WarningCode.GuaranteeUnmet));

            var one = Galaxy.Generate("one", Preset.Default with { Systems = 1, Require = Stars });
            Assert.That(one.Warnings.Count(x => x.Code == WarningCode.GuaranteeUnmet), Is.GreaterThanOrEqualTo(4), "one system holds one star");
        }

        [Test]
        public void Require_has_a_code_and_bad_flags_are_refused()
        {
            var options = Preset.Default with { Require = Guarantee.BlackHole | Guarantee.GardenWorld };
            Assert.That(options.ToCode(), Is.EqualTo("require=garden,blackhole"));
            Assert.That(GeneratorOptions.FromCode("require=blackhole,garden"), Is.EqualTo(options));
            Assert.That(GeneratorOptions.FromCode("require=none"), Is.EqualTo(Preset.Default));
            Assert.That(() => GeneratorOptions.FromCode("require=dragons"), Throws.ArgumentException.With.Message.Contains("require takes none or a comma-separated list"));
            Assert.That(() => GeneratorOptions.FromCode("require=garden,garden"), Throws.ArgumentException.With.Message.Contains("twice"));
            Assert.That(() => (Preset.Default with { Require = (Guarantee)4096 }).Validate(), Throws.ArgumentException.With.Message.Contains("Require must combine"));
        }
    }
}
