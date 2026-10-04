using System;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorTables (tables.json): editable weight tables, read from JSON by id, with the built-in ones changing nothing.</summary>
    public class TableTests
    {
        private static WeightTable Only(string id, params (string Name, int Weight)[] entries) =>
            new WeightTable { Id = id, Entries = entries.Select(e => new WeightEntry { Name = e.Name, Weight = e.Weight }).ToList() };

        private static GeneratorTables RedSky() => GeneratorTables.Default.With(
            "red-sky",
            Only("star-classes.young", ("M", 1)),
            Only("star-classes.mature", ("M", 3), ("K", 1)),
            Only("star-classes.old", ("M", 1)));

        private static GeneratorTables Gardens() => GeneratorTables.Default.With(
            "gardens",
            Only("planet-kinds.temperate", ("Garden", 1)),
            Only("moon-kinds.giant", ("Ocean", 1)));

        [OneTimeSetUp]
        public void Register()
        {
            GeneratorTables.Register(RedSky());
            GeneratorTables.Register(Gardens());
        }

        [Test]
        public void Tables_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("default").String(GeneratorTables.Default.ToJson());
            w.Name("cases").BeginArray();
            foreach (var id in new[] { "red-sky", "gardens" })
            {
                var galaxy = Galaxy.Generate("my-seed", Preset.Pocket with { Tables = id });
                w.BeginObject().Name("tables").String(id).Name("galaxy");
                GalaxyJson.Write(w, galaxy);
                w.Name("systems").BeginArray();
                foreach (var system in galaxy.Systems.Take(3))
                {
                    SystemJson.Write(w, system);
                }

                w.EndArray().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/tables.json", w.ToString());
        }

        [Test]
        public void The_built_in_tables_round_trip_and_change_nothing()
        {
            var json = GeneratorTables.Default.ToJson(indented: true);
            var copy = GeneratorTables.FromJson(json.Replace("\"id\": \"default\"", "\"id\": \"copy\""));
            Assert.That(copy.Id, Is.EqualTo("copy"));
            Assert.That(copy.Tables.Select(t => t.Id), Is.EqualTo(GeneratorTables.Ids));
            for (var t = 0; t < copy.Tables.Count; t++)
            {
                Assert.That(copy.Tables[t].Entries, Is.EqualTo(GeneratorTables.Default.Tables[t].Entries));
            }

            GeneratorTables.Register(copy);
            foreach (var seed in new[] { "my-seed", "42" })
            {
                var plain = Galaxy.Generate(seed);
                var same = Galaxy.Generate(seed, Preset.Default with { Tables = "copy" });
                Assert.That(same.ToJson(children: true), Is.EqualTo(plain.ToJson(children: true)));
                Assert.That(Planet.Generate(seed, Preset.Default with { Tables = "default" }).ToJson(), Is.EqualTo(Planet.Generate(seed).ToJson()));
            }
        }

        [Test]
        public void Edited_tables_steer_what_is_drawn()
        {
            var red = Galaxy.Generate("my-seed", Preset.Default with { Tables = "red-sky" });
            Assert.That(red.Map.Select(m => m.StarClass), Is.All.EqualTo(StarClass.M).Or.EqualTo(StarClass.K));

            var gardens = Galaxy.Generate("my-seed", Preset.Default with { Tables = "gardens" });
            var temperate = gardens.Systems.SelectMany(s => s.Planets).Where(p => p.Zone == OrbitZone.Temperate && p.Mass >= 0.2 && p.Mass <= 2.8).ToList();
            Assert.That(temperate, Is.Not.Empty);
            Assert.That(temperate.Count(p => p.Kind == PlanetKind.Garden), Is.GreaterThan(temperate.Count / 2), "gardens except where the star cannot host one");
        }

        [Test]
        public void A_link_carries_the_tables_id_and_regenerates_the_same_objects()
        {
            var options = Preset.Pocket with { Tables = "red-sky" };
            Assert.That(options.ToCode(), Is.EqualTo("systems=20&planets=6&tables=red-sky"));
            var galaxy = Galaxy.Generate("linked", options);
            foreach (var system in galaxy.Systems)
            {
                var link = Universe.Link(system.Address, options);
                Assert.That(((StarSystem)Universe.At(link)).ToJson(), Is.EqualTo(system.ToJson()), link);
            }

            Assert.That(() => Universe.At("v1-x/galaxy?tables=never-registered"), Throws.ArgumentException.With.Message.Contains("No tables are registered as \"never-registered\""));
        }

        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"star-classes.ancient\",\"entries\":[]}]}", "\"star-classes.ancient\" is not a table")]
        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"moon-kinds.giant\",\"entries\":[{\"name\":\"Lava\",\"weight\":1}]}]}", "\"Lava\" is not a choice of moon-kinds.giant")]
        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"moon-kinds.giant\",\"entries\":[{\"name\":\"Ice\",\"weight\":0}]}]}", "moon-kinds.giant needs at least one weight above 0")]
        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"moon-kinds.giant\",\"entries\":[{\"name\":\"Ice\",\"weight\":-2}]}]}", "the weight of Ice must be 0 to 1,000,000")]
        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"moon-kinds.giant\",\"entries\":[{\"name\":\"Ice\",\"weight\":1},{\"name\":\"Ice\",\"weight\":2}]}]}", "lists \"Ice\" twice")]
        [TestCase("{\"id\":\"My Mod\"}", "A tables id is 1 to 40 lowercase letters")]
        public void Bad_tables_are_refused_with_a_reason(string json, string message)
        {
            Assert.That(() => GeneratorTables.FromJson(json), Throws.ArgumentException.With.Message.Contains(message));
        }

        [TestCase("", "at character 0: expected {")]
        [TestCase("{\"id\":\"x\",}", "at character 10: expected \"")]
        [TestCase("{\"id\":\"x\"} extra", "unexpected text after the end")]
        [TestCase("{\"name\":\"x\"}", "\"name\" is not a key of a table set")]
        [TestCase("{\"tables\":[]}", "the table set has no \"id\"")]
        [TestCase("{\"id\":\"x\",\"tables\":[{\"id\":\"moon-kinds.giant\",\"entries\":[{\"name\":\"Ice\",\"weight\":1.5}]}]}", "expected a whole number")]
        [TestCase("{\"schema\":\"universe-generator-tables/9\",\"id\":\"x\"}", "this package reads \"universe-generator-tables/1\"")]
        public void Bad_json_is_refused_with_its_position(string json, string message)
        {
            Assert.That(() => GeneratorTables.FromJson(json), Throws.TypeOf<FormatException>().With.Message.Contains(message));
        }

        [Test]
        public void Json_escapes_and_white_space_are_read()
        {
            var tables = GeneratorTables.FromJson(" {\n\t\"id\" : \"esc\", \"tables\" : [ { \"id\" : \"moon-kinds.giant\", \"entries\" : [ { \"name\" : \"\\u0049ce\", \"weight\" : 7 } ] } ] }\r\n");
            Assert.That(tables.Table("moon-kinds.giant").Entries.Single(e => e.Name == "Ice").Weight, Is.EqualTo(7));
            Assert.That(tables.Table("moon-kinds.giant").Entries.Where(e => e.Name != "Ice").Select(e => e.Weight), Is.All.Zero);
        }

        [Test]
        public void The_default_id_is_reserved_and_lookups_are_by_id()
        {
            Assert.That(() => GeneratorTables.Register(GeneratorTables.Default), Throws.ArgumentException.With.Message.Contains("belongs to the built-in tables"));
            Assert.That(GeneratorTables.Find("default"), Is.SameAs(GeneratorTables.Default));
            Assert.That(GeneratorTables.Find("red-sky")!.Table("star-classes.old").Entries[0].Name, Is.EqualTo("M"));
            Assert.That(GeneratorTables.Find("nothing-here"), Is.Null);
            Assert.That(() => (Preset.Default with { Tables = "Bad Id" }).Validate(), Throws.ArgumentException.With.Message.Contains("lowercase letters"));
        }
    }
}
