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

Written 2026-10-03, after the scan re-run (PR #4) and the hosted CI matrix (PR #5), both waiting for Mark. Use it
once Mark has merged them or made the repository public.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: stage4-prep is merged, the
history scan was re-run clean on the final master (PR #4), RUNS_ON is "ubuntu-latest" and ci passed on hosted
runners, and PR #5 turns ci.yml's build job into the hosted Ubuntu, Windows and macOS matrix. In the
UniverseGenerator clone, read ai-docs/HANDOFF.md, then AGENTS.md, then ai-docs/notes/2026-10-03-stage-4-checklist.md
in full. Nothing else unless a step needs it. Re-check any nuget.org or GitHub fact older than three months against
the official docs before using it (AGENTS.md, "Research beats recall"); in particular check which repository
settings and rulesets the free plan allows on a public repository before applying them.

First find out how far Mark got, and say so: are PR #4 and PR #5 merged (gh pr list --state all), is the repository
public, is the self-hosted runner universe still registered (gh api repos/m4bwav/UniverseGenerator/actions/runners),
does the nuget environment exist and does it have NUGET_USER, is there a v* tag. Then, in this order, only as far as
Mark's steps before each are done:
1. After PR #5 is merged: delete the Actions variable RUNS_ON (nothing reads it any more) and check that ci on
   master passed on all three operating systems.
2. Once Mark has made the repository public (never before): secret scanning and push protection, private
   vulnerability reporting, workflow permissions read (default GITHUB_TOKEN read, Actions may not approve pull
   requests), the master ruleset (deletion and non-fast-forward blocked, required status check ci, admin bypass;
   the package-modernize templates have no file for it, so write it from the checklist) and the admins-only v* tag
   ruleset from package-modernize templates/rulesets/tags-admins-only.json. Read each setting back with gh api and
   record what is on in the checklist note. The nuget environment only if Mark asks you to create it.
3. Once Mark has the Trusted Publishing policy and the nuget environment (checklist steps 3 and 4): the release pull
   request, dating the CHANGELOG heading for 1.0.0-beta.1 and checking its size numbers against the size gate's
   output in that pull request's ci run (correct them if they moved).
4. After Mark has tagged and approved: verify on nuget.org as the checklist's step 7 says, and record it.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge
yourself once its checks pass). No Runtime change is expected: tests/Golden/v1/ must not change (git status shows
nothing there), the tests stay green on net10.0 and net48, CI stays green on every operating system, and the size
gate stays green (warn me at yellow). Never tag, publish, approve a deployment, make the repository public, remove
the runner or merge a non-docs pull request: those wait for Mark. No local path, LAN address or private name in any
committed file: run scripts/history-scan.py before each commit and read its last line before committing (a note
that quotes a scan pattern trips it; describe the pattern in words).

Stop and ask at once if ci fails on any operating system in a way that needs a Runtime or golden change, if a
repository setting or ruleset is refused or behaves differently from the checklist, if the release run fails, or if
a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do. When you reach a
step that waits for Mark, update the handoff, log, plan (Stage 4 checklist, next action), the checklist note and the
index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
