using System;
using System.Linq;
using UniverseGeneration;

namespace ConsoleSample
{
    /// <summary>
    /// Every C# example in the README, one region each, in README order. The test project compiles this file and runs
    /// each example on .NET 10 and .NET Framework 4.8, and checks that the README's code blocks match these regions.
    /// Change an example here first, then copy it into the README.
    /// </summary>
    public static class Examples
    {
        public static void FirstExample()
        {
            #region readme
            var galaxy = Galaxy.Generate("my-seed");
            foreach (var system in galaxy.Systems)
                Console.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
            #endregion
        }

        public static void PresetsAndAddresses()
        {
            #region readme
            var options = Preset.SpaceOpera with { Systems = 120, Shape = GalaxyShape.Barred };
            var galaxy = Galaxy.Generate("my-seed", options);
            var planet = galaxy.System(31).Planets[1];   // generated on request; same result as full generation
            Console.WriteLine(planet.Descriptor);         // a ringed ice giant far from a red dwarf
            Console.WriteLine(planet.Address);            // v1-my-seed/galaxy/system/31/planet/1
            var same = (Planet)Universe.At(planet.Address, options);   // any object regenerates alone from its address
            Console.WriteLine(same.Summary == planet.Summary);         // True
            #endregion
        }

        public static void Presets()
        {
            #region readme
            var run = Galaxy.Generate("my-seed", Preset.Roguelike);
            var cozy = Galaxy.Generate("my-seed", Preset.Cozy);
            Console.WriteLine($"{run.Map.Count(m => m.Danger >= 8)} of {run.Map.Count} systems at danger 8 or more");    // 15 of 30 systems at danger 8 or more
            Console.WriteLine($"{cozy.Map.Count(m => m.Danger >= 8)} of {cozy.Map.Count} systems at danger 8 or more");  // 6 of 40 systems at danger 8 or more
            #endregion
        }

        public static void Guarantees()
        {
            #region readme
            var options = Preset.Pocket with { Require = Guarantee.GardenWorld | Guarantee.BlackHole };
            var galaxy = Galaxy.Generate("my-seed", options);
            var garden = galaxy.Systems.SelectMany(s => s.Planets).First(p => p.Kind == PlanetKind.Garden);
            Console.WriteLine(garden.Address);                           // v1-my-seed/galaxy/system/13/planet/1
            Console.WriteLine(Universe.Link(galaxy.Address, options));   // v1-my-seed/galaxy?systems=20&planets=6&require=garden,blackhole
            #endregion
        }

        public static void Validation()
        {
            #region readme
            try
            {
                Galaxy.Generate("my-seed", Preset.Default with { Systems = 0 });
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);   // Systems must be 1 to 2000; you asked for 0. (Parameter 'Systems')
            }
            #endregion
        }

        public static void ClustersAndUniverses()
        {
            #region readme
            var cluster = GalaxyCluster.Generate("my-seed");
            Console.WriteLine($"{cluster.Name}: {cluster.Map.Count} galaxies, {cluster.Links.Count} links");
            var fenris = cluster.Galaxy(0);   // each galaxy is generated when you open it

            var universe = Universe.Generate("my-seed");
            Console.WriteLine($"{universe.Name}: {universe.Nodes.Count} groups and clusters, {universe.Voids.Count} voids");
            Console.WriteLine(universe.Merger.Hook);   // A truce holds the frontier, and someone is working to break it.
            #endregion
            Console.WriteLine(fenris.Name);
        }

        public static void GalaxyExtras()
        {
            #region readme
            var galaxy = Galaxy.Generate("my-seed");
            foreach (var faction in galaxy.Factions)
                Console.WriteLine(faction.Description);   // an empire ruled from HD 31487, holding 17 systems
            foreach (var site in galaxy.PointsOfInterest)
                Console.WriteLine($"{site.Name}: {site.Text}");
            Console.WriteLine($"{galaxy.Hazards.Count} hazards, {galaxy.Monuments.Count} monuments, {galaxy.Beacons.Count} beacons");
            #endregion
        }

