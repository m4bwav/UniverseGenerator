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

Written 2026-10-03, after 1.0.0-beta.1 was published to nuget.org and verified (checklist step 7).

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: 1.0.0-beta.1 is on nuget.org,
published by release run 37158022980 through Trusted Publishing and verified (flat container, registration listed,
repository signature, snupkg, prerelease GitHub Release, attestation, net10.0 and net48 consoles printing the README's
first example). Mark has said he wants everything done (no one uses the library yet). The next step is his choice:
Stage 4's 1.0.0 items (N1 first) or Stage 5. If his message does not say which, ask him before doing anything else.
In the UniverseGenerator clone, read ai-docs/HANDOFF.md, then AGENTS.md. Nothing else unless the chosen step needs it.
Re-check any nuget.org, GitHub or Unity fact older than three months against the official docs before using it
(AGENTS.md, "Research beats recall").

If Mark chose 1.0.0 (N1 first): read ai-docs/notes/2026-10-03-stop-2-questions.md (N1 and N2) and the plan's Stage 4.
N1 lists the feature-matrix rows marked 1.0 but not built. The note's recommendation is to move them to 1.x in
kb/features/status.json and regenerate the matrix with python kb/features/build_matrix.py (never edit the matrix by
hand), building only what Mark asks for (export-json and seed-url are the most wanted). Show Mark the list with that
recommendation and do what he rules. Any feature you build is additive: a new field adds a golden file and never
changes one. Then prepare the 1.0.0 release pull request (CHANGELOG dated with the size numbers from the size gate on
master, <Version> and package.json 1.0.0) and stop: tagging and approving wait for Mark. After 1.0.0 is published,
verify it as checklist step 7 did, then set PackageValidationBaselineVersion 1.0.0.
If Mark chose Stage 5: read the plan's Stage 5 and kb/rules/determinism.md. Begin with the scratch Unity 6 project
(install by git URL, the sample, EditMode golden tests in Mono, then IL2CPP) in a scratch folder outside the
repository, and prepare the OpenUPM submission for Mark to open. The game's switch (Stop 3) waits for him.
Also open the small AGENTS.md pull request if no one has: "What this is" still says nothing is released and the
repository is private.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge yourself
once its checks pass). tests/Golden/v1/ must not change (git status shows nothing there) unless Mark rules a seed
change; the tests stay green on net10.0 and net48, CI stays green on every operating system, and the size gate stays
green (warn Mark at yellow, never cross red). Tagging, publishing, approving a deployment, unlisting, secrets,
visibility, rulesets and merging a non-docs pull request need Mark's explicit go-ahead in this session. No local path,
LAN address or private name in any committed file (the repository is public): run scripts/history-scan.py before
each commit and read its last line before committing (a note that quotes a scan pattern trips it; describe the
pattern in words). Use dotnet add package with --prerelease or --version, never both (the CLI refuses the pair).

Stop and ask at once if ci fails on any operating system in a way that needs a Runtime or golden change, if a step
would change any existing seed's output, if a change moves a size metric into yellow, if a release run fails or
Trusted Publishing is refused, or if a nuget.org, GitHub or Unity fact differs from the plan in a way that changes
what Mark has to do. When you reach a step that waits for Mark, update the handoff, log, plan (the stage's checklist,
next action), any note the step uses and the index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
```
