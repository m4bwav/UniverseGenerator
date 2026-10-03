using System;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The public star-name entry point for name tools (plan Stage 6, the website's star name tool).</summary>
    public class StarNameTests
    {
        private static readonly StarClass?[] s_classes = { null, StarClass.G, StarClass.Supergiant, StarClass.NeutronStar, StarClass.BlackHole, StarClass.WhiteDwarf };

        [Test]
        public void Star_names_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("lists").BeginArray();
            foreach (var (seed, starClass, options) in new (string, StarClass?, GeneratorOptions)[]
            {
                ("my-seed", null, Preset.Default),
                ("42", null, Preset.Plausible),
                ("names", StarClass.G, Preset.Default),
                ("names", StarClass.Supergiant, Preset.Default),
                ("names", StarClass.NeutronStar, Preset.Default),
                ("hé/ü", StarClass.WhiteDwarf, Preset.Default),
            })
            {
                w.BeginObject().Name("seed").String(seed).Name("class").String(starClass?.ToString()).Name("names").BeginArray();
                foreach (var n in StarName.Generate(seed, 25, starClass, options))
                {
                    w.BeginArray().String(n.Name).String(n.Class.ToString()).EndArray();
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/star-name.json", w.ToString());
        }

        [Test]
        public void Names_are_different_fit_their_class_and_repeat_for_the_same_seed()
        {
            foreach (var c in s_classes)
            {
                var names = StarName.Generate("tool", StarName.MaxCount, c);
                Assert.That(names.Select(n => n.Name).Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(StarName.MaxCount), $"{c}");
                Assert.That(names.All(n => c == null || n.Class == c), Is.True);
                Assert.That(names.Select(n => n.Name), Is.EqualTo(StarName.Generate("tool", StarName.MaxCount, c).Select(n => n.Name)));
                Assert.That(StarName.Generate("tool", c), Is.EqualTo(names[0]));
            }

            Assert.That(StarName.Generate("tool", 50, StarClass.NeutronStar).All(n => n.Name.StartsWith("PSR J", StringComparison.Ordinal)), Is.True);
            Assert.That(StarName.Generate("tool", 300).Select(n => n.Class).Distinct().Count(), Is.GreaterThan(8), "classes drawn with the star mix");
            Assert.That(StarName.Generate(42, 10).Select(n => n.Name), Is.EqualTo(StarName.Generate("42", 10).Select(n => n.Name)));
            Assert.That(StarName.Generate("a").Name, Is.Not.EqualTo(StarName.Generate("b").Name));
        }

        [Test]
        public void A_count_out_of_range_is_refused_with_a_reason()
        {
            var e = Assert.Throws<ArgumentException>(() => StarName.Generate("x", 0));
            Assert.That(e!.Message, Does.StartWith("Count must be 1 to 1000; you asked for 0."));
            e = Assert.Throws<ArgumentException>(() => StarName.Generate("x", 1001));
            Assert.That(e!.Message, Does.StartWith("Count must be 1 to 1000; you asked for 1001."));
        }
    }
}
