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

Written 2026-10-03, after Stop 2 was ruled (Mark merged PR #1 into master, 32df421, with no rulings: every question
keeps its current value). Use it to prepare Stage 4.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 3 is merged into master (PR #1, 32df421)
and Stop 2 is ruled: every question keeps its current value, and no golden file changed. Now prepare Stage 4, the
NuGet release. In the UniverseGenerator clone, start by reading
ai-docs/HANDOFF.md, then AGENTS.md, then the 1.0 plan's
"Stage 4", "Security" and "Build and package specifics" sections only
(ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md). For the release workflow, read the package-modernize
skill's NuGet reference on Trusted Publishing and release.yml (the skill's private overlay holds the maintainer's
identities). Re-check any nuget.org or GitHub Actions fact older than three months against the official docs before
using it (AGENTS.md, "Research beats recall").

Next step, on a new branch stage4-prep off origin/master, in one pull request:
1. The history scan (N8): every commit on every branch (git log -p --all) for secrets (API keys, tokens,
   connection strings, private keys), private names (people other than Mark, internal hosts, LAN addresses such as
   192.168.*, the vault path), email addresses other than the public author one, and local paths (the maintainer's drives and
   user profile, the runner's folder). Use gitleaks if it installs cleanly (say what you installed), plus
   a stdlib Python regex pass for the names and paths. Write the report to ai-docs/notes/<date>-history-scan.md:
   each finding as commit, file and kind, never the secret's value. Fix what is in the current tree in this
   branch (move anything private to the everlast private sidecar with everlast.py note --private). Do not rewrite
   history or force-push.
2. release.yml (it does not exist yet, though AGENTS.md describes it): on a pushed tag v*, restore, build, test
   and pack, then a publish job that waits at the GitHub environment nuget and pushes to nuget.org through
   Trusted Publishing (OIDC login, no API key anywhere). Lint it (actionlint if available). Do not push a tag
   and do not run it.
3. A Stage 4 checklist note for Mark (ai-docs/notes/<date>-stage-4-checklist.md): make the repository public
   once the scan is clean, create the nuget.org Trusted Publishing policy (owner, repository, workflow file,
   environment; a policy that can publish a new package), create the nuget environment with himself as the
   required reviewer, then tag v1.0.0-beta.1 after ci is green and approve the deployment; then verify on nuget.org
   (flat container, registration listed).

Commit per step. No Runtime change is expected: tests/Golden/v1/ must not change (git status shows nothing there),
the tests stay green on net10.0 and net48, CI stays green on the self-hosted runner, and the size gate stays green
(warn me at yellow). Do not tag, publish, merge a non-docs pull request or make the repository public: those wait
for Mark. Open the pull request, assign it to m4bwav with the needs-review label, and give me its URL.

Stop and ask at once if the scan finds a live secret (it must be rotated before anything else), if anything
would need a history rewrite, or if the current Trusted Publishing docs differ from the plan in a way that changes
what Mark has to set up. When the three steps are done, update the handoff, log, plan (Stage 4 checklist, next
action) and index, and stop for Mark.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
