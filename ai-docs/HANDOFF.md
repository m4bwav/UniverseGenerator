# Handoff

## Current state
2026-10-03 (evening, US Central; 2026-10-04 UTC). **Stage 4 is done.** **1.0.0 is released and verified.** On Mark's explicit instruction in the session the agent pushed tag `v1.0.0` on 3505547 and approved the `nuget` deployment; release run 37173982448 passed every job (Trusted Publishing accepted, GitHub Release a full release with nupkg and snupkg). Verified from nuget.org as [checklist](notes/2026-10-03-stage-4-checklist.md) step 7 (the 1.0.0 block): listed, repository signature, DLLs identical to the attested artifact, snupkg served, attestation verified, fresh net10.0 and net48 consoles print the README's first example identically. Docs PR #40 merged (dd08aeb). **The GitHub wiki for 1.0.0 is live** (11 pages, wiki commit 170339b; [the wiki note](notes/2026-10-03-github-wiki.md)).

## In progress
Nothing. PR #39 (`PackageValidationBaselineVersion` 1.0.0) was merged by Mark (bd86cf8); `ci` run 37175151368 on it green on Linux, Windows and macOS, pack step without a warning; a diagnostic pack (`dotnet pack ... -v diag`) shows the validator's baseline path is the nuget.org 1.0.0 nupkg. Docs PR `docs/stage-4-done` ticks Stage 4 in the plan and checklist step 7. **Mark picked Stage 5, Unity and OpenUPM** (2026-10-03): scratch Unity 6 project installed by git URL, golden tests in Mono and IL2CPP, WebGL size delta, OpenUPM submission prepared for Mark to open. Not started. On 2026-10-03 no installed editor had the IL2CPP module and only 6000.5.8f1 had WebGL; installing modules waits for Mark's go-ahead.

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
- Right after a push, `dotnet add package X --version N` can fail with NU1102 although the flat container lists N: NuGet's HTTP cache holds the old index. Use an empty `NUGET_HTTP_CACHE_PATH` (and `NUGET_PACKAGES`) for fresh-consumer checks.
- Package validation is silent at the CI log's verbosity when it passes. To prove it ran against the baseline, pack locally with `-v diag` and look for `_packageValidationBaselinePath`.
- Auto mode's permission classifier refused the tag push even with Mark's go-ahead; it ran once Mark switched the session to manual mode and approved the prompt.

## Left / follow-ups
1. Done: PR #39 merged, Stage 4 done.
2. Corrections the wiki run found ([the wiki note](notes/2026-10-03-github-wiki.md), "Inaccuracies"): the README's Unity install line names OpenUPM before the listing exists; about 30 XML doc summaries cite internal plan IDs ("(plan D17)") that ship in IntelliSense; `ToJson` docs say "This galaxyCluster as JSON"; `kb/rules/plausibility-rules.md` lists the conservative habitable zone while 1.0.0 uses sqrt(L/1.776) to sqrt(L/0.32), and its "Proto" column describes the Stage 1 prototype. The kb fix needs no release; the README and XML docs ship with the next release.
3. Picked: Stage 5 (Unity, OpenUPM; the throwaway-project recipe above covers the compile check). Still open for later: the real stars research pass (`notes/2026-10-03-real-stars-track.md`), the doc corrections of item 2, optional narrowing of the Trusted Publishing scope to "push only new package versions" (checklist step 3). Start none unless Mark asks. Stage 7 adds the Unity and OpenUPM wiki pages (update mode; the wiki note has the procedure).
4. Optional for Mark: turn on "Automatically delete head branches" so stacked pull requests retarget themselves.

## Next single action
Start Stage 5: list the Unity editors and modules, ask Mark before installing IL2CPP or WebGL, then the scratch project with the package installed by git URL at `v1.0.0`. The prompt is [next-session-prompt.md](next-session-prompt.md).
