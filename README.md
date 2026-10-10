# UniverseGenerator

![A generated barred spiral galaxy of 300 star systems joined by lanes, from the seed my-seed](https://raw.githubusercontent.com/m4bwav/UniverseGenerator/6e87ad4586ce6e32986bb3619d2cc25e3e8656db/docs/images/openupm-cover.png)

A seeded universe, galaxy cluster, galaxy, star system, planet and moon generator for games and fiction, for .NET and Unity. One seed gives the same galaxy on every platform and runtime: names, star types, planets, moons, lanes, regions, factions, danger and story tags, as plain C# records with no dependencies.

**Status: 1.0.0, the first stable release, on nuget.org. The OpenUPM listing comes next; until then, install in Unity by git URL.**

## Install

- NuGet: `dotnet add package UniverseGenerator` (netstandard2.0 and net10.0; trimmable and AOT-compatible).
- Unity 6: Package Manager, +, "Install package from git URL", then `https://github.com/m4bwav/UniverseGenerator.git?path=Packages/com.m4bwav.universe-generator#v1.0.0` (the `#v1.0.0` pins the release; leave it off to follow `master`). OpenUPM (`com.m4bwav.universe-generator`) comes next. No engine references, so it also runs on servers and in tools.

## First example

```csharp
using UniverseGeneration;

var galaxy = Galaxy.Generate("my-seed");
foreach (var system in galaxy.Systems)
    Console.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
```

```text
HD 147927: K star, 6 planets, danger 9
HD 165595: M star, 1 planets, danger 6
HD 28101: K star, 6 planets, danger 6
...
```

Any text (up to 200 characters) or a 64-bit number is a seed. Sixty systems is the default, a map that reads on one screen.

## Presets, options and addresses

```csharp
var options = Preset.SpaceOpera with { Systems = 120, Shape = GalaxyShape.Barred };
var galaxy = Galaxy.Generate("my-seed", options);
var planet = galaxy.System(31).Planets[1];   // generated on request; same result as full generation
Console.WriteLine(planet.Descriptor);         // a ringed ice giant far from a red dwarf
Console.WriteLine(planet.Address);            // v1-my-seed/galaxy/system/31/planet/1
var same = (Planet)Universe.At(planet.Address, options);   // any object regenerates alone from its address
Console.WriteLine(same.Summary == planet.Summary);         // True
```

The presets are `Default`, `SpaceOpera` (more outliers), `Plausible` (real star shares, few outliers), `Pocket` (20 systems for a short game), `Roguelike` (30 systems, more chokepoints, more danger), `Cozy` (40 well-connected, safer systems) and `Epic` (300 systems). The options are `Systems` (1 to 2000), `Shape` (Auto, Spiral, Barred, Elliptical, Ring, Irregular, and by name only Colliding, Starburst and Clustered), `Arms` (2 to 4 for spirals and bars), `ExtraLanes` (the chance of a lane beyond the connected network, 0 to 100), `DangerShift` (-5 to 5 on every system's danger), `Names` (`Catalogue` star names, or `Invented` names such as Olmex or Zertron), `StarMix`, `Weirdness` (the share of systems that break the rules on purpose), `MaxPlanetsPerSystem`, `ClusterKind`, `Epoch` and `Require` (below). Systems are generated when you first open them, so a 2,000-system map costs little until you look. Invalid options are refused before anything is generated, with a message that says what to change; generation itself never throws. Settings that are valid but change nothing or read badly (arms on a ring galaxy, a spiral of 40 systems) are listed by `options.Check()`, and `galaxy.Warnings` says what generation had to change; both are `GeneratorWarning` values with a `WarningCode` and a message.

```csharp
var run = Galaxy.Generate("my-seed", Preset.Roguelike);
var cozy = Galaxy.Generate("my-seed", Preset.Cozy);
Console.WriteLine($"{run.Map.Count(m => m.Danger >= 8)} of {run.Map.Count} systems at danger 8 or more");    // 15 of 30 systems at danger 8 or more
Console.WriteLine($"{cozy.Map.Count(m => m.Danger >= 8)} of {cozy.Map.Count} systems at danger 8 or more");  // 6 of 40 systems at danger 8 or more
```

```csharp
var options = Preset.Pocket with { Require = Guarantee.GardenWorld | Guarantee.BlackHole };
var galaxy = Galaxy.Generate("my-seed", options);
var garden = galaxy.Systems.SelectMany(s => s.Planets).First(p => p.Kind == PlanetKind.Garden);
Console.WriteLine(garden.Address);                           // v1-my-seed/galaxy/system/13/planet/1
Console.WriteLine(Universe.Link(galaxy.Address, options));   // v1-my-seed/galaxy?systems=20&planets=6&require=garden,blackhole
```

`Require` guarantees that every galaxy holds at least one of each thing it names: a garden or ocean world, a precursor site, or a blue star, giant, white dwarf, neutron star, black hole or Sun-like star. A galaxy that already has one is unchanged; one that lacks it gets it in a single system its own seed picks, and every other system stays as it was. A guarantee that cannot be met (a garden world with no planets allowed) is a warning, never an error.

```csharp
try
{
    Galaxy.Generate("my-seed", Preset.Default with { Systems = 0 });
}
catch (ArgumentException e)
{
    Console.WriteLine(e.Message);   // Systems must be 1 to 2000; you asked for 0. (Parameter 'Systems')
}
```

## What it makes

| Level | In 1.0 |
|---|---|
| Universe | 4 to 7 groups and clusters joined by filaments, named voids with lone systems, a home spiral, a dying giant, a ring galaxy, a distant quasar in every galaxy's sky, a merging pair with a lawless frontier and a story hook, an epoch |
| Galaxy cluster | a small group (two big spirals, satellites, dwarfs) or a rich cluster (a giant elliptical at the centre); Hubble type codes, ages, richness, active cores; gates, a wormhole ring and tethers |
| Galaxy | eight shapes (a colliding pair, a starburst and a clustered galaxy by name only), spaced systems, hyperlanes (always connected), chokepoints and bridges, named regions with age and theme, danger 1 to 10, factions, points of interest, hazards, monuments and beacons |
| Star system | real spectral classes with consistent mass, luminosity, radius, temperature and colour; giants, remnants and companions; habitable zone and frost line; catalogue names (IAU proper names, Bayer, HD, HIP, GJ, Kepler, TOI); landmarks; story tags; a one-line descriptor |
| Planet | orbits from exoplanet statistics with eccentricity, inclination and periapsis (never crossing a neighbour's), 13 kinds, gravity, density, day and night temperature, air by escape velocity, water and ice, rotation, tilt and tidal lock, climate bands, biomes, life from none to sentient, traits, resources, hazards, Earth similarity, habitability per species |
| Moons, rings, belts | moons inside the Hill sphere with their own physics, tidal heat, hidden oceans and life; rings; belts in the gaps with composition and resources; stations that orbit real bodies |

Later, in 1.x and without changing any seed: travel routes, generated paragraphs, populations and trade, exports (Markdown, image, CSV, tabletop formats), map rendering and more. The full list, checked against 176 other generators, is the [feature matrix](kb/features/feature-matrix.md).

## Clusters and universes

```csharp
var cluster = GalaxyCluster.Generate("my-seed");
Console.WriteLine($"{cluster.Name}: {cluster.Map.Count} galaxies, {cluster.Links.Count} links");
var fenris = cluster.Galaxy(0);   // each galaxy is generated when you open it

var universe = Universe.Generate("my-seed");
Console.WriteLine($"{universe.Name}: {universe.Nodes.Count} groups and clusters, {universe.Voids.Count} voids");
Console.WriteLine(universe.Merger.Hook);   // A truce holds the frontier, and someone is working to break it.
```

A level above never changes how the levels below are built: a galaxy in a cluster is made the same way as a galaxy alone, and only its context (age, richness, an active core, its gates) comes from above. `Distances` turns map units into light-years and travel days.

## Galaxy extras

```csharp
var galaxy = Galaxy.Generate("my-seed");
foreach (var faction in galaxy.Factions)
    Console.WriteLine(faction.Description);   // an empire ruled from HD 31487, holding 17 systems
foreach (var site in galaxy.PointsOfInterest)
    Console.WriteLine($"{site.Name}: {site.Text}");
Console.WriteLine($"{galaxy.Hazards.Count} hazards, {galaxy.Monuments.Count} monuments, {galaxy.Beacons.Count} beacons");
```

Factions grow over the lanes from their capitals; each map entry names its holder and whether it is contested. Points of interest follow placement rules (wrecks near chokepoints, ruins only in old regions) and most galaxies hide a precursor trail whose sites point to each other. Hazards are areas (nebulae, dark clouds, ion storms, gravity rifts, an active core's radiation); every region has a monument, and two to four beacons help navigators. The extras read the finished map and never change it.

## Star names

```csharp
foreach (var star in StarName.Generate("my-seed", 5))
    Console.WriteLine($"{star.Name} ({star.Class})");   // HD 45528 (K), HD 19029 (F), Kepler-1971 (F), ...
var red = StarName.Generate("my-seed", StarClass.M);    // a name that fits a red dwarf
```

Up to 1,000 different catalogue-style names per call, for a name tool or a story, from their own seeds.

## Universe.At and addresses

Every object has an `Address` such as `v1-my-seed/galaxy/system/31/planet/1`, and `Universe.At` regenerates it alone, with the same result as generating its level and walking down. Save the address, not the object. An address does not carry options: pass the options the object was made with, or share a link, which does. Every level can also be generated on its own:

```csharp
var system = StarSystem.Generate("my-seed");   // v1-my-seed/system
var planet = Planet.Generate("my-seed");       // v1-my-seed/planet
var moon = (Moon)Universe.At("v1-my-seed/galaxy/system/0/planet/0/moon/0");   // HD 147927 e I
```

A link is an address followed by `?` and the options' code, such as `systems=120&shape=barred`: only the settings that differ from the defaults, as lowercase `name=value` pairs that stay readable for the whole major version. `GeneratorOptions.ToCode()` and `GeneratorOptions.FromCode(code)` convert options on their own, for a save file or a URL.

```csharp
var options = Preset.Default with { Systems = 120 };
var system = Galaxy.Generate("my-seed", options).System(31);
var link = Universe.Link(system.Address, options);   // the address, ? and options.ToCode()
Console.WriteLine(link);                              // v1-my-seed/galaxy/system/31?systems=120
var again = (StarSystem)Universe.At(link);            // the link carries the options
Console.WriteLine(again.Name == system.Name);         // True
```

## Export to JSON

`ToJson()` on a universe, cluster, void, galaxy, system, planet, moon, belt or station writes every field as JSON, with `"schema": "universe-generator/1"` and the object's type first, enum values as names and numbers as stored. The text is the same on every runtime, written by hand with no reflection. Lists that are generated when first read (a galaxy's systems, a cluster's galaxies, a void's systems) are null unless you pass `children: true`, which generates everything below; each map entry carries the address of what it leaves out.

```csharp
var planet = Planet.Generate("my-seed");
var json = planet.ToJson(indented: true);   // every field, after "schema" and "type"
Console.WriteLine(json.Split('\n')[3]);       //   "address": "v1-my-seed/planet",
var galaxy = Galaxy.Generate("my-seed", Preset.Pocket);
var map = galaxy.ToJson();                  // the map and extras; "systems" is null
var all = galaxy.ToJson(children: true);    // every system in full as well
Console.WriteLine(all.Length > map.Length); // True
```

## Hooks and custom fields

`GeneratorHooks` runs your own post-processing on every planet, system, galaxy, cluster and universe as it is made: rename a system, mark a colony, attach a faction. Every generated object has `Custom`, a dictionary of your own text fields (empty from the generator), which `ToJson` writes; `Custom.With(name, value)` returns a copy with one field set. Pass the same hooks to `Generate` and to `Universe.At`, which runs them again on everything it regenerates. Hooks are code, so they stay out of `GeneratorOptions` and out of links. A hook must give the same result for the same object, and it never changes what the generator draws.

```csharp
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
```

## Editable tables

Some of the generator's choices come from weight tables you can edit: which star classes a region holds (by its age, and for `StarMix.Plausible`), which rocky planet kinds each orbit zone holds, and the moons of giant planets. `GeneratorTables.Ids` lists them. Change the built-in tables with `With`, or load a set from JSON with `GeneratorTables.FromJson` (`ToJson` writes the format, every choice listed). Register the set under an id and name it in `GeneratorOptions.Tables`; a link carries the id, so every program that registers the same tables regenerates the same objects. The built-in tables (id `default`) give exactly the output of no tables. In Unity, the `TablesAsset` ScriptableObject (Assets > Create > Universe Generator > Tables) holds a set you edit in the Inspector; call its `Register()` before generating.

```csharp
var tables = GeneratorTables.Default.With("red-sky",
    new WeightTable { Id = "star-classes.mature", Entries = new[] { new WeightEntry { Name = "M", Weight = 1 } } });
GeneratorTables.Register(tables);   // once, before generating; every program that reads the links does the same
var galaxy = Galaxy.Generate("my-seed", Preset.Default with { Tables = "red-sky" });
Console.WriteLine(galaxy.Map.Count(m => m.StarClass == StarClass.M));   // 26 red dwarfs of 60
var json = tables.ToJson(indented: true);   // edit it, ship it, read it back with GeneratorTables.FromJson
Console.WriteLine(GeneratorTables.FromJson(json).Table("star-classes.mature").Entries[0].Weight);   // 1
```

## Units

Values are plain doubles in the unit each field's documentation names: orbits in au, planet radii and masses in Earths, stars in Suns, moons and belts in kilometres, temperatures in kelvin, rotation in hours and orbital periods in days; maps in their own units (`Distances`). `Units.Convert` turns any of them into another unit (`LengthUnit`, `MassUnit`, `TemperatureUnit`, `TimeUnit`), and `Units.Map` turns map units into a length. No units library, so nothing to install and nothing added to a WebGL build.

```csharp
var world = Planet.Generate("my-seed");
var km = Units.Convert(world.Orbit, LengthUnit.AstronomicalUnits, LengthUnit.Kilometres);
var celsius = Units.Convert(world.Temperature, TemperatureUnit.Kelvin, TemperatureUnit.Celsius);
Console.WriteLine($"{(int)Math.Round(km / 1e6)} million km out, {(int)Math.Round(celsius)} C");   // 229 million km out, -37 C
var across = Units.Map(MapLevel.Galaxy, 2000, LengthUnit.Parsecs);   // a galaxy map edge to edge: about 30,660
```

## Unity

The Unity package adds a second assembly, `UniverseGenerator.Unity`, that only Unity compiles (the NuGet package holds the engine-free core alone). `UnityVectors` turns map positions into `Vector2` or `Vector3` (`galaxy.Map[i].ToVector3(scale)`, on the XZ ground plane or the XY screen plane), gives a lane's two ends for a line renderer (`lane.Ends(galaxy)`) and places a planet on its orbit (`planet.OrbitPosition(days)`, from its eccentricity, inclination and periapsis angle). These floats are for drawing; the generated doubles are what the seed promise covers. `TablesAsset` holds editable tables (see "Editable tables").

## The seed promise

From 1.0, a seed and a generator version (the `v1` in every address) give the same output on Windows, Linux and macOS (.NET 10 and .NET Framework 4.8) and in Unity (Mono and IL2CPP) for the whole major version, so a server and its clients can share a world by its seed alone. The golden files in `tests/Golden/v1/` are the proof, and the rules that make it true are in [kb/rules/determinism.md](kb/rules/determinism.md). A change that would alter any seed's output adds a generator version and keeps the old one.

## Samples

- [samples/ConsoleSample](samples/ConsoleSample): every example on this page, run by the tests.
- Unity: Package Manager, Universe Generator, Samples, "Galaxy printer": a MonoBehaviour that writes a seed's galaxy to the console.

## Package page

- NuGet: [UniverseGenerator](https://www.nuget.org/packages/UniverseGenerator)

## Licence

MIT. See [LICENSE](LICENSE).
