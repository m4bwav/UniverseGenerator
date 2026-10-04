# Handoff

## Current state
2026-10-03, Stage 4 towards 1.0.0 (1.0.0-beta.1 is on nuget.org and verified). **Every N1 row is built** (Mark's ruling: all unbuilt or partial 1.0 rows go in 1.0.0). On `master` (744eb77): #17 to #25 (carried by #28) and #29 `constraints`. Waiting for Mark, in order: #30 hooks and custom fields (base `master`), #31 editable tables (base #30), #32 Unity float adapter (base #31). The table is in the [Stop 2 note](notes/2026-10-03-stop-2-questions.md) under "N1 progress". `tests/Golden/v1/` gained only new files. Size at #32 (local): nupkg 441.2 KB, DLL 563.0 KB (green under 750), 46 Runtime files, 13,423 lines.

## In progress
Nothing half-done. Next: the 1.0.0 release pull request, once Mark has merged #30 to #32.

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

## Left / follow-ups
1. Mark: review and merge #30, #31, #32 in that order (#30 states where hooks live and why).
2. The 1.0.0 release pull request: CHANGELOG heading dated, with the size numbers from `master`'s size gate; `<Version>` and package.json 1.0.0; the README status line ("1.0.0-beta.1 in review, not released yet" is stale). Then Mark tags and approves; verify as checklist step 7; then `PackageValidationBaselineVersion` 1.0.0.
3. After 1.0.0: Stage 5 (Unity, OpenUPM; the throwaway-project recipe above covers the compile check), the real stars research pass, optional narrowing of the Trusted Publishing scope.
4. Optional for Mark: turn on "Automatically delete head branches" so stacked pull requests retarget themselves.

## Next single action
When #30 to #32 are merged: the 1.0.0 release pull request. The prompt is [next-session-prompt.md](next-session-prompt.md).
