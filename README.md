# UniverseGenerator

A seeded universe, galaxy, star system, planet and moon generator for games and fiction, for .NET and Unity. One seed gives the same galaxy on every platform and runtime: names, star types, planets, moons, lanes, regions, danger and story tags, as plain C# records with no dependencies.

**Status: in development, not released.** The 1.0 plan is in [ai-docs/plans](ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md). This is the example the 1.0 release is being built to run:

```csharp
using UniverseGeneration;

var galaxy = Galaxy.Generate("my-seed");
foreach (var system in galaxy.Systems)
    Console.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
```

## The seed promise

From 1.0, a seed and a generator version give the same output on Windows, Linux and macOS (.NET 10 and .NET Framework 4.8) and in Unity (Mono and IL2CPP) for the whole major version. The golden files in `tests/Golden/v1/` are the proof, and the rules that make it true are in [kb/rules/determinism.md](kb/rules/determinism.md).

## Packages

- NuGet: `UniverseGenerator` (netstandard2.0 and net10.0).
- Unity (OpenUPM): `com.m4bwav.universe-generator`, built from the same source folder, `Packages/com.m4bwav.universe-generator/Runtime/`.

## Licence

MIT. See [LICENSE](LICENSE).
