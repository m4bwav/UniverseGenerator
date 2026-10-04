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

Continue the UniverseGenerator run (C#; the F# port is deferred). Stage 4, towards 1.0.0. 1.0.0-beta.1 is on nuget.org and verified. Mark ruled N1 on 2026-10-03: every unbuilt or partial 1.0 matrix row goes in 1.0.0 ("Do all the things"); nothing moves to 1.x. Eight rows are built as stacked pull requests waiting for him in order: #17, #18, #19, #20, #21, #22, #23, #25. Four rows are left. Mark wants everything done (no one uses the library yet), and he wants estimates in agent time, never human working days.

In the UniverseGenerator clone, read in this order:

- `ai-docs/HANDOFF.md`;
- `AGENTS.md`;
- the "N1 ruling table" and "N1 progress" sections of `ai-docs/notes/2026-10-03-stop-2-questions.md`;
- the plan's Stage 4 line (`ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md`).

Open the Runtime folder, `kb/features/status.json` and the tests only as each step needs them. Re-check any nuget.org or GitHub fact older than three months against the official docs before using it (AGENTS.md, "Research beats recall").

1. Check the stack first: `gh pr list -R m4bwav/UniverseGenerator`. If Mark merged some of it, rebase the rest on `master` and build on the top. Otherwise branch from `feat/galaxy-shapes` (#25) and open each new pull request against the branch before it.
2. Build the four rows left, one pull request each (hooks and custom fields together), in this order:
   - **`constraints`**: "at least one X" guarantees, such as a garden world, a precursor site or a given star class in a galaxy.
     - Make it an option whose default is none, decided at the galaxy level on its own stream and passed down in the system's context, so `Universe.At(link)` regenerates the same system (the hierarchy tests must hold).
     - It needs a name in the options code.
   - **`hooks-plugins` and `custom-fields`**: post-processors that also run when `Universe.At` regenerates an object, and a per-object user dictionary.
     - The name `Tags` is taken by story tags on `StarSystem`.
     - Delegates inside the `GeneratorOptions` record would break value equality and `ToCode`, so decide where hooks live and say why in the pull request.
   - **`data-tables-editable`**: public table records, JSON tables by id read by a hand-written parser (no reflection, nothing parsed in a static constructor), and a ScriptableObject adapter for Unity.
     - This is the largest row and the biggest size risk. The DLL is 536 KB against a green line of 750: warn Mark at yellow, never cross red.
   - **The Unity float adapter**: Vector2 and Vector3 helpers in a second asmdef that references UnityEngine, kept out of the NuGet build.
     - Compile-check it in Unity if an editor is installed (the unity-agent-cli skill); otherwise say the compile check waits for Stage 5.
3. Every feature pull request must follow these rules:
   - Additive: no existing seed's output changes. A new field or option adds a golden file and never changes one.
   - A new record property: run `python scripts/gen-json-export.py`, add its keys to `JsonKeyFilter.AddedLater`, and write its own golden file.
   - A new option: give it a name in `Options/OptionsCode.cs`.
   - When the public API grows, update the README and `samples/ConsoleSample/Examples.cs` together, in the same order (SampleTests check them).
   - Also: a CHANGELOG "Unreleased" entry, and status.json plus the matrix updated (the scratch helper is gone, so edit the row's `why` with a Python script, then `python kb/features/build_matrix.py`; never edit the matrix by hand).
   - Targets: netstandard2.0 and net10.0 with LangVersion 9, no reflection, no engine references in Runtime.
4. When all N1 rows are built and Mark has merged the stack: the 1.0.0 release pull request.
   - The CHANGELOG heading is dated, with the size numbers from the size gate on `master`. `<Version>` and package.json go to 1.0.0. Fix the README status line, which is stale.
   - Then stop: tagging and approving wait for Mark. After he tags and approves, verify 1.0.0 as checklist step 7 did, then set `PackageValidationBaselineVersion` 1.0.0.

Commit per step; every pull request goes to Mark (assign m4bwav, label needs-review). You may merge a docs-only pull request yourself once its checks pass.

- `tests/Golden/v1/` must not change: `git status` shows only new files there.
- The tests stay green on net10.0 and net48, and CI stays green on every operating system.
- The size gate stays green: warn Mark at yellow, never cross red.
- Tagging, publishing, approving a deployment, unlisting, secrets, visibility, rulesets and merging a non-docs pull request need Mark's explicit go-ahead in this session.
- No local path, LAN address or private name in any committed file (the repository is public). Run `scripts/history-scan.py` before each commit and read its last line. A note that quotes a scan pattern trips it, so describe the pattern in words.
- Use `dotnet add package` with `--prerelease` or `--version`, never both.

Stop and ask at once if any of these happens:

- a feature would change any existing seed's output or need a golden change;
- ci fails on any operating system;
- a change moves a size metric into yellow;
- a release run fails, or Trusted Publishing is refused;
- a nuget.org or GitHub fact differs from the checklist in a way that changes what Mark has to do.

When you reach a step that waits for Mark, update the handoff, the log, the plan (Stage 4, next action), the Stop 2 note's N1 progress table and the index, and stop for him. The real stars track (`ai-docs/notes/2026-10-03-real-stars-track.md`) comes after 1.0.0. Do not start it unless Mark asks.

Before you stop, rewrite `ai-docs/next-session-prompt.md` with the prompt for the session after yours, in this same form: what to read first (only what that step needs), the next step and its limits, the commit, CI, size-gate and golden-file rules, when to stop and ask, and, as its last paragraph, this same instruction to rewrite the file for the session after it. Commit it with the handoff, and give Mark the prompt in your final message.
