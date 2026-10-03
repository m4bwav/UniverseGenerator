# Handoff

## Current state
2026-10-03 (moon and belt detail done). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core (`tests/Golden/v1/core.json`), the star system level (`system.json`), the galaxy level with `Universe.At` (`galaxy.json`), the planet level (`planet.json`), and moon and belt detail (`moon-belt.json`, 49c5fa5). 130 tests pass on net10.0 and net48; all five golden files identical on both. Size gate green: DLL 400.0 KB, nupkg 275.8 KB.
- Moons and belts: every moon gains physics, orbit and period, tidal heat, temperature, air by the planet level's Jeans rule, a hidden ocean, life, climate, traits, resources, hazards, habitability (`Moon.HabitabilityFor`), a descriptor and a summary; every belt an address (`.../belt/k`, seed `Child(system, "belt", k)`), name, composition, mass, largest body, temperature, resources and summary; all on new streams, so the other golden files did not move. Where a moon's kind promises more than physics allows (volcanic heat, hazy and garden air), the kind wins and the note says how. `PlanetResources` is now `ResourceGrades`. Record: [notes/2026-10-03-moon-and-belt-level-design.md](notes/2026-10-03-moon-and-belt-level-design.md).
- Planet: [notes/2026-10-02-planet-level-design.md](notes/2026-10-02-planet-level-design.md). Galaxy: [notes/2026-10-02-galaxy-level-design.md](notes/2026-10-02-galaxy-level-design.md). Default galaxy with every system in full 1.8 to 2.8 ms.
- `Universe.At(address, options)` returns the galaxy, system, planet, moon, station or belt; the hierarchy test checks 5,296 objects of seven galaxies against their addresses, plus 300 lone systems and 900 lone planets.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.

## In progress
Nothing half-done.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- A new level must not add draws to an existing stream: give it new stream names on the object's seed, and check `git status` shows no change under `tests/Golden/v1/` before writing the new golden file (`UG_WRITE_GOLDEN=1` only writes missing files).
- Records holding lists compare by reference: once `Moon` held arrays, the hierarchy test's `Is.EqualTo(moon)` failed on equal moons. Compare such records by their golden JSON text.
- `Enum.GetValues<T>()` does not exist on net48 and CA2263 rejects the non-generic one in the tests: list the values in an array.
- The planet level's Jeans rule (exosphere 4 T_eq) says Titan loses its nitrogen; a cold body's exosphere is nearer 2 T_eq. Mark asked for the same rule, so hazy and garden moons keep their air by renewal instead.
- The prototype's lanes were O(n³); at 2,000 systems that is hopeless. The grid search gives identical lanes (tested against the brute-force definition).
- CA2208 rejects a literal `"address"` as paramName in a helper; name the helper's parameter `address` and use `nameof`.
- `StringAssert` is not available (NUnit 4 without the legacy using); use `Assert.That(x, Does.Match(...))`.
- ArgumentException messages end with " (Parameter 'x')" on .NET but a new line and "Parameter name: x" on net48: assert with `StartWith` or `Contain`, never `EndWith`.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool and run them, or keep heredocs free of apostrophes.
- The analyzers reject constant array arguments (CA1861) and `new T[0]` (CA1825): keep weight tables in static readonly fields; `StartsWith(char)` does not exist on net48.

## Left / follow-ups
- Galaxy extras still owed (own streams, no seed changes): factions over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy.
- Before beta, seed-changing questions for Mark: the colliding-pair shape in `Auto`; region themes steering system tags; giants are 30% of planets; "holy site" is 11% of tags; temperate deserts can be cold, 41% of planets are locked, hazard 4 is rare; the system level draws moon kinds without size or distance (97% of volcanic and 89% of ocean moons are heated by their kind's floor, every hazy and garden moon keeps its air by renewal, 8.6% of ice moons sit above 273 K). Not seed-changing but worth his view: garden moons are mostly marginal for people (low gravity).
- A golden file may be rewritten before the first release only on purpose, said in the commit (AGENTS.md). The CI golden check guards golden files from the first release tag on: Mark to confirm at Stop 2.
- PR #1 stays a draft until Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
The galaxy cluster level (D14's next; cluster and universe together about 3 days, and the valve to 1.1): a short design note first, covering `GalaxyCluster.Generate`, ideas U5 to U9, U2 and N10, and how a galaxy in a cluster (`v1-seed/cluster/galaxy/i`) takes its type and flavour from the cluster while `Galaxy.Generate` alone keeps its present output (`galaxy.json` must not move). Then code, tests and a new golden file, as for the moon level.
