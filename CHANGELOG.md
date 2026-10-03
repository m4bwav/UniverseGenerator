# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

The seed promise: from 1.0.0, a seed and a generator version give the same output on every supported runtime for the whole major version. A change that would alter any seed's output adds a generator version and keeps the old one selectable.

## [Unreleased]

### Added

- The deterministic core: PCG32 streams, hierarchical seeds by SplitMix64 and FNV-1a over UTF-8, deterministic maths (exp, log, pow, sin, cos, atan2 and rounding from + - * /, square root and floor), versioned and URL-safe addresses, and a hand-written JSON writer.
- Golden test of the core's raw bits, identical on .NET 10 and .NET Framework 4.8.
- The star system level: `StarSystem.Generate(seed, options)` with stars (spectral types, companions), planets in two orbit chains with the radius valley and the hot Neptune desert, moons inside the Hill sphere, rings, belts in gaps, stations that orbit real bodies, catalogue names from the embedded IAU, Bright Star and Hipparcos tables, landmarks with an outlier share, story tags that agree with the data, and one-line descriptors.
- `GeneratorOptions` and `Preset` (Default, SpaceOpera, Plausible), validated with messages that say what to change.
