using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorHooks and Custom fields (hooks.json): hooks run wherever an object is made, Universe.At included.</summary>
    public class HookTests
    {
        // A game's post-processing: colonies on garden worlds, a faction field on every system, a campaign on each galaxy.
        private static readonly GeneratorHooks Game = new GeneratorHooks
        {
            OnPlanet = p => p.Kind == PlanetKind.Garden ? p with { Name = p.Name + " (colony)", Custom = p.Custom.With("colony", "yes") } : p,
            OnSystem = s => s with { Custom = s.Custom.With("faction", s.Danger >= 7 ? "pirates" : "league").With("planets", s.Planets.Count.ToString(CultureInfo.InvariantCulture)) },
            OnGalaxy = g => g with { Custom = g.Custom.With("campaign", "alpha") },
            OnCluster = c => c with { Custom = c.Custom.With("sector", c.Name) },
            OnUniverse = u => u with { Custom = u.Custom.With("save", "1") },
        };

        [Test]
        public void Hooked_objects_match_the_golden_file()
        {
            var galaxy = Galaxy.Generate("my-seed", Preset.Pocket, Game);
            var exports = new List<string> { galaxy.ToJson() };
            exports.AddRange(galaxy.Systems.Take(4).Select(s => s.ToJson()));
            exports.Add(Planet.Generate("my-seed", null, Game).ToJson());
            exports.Add(StarSystem.Generate("my-seed", null, Game).ToJson());
            exports.Add(Universe.Generate("my-seed", null, Game).Custom["save"]);
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current).Name("exports").BeginArray();
            foreach (var json in exports)
            {
                w.String(json);
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/hooks.json", w.ToString());
        }

        [Test]
        public void Every_object_reached_by_At_has_run_its_hooks()
        {
            var options = Preset.Pocket with { Require = Guarantee.GardenWorld };
            var galaxy = Galaxy.Generate("hooked", options, Game);
            Assert.That(galaxy.Custom["campaign"], Is.EqualTo("alpha"));
            Assert.That(((Galaxy)Universe.At(galaxy.Address, options, Game)).Custom["campaign"], Is.EqualTo("alpha"));
            var gardens = 0;
            foreach (var system in galaxy.Systems)
            {
                Assert.That(system.Custom["faction"], Is.Not.Empty);
                Assert.That(((StarSystem)Universe.At(system.Address, options, Game)).ToJson(), Is.EqualTo(system.ToJson()), system.Address);
                Assert.That(((StarSystem)Universe.At(Universe.Link(system.Address, options), null, Game)).ToJson(), Is.EqualTo(system.ToJson()));
                foreach (var planet in system.Planets)
                {
                    Assert.That(((Planet)Universe.At(planet.Address, options, Game)).ToJson(), Is.EqualTo(planet.ToJson()), planet.Address);
                    if (planet.Kind == PlanetKind.Garden)
                    {
                        gardens++;
                        Assert.That(planet.Name, Does.EndWith(" (colony)"));
                        Assert.That(planet.Custom["colony"], Is.EqualTo("yes"));
                    }
                }
            }

            Assert.That(gardens, Is.GreaterThan(0), "the garden guarantee met the hook");
        }

        [Test]
        public void Hooks_run_in_clusters_universes_voids_and_lone_objects()
        {
            var universe = Universe.Generate("hooked", null, Game);
            Assert.That(universe.Custom["save"], Is.EqualTo("1"));
            Assert.That(universe.Cluster(0).Custom["sector"], Is.EqualTo(universe.Cluster(0).Name));
            Assert.That(universe.Cluster(0).Galaxy(0).Custom["campaign"], Is.EqualTo("alpha"));
            Assert.That(universe.Voids[0].Systems[0].Custom, Does.ContainKey("faction"));
            var cluster = (GalaxyCluster)Universe.At(universe.Cluster(1).Address, null, Game);
            Assert.That(cluster.Custom["sector"], Is.EqualTo(cluster.Name));
            Assert.That(GalaxyCluster.Generate("alone", null, Game).Galaxy(0).System(0).Custom, Does.ContainKey("faction"));

            var lone = StarSystem.Generate("alone", null, Game);
            Assert.That(lone.Custom["planets"], Is.EqualTo(lone.Planets.Count.ToString(CultureInfo.InvariantCulture)));
            var moonAddress = Planet.Generate("moony").Moons.Select(m => m.Address).FirstOrDefault();
            if (moonAddress != null)
            {
                Assert.That(Universe.At(moonAddress, null, Game), Is.TypeOf<Moon>());
            }
        }

        [Test]
        public void Hooks_change_nothing_the_generator_draws()
        {
            var plain = Galaxy.Generate("my-seed");
            var hooked = Galaxy.Generate("my-seed", null, Game);
            Assert.That(GalaxyJson.Text(hooked), Is.EqualTo(GalaxyJson.Text(plain)));
            Assert.That(ExtrasJson.Text(hooked), Is.EqualTo(ExtrasJson.Text(plain)));
            var none = Galaxy.Generate("my-seed", null, new GeneratorHooks());
            Assert.That(none.ToJson(children: true), Is.EqualTo(plain.ToJson(children: true)));
            Assert.That(plain.Custom, Is.Empty);
            Assert.That(plain.System(0).Custom, Is.SameAs(CustomFields.Empty));
        }

        [Test]
        public void A_hook_that_returns_null_is_named()
        {
            var bad = new GeneratorHooks { OnSystem = _ => null! };
            Assert.That(() => StarSystem.Generate("x", null, bad), Throws.InvalidOperationException.With.Message.Contains("The OnSystem hook returned null"));
        }

        [Test]
        public void Custom_fields_copy_and_export_in_ordinal_order()
        {
            var fields = CustomFields.Empty.With("b", "2").With("a", "1").With("B", "3");
            Assert.That(CustomFields.Empty, Is.Empty, "With never changes the original");
            Assert.That(fields["a"], Is.EqualTo("1"));
            var planet = Planet.Generate("my-seed") with { Custom = fields };
            Assert.That(planet.ToJson(), Does.EndWith("\"custom\":{\"B\":\"3\",\"a\":\"1\",\"b\":\"2\"}}"));
            Assert.That(() => fields.With("c", null!), Throws.ArgumentNullException);
        }
    }
}
