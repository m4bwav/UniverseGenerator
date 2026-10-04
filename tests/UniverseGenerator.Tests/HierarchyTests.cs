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
            ("h6", Preset.Pocket with { Require = Guarantee.GardenWorld | Guarantee.OceanWorld | Guarantee.BlackHole | Guarantee.PrecursorSite | Guarantee.BlueStar }),
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
                Assert.That(ExtrasJson.Text(again), Is.EqualTo(ExtrasJson.Text(galaxy)), galaxy.Address);
                foreach (var system in galaxy.Systems)
                {
                    objects += CheckSystem(system, options);
                }
            }

            TestContext.Out.WriteLine($"{objects} objects regenerated from their addresses");
        }

        [Test]
        public void Every_object_in_a_cluster_regenerates_alone_from_its_address()
        {
            var objects = 0;
            foreach (var (seed, options) in new[]
            {
                ("hc0", Preset.Default),
                ("hc1", Preset.Default with { ClusterKind = ClusterKind.Cluster, Systems = 20 }),
                ("hc/é 2", Preset.Plausible with { ClusterKind = ClusterKind.Group }),
            })
            {
                var cluster = GalaxyCluster.Generate(seed, options);
                Assert.That(ClusterJson.Text((GalaxyCluster)Universe.At(cluster.Address, options)), Is.EqualTo(ClusterJson.Text(cluster)), cluster.Address);
                foreach (var galaxy in cluster.Galaxies)
                {
                    var again = (Galaxy)Universe.At(galaxy.Address, options);
                    Assert.That(ClusterJson.GalaxyText(again), Is.EqualTo(ClusterJson.GalaxyText(galaxy)), galaxy.Address);
                    Assert.That(ExtrasJson.Text(again), Is.EqualTo(ExtrasJson.Text(galaxy)), galaxy.Address);
                    objects++;
                }

                // Every object below the smallest galaxy, and one system of every galaxy.
                var smallest = cluster.Galaxies.OrderBy(g => g.Map.Count).First();
                foreach (var system in smallest.Systems)
                {
                    objects += CheckSystem(system, options);
                }

                foreach (var galaxy in cluster.Galaxies)
                {
                    objects += CheckSystem(galaxy.System(galaxy.Map.Count - 1), options);
                }
            }

            TestContext.Out.WriteLine($"{objects} objects of clusters regenerated from their addresses");
        }

        [Test]
        public void Every_object_in_a_universe_regenerates_alone_from_its_address()
        {
            var objects = 0;
            foreach (var (seed, options) in new[]
            {
                ("hu0", Preset.Default),
                ("hu1", Preset.Plausible with { Epoch = Epoch.Old, Systems = 20 }),
            })
            {
                var universe = Universe.Generate(seed, options);
                Assert.That(UniverseJson.Text((Universe)Universe.At(universe.Address, options)), Is.EqualTo(UniverseJson.Text(universe)), universe.Address);
                foreach (var cluster in universe.Clusters)
                {
                    Assert.That(ClusterJson.Text((GalaxyCluster)Universe.At(cluster.Address, options)), Is.EqualTo(ClusterJson.Text(cluster)), cluster.Address);
                    foreach (var galaxy in cluster.Galaxies)
                    {
                        var again = (Galaxy)Universe.At(galaxy.Address, options);
                        Assert.That(UniverseJson.GalaxyText(again), Is.EqualTo(UniverseJson.GalaxyText(galaxy)), galaxy.Address);
                        Assert.That(ExtrasJson.Text(again), Is.EqualTo(ExtrasJson.Text(galaxy)), galaxy.Address);
                        objects++;
                    }

                    var smallest = cluster.Galaxies.OrderBy(g => g.Map.Count).First();
                    objects += CheckSystem(smallest.System(smallest.Map.Count - 1), options);
                }

                // Every object of the merging pair's frontier system, and of every void.
                var pair = universe.Cluster(universe.Merger.Cluster).Galaxy(0);
                objects += CheckSystem(pair.System(pair.Gates.Single(g => g.Tier == LinkTier.Tidal).System), options);
                foreach (var hole in universe.Voids)
                {
                    var again = (CosmicVoid)Universe.At(hole.Address, options);
                    Assert.That((again.Name, again.X, again.Y, again.Radius, again.Systems.Count), Is.EqualTo((hole.Name, hole.X, hole.Y, hole.Radius, hole.Systems.Count)));
                    foreach (var system in hole.Systems)
                    {
                        objects += CheckSystem(system, options);
                    }
                }
            }

            TestContext.Out.WriteLine($"{objects} objects of universes regenerated from their addresses");
            var e = Assert.Throws<ArgumentException>(() => Universe.At("v1-x/universe/galaxy/0"));
            Assert.That(e!.Message, Does.StartWith("Below v1-x/universe comes a cluster or void; the address has \"galaxy\"."));
            e = Assert.Throws<ArgumentException>(() => Universe.At("v1-x/universe/void/9"));
            Assert.That(e!.Message, Does.Contain("the address asks for void 9."));
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
                        Assert.That(MoonBeltJson.Text((Moon)Universe.At(moon.Address, options)), Is.EqualTo(MoonBeltJson.Text(moon)), moon.Address);
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
            Refused("v1-my-seed/system/moon/0", "Below v1-my-seed/system comes a planet, station or belt; the address has \"moon\".");
            Refused("v1-my-seed/system/belt/9", "v1-my-seed/system has ");
            Refused($"v1-my-seed/system/planet/{planets}", $"v1-my-seed/system has {planets} planets");
            Refused("v1-my-seed/system/station/9", "v1-my-seed/system has ");
            Refused("v1-my-seed/system/planet/0/moon/40", "v1-my-seed/system/planet/0 has ");
            var station = SystemJson.Seeds(50).Select(x => StarSystem.Generate(x)).First(x => x.Stations.Count > 0).Stations[0];
            Refused(station.Address + "/moon/0", "Nothing in this package lies below a station");
            var belt = SystemJson.Seeds(50).Select(x => StarSystem.Generate(x)).First(x => x.Belts.Count > 0).Belts[0];
            Refused(belt.Address + "/moon/0", "Nothing in this package lies below a belt");
            Refused("v1-my-seed/planet/station/0", "Below v1-my-seed/planet comes a moon; the address has \"station\".");
            var mooned = SystemJson.Seeds(50).Select(x => Planet.Generate(x)).First(x => x.Moons.Count > 0);
            Refused(mooned.Moons[0].Address + "/moon/0", "Nothing in this package lies below a moon");
            Refused($"{mooned.Address}/moon/{mooned.Moons.Count}", $"{mooned.Address} has {mooned.Moons.Count} moons");
            Refused("v1-my-seed/galaxy", "Systems must be 1 to 2000", Preset.Default with { Systems = 0 });
            var galaxies = GalaxyCluster.Generate("my-seed").Map.Count;
            Refused($"v1-my-seed/cluster/galaxy/{galaxies}", $"v1-my-seed/cluster has {galaxies} galaxies (numbered 0 to {galaxies - 1})");
            Refused("v1-my-seed/cluster/system/0", "Below v1-my-seed/cluster comes a galaxy; the address has \"system\".");
            Refused("v1-my-seed/cluster/galaxy/0/planet/0", "Below v1-my-seed/cluster/galaxy/0 comes a system; the address has \"planet\".");
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
                    Assert.That(MoonBeltJson.Text((Moon)Universe.At(moon.Address, options)), Is.EqualTo(MoonBeltJson.Text(moon)), moon.Address);
                }

                count += 1 + planet.Moons.Count;
            }

            foreach (var station in system.Stations)
            {
                Assert.That(Universe.At(station.Address, options), Is.EqualTo(station), station.Address);
            }

            foreach (var belt in system.Belts)
            {
                Assert.That(MoonBeltJson.Text((Belt)Universe.At(belt.Address, options)), Is.EqualTo(MoonBeltJson.Text(belt)), belt.Address);
            }

            return count + system.Stations.Count + system.Belts.Count;
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
