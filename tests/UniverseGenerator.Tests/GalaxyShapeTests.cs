using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The shapes added after 1.0.0-beta.1 (Colliding, Starburst, Clustered): shapes.json, and Auto never draws them.</summary>
    public class GalaxyShapeTests
    {
        private static readonly GalaxyShape[] NewShapes = { GalaxyShape.Colliding, GalaxyShape.Starburst, GalaxyShape.Clustered };

        [Test]
        public void New_shapes_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("galaxies").BeginArray();
            foreach (var shape in NewShapes)
            {
                foreach (var (seed, systems) in new[] { ("my-seed", 60), ("42", 150) })
                {
                    var g = Galaxy.Generate(seed, Preset.Default with { Shape = shape, Systems = systems });
                    w.BeginObject().Name("type").String(g.Type).Name("descriptor").String(g.Descriptor).Name("galaxy");
                    GalaxyJson.Write(w, g);
                    w.EndObject();
                }
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/shapes.json", w.ToString());
        }

        [Test]
        public void New_shapes_hold_every_system_with_connected_lanes_and_no_arms()
        {
            foreach (var shape in NewShapes)
            {
                foreach (var seed in new[] { "a", "b", "c", "d" })
                {
                    var g = Galaxy.Generate(seed, Preset.Default with { Shape = shape, Systems = 200 });
                    Assert.That(g.Shape, Is.EqualTo(shape));
                    Assert.That(g.Map, Has.Count.EqualTo(200), $"{shape} {seed}");
                    Assert.That(g.Arms, Is.Zero);
                    Assert.That(g.Map.Max(m => m.Hops), Is.LessThan(200), "every system reachable from the core");
                }
            }
        }

        [Test]
        public void Auto_never_draws_the_new_shapes()
        {
            for (var i = 0; i < 300; i++)
            {
                var g = Galaxy.Generate("auto" + i, Preset.Default with { Systems = i % 2 == 0 ? 60 : 120 });
                Assert.That(NewShapes, Does.Not.Contain(g.Shape), g.Address);
            }
        }

        [Test]
        public void The_new_shapes_have_their_own_type_codes_and_words()
        {
            Assert.That(Galaxy.Generate("x", Preset.Default with { Shape = GalaxyShape.Colliding }).Descriptor, Does.Contain("colliding pair of galaxies"));
            Assert.That(Galaxy.Generate("x", Preset.Default with { Shape = GalaxyShape.Starburst }).Type, Is.EqualTo("Burst"));
            Assert.That(Galaxy.Generate("x", Preset.Default with { Shape = GalaxyShape.Clustered }).Descriptor, Does.Contain("clumpy galaxy"));
            Assert.That((Preset.Default with { Shape = GalaxyShape.Starburst, Arms = 3 }).Check().Select(x => x.Code), Is.EqualTo(new[] { WarningCode.ArmsIgnored }));
        }
    }
}
