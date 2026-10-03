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

Written 2026-10-03, after Stage 4 was prepared (branch `stage4-prep`, a pull request waiting for Mark). Use it once
Mark has merged that pull request or started the Stage 4 checklist.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4 was prepared on 2026-10-03: the
stage4-prep pull request holds the history scan, release.yml and Mark's Stage 4 checklist. In the UniverseGenerator
clone, read ai-docs/HANDOFF.md, then AGENTS.md, then ai-docs/notes/2026-10-03-stage-4-checklist.md in full and
ai-docs/notes/2026-10-03-history-scan.md's "Re-run before going public" section. Nothing else unless a step needs
it. Re-check any nuget.org or GitHub Actions fact older than three months against the official docs before using it
(AGENTS.md, "Research beats recall").

First find out how far Mark got, and say so: is stage4-prep merged (gh pr list --state all), is the repository
public, does the nuget environment exist, is the Actions variable RUNS_ON set, is there a v* tag. Then do the
checklist's steps marked (agent), in its order, only as far as Mark's steps before them are done:
1. Re-run the history scan on the final master (gitleaks and scripts/history-scan.py with the private names file
   from the everlast private sidecar); add the result to the scan note.
2. Before the repository is public: with Mark's go-ahead, set RUNS_ON to "ubuntu-latest" (with the quotes) and
   check that ci still passes on hosted runners. Then, in a pull request, ci.yml's build job becomes the
   package-modernize template's Windows, Linux and macOS matrix on hosted runners (plan D9), keeping every check the
   current job runs (golden check, format, audit, net48 on Windows, console sample, .meta check, package check,
   size gate).
3. Once Mark has made it public: secret scanning and push protection, private vulnerability reporting, workflow
   permissions read, the master ruleset (required check ci) and the admins-only v* tag ruleset, from the
   package-modernize rulesets templates. The nuget environment only if Mark asks you to create it.
4. The release pull request: date the CHANGELOG heading for 1.0.0-beta.1 and check its size numbers against the
   size gate.
5. After Mark has tagged and approved: verify on nuget.org as the checklist's step 7 says, and record it.

Commit per step, each pull request for Mark (assign m4bwav, label needs-review; a docs-only one you may merge
yourself once its checks pass). No Runtime change is expected: tests/Golden/v1/ must not change (git status shows
nothing there), the tests stay green on net10.0 and net48, CI stays green, and the size gate stays green (warn me at
yellow). Never tag, publish, approve a deployment, make the repository public or merge a non-docs pull request:
those wait for Mark. No local path, LAN address or private name in any committed file (run
scripts/history-scan.py before each commit).

Stop and ask at once if the re-run scan finds anything new, if ci fails on hosted runners in a way that needs a
Runtime or golden change, if the release run fails, or if a nuget.org or GitHub fact differs from the checklist in a
way that changes what Mark has to do. When you reach a step that waits for Mark, update the handoff, log, plan
(Stage 4 checklist, next action) and index, and stop for him.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
