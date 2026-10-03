# Handoff

## Current state
2026-10-03: Stage 3 is complete and **waiting for Mark at Stop 2**. Branch `stage3-core`, PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1), ready for review, assigned to m4bwav with `needs-review`. C#, as ruled by Mark (F# port deferred; the 1.0 plan's "Deferred: F# and Fable" keeps the facts).

- Done: the core and all six levels, galaxy extras, `StarName`, `Universe.At`; golden files `core`, `system`, `galaxy`, `planet`, `moon-belt`, `cluster`, `universe`, `galaxy-extras`, `star-name` in `tests/Golden/v1/`, identical on net10.0 and net48. 170 tests. CI green on the self-hosted runner.
- This session (7a3fe40, c14c7ba): README (the plan's first example as it runs, presets and addresses, validation, the feature matrix's short form, clusters and universes, extras, StarName, `Universe.At`, the seed promise, samples); `samples/ConsoleSample` (every README example lives in `Examples.cs`; `SampleTests` checks the README's C# blocks against its `#region readme` blocks in order and runs them on both frameworks; CI runs the sample); the Unity sample `Packages/com.m4bwav.universe-generator/Samples~/GalaxyPrinter` (a MonoBehaviour plus engine-free `GalaxyReport.cs` that the tests compile), listed in `package.json`; CHANGELOG `[1.0.0-beta.1] - Unreleased` with the size numbers. No Runtime change.
- Size gate green: nupkg 373.5 KB, DLL 499.0 KB, UPM 456.9 KB unpacked and 116.0 KB compressed (the gate now counts `Samples~`, which OpenUPM ships), 35 Runtime files, 9,736 lines. README 8.0 KB (+2.8 KB in the nupkg).
- Stop 2 questions: [notes/2026-10-03-stop-2-questions.md](notes/2026-10-03-stop-2-questions.md), the same list in PR #1's description: S1 to S16 seed-changing (each with the golden files it moves), E1 to E4 changing only `galaxy-extras.json`, N1 to N8 not seed-changing (matrix rows marked 1.0 but not built; awkward APIs the README found; CI and test items).
- Level records: [universe](notes/2026-10-03-universe-level-design.md), [cluster](notes/2026-10-03-galaxy-cluster-level-design.md), [moons and belts](notes/2026-10-03-moon-and-belt-level-design.md), [planet](notes/2026-10-02-planet-level-design.md), [galaxy](notes/2026-10-02-galaxy-level-design.md), [galaxy extras](notes/2026-10-03-galaxy-extras-design.md), [star system](notes/2026-10-02-star-system-level-design.md).
- CI: self-hosted runner `universe` (`D:\actions-runner-universe`, scheduled task at logon); `RUNS_ON="ubuntu-latest"` moves jobs to hosted runners.

## In progress
Nothing half-done. Waiting for Mark's Stop 2 rulings.

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
1. Mark rules Stop 2 (the note's S, E and N items).
2. Apply the rulings: seed-changing ones rewrite only the golden files the note names, on purpose, said in the commit message.
3. Test checklist items not done (N7): the Unity compile check (Stage 5 scratch project), scale tests, BenchmarkDotNet.
4. Stage 4 needs Mark: the history scan for secrets, private names and local paths, making the repository public, the Trusted Publishing policy.

## Next single action
Wait for Mark's Stop 2 rulings on PR #1, then apply them. The prompt to start that session is [next-session-prompt.md](next-session-prompt.md).
