---
title: "Next session prompt"
kind: note
status: active
date: 2026-10-04
stale_after: never
tags: [universegenerator, handoff, session-prompt, stage-5, unity, openupm]
summary: "paste into a fresh session to continue the run; every session rewrites it before stopping (AGENTS.md), ending with the same instruction"
---

# Next session prompt

Continue the UniverseGenerator run (C#; the F# port is deferred). **Stage 5 (Unity and OpenUPM) is under way.** 1.0.0 from the git tag passes every Unity check run so far: it compiles in 6000.6.4f1 and 6000.5.8f1 with no warnings; 266 of 266 tests pass in Unity Mono; all 18 golden checks pass in an IL2CPP WebGL player; and the package adds 116.0 KB to a WebGL build, which is green. The harness is `tests/Unity/` (PR #44, merged). The OpenUPM form values are ready for Mark. PR #46 (LICENSE inside the package folder) and PR #47 (the harness flag rename) waited for Mark.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`;
- `ai-docs/notes/2026-10-04-openupm-submission.md`;
- `tests/Unity/README.md` and `ai-docs/notes/2026-10-03-stage-5-unity-checks.md`, before any Unity run.

Open other files only if a step needs them. Re-check any OpenUPM, Unity, nuget.org or GitHub fact older than three months against the official docs (AGENTS.md, "Research beats recall").

1. **State.** `gh pr view 46` and `gh pr view 47` (`-R m4bwav/UniverseGenerator --json state,mergeCommit`). Has Mark submitted to OpenUPM? Look for his pull request (`gh pr list -R openupm/openupm --author m4bwav --state all`) and the package (`https://package.openupm.com/com.m4bwav.universe-generator`, and the page `https://openupm.com/packages/com.m4bwav.universe-generator/`). Nothing submitted: tell Mark it waits for him, with the form values, and do only the steps below that need nothing from him.
2. **Once OpenUPM lists 1.0.0:** read the build log for errors and check the published version list. Then install from the registry in a fresh scratch project (scoped registry `https://package.openupm.com`, scope `com.m4bwav.universe-generator`). Add a registry source to `make_project.py` for this, then run the sample and the PlayMode tests as `tests/Unity/README.md` describes. Then update the README's Unity line to OpenUPM, with the git URL as the alternative. That change ships in the nupkg, so it is a pull request for Mark. Update the plan's Stage 5 lines and the size budget note if anything moved.
3. **Windows IL2CPP and ARM64,** only if Mark has installed the `windows-il2cpp` module and Visual Studio's C++ build tools, or named an ARM64 device. Build `UGBuild.WindowsIL2CPP` from a tests project, run the exe with `-logFile`, and record the `UG-GOLDEN` lines in the Stage 5 note. Never install a module or a toolchain yourself: the editors live under Program Files, and an elevated install is Mark's.
4. **The game's switch** (plan Stage 5, third line, Stop 3; D10): only when Mark asks, and only after OpenUPM lists 1.0.0. It happens in SpaceDeckBuilder2, on a branch.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change.
- Tests stay green on net10.0 and net48, CI stays green on every operating system, and the Unity tests stay at failed="0".
- The size gate stays green, the WebGL delta included: warn Mark at yellow, never cross red.
- Package validation against the 1.0.0 baseline stays clean.
- Every file under `Packages/com.m4bwav.universe-generator/` has a committed `.meta`, and a GUID never changes.
- Installing Unity modules, editors or toolchains, tagging, publishing, the OpenUPM submission, approving a deployment, secrets, visibility, rulesets, repository settings and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file or wiki page; the repository and its wiki are public. Unity logs carry local paths: summarise them, never commit them. Scratch projects stay outside the repository.
- Run `scripts/history-scan.py` before each commit and read its last line ("current: 0 distinct finding(s)"). A note that quotes a scan pattern trips the scan, so describe the pattern in words.
- A wiki change follows the update procedure in `ai-docs/notes/2026-10-03-github-wiki.md`. The Unity and OpenUPM wiki pages are Stage 7.

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change, or Unity output (Mono or IL2CPP) differs from a golden file;
- the OpenUPM build fails, or the published package differs from the tag;
- a size metric, the WebGL delta included, moves into yellow;
- package validation against the 1.0.0 baseline reports a break;
- an OpenUPM, Unity, nuget.org or GitHub fact differs from the notes in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update these and stop for him: the handoff; the log; the plan (Stage 5, next action); the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
