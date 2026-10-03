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

Written 2026-10-03, after the universe level (8a42448).

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Start by reading
D:\m4bwa\Claude\Projects\Ai\labs\UniverseGenerator\ai-docs\HANDOFF.md, then AGENTS.md, the 1.0 plan it links,
and ai-docs/notes/2026-10-02-galaxy-level-design.md in full (what the galaxy level still lacks); the improvement
ideas note only for the factions, points of interest, hazards, monuments and beacons rows (D24); the cluster and
universe notes only where a galaxy's context matters. Work on branch stage3-core (draft PR #1).

Next step: the galaxy extras, the last code of Stage 3, as the handoff's "Next single action" describes: factions
over lanes, points of interest, hazards, D24 monuments per region and beacons per galaxy. Write a short design note
first, then code. Every new value comes from new streams of the galaxy's or system's seed, so tests/Golden/v1/core.json,
system.json, galaxy.json, planet.json, moon-belt.json, cluster.json and universe.json stay exactly as they are. Add a
new golden file for the extras, property and distribution tests, and make sure Universe.At still returns equal objects
(extend the hierarchy tests if the extras add addressable objects). If time remains, add a public star-name entry
point (StarNames is internal) for the website's star name tool, without changing any seed's output.

Commit per step, keep CI green on the self-hosted runner, keep the size gate green (warn me at yellow), and update
the handoff, log, plan, index and PR #1 description before you stop. Do not change an existing golden file; if a
change would, stop and ask. If the extras run long, say so: any of them can move to 1.1 without seed changes. After
the extras, the session after yours writes the README, CHANGELOG and samples and prepares Stop 2 with the
seed-changing questions listed in the handoff.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
