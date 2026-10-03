# Handoff

## Current state
2026-10-03: **Stage 4 under way, waiting for Mark.** `stage4-prep` is merged (PR #3). The repository is still private; no `nuget` environment, no `v*` tag. C#, as ruled by Mark (F# port deferred).

- Scan re-run on the final `master` (272b0e0): clean, nothing new. gitleaks 8.30.1 no leaks; `scripts/history-scan.py` only the five known kinds plus their removal commits, current tree 0. The script's own patterns matched themselves in history; it now skips itself there too. PR #4 (scan note and that script fix), waiting for Mark.
- `RUNS_ON` set to `"ubuntu-latest"` with Mark's go-ahead; `ci` on `master` passed on hosted Ubuntu 24.04 (run 37139764461).
- PR #5: `ci.yml`'s build job is the template's matrix on `ubuntu-24.04`, `windows-latest`, `macos-latest`, every check kept (package check and size gate on Linux, as in `release.yml`); 170 tests green on each, net48 on Windows; actionlint 1.7.12 and zizmor 1.30.1 clean. Once merged, `RUNS_ON` is unread and can be deleted, and the runner `universe` removed.
- On `master`: every level, 170 tests (net10.0 and net48), golden files in `tests/Golden/v1/` unchanged. Size gate green (hosted run: nupkg 373.2 KB, DLL 499.0 KB).
- The package-modernize templates hold only the tag ruleset (`templates/rulesets/tags-admins-only.json`); the `master` ruleset must be written from the checklist's description.

## In progress
PR #4 and PR #5 wait for Mark's review. Nothing else half-done.

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
1. Mark: merge PR #4 and PR #5; remove the runner `universe` and stop its scheduled task; make the repository public; the Trusted Publishing policy and the `nuget` environment ([Stage 4 checklist](notes/2026-10-03-stage-4-checklist.md) steps 2 to 4).
2. Agent, once public: secret scanning and push protection, private vulnerability reporting, workflow permissions read, the `master` ruleset (required check `ci`) and the `v*` tag ruleset; delete the `RUNS_ON` variable after PR #5 is merged.
3. Agent, after the policy and the environment: the release pull request (dated CHANGELOG heading, size numbers checked). Then Mark tags and approves; then the agent verifies from nuget.org (checklist step 7).
4. Before 1.0.0: N1 (matrix rows) and, if wanted, N2 (API additions).
5. Stage 5: the Unity compile check, then scale tests and BenchmarkDotNet (N7).

## Next single action
Mark reviews PR #4 and PR #5, removes the runner and makes the repository public. The next session's prompt is [next-session-prompt.md](next-session-prompt.md): it checks how far Mark got, then does the public-repository settings and the release pull request.
