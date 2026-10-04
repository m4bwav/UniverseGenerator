# UniverseGenerator

A seeded universe, galaxy cluster, galaxy, star system, planet and moon generator for games and fiction, for .NET and Unity. One seed gives the same galaxy on every platform and runtime: names, star types, planets, moons, lanes, regions, factions, danger and story tags, as plain C# records with no dependencies.

**Status: 1.0.0-beta.1 in review, not released yet.**

## Install

- NuGet: `dotnet add package UniverseGenerator` (netstandard2.0 and net10.0; trimmable and AOT-compatible).
- Unity 6: add `com.m4bwav.universe-generator` from OpenUPM, or by git URL `https://github.com/m4bwav/UniverseGenerator.git?path=Packages/com.m4bwav.universe-generator`. No engine references, so it also runs on servers and in tools.

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

The presets are `Default`, `SpaceOpera` (more outliers), `Plausible` (real star shares, few outliers), `Pocket` (20 systems for a short game), `Roguelike` (30 systems, more chokepoints, more danger), `Cozy` (40 well-connected, safer systems) and `Epic` (300 systems). The options are `Systems` (1 to 2000), `Shape` (Auto, Spiral, Barred, Elliptical, Ring, Irregular), `Arms` (2 to 4 for spirals and bars), `ExtraLanes` (the chance of a lane beyond the connected network, 0 to 100), `DangerShift` (-5 to 5 on every system's danger), `StarMix`, `Weirdness` (the share of systems that break the rules on purpose), `MaxPlanetsPerSystem`, `ClusterKind` and `Epoch`. Systems are generated when you first open them, so a 2,000-system map costs little until you look. Invalid options are refused before anything is generated, with a message that says what to change; generation itself never throws.

```csharp
var run = Galaxy.Generate("my-seed", Preset.Roguelike);
var cozy = Galaxy.Generate("my-seed", Preset.Cozy);
Console.WriteLine($"{run.Map.Count(m => m.Danger >= 8)} of {run.Map.Count} systems at danger 8 or more");    // 15 of 30 systems at danger 8 or more
Console.WriteLine($"{cozy.Map.Count(m => m.Danger >= 8)} of {cozy.Map.Count} systems at danger 8 or more");  // 6 of 40 systems at danger 8 or more
```

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
| Galaxy | five shapes, spaced systems, hyperlanes (always connected), chokepoints and bridges, named regions with age and theme, danger 1 to 10, factions, points of interest, hazards, monuments and beacons |
| Star system | real spectral classes with consistent mass, luminosity, radius, temperature and colour; giants, remnants and companions; habitable zone and frost line; catalogue names (IAU proper names, Bayer, HD, HIP, GJ, Kepler, TOI); landmarks; story tags; a one-line descriptor |
| Planet | orbits from exoplanet statistics, 13 kinds, gravity, density, day and night temperature, air by escape velocity, water and ice, rotation, tilt and tidal lock, climate bands, biomes, life from none to sentient, traits, resources, hazards, Earth similarity, habitability per species |
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

## The seed promise

From 1.0, a seed and a generator version (the `v1` in every address) give the same output on Windows, Linux and macOS (.NET 10 and .NET Framework 4.8) and in Unity (Mono and IL2CPP) for the whole major version, so a server and its clients can share a world by its seed alone. The golden files in `tests/Golden/v1/` are the proof, and the rules that make it true are in [kb/rules/determinism.md](kb/rules/determinism.md). A change that would alter any seed's output adds a generator version and keeps the old one.

## Samples

- [samples/ConsoleSample](samples/ConsoleSample): every example on this page, run by the tests.
- Unity: Package Manager, Universe Generator, Samples, "Galaxy printer": a MonoBehaviour that writes a seed's galaxy to the console.

## Licence

MIT. See [LICENSE](LICENSE).
