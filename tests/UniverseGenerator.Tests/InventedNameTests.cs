using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorOptions.Names = Invented (names.json): invented system names on the systems' own names streams.</summary>
    public class InventedNameTests
    {
        private static readonly GeneratorOptions Invented = Preset.Default with { Names = NameStyle.Invented };

        [Test]
        public void Invented_names_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("galaxies").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "Andromeda 7" })
            {
                w.BeginArray();
                foreach (var m in Galaxy.Generate(seed, Invented).Map)
                {
                    w.String(m.Name);
                }

                w.EndArray();
            }

            w.EndArray().Name("systems").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2" })
            {
                var s = StarSystem.Generate(seed, Invented);
                w.BeginObject().Name("name").String(s.Name).Name("planets").BeginArray();
                foreach (var p in s.Planets)
                {
                    w.String(p.Name);
                }

                w.EndArray().EndObject();
            }

            w.EndArray().Name("planets").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2" })
            {
                w.String(Planet.Generate(seed, Invented).Name);
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/names.json", w.ToString());
        }

        [Test]
        public void Invented_names_change_only_names()
        {
            foreach (var seed in new[] { "my-seed", "42", "x" })
            {
                var plain = Galaxy.Generate(seed);
                var invented = Galaxy.Generate(seed, Invented);
                Assert.That(invented.Map.Select(m => (m.X, m.Y, m.StarClass, m.Danger)), Is.EqualTo(plain.Map.Select(m => (m.X, m.Y, m.StarClass, m.Danger))), seed);
                var a = plain.System(5);
                var b = invented.System(5);
                Assert.That(b.Planets.Select(p => (p.Kind, p.Orbit, p.Mass)), Is.EqualTo(a.Planets.Select(p => (p.Kind, p.Orbit, p.Mass))));
                Assert.That(b.Name, Is.Not.EqualTo(a.Name));
            }
        }

        [Test]
        public void Invented_names_read_as_words_and_are_unique_in_a_galaxy()
        {
            var galaxy = Galaxy.Generate("names", Invented with { Systems = 2000 });
            var names = galaxy.Map.Select(m => m.Name).ToList();
            Assert.That(names.Distinct().Count(), Is.EqualTo(names.Count));
            foreach (var name in names)
            {
                Assert.That(name, Does.Match("^[A-Z][a-z]{2,15}$"), name);
                Assert.That(Regex.IsMatch(name, "(.)\\1\\1"), Is.False, name);
                Assert.That(Regex.IsMatch(name, "[aeiouy]{4}"), Is.False, name);
            }
        }

        [Test]
        public void Planets_and_moons_take_the_invented_system_name()
        {
            var system = StarSystem.Generate("my-seed", Invented);
            Assert.That(system.Planets, Has.All.Matches<Planet>(p => p.Name.StartsWith(system.Name + " ", System.StringComparison.Ordinal)));
            var again = (StarSystem)Universe.At(Universe.Link(system.Address, Invented));
            Assert.That(again.Name, Is.EqualTo(system.Name));
        }
    }
}
