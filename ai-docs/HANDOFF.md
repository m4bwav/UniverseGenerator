# Handoff

## Current state
2026-10-02 (late night). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core (`tests/Golden/v1/core.json`), the star system level (`system.json`), and the galaxy level with `Universe.At` (`galaxy.json`, 8 galaxies with two systems each in full). 101 tests pass on net10.0 and net48; all three golden files identical on both. Size gate green (CI, 4e63c06): DLL 334.0 KB, nupkg 209.0 KB.
- Galaxy: `Galaxy.Generate`, `galaxy.Map` (cheap, no planets), lazily generated `galaxy.Systems` / `galaxy.System(i)`, `Lanes`, `Regions`; options `Systems` (1 to 2,000, default 60) and `Shape` (`Auto`). A default galaxy with every system in full: 1.7 ms. Record: [notes/2026-10-02-galaxy-level-design.md](notes/2026-10-02-galaxy-level-design.md).
- `Universe.At(address, options)` returns the galaxy, system, planet, moon or station; the hierarchy test checks 4,929 objects against their addresses.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.

## In progress
Nothing half-done.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- The prototype's lanes were O(n³); at 2,000 systems that is hopeless. The grid search gives identical lanes (tested against the brute-force definition).
- CA2208 rejects a literal `"address"` as paramName in a helper; name the helper's parameter `address` and use `nameof`.
- `StringAssert` is not available (NUnit 4 without the legacy using); use `Assert.That(x, Does.Match(...))`.
- ArgumentException messages end with " (Parameter 'x')" on .NET but a new line and "Parameter name: x" on net48: assert with `StartWith` or `Contain`, never `EndWith`.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool and run them, or keep heredocs free of apostrophes.
- The analyzers reject constant array arguments (CA1861) and `new T[0]` (CA1825): keep weight tables in static readonly fields; `StartsWith(char)` does not exist on net48.

## Left / follow-ups
- Galaxy extras still owed (own streams, no seed changes): factions over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy.
- Before beta, seed-changing questions: the colliding-pair shape in `Auto`; region themes steering system tags. Also from the system level: giants are 30% of planets; "holy site" is 11% of tags.
- A golden file may be rewritten before the first release only on purpose, said in the commit (AGENTS.md). The CI golden check guards golden files from the first release tag on: Mark to confirm at Stop 2.
- PR #1's title and description were rewritten for the C# work on 2026-10-02; it stays a draft until Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
The planet level (ideas P1 to P17, A7): `Planet.Generate` and each planet's detail on its own streams, so `system.json` and `galaxy.json` do not change; its golden file; `Universe.At` reaching it. Start from the plan's Planets table and `kb/rules/plausibility-rules.md`.
