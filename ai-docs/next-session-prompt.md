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

Continue the UniverseGenerator run (C#; the F# port is deferred). This is Stage 4, the 1.0.0 release; 1.0.0-beta.1 is on nuget.org and verified.

Every N1 row is built (Mark's ruling of 2026-10-03: every unbuilt or partial 1.0 matrix row goes in 1.0.0). On `master`: #17 to #25 (carried by #28) and #29 `constraints`. Waiting for Mark, in order:

- #30, hooks and custom fields (base `master`);
- #31, editable tables (base #30);
- #32, the Unity float adapter (base #31).

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`, including "Dead ends hit": stacked pull requests must be retargeted;
- `AGENTS.md`;
- the plan's Stage 4 lines (`ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md`);
- `ai-docs/notes/2026-10-03-stage-4-checklist.md`, step 7 only, when you reach the verification.

Open the Runtime folder and the tests only if a step needs them. Re-check any nuget.org or GitHub fact older than three months against the official docs before using it (AGENTS.md, "Research beats recall").

1. Check the stack: `gh pr list -R m4bwav/UniverseGenerator --state all --limit 8`. For each of #30 to #32, check whether it merged into `master` or into its old base branch: `git merge-base --is-ancestor <head> origin/master`.
   - If one landed in an old branch, open a carry pull request from that branch to `master`, as #28 did.
   - If any is still open, its base has merged and the base branch still exists, retarget it with `gh pr edit N --base master`. Then stop and tell Mark what waits.
   - Do not merge a code pull request yourself.
2. When all three are on `master` and `ci` is green there, open the 1.0.0 release pull request (branch `release/1.0.0`):
   - CHANGELOG: turn "Unreleased" into `## [1.0.0] - <date>`, with the size numbers from the size gate on `master`'s latest `ci` run (quote that run).
   - Set `<Version>` in `src/UniverseGenerator/UniverseGenerator.csproj` and `version` in the package's `package.json` to 1.0.0; a repo test checks they match.
   - Fix the README status line ("1.0.0-beta.1 in review, not released yet" is stale), and AGENTS.md's "What it is" line if it names beta.1 as the latest.
   - Assign m4bwav, label needs-review.
3. Then stop: merging that pull request, tagging `v1.0.0` and approving the `nuget` deployment wait for Mark.
4. After Mark tags and approves:
   - Verify 1.0.0 from nuget.org exactly as checklist step 7 did for beta.1: the flat container, registration, `dotnet nuget verify`, the snupkg, the GitHub Release, `gh attestation verify` on the run's artifact, and fresh net10.0 and net48 consoles. Use `dotnet add package` with `--version 1.0.0` alone.
   - Then a pull request that sets `<PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>` and replaces the csproj comment that says there is no baseline yet.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change. The release changes no generated output.
- Tests stay green on net10.0 and net48, and CI stays green on every operating system.
- The size gate stays green: warn Mark at yellow, never cross red.
- Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets, repository settings and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file; the repository is public.
  - Run `scripts/history-scan.py` before each commit and read its last line.
  - A note that quotes a scan pattern trips the scan, so describe the pattern in words.

Stop and ask at once if any of these happens:

- CI fails on any operating system;
- a golden file would change;
- a size metric moves into yellow;
- a release run fails, or Trusted Publishing is refused;
- a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do.

After 1.0.0 comes Stage 5 (Unity and OpenUPM; HANDOFF has the throwaway-project recipe for Unity compile checks). The real stars track (`ai-docs/notes/2026-10-03-real-stars-track.md`) is also after 1.0.0. Start neither unless Mark asks.

When you reach a step that waits for Mark, update these and stop for him:

- the handoff;
- the log;
- the plan (Stage 4, next action);
- the Stop 2 note's N1 progress table, if a merge changed it;
- the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
