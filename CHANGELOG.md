# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

The seed promise: from 1.0.0, a seed and a generator version give the same output on every supported runtime for the whole major version. A change that would alter any seed's output adds a generator version and keeps the old one selectable.

## [Unreleased]

### Added

- The deterministic core: PCG32 streams, hierarchical seeds by SplitMix64 and FNV-1a over UTF-8, deterministic maths (exp, log, pow, sin, cos, atan2 and rounding from + - * /, square root and floor), versioned and URL-safe addresses, and a hand-written JSON writer.
- Golden test of the core's raw bits, identical on .NET 10 and .NET Framework 4.8.
