# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

The seed promise: from 1.0.0, a seed and a generator version give the same output on every supported runtime for the whole major version. A change that would alter any seed's output adds a generator version and keeps the old one selectable.

## [1.0.0-beta.1] - Unreleased

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

Measured by the size gate (`scripts/size-gate.py`), every metric green: nupkg 373.5 KB (green under 1 MB), largest DLL 499.0 KB (green under 750 KB), Unity package 456.9 KB unpacked (green under 2 MB) and 116.0 KB compressed, 35 Runtime source files and 9,736 lines.