        public static void StarNames()
        {
            #region readme
            foreach (var star in StarName.Generate("my-seed", 5))
                Console.WriteLine($"{star.Name} ({star.Class})");   // HD 45528 (K), HD 19029 (F), Kepler-1971 (F), ...
            var red = StarName.Generate("my-seed", StarClass.M);    // a name that fits a red dwarf
            #endregion
            Console.WriteLine(red.Name);
        }

        public static void LoneObjects()
        {
            #region readme
            var system = StarSystem.Generate("my-seed");   // v1-my-seed/system
            var planet = Planet.Generate("my-seed");       // v1-my-seed/planet
            var moon = (Moon)Universe.At("v1-my-seed/galaxy/system/0/planet/0/moon/0");   // HD 147927 e I
            #endregion
            Console.WriteLine($"{system.Descriptor}; {planet.Summary}; {moon.Name}");
        }

        public static void Links()
        {
            #region readme
            var options = Preset.Default with { Systems = 120 };
            var system = Galaxy.Generate("my-seed", options).System(31);
            var link = Universe.Link(system.Address, options);   // the address, ? and options.ToCode()
            Console.WriteLine(link);                              // v1-my-seed/galaxy/system/31?systems=120
            var again = (StarSystem)Universe.At(link);            // the link carries the options
            Console.WriteLine(again.Name == system.Name);         // True
            #endregion
        }

        public static void Export()
        {
            #region readme
            var planet = Planet.Generate("my-seed");
            var json = planet.ToJson(indented: true);   // every field, after "schema" and "type"
            Console.WriteLine(json.Split('\n')[3]);       //   "address": "v1-my-seed/planet",
            var galaxy = Galaxy.Generate("my-seed", Preset.Pocket);
            var map = galaxy.ToJson();                  // the map and extras; "systems" is null
            var all = galaxy.ToJson(children: true);    // every system in full as well
            Console.WriteLine(all.Length > map.Length); // True
            #endregion
        }

        public static void Hooks()
        {
            #region readme
            var hooks = new GeneratorHooks
            {
                OnPlanet = p => p.Kind == PlanetKind.Garden ? p with { Custom = p.Custom.With("colony", "yes") } : p,
                OnSystem = s => s with { Custom = s.Custom.With("faction", s.Danger >= 7 ? "pirates" : "league") },
            };
            var galaxy = Galaxy.Generate("my-seed", null, hooks);
            var system = galaxy.System(0);
            Console.WriteLine(system.Custom["faction"]);                         // pirates
            var again = (StarSystem)Universe.At(system.Address, null, hooks);   // At runs the same hooks
            Console.WriteLine(again.Custom["faction"]);                          // pirates
            #endregion
        }

        public static void UnitConversions()
        {
            #region readme
            var world = Planet.Generate("my-seed");
            var km = Units.Convert(world.Orbit, LengthUnit.AstronomicalUnits, LengthUnit.Kilometres);
            var celsius = Units.Convert(world.Temperature, TemperatureUnit.Kelvin, TemperatureUnit.Celsius);
            Console.WriteLine($"{(int)Math.Round(km / 1e6)} million km out, {(int)Math.Round(celsius)} C");   // 229 million km out, -37 C
            var across = Units.Map(MapLevel.Galaxy, 2000, LengthUnit.Parsecs);   // a galaxy map edge to edge: about 30,660
            #endregion
            Console.WriteLine((int)across);
        }

        /// <summary>Runs every example in README order.</summary>
        public static void All()
        {
            FirstExample();
            PresetsAndAddresses();
            Presets();
            Guarantees();
            Validation();
            ClustersAndUniverses();
            GalaxyExtras();
            StarNames();
            LoneObjects();
            Links();
            Export();
            Hooks();
            UnitConversions();
        }
    }
}
