# Handoff

## Current state
2026-10-03: **Stage 4: 1.0.0-beta.1 is on nuget.org and verified.** Release run 37158022980 (tag `v1.0.0-beta.1` on `master` c4f4e9e) passed every job: build and test, attest, Windows net48 and net10.0, `push to nuget.org` (Trusted Publishing accepted, nupkg and snupkg pushed) and the GitHub Release. Checklist step 7 passed in full: flat container and registration (listed), repository signature, snupkg on the symbol server, prerelease GitHub Release with both files, nupkg attestation, and fresh net10.0 and net48 consoles printing the README's first example identically ([Stage 4 checklist](notes/2026-10-03-stage-4-checklist.md) step 7). C#, as ruled by Mark (F# port deferred).

- Golden files in `tests/Golden/v1/` unchanged; no Runtime change this session.
- AGENTS.md's "What this is" still says nothing is released and the repository is private; a one-line fix is its own pull request for Mark (instruction file, not docs-only).

## In progress
Nothing half-done. Waiting for Mark to choose the next step.

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
- The history scan reads a diff line's leading `+` as part of an email (`+@AGENTS.md`) and the `everlast-vault` skill name as a vault path: `scripts/history-scan.py` requires an alphanumeric local part and a path separator after the vault name.
- `scripts/history-scan.py` matched its own regex text in history (a connection-string prefix, the overlay folder name) once it was committed; it skips its own file in both passes now. Writing a pattern's literal into a note trips the current-tree pass too: describe it in words.
- The size gate's "UPM compressed" number wobbles by about 0.1 KB between builds of identical content (114.5 KB on `master` 1776834, 114.6 KB on PR #10's run of the same tree), most likely archive timestamps: do not chase a 0.1 KB change there; quote `master`'s run.

## Left / follow-ups
1. Mark chooses: Stage 4's 1.0.0 items (N1 first: build the unbuilt 1.0 matrix rows or move them to 1.x via `kb/features/status.json` and `build_matrix.py`, [Stop 2 note](notes/2026-10-03-stop-2-questions.md); N2 API additions if wanted), then 1.0.0 the same way and `PackageValidationBaselineVersion` 1.0.0; or Stage 5 (the Unity compile check, OpenUPM, then scale tests and BenchmarkDotNet, N7).
2. Optional for Mark: narrow the Trusted Publishing scope to "push only new package versions" (checklist step 3).
3. `dotnet add package` refuses `--version` with `--prerelease`; use one or the other (checklist step 7).

## Next single action
Mark picks N1 (towards 1.0.0) or Stage 5. The next session's prompt is [next-session-prompt.md](next-session-prompt.md).
