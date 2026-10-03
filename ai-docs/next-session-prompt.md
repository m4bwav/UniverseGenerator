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

Written 2026-10-03, after the repository went public and the public-repository settings and rulesets were applied
and read back. Use it once Mark has added the Trusted Publishing policy and the nuget environment, or has gone
further.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: the repository is public,
the self-hosted runner is removed, ci.yml runs the hosted Ubuntu, Windows and macOS matrix (green on master 558c544),
and the public-repository settings are on and read back: secret scanning, push protection, private vulnerability
reporting, workflow token read, ruleset master (deletion and force-push blocked, required check ci, admin bypass)
and ruleset "Tags only by admins". There was no nuget environment, no v* tag and no release run yet. In the
UniverseGenerator clone, read ai-docs/HANDOFF.md, then AGENTS.md, then ai-docs/notes/2026-10-03-stage-4-checklist.md
in full. Nothing else unless a step needs it (the release pull request needs CHANGELOG.md and the size gate's
output). Re-check any nuget.org or GitHub fact older than three months against the official docs before using it
(AGENTS.md, "Research beats recall").

First find out how far Mark got, and say so: is the nuget environment there with required reviewer m4bwav and the
tag rule v* (gh api repos/m4bwav/UniverseGenerator/environments/nuget and .../deployment-branch-policies), does it
have NUGET_USER (gh secret list --env nuget -R m4bwav/UniverseGenerator), has he said the Trusted Publishing policy
exists (you cannot see it; ask if he has not said), is there a v* tag (git ls-remote --tags origin), and is there a
release run (gh run list --workflow release.yml). Also check the repository is still public and both rulesets are
still active (gh api repos/m4bwav/UniverseGenerator/rulesets). Then, in this order, only as far as Mark's steps
before each are done:
1. Once Mark has the Trusted Publishing policy and the nuget environment (checklist steps 3 and 4; create the
   environment yourself only if he asks, with the commands in the checklist, and never set NUGET_USER for him): the
   release pull request (checklist step 5), dating the CHANGELOG heading for 1.0.0-beta.1 and checking its size
   numbers against the size gate's output in that pull request's ci run (correct them if they moved). It touches
   CHANGELOG.md, so it is not docs-only: it waits for Mark. The master ruleset requires ci green; Mark merges.
2. After Mark has merged it, ci is green on master, and he has tagged v1.0.0-beta.1 and approved the deployment:
   watch the release run, then verify on nuget.org as the checklist's step 7 says, and record every result.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge
yourself once its checks pass). No Runtime change is expected: tests/Golden/v1/ must not change (git status shows
nothing there), the tests stay green on net10.0 and net48, CI stays green on every operating system, and the size
gate stays green (warn me at yellow). Never tag, publish, approve a deployment, change the repository's visibility
or rulesets beyond the checklist, or merge a non-docs pull request: those wait for Mark. No local path, LAN address
or private name in any committed file (the repository is public now): run scripts/history-scan.py before each commit
and read its last line before committing (a note that quotes a scan pattern trips it; describe the pattern in
words).

Stop and ask at once if ci fails on any operating system in a way that needs a Runtime or golden change, if a ruleset
blocks something the checklist expects to work, if the release run fails, or if a nuget.org or GitHub fact differs
from the checklist in a way that changes what Mark has to do. When you reach a step that waits for Mark, update the
handoff, log, plan (Stage 4 checklist, next action), the checklist note and the index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
