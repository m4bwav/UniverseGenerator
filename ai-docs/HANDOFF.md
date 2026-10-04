# Handoff

## Current state
2026-10-04 (overnight, US Central). Stage 4 done (1.0.0 on nuget.org, verified; wiki live; package validation against 1.0.0, PR #39). **Stage 5 under way** on Mark's go-ahead to work through it overnight. 1.0.0 from the git tag `v1.0.0` compiles in Unity 6000.6.4f1 and 6000.5.8f1 with no warnings, and the Galaxy printer sample runs. The repository's tests pass in Unity Mono (266 of 266 on both editors, the 18 golden checks among them), and all 18 golden checks pass in an IL2CPP WebGL player in Chrome. The package adds 116.0 KB to a WebGL build, which is green. Details: [the Stage 5 Unity checks](notes/2026-10-03-stage-5-unity-checks.md).

## In progress
- Merged by Mark: **PR #44** (`tests/Unity/` harness and the results note, 480ba5e) and **PR #45** (README Unity line by git URL pinned to `#v1.0.0`, d5560a0). The scratch projects live outside the repository, next to the clone (`ug-unity-scratch/`).
- Merged by Mark on 2026-10-04: **PR #46** (268bc70, `LICENSE.md` in the package folder, because OpenUPM's tarball is `npm pack` of that folder; it ships with the next release, so the 1.0.0 tarball still has no LICENSE) and **PR #47** (21780c3, `--local` renamed to `--working-tree`; the history scan read the old flag's attribute name as a host name).
- **OpenUPM: Mark filled in the form on 2026-10-04** (cover image `docs/images/openupm-cover.png`, PR #50, merged; topic Procedural Generation), and the form's "Submit metadata" opened GitHub's new-file page. On Mark's instruction the agent finished it from his account: fork m4bwav/openupm, branch `add-com.m4bwav.universe-generator`, **openupm/openupm PR #7044** (https://github.com/openupm/openupm/pull/7044), the form's YAML unchanged; data validation passed, and Mergify merged it at 17:34 UTC. **1.0.0 is on the OpenUPM registry** (built from the tag, published tarball identical to the tag apart from OpenUPM's own package.json fields; details in the submission note). Form values in [the submission note](notes/2026-10-04-openupm-submission.md): branch master, the package.json path, MIT License, hunter m4bwav, tracking git, topic procedural-generation, min version 1.0.0.
- Not run: a Windows x64 IL2CPP player (no `windows-il2cpp` module and no C++ build tools, re-checked 2026-10-04 afternoon: the Windows player has only Mono variations and Visual Studio is absent; both need an elevated install, which Mark approves or clicks) and the ARM64 fused multiply-add check (no device).

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
- Unity ships a custom NUnit 3.5: the copied tests need the rewrites listed in `tests/Unity/make_project.py`. An all-platform test asmdef runs only with `-testPlatform PlayMode` (EditMode finds 0 tests). A player holding NUnit code needs `BuildOptions.IncludeTestAssemblies`, or the IL2CPP linker fails to resolve `nunit.framework`.
- Unity editors live under Program Files, so `unity install-modules` needs elevation: a UAC prompt that blocks an unattended session.
- Auto mode's permission classifier refused the tag push even with Mark's go-ahead; it ran once Mark switched the session to manual mode and approved the prompt.

## Left / follow-ups
1. Done: PR #39 merged, Stage 4 done.
2. Corrections the wiki run found (the README line was fixed by PR #45) ([the wiki note](notes/2026-10-03-github-wiki.md), "Inaccuracies"): the README's Unity install line names OpenUPM before the listing exists; about 30 XML doc summaries cite internal plan IDs ("(plan D17)") that ship in IntelliSense; `ToJson` docs say "This galaxyCluster as JSON"; `kb/rules/plausibility-rules.md` lists the conservative habitable zone while 1.0.0 uses sqrt(L/1.776) to sqrt(L/0.32), and its "Proto" column describes the Stage 1 prototype. The kb fix needs no release; the README and XML docs ship with the next release.
3. In progress: Stage 5 (see In progress; `tests/Unity/README.md` replaces the throwaway-project recipe above). Still open for later: the real stars research pass (`notes/2026-10-03-real-stars-track.md`), the doc corrections of item 2, optional narrowing of the Trusted Publishing scope to "push only new package versions" (checklist step 3). Start none unless Mark asks. Stage 7 adds the Unity and OpenUPM wiki pages (update mode; the wiki note has the procedure).
4. Optional for Mark: turn on "Automatically delete head branches" so stacked pull requests retarget themselves.

## Next single action
Mark: decide on the Windows IL2CPP module with C++ build tools (elevated install) and an ARM64 device. The next session installs from the OpenUPM registry in a scratch project and switches the README, then does the 1.0.1 doc fixes and the Stage 7 wiki pages; the prompt lists everything left in the 1.0 project. The prompt is [next-session-prompt.md](next-session-prompt.md).
