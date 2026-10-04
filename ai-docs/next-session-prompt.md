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

Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4 is at its last step. 1.0.0 is on nuget.org, released on 2026-10-03 (tag `v1.0.0` on 3505547, release run 37173982448) and verified from nuget.org (checklist step 7). The GitHub wiki for 1.0.0 is live (11 pages, wiki commit 170339b). Pull request #39 (`chore/package-validation-baseline`: `PackageValidationBaselineVersion` 1.0.0) waits for Mark; `ci` was green on all three OSes.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`.

Open other files only if a step needs them. Re-check any nuget.org or GitHub fact older than three months against the official docs before using it (AGENTS.md, "Research beats recall").

1. Check #39: `gh pr view 39 -R m4bwav/UniverseGenerator --json state,mergeCommit`.
   - Not merged: do nothing that needs Mark. Tell him #39 waits for him, with its URL, and stop. Do not merge it yourself unless he says so in your session.
   - Merged: check that `ci` on the merge commit on `master` is green on every OS (`gh run list -R m4bwav/UniverseGenerator --workflow ci.yml --branch master --limit 3`), and that its pack step ran package validation against 1.0.0 without a warning (the job log). Tick the baseline line in the plan (Stage 4) and in checklist step 7 in a docs-only pull request, which you may merge yourself once its checks pass. Stage 4 is then done.
2. Then ask Mark which comes next, and start none of them unless he picks it:
   - Stage 5: Unity and OpenUPM (HANDOFF has the throwaway-project recipe for Unity compile checks);
   - the real stars track (`ai-docs/notes/2026-10-03-real-stars-track.md`);
   - the four doc corrections the wiki run found (HANDOFF "Left / follow-ups" item 2; `ai-docs/notes/2026-10-03-github-wiki.md`, "Inaccuracies"). The `kb/` one needs no release; the README and XML-doc ones change the source and ship with the next release, so they need a pull request for Mark;
   - narrowing the Trusted Publishing scope to "push only new package versions" (checklist step 3; Mark's click on nuget.org).

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change.
- Tests stay green on net10.0 and net48, and CI stays green on every operating system.
- The size gate stays green: warn Mark at yellow, never cross red.
- Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets, repository settings and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file or wiki page; the repository and its wiki are public.
- Run `scripts/history-scan.py` before each commit and read its last line.
- A note that quotes a scan pattern trips the scan, so describe the pattern in words.
- A wiki change follows the update procedure in `ai-docs/notes/2026-10-03-github-wiki.md` (re-run the verification program, the `outputs`, `snippets`, `check` and `live` gates).

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change;
- a size metric moves into yellow;
- package validation against the 1.0.0 baseline reports a break;
- a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update these and stop for him: the handoff; the log; the plan (Stage 4 or the stage Mark picked, next action); the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
