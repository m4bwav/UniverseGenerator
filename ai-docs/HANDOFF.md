# Handoff

## Current state
2026-10-03: **Stage 4 prepared, waiting for Mark.** Branch `stage4-prep` (pull request for Mark) holds three commits: the history scan ([note](notes/2026-10-03-history-scan.md), `scripts/history-scan.py`), `release.yml` with `scripts/check-package.sh`, and [Mark's Stage 4 checklist](notes/2026-10-03-stage-4-checklist.md). C#, as ruled by Mark (F# port deferred).

- Scan: no secret in any commit (gitleaks 8.30.1 over every ref and the working tree, plus the regex pass). Old commits keep local paths and Mark's second commit address; all already public in other m4bwav repositories, so no history rewrite. The current tree is clean; the runner's folder moved to the private sidecar. Private name list for `--names-file`: in the private sidecar.
- `release.yml`: from the package-modernize template; tag v* on a `master` commit with `ci` passed, build, tests (Linux net10.0; Windows net48 and net10.0), package check, size gate, attestation, publish job gated by the `nuget` environment through Trusted Publishing, then the GitHub Release. Linted clean, never run. Hosted runners: the repository must be public first.
- nuget.org Trusted Publishing doc (updated 2026-09-01) matches the plan; package ID `UniverseGenerator` is free; the repository is private with no environments and no Actions variables.
- On `master` since Stage 3: every level, 170 tests (net10.0 and net48), golden files in `tests/Golden/v1/` unchanged. Size gate green: nupkg 373.5 KB, DLL 499.0 KB, UPM 456.9 KB unpacked and 115.4 KB compressed, 35 Runtime files.
- CI: self-hosted runner `universe` on Mark's PC (its folder in the private sidecar); `RUNS_ON` moves the jobs to hosted runners and must be set before the repository goes public (checklist step 2).

## In progress
The `stage4-prep` pull request, waiting for Mark's review. Nothing else half-done. The branch `stage3-core` is merged and can be deleted.

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

## Left / follow-ups
1. Mark: review and merge `stage4-prep`, then the [Stage 4 checklist](notes/2026-10-03-stage-4-checklist.md) (CI off the runner, public, Trusted Publishing policy, `nuget` environment, tag after green, approve).
2. Agent steps in that checklist: re-run the scan on the final `master`; `RUNS_ON` and a pull request for the hosted Windows, Linux and macOS matrix (plan D9); the public-repository settings and rulesets; the dated CHANGELOG pull request; verification from nuget.org.
3. Before 1.0.0: N1 (matrix rows) and, if wanted, N2 (API additions).
4. Stage 5: the Unity compile check, then scale tests and BenchmarkDotNet (N7).

## Next single action
Mark reviews the `stage4-prep` pull request. The next session's prompt is [next-session-prompt.md](next-session-prompt.md): it checks how far Mark got and does the checklist's agent steps up to the tag.
