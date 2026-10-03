---
title: "Galaxy generator Stage 0: survey of the game's generator and the legacy golden capture"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-generator, stage-0, golden-capture, unity, determinism, mono, system-random, spacedeckbuilder]
summary: "read before porting the SpaceDeckBuilder2 generator or relying on its recorded output: what the code really does, how the game uses it and saves it, the Unity and .NET capture (and why Mono's float maths differs), and the statistics that drive the Stage 1 decisions"
---

# Galaxy generator Stage 0: survey and legacy capture

## Summary

The game's generator was read in full and recorded in Unity and .NET. The game never regenerates a galaxy from a seed, so changing the generator cannot alter saves. Unity's Mono computes float maths in double precision, and seeded System.Random gives seeds s and -s the same output, both reasons for the package's own PRNG and integer decisions. Ten faults found here are fixed in the 1.0 plan.

Stage 0 of [the extraction plan](../plans/2026-10-02-galaxy-generator-extraction.md), done on 2026-10-02 against SpaceDeckBuilder2 `main` at 7bf13f2 (tag build-39).

## Where the work is

- The capture and the prototype were made in a worktree of the game's private repository, on a branch that is never merged. The recordings are copied into this repository's `tests/Golden/legacy/`.
  - f0c7438: the capture (Assets/Editor/GalaxyCapture, Tests/GalaxyCapture with the recordings in `legacy/`).
  - a3ede9b: the Stage 1 prototype (Tests/GalaxyPrototype, with its SVGs and statistics in `out/`).
- The repository pins Unity 6000.6.0f1 (ProjectSettings/ProjectVersion.txt on `main`). The open Editor on the main clone shows a local, uncommitted upgrade to 6000.6.4f1.

## What the code does (corrections to the plan's survey)

- **The game never regenerates a galaxy from a seed.** `GameManager.StartNewGameRoutine` calls `GalaxyGenerator.Generate()` with no seed, so the seed comes from whatever state UnityEngine.Random is in. `SightingWorldAdapter.Populate` then copies each system's designation, star type, centre and position (as an offset from home) into the save's sighting records. No galaxy seed is stored anywhere. The plan's line "regenerates the rest from seeds" is wrong. **Consequence for D3:** changing what a seed produces cannot change any existing save, so no save-format bump is needed for that. For "same seed as the game" (D10, D11) to mean anything, the game must start choosing, storing and showing its galaxy seed.
- **`StarSystemGenerator.Generate(entry)` is never called by the game**; only the home system is generated in full (`scenario.systemTemplate`, random seed). Other systems exist only as galaxy entries.
- **Data in practice:** one system template (`home_system`, 3 to 6 planets, star mix RedDwarf 40, OrangeDwarf 20, Binary 15, YellowDwarf 12, Multiple 6, WhiteDwarf 3, Neutron 2), four planet archetypes (barren 25, rocky 20, ice 15, gas 10) and four belt archetypes. **Every planet archetype has moons, atmosphere and life switched off,** so moons, atmospheres and life never occur. Orbit types are Circular or Elliptical only. `PlanetData.spinSpeed`, and `StarData.radius`, `surfaceTemperature`, `habitableZoneInner/Outer`, `frostLine` and `isStable`, are never set.
- Templates and archetypes are ScriptableObjects loaded with `Resources.LoadAll`; the load order (alphabetical) decides weighted picks.
- `CelestialNaming` depends on RandomNameGeneratorLibrary 2.2.0 (`StarNameGenerator`, in `Assets/Plugins`) for HD/HIP catalogue numbers and proper, Bayer and Flamsteed names. The star tables in that package are 61 KB raw, 17 KB gzipped (designations 40.6 KB, HIP gaps 15.3 KB, proper names 5.2 KB); the whole package is 954 KB, mostly surnames and places.
- `StationGenerator` sets `Id = Guid.NewGuid()`, so station identity is not reproducible from the seed.
- UnityEngine symbols used: `Vector2`, `Vector2Int`, `Mathf` (Lerp, PI, Sqrt, Cos, Sin, Clamp, Clamp01, FloorToInt, RoundToInt, Max, Min), `Random` (InitState, Range, value), `ScriptableObject`, `Resources.LoadAll`, `Debug`, and the attributes. `PlanetArchetype.terrainProfile` references the game's `ColonyTerrainProfile`.
- Game-only fields as the plan says: `StarSystemTemplate.BaseEnemySpawnRate`, `MinEnemyLevel`, `MaxEnemyLevel`, `StationData.IsHostileToPlayer`; also `PlanetArchetype.terrainProfile`.

## The capture

