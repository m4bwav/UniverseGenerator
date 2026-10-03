using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The moon and belt level's golden file: the new fields of the moons and belts of fixed lone planets, systems and a
    /// galaxy (their older fields are in system.json, galaxy.json and planet.json), and one moon of every kind and one
    /// belt of every kind in full.
    /// </summary>
    public class GoldenMoonBeltTests
    {
        [Test]
        public void Moons_and_belts_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("loneMoons").BeginArray();
            void Lone(string seed, GeneratorOptions options)
            {
                foreach (var m in Planet.Generate(seed, options).Moons)
                {
                    MoonBeltJson.Moon(w, m);
                }
            }

            foreach (var seed in new[] { "my-seed", "42", "-42", "", "Andromeda 7", "a/b c", "s1", "s2", "s3", "s4", "s5", "s6", "s7", "s8" })
            {
                Lone(seed, Preset.Default);
            }

            foreach (var seed in new[] { "p1", "p2", "p3" })
            {
                Lone(seed, Preset.Plausible);
            }

            foreach (var seed in new[] { "w1", "w2", "w3" })
            {
                Lone(seed, Preset.Default with { Weirdness = 100 });
            }

            w.EndArray();
            w.Name("inSystems").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2", "s3", "s4" })
            {
                Write(w, StarSystem.Generate(seed));
            }

            w.EndArray();
            w.Name("inGalaxy").BeginArray();
            var galaxy = Galaxy.Generate("my-seed");
            foreach (var index in new[] { 0, galaxy.Core })
            {
                Write(w, galaxy.System(index));
            }

            w.EndArray();

            // The first moon of each kind and belt of each kind in systems "k0", "k1" ..., in full.
            var moons = new SortedDictionary<MoonKind, Moon>();
            var belts = new SortedDictionary<BeltKind, Belt>();
            for (var i = 0; moons.Count < 6 || belts.Count < 2; i++)
            {
                var system = StarSystem.Generate("k" + i.ToString(CultureInfo.InvariantCulture));
                foreach (var p in system.Planets)
                {
                    foreach (var m in p.Moons)
                    {
                        if (!moons.ContainsKey(m.Kind))
                        {
                            moons[m.Kind] = m;
                        }
                    }
                }

                foreach (var b in system.Belts)
                {
                    if (!belts.ContainsKey(b.Kind))
                    {
                        belts[b.Kind] = b;
                    }
                }

                Assert.That(i, Is.LessThan(10000), "every moon and belt kind occurs");
            }

            w.Name("moonOfEachKind").BeginArray();
            foreach (var m in moons.Values)
            {
                w.BeginArray();
                SystemJson.Moon(w, m);
                MoonBeltJson.Moon(w, m);
                w.EndArray();
            }

            w.EndArray();
            w.Name("beltOfEachKind").BeginArray();
            foreach (var b in belts.Values)
            {
                w.BeginArray();
                SystemJson.Belt(w, b);
                MoonBeltJson.Belt(w, b);
                w.EndArray();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/moon-belt.json", w.ToString());
        }

        private static void Write(JsonWriter w, StarSystem s)
        {
            w.BeginObject().Name("address").String(s.Address).Name("moons").BeginArray();
            foreach (var p in s.Planets)
            {
                foreach (var m in p.Moons)
                {
                    MoonBeltJson.Moon(w, m);
                }
            }

            w.EndArray().Name("belts").BeginArray();
            foreach (var b in s.Belts)
            {
                MoonBeltJson.Belt(w, b);
            }

            w.EndArray().EndObject();
        }
    }
}
