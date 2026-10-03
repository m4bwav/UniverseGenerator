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

Written 2026-10-03, after the galaxy extras (b8dc686) and StarName (958960b).

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Start by reading
D:\m4bwa\Claude\Projects\Ai\labs\UniverseGenerator\ai-docs\HANDOFF.md, then AGENTS.md, and in the 1.0 plan it links
only the README's first example, the API section, the feature matrix's short form, the release ritual and the Stage 3
checklist. For Stop 2 read only the "Open for tuning" sections of the notes the handoff lists (galaxy, planet, moon and
belt, cluster, universe, galaxy extras). Work on branch stage3-core (draft PR #1).

Next step: the last of Stage 3 before review. Write the README (the plan's first example first, as it runs today,
then the feature matrix's short form, then short sections on clusters and universes, the galaxy extras, StarName,
Universe.At and the seed promise), the CHANGELOG (an Unreleased 1.0.0-beta.1 entry with the size numbers from the
size gate), and samples (a small console sample for NuGet and the UPM package's Samples~ folder, both built or
checked by a test so they cannot rot). Check every code line in the README against the real API by running it.
No Runtime behaviour changes in this step: if the README shows that an API is awkward, write it down for Stop 2
instead of changing it.

Then prepare Stop 2: a short note listing the seed-changing questions from the handoff and the notes' "Open for
tuning" sections, each with the current value, the alternative and what it would move (which golden files), plus
the non-seed questions; put the same list in PR #1's description. Then mark PR #1 ready for review, assign it to
m4bwav with the needs-review label (gh pr edit 1 --add-assignee m4bwav --add-label needs-review), and stop for Mark.

Commit per step, keep CI green on the self-hosted runner, keep the size gate green (warn me at yellow; the README
ships inside the nupkg, so watch its size), and update the handoff, log, plan, index and PR #1 description before
you stop. Never change a golden file in tests/Golden/v1/ (core, system, galaxy, planet, moon-belt, cluster,
universe, galaxy-extras, star-name); if anything would, stop and ask. If the Unity compile check, scale tests or
BenchmarkDotNet would take long, list them for Stop 2 rather than starting them.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
