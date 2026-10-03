# Handoff

## Current state
2026-10-03 (universe level done; all six levels of D14 are in). Stage 3 on branch `stage3-core`, draft PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1); one branch and pull request for all of Stage 3, a commit per step. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core (`tests/Golden/v1/core.json`), the star system level (`system.json`), the galaxy level with `Universe.At` (`galaxy.json`), the planet level (`planet.json`), moon and belt detail (`moon-belt.json`), the galaxy cluster level (`cluster.json`) and the universe level (`universe.json`, 8a42448). 158 tests pass on net10.0 and net48; all seven golden files identical on both. Size gate green: DLL 467.0 KB, nupkg 340.3 KB.
- Universe: `Universe` is now a record with `Generate` beside `At` (`v1-seed/universe`). The `Epoch` option (or a draw) sets every cluster's age. 4 to 7 groups and clusters joined by filaments (the Gabriel graph, `LinkTier.Filament`, `GalaxyGate.Cluster`), voids bounded by clusters, filaments and the map edge, each with 1 to 3 old, quiet, poor lone systems (`.../void/k/system/j`). Fixed roles: node 0 the home group (galaxy 0 the home spiral, quiet), node 1 the great cluster (the dying `cD`, old and quiet; its first member a ring galaxy), node 2 the merging group (galaxies 0 and 1 touching on a `Tidal` link, young with starburst regions; 2 more danger within 300 units of the tidal link and a "tidal frontier" region; a conflict hook), the field node farthest from home holds the distant quasar (galaxy 1), which every other galaxy sees in its sky (`galaxy.Landmark`). `Distances` gives light-years per unit and stylised travel days. A cluster in a universe gets a `ClusterContext` that changes values only, never draws, so `GalaxyCluster.Generate` alone keeps its output. Record: [notes/2026-10-03-universe-level-design.md](notes/2026-10-03-universe-level-design.md).
- Earlier levels: [cluster](notes/2026-10-03-galaxy-cluster-level-design.md), [moons and belts](notes/2026-10-03-moon-and-belt-level-design.md), [planet](notes/2026-10-02-planet-level-design.md), [galaxy](notes/2026-10-02-galaxy-level-design.md), [star system](notes/2026-10-02-star-system-level-design.md).
- `Universe.At(address, options)` returns the universe, a cluster (alone or in a universe), a void, a galaxy, system, planet, moon, station or belt; the hierarchy tests cover seven galaxies, three clusters, two universes, 300 lone systems and 900 lone planets.
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.
- Mark asked (2026-10-03) for a random star name generator in his website's tools: in the site plan's Stage 3 (m4bwav/markdavidrogers-site PR #2, merged) and this plan's Stage 6. It needs a public star-name entry point here; `StarNames` is internal.

## In progress
Nothing half-done.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- A new level must not add draws to an existing stream: give it new stream names on the object's seed, or (as the cluster and universe contexts do) change only values after every draw; check `git status` shows no change under `tests/Golden/v1/` before writing the new golden file (`UG_WRITE_GOLDEN=1` only writes missing files).
- Records holding lists compare by reference: compare such records by their golden JSON text.
- `string.StartsWith(char)` and `string.Contains(char)` do not exist on net48 and CA1865 or CA2249 reject the string forms on net10: compare `s[0] == 'E'`. `Enum.GetValues<T>()` does not exist on net48 and CA2263 rejects the non-generic one in the tests: list the values in an array.
- The planet level's Jeans rule (exosphere 4 T_eq) says Titan loses its nitrogen; Mark asked for the same rule, so hazy and garden moons keep their air by renewal instead.
- CA2208 rejects a literal `"address"` as paramName in a helper; name the helper's parameter `address` and use `nameof` (done again in `Universe.NothingBelow`).
- A public enum name can already exist in another level: the system level owns `LandmarkKind`, so the universe's is `UniverseLandmarkKind`. Grep the Runtime folder before naming a new public type.
- `StringAssert` is not available (NUnit 4 without the legacy using); use `Assert.That(x, Does.Match(...))`. ArgumentException messages differ in their ending between .NET and net48: assert with `StartWith` or `Contain`.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool and run them, or use the Edit tool.
- The analyzers reject constant array arguments (CA1861) and `new T[0]` (CA1825): keep weight tables in static readonly fields.
- `dotnet test --filter "FullyQualifiedName~Universe"` matches every test (the namespace is `UniverseGeneration`); filter on `Tests.UniverseTests`.

## Left / follow-ups (what Stage 3 still owes before Stop 2)
1. Galaxy extras (own streams, no seed changes): factions over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy.
2. A public star-name entry point for the site tool, without changing any seed's output.
3. The plan's test checklist items not yet done: the Unity compile check, scale tests, BenchmarkDotNet (golden, property, distribution, hierarchy tests and the size gate exist).
4. README (the plan's first example, then the feature matrix's short form), CHANGELOG (with the size numbers), samples.
5. Stop 2: the seed-changing questions for Mark, gathered from the notes' "Open for tuning" sections: the colliding-pair shape in `Auto`; region themes steering system tags; giants are 30% of planets; "holy site" is 11% of tags; temperate deserts can be cold, 41% of planets are locked, hazard 4 is rare; moon kinds drawn without size or distance; cluster member and dwarf counts, quasar share, satellite floor; the universe's 4 to 7 clusters with no option, fixed node roles, the home spiral as S0 under 40 systems, galaxy names unique only within a cluster, large voids. Not seed-changing but worth his view: garden moons are mostly marginal for people. Also: the CI golden check guards golden files from the first release tag (confirm), and `universe.json` is 813 KB, the largest golden file (every galaxy of four universes).
- PR #1 stays a draft until Stop 2. Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
The galaxy extras: a short design note first (factions over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy, each on its own streams of the galaxy or system seed), then code, tests and a new golden file, as for every level; no existing golden file may move. The prompt to start that session is [next-session-prompt.md](next-session-prompt.md).
