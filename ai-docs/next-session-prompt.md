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

Written 2026-10-03, after the tag v1.0.0-beta.1 was pushed and the release run's approval was sent (unconfirmed).

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: on Mark's explicit instruction
the agent pushed tag v1.0.0-beta.1 on master c4f4e9e (ci green); release run 37158022980 built, tested (Linux, Windows
net48 and net10.0) and attested, then waited at "push to nuget.org". An approval was sent through the API on Mark's
instruction, but the agent could not watch the run afterwards, so whether the push ran is unconfirmed. Mark has said he
wants everything done (no one uses the library yet). In the UniverseGenerator clone, read ai-docs/HANDOFF.md, then
AGENTS.md, then ai-docs/notes/2026-10-03-stage-4-checklist.md (steps 6 and 7). Nothing else unless a step needs it.
Re-check any nuget.org or GitHub fact older than three months against the official docs before using it (AGENTS.md,
"Research beats recall").

First say what release run 37158022980 did (gh run view 37158022980; gh run view --log-failed if it failed).
1. If it still waits for approval: tell Mark to open the run, Review deployments, approve nuget, and stop.
2. If it failed: read the log, say which step and why, and stop (never re-tag or move a tag yourself).
3. Once it has succeeded: verify on nuget.org as the checklist's step 7 says (flat container, registration listed,
   dotnet nuget verify, snupkg, gh release view, gh attestation verify, fresh net10.0 and net48 console projects
   restoring 1.0.0-beta.1 --prerelease and printing the README's first example), and record every result in the
   checklist, the plan (Stage 4) and the log. Use a scratch folder outside the repository for the console projects.
   Then ask Mark: Stage 4's 1.0.0 items (N1 first, plan "Before 1.0.0") or Stage 5.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge yourself
once its checks pass). No Runtime change is expected: tests/Golden/v1/ must not change (git status shows nothing
there), the tests stay green on net10.0 and net48, CI stays green on every operating system, and the size gate stays
green (warn Mark at yellow). Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets and
merging a non-docs pull request need Mark's explicit go-ahead in this session. No local path, LAN address or
private name in any committed file (the repository is public): run scripts/history-scan.py before each commit and read
its last line before committing (a note that quotes a scan pattern trips it; describe the pattern in words).

Stop and ask at once if ci fails on any operating system in a way that needs a Runtime or golden change, if the
release run failed, if Trusted Publishing was refused, if nuget.org shows something the checklist does not expect, or
if a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do. When you reach a
step that waits for Mark, update the handoff, log, plan (Stage 4 checklist, next action), the checklist note and the
index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
```
