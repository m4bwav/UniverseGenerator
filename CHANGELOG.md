# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

The seed promise: from 1.0.0, a seed and a generator version give the same output on every supported runtime for the whole major version. A change that would alter any seed's output adds a generator version and keeps the old one selectable.

## [Unreleased]

### Added

- `GeneratorOptions.ToCode()` and `GeneratorOptions.FromCode(code)`: the settings that differ from the defaults as short text (`systems=120&shape=barred`), and links: `Universe.Link(address, options)` writes the address, `?` and the code, and `Universe.At(link)` regenerates the object with those options, so a shared address carries its options.
- Presets `Pocket` (20 systems, at most 6 planets), `Roguelike` (30 systems, few extra lanes, danger +2, more outliers), `Cozy` (40 well-connected systems, danger -3, few outliers) and `Epic` (300 systems), and the options they use: `Arms` (2 to 4 arms for spirals and bars; null draws), `ExtraLanes` (0 to 100, the chance of a lane beyond the connected network; default 25) and `DangerShift` (-5 to 5, added to every system's danger). Each changes values after every draw, so the defaults give the same output as before; their galaxies are in the new golden file `presets.json`.
- `ToJson(indented, children)` on `Universe`, `GalaxyCluster`, `CosmicVoid`, `Galaxy`, `StarSystem`, `Planet`, `Moon`, `Belt` and `Station`: every field as JSON with `"schema": "universe-generator/1"` and the type first, the same text on every runtime, no reflection (the writer is generated from the records by `scripts/gen-json-export.py`). Lists generated when first read stay null unless `children: true`. Examples in the new golden file `export.json`.
- `Planet.Eccentricity`, `Planet.Inclination` and `Planet.PeriapsisAngle`, drawn on a new stream of each planet's seed (so nothing else moves): Rayleigh draws as Kepler's multi-planet systems show (about 0.05 and 1.5 degrees), larger for a planet alone, damped for hot planets, and capped so no orbit crosses a neighbour's. In the new golden file `orbital-elements.json`; `export.json` filters them out and stays as written.
- `GeneratorOptions.Names`: `NameStyle.Catalogue` (the default, real catalogue names) or `NameStyle.Invented` (two- or three-syllable names such as Olmex, Zertron or Bethor, at most ten letters, unique in a galaxy). Drawn from each system's own names stream, so only names change; planets and moons take the system's name. Option code `names=invented`; in the new golden file `names.json`.
- `Units.Convert` between `LengthUnit` (km, Earth, Jupiter and solar radii, au, light-years, parsecs), `MassUnit`, `TemperatureUnit` and `TimeUnit`, and `Units.Map` from galaxy, cluster or universe map units to any length. Conversions on plain doubles (IAU nominal values, no units library); the generated values keep the units their documentation names.
- `GeneratorOptions.Check()` and `Galaxy.Warnings`: soft problems as `GeneratorWarning` values (`WarningCode` `SystemsTrimmed`, `ArmsIgnored`, `ShapeTooSmall`, `NameNumbered`, and a message), reported and never thrown. In the new golden file `diagnostics.json`; `export.json` filters the new `warnings` key.
- `GalaxyShape.Colliding` (two discs, a tidal bridge and tails), `GalaxyShape.Starburst` (a dense core with knots) and `GalaxyShape.Clustered` (tight knots over a faint oval), with type codes Pec, Burst and Clumpy. Asked for by name only: `Auto` never draws them (Stop 2, S1), and they use only parameters already drawn, so no seed moves. In the new golden file `shapes.json`.
- `GeneratorOptions.Require`: `Guarantee` flags (`GardenWorld`, `OceanWorld`, `PrecursorSite`, `BlueStar`, `Giant`, `WhiteDwarf`, `NeutronStar`, `BlackHole`, `SunLikeStar`) that every galaxy must hold at least one of. A galaxy that lacks one gets it in a single system picked on the galaxy's own `constraints` stream and passed down in that system's context, so `Universe.At` regenerates the same system; the default (none) draws nothing, so no seed moves. Unmet guarantees are `WarningCode.GuaranteeUnmet` warnings. Option code `require=garden,blackhole`; in the new golden file `constraints.json`.
- `GeneratorHooks` (`OnPlanet`, `OnSystem`, `OnGalaxy`, `OnCluster`, `OnUniverse`): post-processors run wherever an object is made, in lazy lists too, passed to new `Generate(seed, options, hooks)` overloads on every level and to `Universe.At(address, options, hooks)`, which runs them again on everything it regenerates. They live outside `GeneratorOptions` because options are a value with a text code and a delegate has neither. Hooks never change what the generator draws (guarantees are judged before them).
- `Custom` on `Universe`, `GalaxyCluster`, `CosmicVoid`, `Galaxy`, `StarSystem`, `Planet`, `Moon`, `Belt` and `Station`: a read-only dictionary of your own text fields, empty from the generator, written by `ToJson` with names in ordinal order; `CustomFields.With(name, value)` returns a copy with one field set. In the new golden file `hooks.json`; `export.json` filters the `custom` key and stays as written.

## [1.0.0-beta.1] - 2026-10-03

The first release: a new package, extracted from the game SpaceDeckBuilder2 and rebuilt on stable seeds.

### Added

- The deterministic core: PCG32 streams, hierarchical seeds by SplitMix64 and FNV-1a over UTF-8, deterministic maths (exp, log, pow, sin, cos, atan2 and rounding from + - * /, square root and floor), versioned and URL-safe addresses, and a hand-written JSON writer for the golden tests.
- `GeneratorOptions` and `Preset` (Default, SpaceOpera, Plausible): systems, shape, star mix, weirdness, planets per system, cluster kind and epoch, validated before anything is generated with messages that say what to change.
- The star system level, `StarSystem.Generate`: real spectral classes with consistent physics, giants, remnants and companions; planets in two orbit chains from exoplanet statistics (the radius valley, the hot Neptune desert); moons inside the Hill sphere, rings, belts in the gaps, stations that orbit real bodies; catalogue names from the embedded IAU, Bright Star and Hipparcos tables; landmarks with an outlier share; story tags that agree with the data; one-line descriptors.
- The planet level, `Planet.Generate`: gravity, escape velocity, density, insolation, albedo, day and night temperatures, air by the Jeans rule, water and ice, rotation, tilt and tidal lock, climate bands, biomes, life, traits and anomalies, resources, hazards, Earth similarity and habitability per species.
- Moon and belt detail: each moon's physics, orbit, tidal heat, temperature, air, hidden oceans, life, climate, resources, hazards and habitability; belts with addresses, names, composition, mass, largest body, temperature and resources.
- The galaxy level, `Galaxy.Generate`: five shapes, spaced systems, connected hyperlanes, chokepoints and bridges, named regions with age and theme, danger 1 to 10, systems generated on request (`galaxy.System(i)`).
- Galaxy extras, each on its own stream: factions grown over lanes, points of interest with placement rules and a precursor trail, hazard areas, a monument per region and two to four beacons.
- The galaxy cluster level, `GalaxyCluster.Generate`: a group or a rich cluster with Hubble type codes, ages, richness, active cores, gates, a wormhole ring and tethers; each galaxy generated on request.
- The universe level, `Universe.Generate`: 4 to 7 groups and clusters on filaments, named voids with lone systems, landmark galaxies, a merging pair with a lawless frontier and a story hook, and `Distances` for light-years and travel days.
- `Universe.At(address)`: any object, from a universe to a moon, station or belt, regenerated alone from its address.
- `StarName.Generate`: 1 to 1,000 catalogue-style star names on their own seeds, optionally for one star class.
- Golden files for every level in `tests/Golden/v1/`, identical on .NET 10 and .NET Framework 4.8; property, distribution and hierarchy tests.
- A console sample holding every README example, and the Unity sample "Galaxy printer".

### Size

Measured by the size gate (`scripts/size-gate.py`), every metric green: nupkg 373.2 KB (green under 1 MB), largest DLL 499.0 KB (green under 750 KB), Unity package 456.9 KB unpacked (green under 2 MB) and 114.5 KB compressed, 35 Runtime source files and 9,736 lines.
