# Handoff

## Current state
2026-10-03, Stage 4 towards 1.0.0 (1.0.0-beta.1 is on nuget.org and verified). **N1 ruled: every unbuilt or partial 1.0 row goes in 1.0.0** ("Do all the things"). Eight are built, as stacked pull requests for Mark: #17 seed-url, #18 presets and options, #19 export-json, #20 orbital elements, #21 invented names, #22 units, #23 diagnostics, #25 shapes. Each base is the one before it, so he merges in order, #17 first. The table is in the [Stop 2 note](notes/2026-10-03-stop-2-questions.md) under "N1 progress". `tests/Golden/v1/` is unchanged on every branch; each new field or option added a new golden file. Size at the top of the stack (#25, local): nupkg 409.3 KB, DLL 536.0 KB (green under 750), 40 files, 12,109 lines.

- Docs merged: #16 (N1 ruling table), #24 ([real stars track](notes/2026-10-03-real-stars-track.md), Mark's request for maps approximating Sol's neighbourhood: a 1.x research and features track after 1.0.0).

## In progress
Nothing half-done. Left in N1, in order: `constraints`, `hooks-plugins` with `custom-fields`, `data-tables-editable`, the Unity float adapter. Then the 1.0.0 release pull request.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md). Mono's `float.ToString("R")` is not round-trip.
- A new level or field must not add draws to an existing stream: use a new stream name on the object's seed, or change only values after every draw. Check that `git status` shows no change under `tests/Golden/v1/` (`UG_WRITE_GOLDEN=1` only writes missing files).
- `export.json` must not change when a field is added: add the new keys to `JsonKeyFilter.AddedLater` (tests) and write a golden file for them; run `python scripts/gen-json-export.py` after adding any record property (`ExportTests` fails until you do).
- A new `GeneratorOptions` property needs its name in `Options/OptionsCode.cs` (`OptionsCodeTests` reflects over the properties and fails until it has one).
- `JsonWriter.Number` needs |value| x 10^decimals under 2^53; the export picks decimals per value.
- Examples printing doubles with `:0.0` are culture-dependent: print whole numbers in README examples.
- A README code block must sit in the same order as its region in `samples/ConsoleSample/Examples.cs` and in `Examples.All()`.
- Analyzers: no `Enum.GetValues<T>` on net48 (suppress CA2263 in tests with that reason), CA1826 rejects `Last()` on a list, CA1861 constant arrays, CA1825 `new T[0]`, CA2208 literal paramName in a helper.
- Bash heredocs turn `\\n` into a newline: write Python edit scripts with the Write tool, or use the Edit tool.
- `dotnet test --filter "FullyQualifiedName~Universe"` matches every test; filter on the class name.
- The size gate's "UPM compressed" wobbles by about 0.1 KB between builds; quote `master`'s run.
- Mark rejects effort estimates in human working days: estimate in agent time.

## Left / follow-ups
1. Mark: review and merge #17, #18, #19, #20, #21, #22, #23, #25 in that order. #22 has a design call for him: units as conversions, not an option.
2. The four remaining N1 rows, each additive, stacked on #25 (or on `master` once the stack is merged).
3. The 1.0.0 release pull request: CHANGELOG heading dated, with the size numbers from `master`'s size gate; `<Version>` and package.json 1.0.0; the README status line ("1.0.0-beta.1 in review, not released yet" is stale). Then Mark tags and approves; verify as checklist step 7; then `PackageValidationBaselineVersion` 1.0.0.
4. After 1.0.0: Stage 5 (Unity, OpenUPM), the real stars research pass, optional narrowing of the Trusted Publishing scope.

## Next single action
Build `constraints` on a branch stacked on `feat/galaxy-shapes` (#25), then hooks with custom fields, editable data tables and the Unity float adapter. The prompt is [next-session-prompt.md](next-session-prompt.md).
