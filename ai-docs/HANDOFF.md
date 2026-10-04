# Handoff

## Current state
2026-10-03 (evening, US Central; 2026-10-04 UTC), Stage 4: **Mark merged the 1.0.0 release pull request #34** (merge commit 3505547, 2026-10-04 02:59 UTC). `ci` run 37172716089 on that `master` commit is green on Linux, Windows and macOS. Size gate on that run, all green: nupkg 441.0 KB, largest DLL 563.0 KB, UPM 606.6 KB unpacked and 143.2 KB compressed, 46 Runtime files, 13,423 lines. **No `v1.0.0` tag yet, so no 1.0.0 release run**: the tag and then the `nuget` approval wait for Mark.

## In progress
Nothing half-done. `master` already says 1.0.0 everywhere (CHANGELOG `## [1.0.0] - 2026-10-03`, `<Version>`, `package.json`, README and AGENTS.md status lines). AGENTS.md calls 1.0.0 "the current release on nuget.org", which is true only once the push runs.

## Dead ends hit
- Stacked pull requests: when a merged base branch is not deleted, GitHub does not retarget the next pull request, and it merges into the old branch, not `master` (#18 to #25 landed in `feat/seed-url`; #28 carried them). After a base merges, retarget the next pull request to `master` (`gh pr edit N --base master`) or ask Mark to delete the branch.
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md). Mono's `float.ToString("R")` is not round-trip.
- A new level or field must not add draws to an existing stream: use a new stream name on the object's seed, or change only values after every draw. A galaxy-level repair (constraints) draws from its own `constraints` stream and reaches a system only through `SystemContext`.
- `export.json` must not change when a field is added: add the keys to `JsonKeyFilter.AddedLater` and write a golden file; run `python scripts/gen-json-export.py` after adding any record property (it now handles `IReadOnlyDictionary<string, string>`).
- A new `GeneratorOptions` property needs its name in `Options/OptionsCode.cs`; `OptionsCodeTests.WithOtherValue` must know its type (string settings register test tables).
- netstandard2.0 has no span overloads, and CA1846 rejects `int.Parse(s.Substring(...))`: parse digits by hand (`JsonReader`).
- Bash heredocs turn a double backslash into one, even quoted: write C# with backslash escapes with the Write or Edit tool, or a Python script file.
- `JsonWriter.Number` needs |value| x 10^decimals under 2^53. Examples printing doubles are culture-dependent: print whole numbers.
- A README code block must sit in the same order as its region in `samples/ConsoleSample/Examples.cs` and in `Examples.All()`.
- Unity compile check without the Unity CLI: `Unity.exe -batchmode -quit -nographics -createProject <dir>`, add `"com.m4bwav.universe-generator": "file:<abs package path>"` to its manifest, open in batchmode with `-logFile`, then look for `error CS` and the DLLs in `Library/ScriptAssemblies`; `-executeMethod` on an `Assets/Editor` class runs a smoke check. Editors installed: 6000.5.8f1, 6000.6.0f1, 6000.6.4f1.
- The size gate's "UPM compressed" wobbles by about 0.1 KB between builds; quote `master`'s run.
- Mark rejects effort estimates in human working days: estimate in agent time.
- `release.yml` only checks that the version's CHANGELOG heading has a date, not which date: a tag on a later day does not fail the run, but the release ritual wants the heading to match the tag day.

## Left / follow-ups
1. Mark: tag `v1.0.0` on 3505547 and push it (`git fetch origin && git tag v1.0.0 3505547 && git push origin v1.0.0`; `master` has moved on with docs-only merges, and `release.yml` accepts any commit on `master` whose `ci` passed); then approve the `nuget` deployment on the release run. If he tags on a day after 2026-10-03 (local), a small pull request should re-date the CHANGELOG heading first, and the tag goes on that merge after `ci` is green.
2. Agent, after the release run: verify 1.0.0 from nuget.org as [checklist](notes/2026-10-03-stage-4-checklist.md) step 7 did for beta.1; then a pull request setting `PackageValidationBaselineVersion` 1.0.0 and replacing the csproj comment about no baseline.
3. After 1.0.0: Stage 5 (Unity, OpenUPM; the throwaway-project recipe above covers the compile check), the real stars research pass, optional narrowing of the Trusted Publishing scope. Start neither unless Mark asks.
4. Optional for Mark: turn on "Automatically delete head branches" so stacked pull requests retarget themselves.

## Next single action
When Mark has tagged `v1.0.0` and approved the deployment: verify 1.0.0 from nuget.org. The prompt is [next-session-prompt.md](next-session-prompt.md).
