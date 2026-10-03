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

Written 2026-10-03, after the galaxy cluster level (64e90de, 816ddfc).

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Start by reading
D:\m4bwa\Claude\Projects\Ai\labs\UniverseGenerator\ai-docs\HANDOFF.md, then AGENTS.md, the 1.0 plan it links,
and the level notes in ai-docs/notes/ (galaxy, and 2026-10-03-galaxy-cluster-level-design.md in full; the star system,
planet and moon-and-belt notes only where you need them). Work on branch stage3-core (draft PR #1).

Next step: the universe level, D14's last, as the handoff's "Next single action" describes (ideas U1, U3, U10 with
A12, U11, A13 and the U7 epoch). Write a short design note first, then code. Universe becomes the record with
Generate beside At. A cluster inside a universe (v1-seed/universe/cluster/i) may take its kind and epoch from the
universe, but GalaxyCluster.Generate alone must keep its present output: tests/Golden/v1/core.json, system.json,
galaxy.json, planet.json, moon-belt.json and cluster.json stay exactly as they are. Add a new golden file for the
universe, property and distribution tests, and make Universe.At reach the universe and everything below it.

Commit per step, keep CI green on the self-hosted runner, keep the size gate green (warn me at yellow), and update
the handoff, log, plan, index and PR #1 description before you stop. Do not change an existing golden file; if a
change would, stop and ask. If the universe level runs long, say so: it can move to 1.1 without seed changes.
After the universe level, list what Stage 3 still owes before Stop 2 (galaxy extras, README, CHANGELOG, samples,
the seed-changing questions for me).

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