- Assets/Editor/GalaxyCapture/LegacyGalaxyCases.cs holds the cases; `LegacyGalaxyCaptureRunner.Run` writes them in batchmode: `unity run <worktree> -- -nographics -executeMethod LegacyGalaxyCaptureRunner.Run -logFile <log>`, with `GALAXY_CAPTURE_OUT` naming the output folder. The first run, including the whole import of a fresh worktree, took about 3 minutes.
- Seeds: 0, 1, -1, int.MaxValue, int.MinValue, 42, then `i * 2654435761` as int.
- Recorded in Unity 6000.6.0f1, Mono 6.13, x64: 100 galaxies (4,000 entries; the first 5 with the full system behind every entry) and 200 systems of `home_system`.
- The same system cases through a .NET console harness (Tests/GalaxyCapture/Harness.csproj, linking the game's files against `UnityStub.cs`): net10.0 and net48 agree with each other exactly.
- **Unity's Mono computes `Mathf.Lerp`'s float expression in double precision and rounds once.** A stub with the single-precision form `a + (b - a) * t` differed from Unity in the last bit in 1,188 values; with `(float)((double)a + ((double)b - a) * t)` it matches all 200 systems exactly. An IL2CPP player probably evaluates in single precision, so the game's own Editor and player builds may disagree in the last bit for the same seed. That supports D5: decisions from integer draws, stored values rounded.
- **Mono's `float.ToString("R")` is not round-trip.** It printed 7 significant digits where 9 were needed. The capture writes `G9`.
- **Seeded `System.Random` folds the seed's sign:** seeds `s` and `-s` generate identical systems, and so do int.MaxValue and int.MinValue (only the names differ, because naming uses `seed * 486187739 + 11`). Half of the seed space is duplicated. That supports D3.

## Statistics (Tests/GalaxyCapture/legacy/stats.txt)

Galaxies, 4,000 entries:
- Star types: RedDwarf 40.2%, OrangeDwarf 20.7%, Binary 15.4%, YellowDwarf 11.9%, Multiple 6.4%, WhiteDwarf 2.8%, Neutron 2.6%. BlueWhite, Giant and Supergiant never occur (the mix leaves them out).
- Centre bodies: 1 (69%), 2 (24%), 3 (6%); kinds Star 77%, white dwarf 13%, black hole 6.5%, neutron star 3%.
- Regions by radius: OuterRim 51%, MidRim 40%, Core 9.5%.
- **Danger 10 never occurs.** `Clamp(FloorToInt(r / R * 10), 1, 10)` gives at most 9 inside the disc, and level 1 covers the inner 20%.
- Spacing on a disc of radius 1,000: nearest neighbour min 2.7, 5th percentile 34.9, median 133.8; 16 systems (0.4%) have a neighbour closer than 10 and 88 (2.2%) closer than 25. There is no shape, lane or cluster.
- Names: no duplicate inside a galaxy (the `used` set works); families HD 18%, HIP 10%, GJ 9%, Kepler 8%, TOI 8%, 2MASS 6%. A galaxy entry and its full system agree on name and star type in 200 of 200 cases.

Systems, 200 seeds:
- Planets 3 to 6 (mean 4.46); barren 33%, rocky 28.5%, ice 25%, gas 13.5%. Moons 0, atmospheres 0, life 0, spin 0.
- Orbits start at 2 to 4 units and grow by 1.5 to 4 units: neighbour ratio min 1.09, median 1.46. Gas giants appear at every orbit index, 29 of 120 as the innermost planet. No zones.
- **Red dwarfs weigh 0.83 to 2.49 Suns** (mass and luminosity are drawn regardless of type).
- Belts: 0 (33%), 1 (25%), 2 (42%). **127 planets sit inside a belt's band.**
- Stations in 64 of 200 systems; **only 27 distinct station names in 64** (6 prefixes times 5 suffixes); **5 stations orbit a planet index that does not exist.**
- Hazards: 0 (the template disables nebulae and anomalies).

## What this changes in the plan

- D3: the cost the plan feared (development saves regenerating differently) does not exist; the recommendation stands, more cheaply. The game's switch (Stage 5) should store the galaxy seed in the save and show it, or "the same seed as the lab page" cannot be checked.
- D5: confirmed by the Mono finding; add "no float expression whose evaluation precision varies by runtime feeds an output".
- The legacy recordings are a record of the old behaviour, not a contract (D3 changes every seed's output); they serve the before-and-after comparison and the game's switch.
- The plan's line about recording systems "through HumbleLogicTests" was done with a separate harness instead: HumbleLogicTests' stub has no Vector2, Resources or Random, and adding them there would change the game's test project.

Related: [the plan](../plans/2026-10-02-galaxy-generator-extraction.md), [improvement ideas](2026-10-02-galaxy-generator-improvement-ideas.md), [package size budget](2026-10-02-package-size-budget.md).
