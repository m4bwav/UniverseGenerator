using System;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The hierarchy promise (plan D17, test strategy): every object regenerated alone from its address equals the same
    /// object reached by generating its level and walking down.
    /// </summary>
    public class HierarchyTests
    {
        private static readonly (string Seed, GeneratorOptions Options)[] s_galaxies =
        {
            ("h0", Preset.Default),
            ("h1", Preset.Default),
            ("h2", Preset.SpaceOpera),
            ("h3", Preset.Plausible with { Systems = 25 }),
            ("h4", Preset.Default with { Systems = 200, Shape = GalaxyShape.Spiral }),
            ("h5", Preset.Default with { Systems = 1 }),
            ("hé/ü 5", Preset.Default),
        };

        [Test]
        public void Every_object_in_a_galaxy_regenerates_alone_from_its_address()
        {
            var objects = 0;
            foreach (var (seed, options) in s_galaxies)
            {
                var galaxy = Galaxy.Generate(seed, options);
                var again = (Galaxy)Universe.At(galaxy.Address, options);
                Assert.That(GalaxyJson.Text(again), Is.EqualTo(GalaxyJson.Text(galaxy)), galaxy.Address);
                foreach (var system in galaxy.Systems)
                {
                    objects += CheckSystem(system, options);
                }
            }

            TestContext.Out.WriteLine($"{objects} objects regenerated from their addresses");
        }

        [Test]
        public void Every_object_in_a_lone_system_regenerates_alone_from_its_address()
        {
            foreach (var seed in SystemJson.Seeds(300))
            {
                CheckSystem(StarSystem.Generate(seed), Preset.Default);
            }

            CheckSystem(StarSystem.Generate("p", Preset.Plausible), Preset.Plausible);
        }

        [Test]
        public void Every_lone_planet_and_its_moons_regenerate_alone_from_their_addresses()
        {
            foreach (var options in new[] { Preset.Default, Preset.Plausible, Preset.Default with { Weirdness = 100 } })
            {
                foreach (var seed in SystemJson.Seeds(300))
                {
                    var planet = Planet.Generate(seed, options);
                    Assert.That(PlanetJson.LoneText((Planet)Universe.At(planet.Address, options)), Is.EqualTo(PlanetJson.LoneText(planet)), planet.Address);
                    foreach (var moon in planet.Moons)
                    {
                        Assert.That(Universe.At(moon.Address, options), Is.EqualTo(moon), moon.Address);
                    }
                }
            }
        }

        [Test]
        public void A_system_alone_and_the_same_seed_in_a_galaxy_are_different_places()
        {
            Assert.That(StarSystem.Generate("my-seed").Address, Is.EqualTo("v1-my-seed/system"));
            Assert.That(Galaxy.Generate("my-seed").System(0).Address, Is.EqualTo("v1-my-seed/galaxy/system/0"));
            Assert.That(SystemJson.Text(StarSystem.Generate("my-seed")), Is.Not.EqualTo(SystemJson.Text(Galaxy.Generate("my-seed").System(0))));
        }

        [Test]
        public void Bad_addresses_are_refused_with_a_reason()
        {
            void Refused(string address, string start, GeneratorOptions? options = null)
            {
                var e = Assert.Throws<ArgumentException>(() => Universe.At(address, options), address);
                Assert.That(e!.Message, Does.StartWith(start), address);
            }

            var planets = StarSystem.Generate("my-seed").Planets.Count;
            Refused("", "The address is empty.");
            Refused("my-seed/galaxy", "An address starts with v");
            Refused("v2-my-seed/galaxy", "This package generates version 1; the address asks for version 2.");
            Refused("v1-my-seed/nebula", "\"nebula\" is not a level this package generates");
            Refused("v1-my-seed/galaxy/system/60", "v1-my-seed/galaxy has 60 systems (numbered 0 to 59); the address asks for system 60.");
            Refused("v1-my-seed/galaxy/planet/0", "Below v1-my-seed/galaxy comes a system; the address has \"planet\".");
            Refused("v1-my-seed/system/moon/0", "Below v1-my-seed/system comes a planet or station; the address has \"moon\".");
            Refused($"v1-my-seed/system/planet/{planets}", $"v1-my-seed/system has {planets} planets");
            Refused("v1-my-seed/system/station/9", "v1-my-seed/system has ");
            Refused("v1-my-seed/system/planet/0/moon/40", "v1-my-seed/system/planet/0 has ");
            var station = SystemJson.Seeds(50).Select(x => StarSystem.Generate(x)).First(x => x.Stations.Count > 0).Stations[0];
            Refused(station.Address + "/moon/0", "Nothing in this package lies below a station");
            Refused("v1-my-seed/planet/station/0", "Below v1-my-seed/planet comes a moon; the address has \"station\".");
            var mooned = SystemJson.Seeds(50).Select(x => Planet.Generate(x)).First(x => x.Moons.Count > 0);
            Refused(mooned.Moons[0].Address + "/moon/0", "Nothing in this package lies below a moon");
            Refused($"{mooned.Address}/moon/{mooned.Moons.Count}", $"{mooned.Address} has {mooned.Moons.Count} moons");
            Refused("v1-my-seed/galaxy", "Systems must be 1 to 2000", Preset.Default with { Systems = 0 });
            Refused("v1-" + new string('x', 201) + "/system", "A seed must be at most 200 characters");
        }

        [Test]
        public void Options_are_part_of_the_request()
        {
            var options = Preset.Default with { Systems = 120 };
            var galaxy = Galaxy.Generate("big", options);
            var address = galaxy.Map[100].Address;
            Assert.That(SystemJson.Text((StarSystem)Universe.At(address, options)), Is.EqualTo(SystemJson.Text(galaxy.System(100))));
            var e = Assert.Throws<ArgumentException>(() => Universe.At(address));
            Assert.That(e!.Message, Does.Contain("Were the same options passed as when it was generated?"));
        }

        private static int CheckSystem(StarSystem system, GeneratorOptions options)
        {
            var count = 1;
            var alone = (StarSystem)Universe.At(system.Address, options);
            Assert.That(SystemJson.Text(alone), Is.EqualTo(SystemJson.Text(system)), system.Address);
            foreach (var planet in system.Planets)
            {
                var p = (Planet)Universe.At(planet.Address, options);
                Assert.That(SystemJson.Text(p), Is.EqualTo(SystemJson.Text(planet)), planet.Address);
                Assert.That(PlanetJson.Text(p), Is.EqualTo(PlanetJson.Text(planet)), planet.Address);
                foreach (var moon in planet.Moons)
                {
                    Assert.That(Universe.At(moon.Address, options), Is.EqualTo(moon), moon.Address);
                }

                count += 1 + planet.Moons.Count;
            }

            foreach (var station in system.Stations)
            {
                Assert.That(Universe.At(station.Address, options), Is.EqualTo(station), station.Address);
            }

            return count + system.Stations.Count;
        }

        [Test]
        public void Planet_addresses_follow_the_plan_example()
        {
            var planet = Galaxy.Generate("my-seed").Systems.First(s => s.Planets.Count > 2).Planets[2];
            Assert.That(planet.Address, Does.Match(@"^v1-my-seed/galaxy/system/\d+/planet/2$"));
            Assert.That(((Planet)Universe.At(planet.Address)).Name, Is.EqualTo(planet.Name));
        }
    }
}
