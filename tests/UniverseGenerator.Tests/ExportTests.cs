using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>ToJson: complete (every init property of every record), valid JSON, stable (export.json), and lazy lists left out by default.</summary>
    public class ExportTests
    {
        private const string Generated = "Packages/com.m4bwav.universe-generator/Runtime/Core/JsonExport.cs";

        [Test]
        public void Every_init_property_of_every_record_is_exported()
        {
            // A field added to a record without running scripts/gen-json-export.py fails here.
            var source = File.ReadAllText(Repo.PathTo(Generated)).Replace("\r\n", "\n");
            var records = typeof(Galaxy).Assembly.GetExportedTypes()
                .Where(t => t.IsSealed && t.GetMethod("<Clone>$") != null && t != typeof(GeneratorOptions) && t != typeof(StarName))
                .ToList();
            Assert.That(records, Has.Count.GreaterThan(30));
            foreach (var type in records)
            {
                var m = Regex.Match(source, $@"private static void Fields\(JsonWriter w, {type.Name} v, bool children\)\n        {{\n(.*?)\n        }}\n", RegexOptions.Singleline);
                Assert.That(m.Success, Is.True, $"{type.Name} has no writer: run python scripts/gen-json-export.py");
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.SetMethod != null))
                {
                    var key = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                    Assert.That(m.Groups[1].Value, Does.Contain($"w.Name(\"{key}\");"), $"{type.Name}.{property.Name} is not exported: run python scripts/gen-json-export.py");
                }
            }
        }

        [Test]
        public void Exports_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("exports").BeginArray();
            foreach (var json in Samples())
            {
                w.String(json);
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/export.json", w.ToString());
        }

        [Test]
        public void An_export_starts_with_its_schema_and_type()
        {
            Assert.That(Planet.Generate("my-seed").ToJson(), Does.StartWith("{\"schema\":\"universe-generator/1\",\"type\":\"planet\",\"address\":\"v1-my-seed/planet\","));
            Assert.That(Galaxy.Generate("my-seed").ToJson(), Does.StartWith("{\"schema\":\"universe-generator/1\",\"type\":\"galaxy\","));
            Assert.That(StarSystem.Generate("my-seed").ToJson(indented: true), Does.StartWith("{\n  \"schema\": \"universe-generator/1\",\n  \"type\": \"starSystem\",\n"));
        }

        [Test]
        public void Lazy_lists_are_null_unless_children_are_asked_for()
        {
            var galaxy = Galaxy.Generate("my-seed", Preset.Pocket);
            Assert.That(galaxy.ToJson(), Does.Contain("\"systems\":null"));
            var full = galaxy.ToJson(children: true);
            Assert.That(full, Does.Not.Contain("\"systems\":null"));
            foreach (var system in galaxy.Systems)
            {
                Assert.That(full, Does.Contain(system.ToJson().Substring(system.ToJson().IndexOf("\"address\"", StringComparison.Ordinal)).TrimEnd('}')));
            }

            Assert.That(GalaxyCluster.Generate("my-seed").ToJson(), Does.Contain("\"galaxies\":null"));
        }

        [Test]
        public void The_same_object_reached_two_ways_exports_the_same_text()
        {
            var galaxy = Galaxy.Generate("my-seed");
            var planet = galaxy.System(31).Planets[1];
            Assert.That(((Planet)Universe.At(planet.Address)).ToJson(), Is.EqualTo(planet.ToJson()));
            var moon = galaxy.System(0).Planets[0].Moons[0];
            Assert.That(((Moon)Universe.At(moon.Address)).ToJson(), Is.EqualTo(moon.ToJson()));
            Assert.That(((Galaxy)Universe.At(galaxy.Address)).ToJson(), Is.EqualTo(galaxy.ToJson()));
        }

        [Test]
        public void Large_worlds_export_without_error()
        {
            foreach (var seed in new[] { "my-seed", "42", "Andromeda 7", "x", "y" })
            {
                Assert.That(Universe.Generate(seed).ToJson(), Does.StartWith("{"));
                Assert.That(GalaxyCluster.Generate(seed).ToJson(children: true), Does.StartWith("{"));
                Assert.That(Galaxy.Generate(seed, Preset.Epic).ToJson(children: true), Does.StartWith("{"));
            }
        }

#if NET
        [Test]
        public void Every_export_is_valid_json()
        {
            foreach (var json in Samples().Concat(new[] { Universe.Generate("42").ToJson(), GalaxyCluster.Generate("42").ToJson(children: true) }))
            {
                using var document = System.Text.Json.JsonDocument.Parse(json);
                Assert.That(document.RootElement.GetProperty("schema").GetString(), Is.EqualTo("universe-generator/1"));
            }
        }
#endif

        /// <summary>One export of each type with ToJson, compact (export.json holds them as strings, so one line each).</summary>
        private static string[] Samples()
        {
            var system = StarSystem.Generate("my-seed");
            var galaxy = Galaxy.Generate("my-seed", Preset.Pocket);
            var withMoons = galaxy.Systems.SelectMany(s => s.Planets).First(p => p.Moons.Count > 0);
            var withBelt = galaxy.Systems.First(s => s.Belts.Count > 0 && s.Stations.Count > 0);
            var universe = Universe.Generate("my-seed");
            return new[]
            {
                system.ToJson(),
                Planet.Generate("my-seed").ToJson(),
                withMoons.Moons[0].ToJson(),
                withBelt.Belts[0].ToJson(),
                withBelt.Stations[0].ToJson(),
                galaxy.ToJson(),
                galaxy.ToJson(children: true),
                GalaxyCluster.Generate("my-seed").ToJson(),
                universe.Voids[0].ToJson(children: true),
                universe.ToJson(),
            };
        }
    }
}
