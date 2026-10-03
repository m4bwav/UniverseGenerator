# Handoff

## Current state
2026-10-02 (night). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark; an F# and Fable port was planned and dropped the same evening (the 1.0 plan's "Deferred: F# and Fable" keeps the facts for a later port).

- Done: the core (PCG32, seeds, DMath, addresses, JSON writer; `tests/Golden/v1/core.json`), and the star system level (`StarSystem.Generate`; `tests/Golden/v1/system.json`, 18 systems). 84 tests pass on net10.0 and net48, both golden files identical on both. Size gate green: DLL 308.5 KB (the star tables as UTF-16 constants are most of it), nupkg 182.9 KB, UPM 48.3 KB compressed.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.
- Design and tuning record of the system level, with its statistics: [notes/2026-10-02-star-system-level-design.md](notes/2026-10-02-star-system-level-design.md).

## In progress
Nothing half-done. Next is the galaxy level.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- The prototype's sine and cosine series stopped at r^13 and r^12 (errors up to 1.4e-14); DMath runs them to r^21 and r^20.
- A belt with a linear half-width overlapped the inner planet in very wide gaps; edges are now proportional to the gap's middle.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool and run them.
- The analyzers (latest-recommended, warnings as errors) reject constant array arguments (CA1861) and `new T[0]` (CA1825): keep weight tables in static readonly fields; `StartsWith(char)` does not exist on net48, so tests index the first character.

## Left / follow-ups
- Tuning before beta: giants are 30% of planets; "holy site" is 11% of tags (always eligible). A golden file may be rewritten before the first release only on purpose, said in the commit (AGENTS.md).
- The CI golden check guards golden files from the first release tag on (and the legacy recordings always): Mark to confirm at Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
The galaxy level: port the prototype's `GalaxyProto.cs` (game repository's capture branch, worktree `Ai/labs/SpaceDeckBuilder2.1-galaxy`, `Tests/GalaxyPrototype`), add the skeleton pass that gives each system its `SystemContext` (unique name, region age, danger), then `Galaxy.Generate`, `galaxy.System(i)`, `Universe.At(address)` and the hierarchy test.
