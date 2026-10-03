# Handoff

## Current state
2026-10-02 (night, planet level done). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core (`tests/Golden/v1/core.json`), the star system level (`system.json`), the galaxy level with `Universe.At` (`galaxy.json`), and the planet level (`planet.json`, d673430). 115 tests pass on net10.0 and net48; all four golden files identical on both. Size gate green: DLL 378.0 KB, nupkg 252.3 KB.
- Planet: every planet gains physics, temperature with day and night sides, a Jeans-rule atmosphere with a why note, water and ice, rotation, tilt and tidal lock, six climate bands, biomes, life, traits, an anomaly, resources 0 to 5, hazard tier, similarity, habitability (`HabitabilityFor(Species)`) and a `Summary` line, all on new streams of the planet's seed, so the older golden files did not move. `Planet.Generate(seed)` makes a lone planet (`v1-seed/planet`, moons below it); `Universe.At` reaches it. Default galaxy with every system in full 3.28 ms. Record: [notes/2026-10-02-planet-level-design.md](notes/2026-10-02-planet-level-design.md).
- Galaxy: `Galaxy.Generate`, `galaxy.Map` (cheap, no planets), lazily generated `galaxy.Systems` / `galaxy.System(i)`, `Lanes`, `Regions`; options `Systems` (1 to 2,000, default 60) and `Shape` (`Auto`). A default galaxy with every system in full: 1.7 ms. Record: [notes/2026-10-02-galaxy-level-design.md](notes/2026-10-02-galaxy-level-design.md).
- `Universe.At(address, options)` returns the galaxy, system, planet, moon or station; the hierarchy test checks 4,929 objects against their addresses.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.

## In progress
Nothing half-done.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- A new level must not add draws to an existing stream: give it new stream names on the object's seed, and check `git status` shows no change under `tests/Golden/v1/` before writing the new golden file (`UG_WRITE_GOLDEN=1` only writes missing files).
- The prototype's lanes were O(n³); at 2,000 systems that is hopeless. The grid search gives identical lanes (tested against the brute-force definition).
- CA2208 rejects a literal `"address"` as paramName in a helper; name the helper's parameter `address` and use `nameof`.
- `StringAssert` is not available (NUnit 4 without the legacy using); use `Assert.That(x, Does.Match(...))`.
- ArgumentException messages end with " (Parameter 'x')" on .NET but a new line and "Parameter name: x" on net48: assert with `StartWith` or `Contain`, never `EndWith`.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool and run them, or keep heredocs free of apostrophes.
- The analyzers reject constant array arguments (CA1861) and `new T[0]` (CA1825): keep weight tables in static readonly fields; `StartsWith(char)` does not exist on net48.

## Left / follow-ups
- Galaxy extras still owed (own streams, no seed changes): factions over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy.
- Before beta, seed-changing questions: the colliding-pair shape in `Auto`; region themes steering system tags. Also from the system level: giants are 30% of planets; "holy site" is 11% of tags. From the planet level: temperate deserts can be cold (0.6 bar cap), 41% of planets are locked, hazard 4 is rare.
- A golden file may be rewritten before the first release only on purpose, said in the commit (AGENTS.md). The CI golden check guards golden files from the first release tag on: Mark to confirm at Stop 2.
- PR #1's title and description were rewritten for the C# work on 2026-10-02; it stays a draft until Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
Moon and belt detail (D14's next level): moon physics from the parent, temperature, Jeans atmosphere, tidal heating, life on garden moons; belt composition and resources; each on its own streams so `system.json`, `galaxy.json` and `planet.json` stay as they are, with a new golden file. Belts have no address or seed yet: give them `Child(system, "belt", k)` and an `Address` field (fields are written explicitly, so `system.json` does not change). Write a short design note first, as for the planet level; reuse `PlanetDetail` where a moon behaves like a small planet.
