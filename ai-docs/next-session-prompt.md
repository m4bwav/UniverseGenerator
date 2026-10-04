---
title: "Next session prompt"
kind: note
status: active
date: 2026-10-04
stale_after: never
tags: [universegenerator, handoff, session-prompt, stage-5, stage-6, stage-7, unity, openupm]
summary: "paste into a fresh session to continue the run; every session rewrites it before stopping (AGENTS.md), ending with the same instruction"
---

# Next session prompt

Continue the UniverseGenerator run (C#; the F# port is deferred). **Stage 5 (Unity and OpenUPM) is nearly done; Stages 6 and 7 and the 1.0.1 doc fixes are what remain of the 1.0 project.** 1.0.0 is on nuget.org. From the git tag it compiles in Unity 6000.6.4f1 and 6000.5.8f1 with no warnings, passes 266 of 266 tests in Unity Mono and all 18 golden checks in an IL2CPP WebGL player, and adds 116.0 KB to a WebGL build (green). The harness is `tests/Unity/`. Mark filled in the OpenUPM form on 2026-10-04 (cover image `docs/images/openupm-cover.png`, PR #50; topic Procedural Generation), and on his instruction the last session opened openupm/openupm PR #7044 from his account with the form's YAML; Mergify merged it at 17:34 UTC on 2026-10-04.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`;
- `ai-docs/notes/2026-10-04-openupm-submission.md`;
- `tests/Unity/README.md` and `ai-docs/notes/2026-10-03-stage-5-unity-checks.md`, before any Unity run;
- `ai-docs/notes/2026-10-03-github-wiki.md`, before step 3 or any wiki change.

Open other files only if a step needs them. Re-check any OpenUPM, Unity, nuget.org or GitHub fact older than three months against the official docs (AGENTS.md, "Research beats recall").

What is left, in order. Do each step whose condition holds; skip the rest and say why.

1. **State.** `git pull` on `master` and check `ci` is green there. Find Mark's OpenUPM pull request (`gh pr list -R openupm/openupm --state all --search "com.m4bwav.universe-generator"`; also `--author m4bwav`), and look at `https://package.openupm.com/com.m4bwav.universe-generator` and `https://openupm.com/packages/com.m4bwav.universe-generator/`. No package an hour after the merge: read the build log on the package page and stop for Mark (never resubmit without him).
2. **Once OpenUPM lists 1.0.0:** read the build log for errors and check the published version list (1.0.0 only, given min version 1.0.0). Install from the registry in a fresh scratch project (scoped registry `https://package.openupm.com`, scope `com.m4bwav.universe-generator`): add a registry source to `make_project.py`, then run the sample and the PlayMode tests as `tests/Unity/README.md` describes. Then switch the README's Unity line to OpenUPM, with the git URL as the alternative. That ships in the nupkg, so it is a pull request for Mark. Tick the plan's Stage 5 OpenUPM line.
3. **The 1.0.1 doc fixes** (the wiki note's "Inaccuracies", items 2 to 4; item 1 is step 2): remove the internal plan IDs ("(plan D17)" and the like) from the XML doc summaries, reword the `ToJson` summaries, and correct the habitable-zone row and the "Proto" column in `kb/rules/plausibility-rules.md`. Comments and docs only: no generated value may change, so the golden files and the public API stay as they are. Add a 1.0.1 CHANGELOG section without a date (LICENSE.md in the UPM folder from PR #46 belongs in it too, and the README line from step 2 once merged). One pull request for Mark. The release itself (version, date, tag) is Mark's call; ask him, never tag.
4. **Stage 7, the wiki:** once OpenUPM lists 1.0.0, add the Unity page (install from OpenUPM and by git URL, `UnityVectors`, `TablesAsset`, the IL2CPP and WebGL results) and the OpenUPM install lines, in update mode by the wiki note's procedure, every example checked against the published package. Then the rest of Stage 7: the package inventory row, skill lessons (package-modernize and wikiwright `LEARNINGS.md`, through their own repositories) and the launch list.
5. **Windows IL2CPP and ARM64,** only if Mark has installed the `windows-il2cpp` module and Visual Studio's C++ build tools (the editor's Windows player folder lists `il2cpp` variations beside the `mono` ones, and `vswhere` finds the C++ tools), or named an ARM64 device. Build `UGBuild.WindowsIL2CPP` from a tests project, run the exe with `-logFile`, and record the `UG-GOLDEN` lines in the Stage 5 note. Never install a module or a toolchain yourself.
6. **Only when Mark asks:** the game's switch (plan Stage 5, Stop 3; D10), in SpaceDeckBuilder2 on a branch, after OpenUPM lists 1.0.0; Stage 6, the lab page and the star name tool on markdavidrogers-web (Stop 4; read that repository's AGENTS.md and HANDOFF first); the real stars track (`ai-docs/notes/2026-10-03-real-stars-track.md`, research pass first, then stop); narrowing the Trusted Publishing scope.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request (ai-docs, notes, images) yourself once its checks pass. README, CHANGELOG, XML docs in source and kb rule files ship or count as code here: they go to Mark.
- `tests/Golden/v1/` must not change.
- Tests stay green on net10.0 and net48, CI stays green on every operating system, and the Unity tests stay at failed="0".
- The size gate stays green, the WebGL delta included: warn Mark at yellow, never cross red.
- Package validation against the 1.0.0 baseline stays clean; step 3 must not change the public API.
- Every file under `Packages/com.m4bwav.universe-generator/` has a committed `.meta`, and a GUID never changes.
- Installing Unity modules, editors or toolchains, tagging, publishing, the OpenUPM submission, approving a deployment, secrets, visibility, rulesets, repository settings and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file or wiki page; the repository and its wiki are public. Unity logs carry local paths: summarise them, never commit them. Scratch projects stay outside the repository.
- Run `scripts/history-scan.py` before each commit and read its last line ("current: 0 distinct finding(s)"). A note that quotes a scan pattern trips the scan, so describe the pattern in words.
- A wiki change follows the update procedure in `ai-docs/notes/2026-10-03-github-wiki.md`.

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change, or Unity output (Mono or IL2CPP) differs from a golden file;
- the OpenUPM build fails, or the published package differs from the tag;
- a size metric, the WebGL delta included, moves into yellow;
- package validation against the 1.0.0 baseline reports a break;
- an OpenUPM, Unity, nuget.org or GitHub fact differs from the notes in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update these and stop for him: the handoff; the log; the plan (Stage 5, next action); the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
