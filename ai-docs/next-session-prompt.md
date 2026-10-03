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

Written 2026-10-03, after the release PR #10 was opened (dated CHANGELOG, size numbers corrected) with the
Trusted Publishing policy and the nuget environment in place. Use it once Mark has merged PR #10, tagged
v1.0.0-beta.1 and approved the deployment, or has gone part of the way.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, 2026-10-03: the repository is public with
its settings and rulesets on; the nuget.org Trusted Publishing policy exists; the nuget environment has reviewer
m4bwav, tag rule v*, NUGET_USER and admin bypass off; release PR #10 (CHANGELOG heading dated 2026-10-03, size numbers
corrected to the size gate: nupkg 373.2 KB, UPM 114.5 KB compressed) was open for Mark. In the UniverseGenerator
clone, read ai-docs/HANDOFF.md, then AGENTS.md, then ai-docs/notes/2026-10-03-stage-4-checklist.md in full. Nothing
else unless a step needs it. Re-check any nuget.org or GitHub fact older than three months against the official docs
before using it (AGENTS.md, "Research beats recall").

First find out how far Mark got, and say so: is PR #10 merged (gh pr view 10 --json state), is ci green on that
master commit (gh run list --workflow ci.yml --branch master --limit 1), is there a v1.0.0-beta.1 tag (git ls-remote
--tags origin), and what the release run did (gh run list --workflow release.yml; gh run view <id> for a waiting,
failed or finished run). Then, only as far as Mark's steps allow:
1. If PR #10 is not merged: check its ci is still green and the CHANGELOG still matches master's size gate; if master
   moved and the numbers changed, correct them on the branch. Then stop for Mark.
2. If the release run is waiting for approval: tell Mark it is ready (Review deployments, approve nuget) and stop.
3. Once the release run has finished: verify on nuget.org as the checklist's step 7 says (flat container, registration
   listed, dotnet nuget verify, snupkg, gh release view, gh attestation verify, fresh net10.0 and net48 console
   projects restoring 1.0.0-beta.1 --prerelease and printing the README's first example), and record every result in
   the checklist, the plan (Stage 4) and the log. Use a scratch folder outside the repository for the console
   projects. Then the next action is Stage 4's 1.0.0 items (N1 first, plan "Before 1.0.0") or Stage 5, as Mark rules:
   ask him which.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge yourself
once its checks pass). No Runtime change is expected: tests/Golden/v1/ must not change (git status shows nothing
there), the tests stay green on net10.0 and net48, CI stays green on every operating system, and the size gate stays
green (warn me at yellow). Never tag, publish, approve a deployment, unlist a package, write a secret, change the
repository's visibility or rulesets, or merge a non-docs pull request: those wait for Mark. No local path, LAN
address or private name in any committed file (the repository is public): run scripts/history-scan.py before each
commit and read its last line before committing (a note that quotes a scan pattern trips it; describe the pattern in
words).

Stop and ask at once if ci fails on any operating system in a way that needs a Runtime or golden change, if the
release run fails (read its log and say which step and why; never re-tag or move a tag yourself), if Trusted
Publishing is refused, if nuget.org shows something the checklist does not expect, or if a nuget.org or GitHub fact
differs from the checklist in a way that changes what Mark has to do. When you reach a step that waits for Mark,
update the handoff, log, plan (Stage 4 checklist, next action), the checklist note and the index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
