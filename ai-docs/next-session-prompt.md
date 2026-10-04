---
title: "Next session prompt"
kind: note
status: active
date: 2026-10-03
stale_after: never
tags: [universegenerator, handoff, session-prompt]
summary: "paste into a fresh session to continue the run; every session rewrites it before stopping (AGENTS.md), ending with the same instruction"
---

# Next session prompt

Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4 is done (2026-10-03): 1.0.0 is on nuget.org (tag `v1.0.0` on 3505547) and verified, the 1.0.0 wiki is live (wiki commit 170339b), and every 1.x pack is validated against the published 1.0.0 (PR #39, bd86cf8). The next track is Mark's pick.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`.

Open other files only if the chosen track needs them. Re-check any nuget.org, OpenUPM, Unity or GitHub fact older than three months against the official docs before using it (AGENTS.md, "Research beats recall").

1. If Mark named a track when he started this session, do that one. If not, ask him which of these comes next, give each a size in agent time, and start none of them until he picks:
   - **Stage 5: Unity and OpenUPM** (plan, Stage 5; HANDOFF "Dead ends hit" has the throwaway-project recipe for Unity compile checks). The first step is the scratch Unity 6 project: install by git URL, run the sample, EditMode golden tests in Mono, an IL2CPP player, the WebGL size delta. OpenUPM builds from tags, and Mark opens the submission himself;
   - **the real stars track** (`ai-docs/notes/2026-10-03-real-stars-track.md`): research first, into `kb/`; no code until Mark rules on the note's questions;
   - **the four doc corrections the wiki run found** (`ai-docs/notes/2026-10-03-github-wiki.md`, "Inaccuracies"). The `kb/` one is docs-only and needs no release. The README and XML-doc ones change the source and ship with the next release, so they go in a pull request for Mark. After they land, update the wiki pages that quote them (the wiki note's update procedure);
   - **narrowing the Trusted Publishing scope** to "push only new package versions" (checklist step 3): Mark's click on nuget.org. The agent only reads the policy back afterwards.
2. Work the chosen track in pull requests, one per step, until it reaches a step that waits for Mark.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change.
- Tests stay green on net10.0 and net48, and CI stays green on every operating system.
- The size gate stays green: warn Mark at yellow, never cross red.
- Package validation against the 1.0.0 baseline stays clean: a break means a new major version, so stop and ask.
- Every file under `Packages/com.m4bwav.universe-generator/` has a committed `.meta`, and a GUID never changes.
- Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets, repository settings, the OpenUPM submission and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file or wiki page; the repository and its wiki are public.
- Run `scripts/history-scan.py` before each commit and read its last line.
- A note that quotes a scan pattern trips the scan, so describe the pattern in words.
- A wiki change follows the update procedure in `ai-docs/notes/2026-10-03-github-wiki.md` (re-run the verification program, the `outputs`, `snippets`, `check` and `live` gates).

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change;
- a size metric moves into yellow;
- package validation against the 1.0.0 baseline reports a break;
- Unity output differs from the golden files (Mono or IL2CPP);
- a nuget.org, OpenUPM, Unity or GitHub fact differs from the plan or checklist in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update these and stop for him: the handoff; the log; the plan (the stage Mark picked, next action); the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
