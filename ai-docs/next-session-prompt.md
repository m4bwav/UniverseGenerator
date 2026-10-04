---
title: "Next session prompt"
kind: note
status: active
date: 2026-10-03
stale_after: never
tags: [universegenerator, handoff, session-prompt, stage-5, unity, openupm]
summary: "paste into a fresh session to continue the run; every session rewrites it before stopping (AGENTS.md), ending with the same instruction"
---

# Next session prompt

Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4 is done (2026-10-03): 1.0.0 is on nuget.org (tag `v1.0.0` on 3505547) and verified, the 1.0.0 wiki is live, and every 1.x pack is validated against the published 1.0.0. **Mark picked Stage 5, Unity and OpenUPM (2026-10-03):** a throwaway Unity 6 project, the golden tests in Mono and IL2CPP, the WebGL size check, then the OpenUPM submission prepared for Mark to open. The game's switch (the third Stage 5 line, Stop 3) is not part of this; it waits for OpenUPM to list 1.0.0.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md` (its "Dead ends hit" has the Unity compile-check recipe and the Mono float note);
- `AGENTS.md`;
- the plan, `ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md`: "Stage 5", "Verification checklist" and "Risks and open points";
- `ai-docs/notes/2026-10-02-package-size-budget.md`, the budget table (WebGL row: green under 300 KB added, yellow 0.3 to 1 MB, red over 1 MB; Brotli, stripping High, default galaxy);
- `kb/rules/determinism.md` before reading any Unity mismatch.

Load the `unity-agent-cli` skill (the Unity CLI is installed) or `unity-agent-headless` for batchmode work. Open other files only if a step needs them. Re-check every OpenUPM and Unity fact against the official docs before using it (openupm.com/docs, docs.unity3d.com): the submission form and its requirements, how OpenUPM finds a package in a subfolder, which tags it builds, and the IL2CPP and WebGL build options (AGENTS.md, "Research beats recall").

What this machine had on 2026-10-03 (check again first): Unity editors 6000.5.8f1, 6000.6.0f1 and 6000.6.4f1; Windows build support (Mono) on all three, **IL2CPP on none**, WebGL only on 6000.5.8f1; no ARM64 machine to run a player on. The package declares `"unity": "6000.0"`.

1. **Modules.** List the installed editors and modules. Installing a module (Windows IL2CPP, WebGL for the pinned editor) is a multi-gigabyte download: ask Mark before installing anything, say which editor and modules and the size. For the ARM64 fused multiply-add check (plan Risks), ask Mark which ARM64 device or machine to use; if there is none, record it as open and go on.
2. **Throwaway project and install by git URL.** Create a scratch Unity 6 project outside the repository. Install the package by git URL from the public repository at tag `v1.0.0` (`?path=` to the package folder), not by a file path, so it tests what users will get. Confirm both assemblies compile with no `error CS` and no warning from the package, import the Galaxy printer sample and run it (batchmode `-executeMethod` is enough), and check every package file has its `.meta` (Unity ignores `Samples~`; note whether it needs metas).
3. **Golden tests in Mono.** EditMode tests in the scratch project that generate the golden seeds and compare them as text with `tests/Golden/v1/` (read the files, never rewrite them). Keep reusable parts in the repository (for example a `tests/Unity/` folder with the test scripts and a script that builds the scratch project from a tag), with no local path; the scratch project itself is never committed. A pull request for Mark.
4. **Golden tests in IL2CPP.** A Windows x64 IL2CPP player (after Mark's go-ahead on the module) that writes the same outputs, compared with the golden files. ARM64 as step 1 settled.
5. **WebGL size delta.** Two WebGL builds of the same minimal scene, with and without the package and a default galaxy generated at start; Brotli, stripping High, "optimize for code size". Record both sizes and the difference against the budget row; add the number to the size budget note and the CHANGELOG's next unreleased section.
6. **OpenUPM submission, prepared.** Check the repository against OpenUPM's current requirements (package.json fields, license, the `v1.0.0` tag holding package.json 1.0.0, the package path), write the exact submission values in a note, and fix what is missing in a pull request. Mark opens the submission himself; the agent never submits. After he does, the next session checks the package page and the build log.
7. Fix the README's Unity install line (it names OpenUPM before the listing exists; HANDOFF "Left / follow-ups" item 2) in the same pull request as the git-URL install instructions, so the README is true before and after the listing.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change.
- Tests stay green on net10.0 and net48, and CI stays green on every operating system.
- The size gate stays green: warn Mark at yellow, never cross red. The WebGL number counts too.
- Package validation against the 1.0.0 baseline stays clean.
- Every file under `Packages/com.m4bwav.universe-generator/` has a committed `.meta`, and a GUID never changes.
- Installing Unity modules or editors, tagging, publishing, the OpenUPM submission, approving a deployment, secrets, visibility, rulesets, repository settings and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file or wiki page; the repository and its wiki are public. Unity logs and build reports carry local paths: summarise them, never commit them.
- Run `scripts/history-scan.py` before each commit and read its last line.
- A note that quotes a scan pattern trips the scan, so describe the pattern in words.

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change, or Unity output (Mono or IL2CPP) differs from a golden file in any field: record the first differing seed and field, check it against `kb/rules/determinism.md`, and fix nothing in the generator without Mark (a fix that changes output needs a new generator version);
- the package does not compile, or warns, in a Unity 6 editor;
- a size metric, the WebGL delta included, moves into yellow;
- package validation against the 1.0.0 baseline reports a break;
- an OpenUPM or Unity fact differs from the plan in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update these and stop for him: the handoff; the log; the plan (Stage 5, next action); the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
