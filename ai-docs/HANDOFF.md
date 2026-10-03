# Handoff

## Current state
2026-10-03: **Stage 4: release PR #10 waits for Mark.** The repository is public, the public-repository settings and rulesets are on (checklist step 2 table), the nuget.org Trusted Publishing policy exists (Mark), and the `nuget` environment is complete: reviewer m4bwav, tag rule `v*`, `NUGET_USER`, admin bypass off. C#, as ruled by Mark (F# port deferred).

- PR #10 (`release/1.0.0-beta.1`): CHANGELOG heading dated 2026-10-03; size numbers corrected to the size gate on `master` 04f3a16 (nupkg 373.2 KB, DLL 499.0 KB, UPM 456.9 KB unpacked and 114.5 KB compressed, all green). Not docs-only, so Mark merges; the `master` ruleset needs `ci` green.
- No `v*` tag, no `release` run yet.
- Golden files in `tests/Golden/v1/` unchanged.

## In progress
Nothing half-done. The public-repository settings (checklist step 2, agent) wait for the repository to be public.

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

## Left / follow-ups
1. Mark: merge PR #10, wait for `ci` green on `master`, tag `v1.0.0-beta.1`, approve the `nuget` deployment ([Stage 4 checklist](notes/2026-10-03-stage-4-checklist.md) steps 5 and 6).
2. Agent, after the approval: watch the release run, verify from nuget.org (checklist step 7).
3. Before 1.0.0: N1 (matrix rows) and, if wanted, N2 (API additions).
4. Stage 5: the Unity compile check, then scale tests and BenchmarkDotNet (N7).

## Next single action
Mark merges the release PR #10, tags `v1.0.0-beta.1` after `ci` is green on `master`, and approves the deployment. The next session's prompt is [next-session-prompt.md](next-session-prompt.md): it checks how far Mark got, then watches the release run and verifies from nuget.org.
