# Handoff

## Current state
2026-10-03: **Stage 3 merged and Stop 2 ruled.** Mark merged PR #1 into `master` (merge commit 32df421, 16:46 UTC) without leaving rulings, so every Stop 2 question keeps its current value ([the note's "Ruled" section](notes/2026-10-03-stop-2-questions.md)). No rule or golden file changed. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- On `master`: the core and all six levels, galaxy extras, `StarName`, `Universe.At`, README, samples, CHANGELOG `[1.0.0-beta.1] - Unreleased`; golden files `core`, `system`, `galaxy`, `planet`, `moon-belt`, `cluster`, `universe`, `galaxy-extras`, `star-name` in `tests/Golden/v1/`, identical on net10.0 and net48. 170 tests. `ci` green on `master` after the merge. `<Version>` and `package.json` are already 1.0.0-beta.1.
- Size gate green: nupkg 373.5 KB, DLL 499.0 KB, UPM 456.9 KB unpacked and 116.0 KB compressed, 35 Runtime files.
- Still open for 1.0.0 (not the beta): N1, the feature-matrix rows marked 1.0 that are not built (build them or move them to 1.x via `kb/features/status.json`); N2's API additions (additive, any 1.x minor).
- `release.yml` does not exist yet, though AGENTS.md describes it.
- Level records: [universe](notes/2026-10-03-universe-level-design.md), [cluster](notes/2026-10-03-galaxy-cluster-level-design.md), [moons and belts](notes/2026-10-03-moon-and-belt-level-design.md), [planet](notes/2026-10-02-planet-level-design.md), [galaxy](notes/2026-10-02-galaxy-level-design.md), [galaxy extras](notes/2026-10-03-galaxy-extras-design.md), [star system](notes/2026-10-02-star-system-level-design.md).
- CI: self-hosted runner `universe` (on the maintainer's PC; its folder is in the private sidecar); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners. Branch protection is unavailable while the repository is private on the free plan.

## In progress
Nothing half-done. The branch `stage3-core` is merged and can be deleted.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- A new level must not add draws to an existing stream: give it new stream names on the object's seed, or change only values after every draw; check `git status` shows no change under `tests/Golden/v1/` before writing a new golden file (`UG_WRITE_GOLDEN=1` only writes missing files).
- Records holding lists compare by reference: compare such records by their golden JSON text (the README compares `Summary`).
- A galaxy-level extra must not change a map entry's danger, name or age: they feed each system's `SystemContext`.
- An address carries no options: `Universe.At(address)` needs the options the object was made with, or it finds a different object or throws.
- `string.StartsWith(char)` and `string.Contains(char)` do not exist on net48 and CA1865 or CA2249 reject the string forms on net10: compare `s[0] == 'E'`. `Enum.GetValues<T>()` does not exist on net48.
- CA2208 rejects a literal `"address"` as paramName in a helper; use `nameof` on a parameter named `address`.
- A public enum name can already exist in another level (`LandmarkKind` is the system level's); grep the Runtime folder before naming a public type.
- `StringAssert` is not available (NUnit 4); use `Assert.That(x, Does.Match(...))`. ArgumentException messages differ in their ending between .NET and net48: assert with `StartWith` or `Contain`.
- Bash heredocs with apostrophes fail here; write Python scripts with the Write tool, or use the Edit tool. A Python `str.replace` that ends before a `;` leaves the `;` behind: re-read the line after a scripted edit.
- The analyzers reject constant array arguments (CA1861) and `new T[0]` (CA1825).
- `dotnet test --filter "FullyQualifiedName~Universe"` matches every test (the namespace is `UniverseGeneration`); filter on `Tests.UniverseTests`.

## Left / follow-ups
1. Stage 4 preparation (an agent): the history scan for secrets, private names and local paths (N8), `release.yml` with Trusted Publishing behind the `nuget` environment, a checklist for Mark. One pull request, waiting for Mark.
2. Stage 4 (Mark): make the repository public, the nuget.org Trusted Publishing policy, the `nuget` environment's reviewer, tag `v1.0.0-beta.1`, approve the deployment.
3. Before 1.0.0: N1 (matrix rows) and, if wanted, N2 (API additions).
4. Stage 5: the Unity compile check, then scale tests and BenchmarkDotNet (N7).

## Next single action
Prepare Stage 4: the history scan, `release.yml` and Mark's checklist, in one pull request. The prompt that starts that session is [next-session-prompt.md](next-session-prompt.md).
