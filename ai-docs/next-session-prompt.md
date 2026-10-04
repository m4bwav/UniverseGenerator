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

Continue the UniverseGenerator run (C#; the F# port is deferred). This is the end of Stage 4: the 1.0.0 release. 1.0.0-beta.1 is on nuget.org and verified. Mark merged the release pull request #34 on 2026-10-03 (merge commit 3505547; `ci` run 37172716089 green on every OS, size gate all green). The tag `v1.0.0` and the approval of the `nuget` deployment wait for Mark.

Mark wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`;
- `ai-docs/notes/2026-10-03-stage-4-checklist.md`, step 7 only (how beta.1 was verified, including the `dotnet add package` flag quirk).

Open the Runtime folder and the tests only if a step needs them. Re-check any nuget.org or GitHub fact older than three months against the official docs before using it (AGENTS.md, "Research beats recall").

1. Check where the release stands:
   - `git ls-remote --tags origin v1.0.0`, and that it points at 3505547 (or at a later `master` commit whose `ci` passed, if a pull request re-dated the CHANGELOG);
   - `gh run list -R m4bwav/UniverseGenerator --workflow release.yml --limit 3`, and whether its `nuget` job ran or still waits for approval.
   - If there is no tag, or the deployment still waits: do nothing that needs Mark. Tell him which of tag and approval waits, and stop. Do not tag or approve yourself unless he says so in your session.
   - If Mark asks you to tag on a later local day than 2026-10-03, first open a small pull request re-dating `## [1.0.0] - 2026-10-03` in CHANGELOG.md (`release.yml` only checks that a date is there, but the release ritual wants the tag day); tag its merge after `ci` is green.
   - If the release run failed, or Trusted Publishing was refused, stop and tell Mark at once with the failing job's log lines.
2. When the release run has pushed 1.0.0, verify it from nuget.org exactly as checklist step 7 did for beta.1, and record each result in step 7's table (a new 1.0.0 block, the beta.1 rows stay):
   - the flat container lists `1.0.0`; registration says listed (`curl --compressed`; it can lag 25 minutes or more);
   - `dotnet nuget verify --all` on the downloaded nupkg; its contents;
   - the snupkg on the symbol server;
   - `gh release view v1.0.0` (a full release now, not a prerelease; notes; nupkg and snupkg);
   - `gh attestation verify` on the run's `release` artifact;
   - fresh net10.0 and net48 consoles in a scratch folder outside the repository, with a nuget.org-only `nuget.config` and an empty package cache, using `dotnet add package UniverseGenerator --version 1.0.0` alone; both print the README's first example identically.
3. Then a pull request (branch `chore/package-validation-baseline`) that sets `<PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>` in `src/UniverseGenerator/UniverseGenerator.csproj` and replaces the comment that says there is no baseline yet. Build and pack locally first to see that package validation passes against 1.0.0. Assign m4bwav, label needs-review. The checklist step 7 update can go in a separate docs-only pull request you merge yourself once its checks pass.

Rules:

- Commit per step. Every pull request goes to Mark (assign m4bwav, label needs-review); you may merge a docs-only pull request yourself once its checks pass.
- `tests/Golden/v1/` must not change.
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
- package validation against the 1.0.0 baseline reports a break;
- a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do.

After 1.0.0 comes Stage 5 (Unity and OpenUPM; HANDOFF has the throwaway-project recipe for Unity compile checks). The real stars track (`ai-docs/notes/2026-10-03-real-stars-track.md`) is also after 1.0.0. Start neither unless Mark asks.

When you reach a step that waits for Mark, update these and stop for him:

- the handoff;
- the log;
- the plan (Stage 4, next action; tick the 1.0.0 line once verified);
- the checklist step 7;
- the index (`everlast.py index`).

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, and when to stop and ask. Its last paragraph is this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
