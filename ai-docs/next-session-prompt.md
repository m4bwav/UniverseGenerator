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

Written 2026-10-03, after README, CHANGELOG and samples (7a3fe40, c14c7ba) and the Stop 2 note. Use it once Mark has
ruled Stop 2.

```
Continue the UniverseGenerator run (C#; the F# port is deferred). Mark has ruled Stop 2: his rulings are in PR #1's
comments or review (https://github.com/m4bwav/UniverseGenerator/pull/1), or in this message. Start by reading
D:\m4bwa\Claude\Projects\Ai\labs\UniverseGenerator\ai-docs\HANDOFF.md, then AGENTS.md, then
ai-docs/notes/2026-10-03-stop-2-questions.md (the questions S1 to S16, E1 to E4 and N1 to N8, with the golden files
each would move). Read a level note only for a question you are about to change, and only its "Open for tuning"
part and the section on the rule you touch. Work on branch stage3-core (PR #1). If you cannot find a ruling for a
question, treat it as "keep the current value"; if a ruling is unclear, stop and ask.

Next step: apply the rulings, one commit per ruling (or per group of rulings that move the same golden files).
Seed-changing rulings first: change the rule, then delete and rewrite only the golden files the note's "Moves"
column names for that question (UG_WRITE_GOLDEN=1 dotnet test), and say in the commit message which files changed
on purpose and why. Before rewriting, run the tests and check that only those files fail; if any other golden file
in tests/Golden/v1/ would change, stop and ask. Then the extras-only rulings (galaxy-extras.json only). Then the
non-seed ones: matrix rows moved to 1.x go through kb/features/status.json and python kb/features/build_matrix.py
(never edit the matrix by hand); API additions (for example StarSystem.Planet(int) or a typed Universe.At) must not
change any output. Update the level notes' statistics and "Open for tuning" parts, the README and its examples
(change samples/ConsoleSample/Examples.cs first, then copy into the README; SampleTests checks them), and the
CHANGELOG entry if a size number changes.

Commit per step, keep CI green on the self-hosted runner, keep the size gate green (warn me at yellow; the README
ships inside the nupkg). Do not start the Unity compile check, scale tests or BenchmarkDotNet unless a ruling asks
for them. Do not merge PR #1, tag, publish or make the repository public: those wait for Mark (Stage 4). When the
rulings are applied, update the handoff, log, plan (Stage 3 checklist, Stop 2 ruled, next action), index and PR #1
description (what changed per ruling), keep PR #1 assigned to m4bwav with the needs-review label, and stop for Mark.

Before you stop, rewrite ai-docs/next-session-prompt.md with the prompt for the session after yours, in this same
form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and
golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for
the session after it. Commit it with the handoff, and give me the prompt in your final message.
```
