# Handoff

## Current state
2026-10-03 (galaxy cluster level done). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core (`tests/Golden/v1/core.json`), the star system level (`system.json`), the galaxy level with `Universe.At` (`galaxy.json`), the planet level (`planet.json`), moon and belt detail (`moon-belt.json`, 49c5fa5) and the galaxy cluster level (`cluster.json`, 64e90de). 144 tests pass on net10.0 and net48; all six golden files identical on both. Size gate green: DLL 435.0 KB, nupkg 308.5 KB.
- Cluster: `GalaxyCluster.Generate` (`v1-seed/cluster`), a group or cluster (`ClusterKind` option) as a cheap map of galaxies with type codes (Sb, SBc, E3, S0, cD, Irr, dSph ...) that drive each galaxy's shape, ages, richness (scales the gas giant weight through `SystemContext.Richness`), active cores (danger + 2 or 3 within 150 or 300 units of the centre) and satellites; links are gates to the majors, a wormhole ring and tethers, and each galaxy opens them at its core or facing edge (`galaxy.Gates`). A galaxy in a cluster (`v1-seed/cluster/galaxy/i`) gets a `GalaxyContext`; `GalaxyContext.Alone` keeps a lone galaxy's map, which gains only Name, Type, Descriptor and fixed Age, Richness and CoreActivity. Record: [notes/2026-10-03-galaxy-cluster-level-design.md](notes/2026-10-03-galaxy-cluster-level-design.md).
- Moons and belts: every moon gains physics, orbit and period, tidal heat, temperature, air by the planet level's Jeans rule, a hidden ocean, life, climate, traits, resources, hazards, habitability (`Moon.HabitabilityFor`), a descriptor and a summary; every belt an address (`.../belt/k`, seed `Child(system, "belt", k)`), name, composition, mass, largest body, temperature, resources and summary; all on new streams, so the other golden files did not move. Where a moon's kind promises more than physics allows (volcanic heat, hazy and garden air), the kind wins and the note says how. `PlanetResources` is now `ResourceGrades`. Record: [notes/2026-10-03-moon-and-belt-level-design.md](notes/2026-10-03-moon-and-belt-level-design.md).
- Planet: [notes/2026-10-02-planet-level-design.md](notes/2026-10-02-planet-level-design.md). Galaxy: [notes/2026-10-02-galaxy-level-design.md](notes/2026-10-02-galaxy-level-design.md). Default galaxy with every system in full 1.8 to 2.8 ms.
- `Universe.At(address, options)` returns the cluster, galaxy, system, planet, moon, station or belt; the hierarchy tests check seven galaxies, three clusters, 300 lone systems and 900 lone planets against their addresses.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.

## In progress
Nothing half-done.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- A new level must not add draws to an existing stream: give it new stream names on the object's seed, and check `git status` shows no change under `tests/Golden/v1/` before writing the new golden file (`UG_WRITE_GOLDEN=1` only writes missing files).
- Records holding lists compare by reference: once `Moon` held arrays, the hierarchy test's `Is.EqualTo(moon)` failed on equal moons. Compare such records by their golden JSON text.
- At the default 60 systems every cluster member is under 80 systems, so "spirals under 80 become S0" erased the morphology and density relation; spirals now grow to 80 (when `Systems` is at least 40), placed with room for it. `string.StartsWith(char)` and `string.Contains(char)` do not exist on net48 and CA1865 or CA2249 reject the string forms on net10: compare `s[0] == 'E'`.
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
- Cluster level, open for tuning before beta (changes `cluster.json`): member and dwarf counts, quasar share, satellite floor; no option for the number of galaxies yet. See the cluster note.
- Before beta, seed-changing questions for Mark: the colliding-pair shape in `Auto`; region themes steering system tags; giants are 30% of planets; "holy site" is 11% of tags; temperate deserts can be cold, 41% of planets are locked, hazard 4 is rare; the system level draws moon kinds without size or distance (97% of volcanic and 89% of ocean moons are heated by their kind's floor, every hazy and garden moon keeps its air by renewal, 8.6% of ice moons sit above 273 K). Not seed-changing but worth his view: garden moons are mostly marginal for people (low gravity).
- A golden file may be rewritten before the first release only on purpose, said in the commit (AGENTS.md). The CI golden check guards golden files from the first release tag on: Mark to confirm at Stop 2.
- PR #1 stays a draft until Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
The universe level, D14's last (the release valve to 1.1 without seed changes): a short design note first, covering `Universe.Generate` beside `Universe.At` (the static class becomes the record), U1 landmark slots, U3 distance frames, U10 merging pairs (A12), U11 the cosmic web, A13 and the U7 epoch, and how a cluster in a universe (`v1-seed/universe/cluster/i`) takes its kind and epoch from it through a cluster context while `GalaxyCluster.Generate` alone keeps its output (`cluster.json` must not move). Then code, tests and a new golden file, as for the cluster level. The plan's "Next single action" has the details. The prompt to start that session is [next-session-prompt.md](next-session-prompt.md).
