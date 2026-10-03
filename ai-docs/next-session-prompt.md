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

Paste the block below into a fresh session to continue the run. Every session rewrites this file before it stops
(AGENTS.md, "Document for handoff"), so the chain continues: the prompt it writes ends with the same instruction.

Written 2026-10-03, after 1.0.0-beta.1 was verified on nuget.org and Mark chose N1 (towards 1.0.0) before Stage 5.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: 1.0.0-beta.1 is on nuget.org,
published through Trusted Publishing and verified (checklist step 7). Mark chose N1 first, towards 1.0.0; Stage 5
(Unity, OpenUPM) comes after 1.0.0. Mark has said he wants everything done (no one uses the library yet).
In the UniverseGenerator clone, read ai-docs/HANDOFF.md, then AGENTS.md, then ai-docs/notes/2026-10-03-stop-2-questions.md
(N1 and N2), then the plan's Stage 4 (ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md). Open kb/features/status.json
and the Runtime folder only as the step needs. Re-check any nuget.org or GitHub fact older than three months against
the official docs before using it (AGENTS.md, "Research beats recall").

1. N1 lists the feature-matrix rows marked 1.0 that are not built (seed-url, the presets Pocket, Roguelike, Cozy and
   Epic, data-tables-editable, hooks-plugins, typed-units, diagnostics, custom-fields, names-fantasy, export-json,
   the Unity float adapter, lane-density and arm-count options). Check that list against kb/features/status.json, then
   show Mark a table: each row, what building it takes (files, size, effort), and the note's recommendation (move them
   to 1.x; export-json and seed-url are the most wanted, and seed-url would fix N2's "an address carries no options").
   Stop for his ruling on each row. Do not build or move anything before he rules.
2. Rows he moves to 1.x: change kb/features/status.json and regenerate the matrix with python kb/features/build_matrix.py
   (never edit the matrix by hand); one docs pull request.
3. Rows he wants in 1.0.0: one pull request per feature (or a small group), each additive: no existing seed's output
   changes, a new field adds a golden file and never changes one, the README and samples/ConsoleSample/Examples.cs
   updated together when the public API grows (SampleTests check them), CHANGELOG "Unreleased" entry, status.json and
   the matrix updated. Netstandard2.0 and net10.0 with LangVersion 9, no reflection, no engine references.
4. When every N1 row is ruled and merged: the 1.0.0 release pull request (CHANGELOG heading dated with the size numbers
   from the size gate on master, <Version> and package.json 1.0.0), then stop: tagging and approving wait for Mark.
   After he tags and approves, verify 1.0.0 as checklist step 7 did, then set PackageValidationBaselineVersion 1.0.0.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge yourself
once its checks pass). tests/Golden/v1/ must not change (git status shows nothing there), the tests stay green on
net10.0 and net48, CI stays green on every operating system, and the size gate stays green (warn Mark at yellow, never
cross red). Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets and merging a
non-docs pull request need Mark's explicit go-ahead in this session. No local path, LAN address or private name in any
committed file (the repository is public): run scripts/history-scan.py before each commit and read its last line
before committing (a note that quotes a scan pattern trips it; describe the pattern in words). Use dotnet add package
with --prerelease or --version, never both (the CLI refuses the pair).

Stop and ask at once if a feature would change any existing seed's output or need a golden change, if ci fails on any
operating system, if a change moves a size metric into yellow, if a release run fails or Trusted Publishing is
refused, or if a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do. When
you reach a step that waits for Mark, update the handoff, log, plan (Stage 4, next action), the Stop 2 note's N1
section and the index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
```
