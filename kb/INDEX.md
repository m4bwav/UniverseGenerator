---
title: "Galaxy generator knowledge base: index"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, index, knowledge-base, galaxy-generator, universegenerator]
summary: "start here for anything about what UniverseGenerator should do and why: the feature matrix against every surveyed generator, the research sources, and the determinism, plausibility and design rules"
---

# Galaxy generator knowledge base

Mark asked on 2026-10-02 that the research behind the generator live in Markdown files and the repository wiki, not only in a session. This folder is that knowledge base (plan decision D22). It was started in the maintainer's private run record during Stages 0 to 2 and moved here at Stage 3; the public parts (feature matrix, rules, how-to) are published to the GitHub wiki by wikiwright.

How to keep it: one fact in one place, each file with front matter and a `stale_after` date; the feature matrix is generated (`python features/build_matrix.py`), never edited by hand. New research goes into `sources/` as a dated report whose table has a "feature keys" column, then `status.json` gets a status for every new key, then the script runs.

## Read when

| File | Read when |
|---|---|
| [features/feature-matrix.md](features/feature-matrix.md) | deciding what the generator must do to out-do every other one; one row per feature found anywhere, who has it, our status, package and size |
| [features/status.json](features/status.json) | changing a feature's status, package, size estimate or reason (then rebuild the matrix) |
| [features/feature-keys.md](features/feature-keys.md) | tagging a newly surveyed generator with the shared vocabulary |
| [features/first-pass-sources.json](features/first-pass-sources.json) | checking which features the first research pass credited to a source |
| [features/build_matrix.py](features/build_matrix.py) | rebuilding the matrix after any change |
| [rules/determinism.md](rules/determinism.md) | writing or reviewing code that turns a seed into output (PRNG, hashes, maths, Mono and IL2CPP traps) |
| [rules/plausibility-rules.md](rules/plausibility-rules.md) | choosing a formula for stars, orbits, planets or moons |
| [rules/design-rules.md](rules/design-rules.md) | deciding output, map structure, game scale, writer features or API shape |
| [sources/2026-10-02-engine-assets-and-libraries.md](sources/2026-10-02-engine-assets-and-libraries.md) | comparing with Unity, OpenUPM, Godot, Unreal and library rivals |
| [sources/2026-10-02-new-tools-papers-and-talks.md](sources/2026-10-02-new-tools-papers-and-talks.md) | looking for prior art from 2020 to 2026, or the exoplanet one-line rules |
| [sources/2026-10-02-writers-and-gm-tools.md](sources/2026-10-02-writers-and-gm-tools.md) | designing text, exports or the lab page for writers and game masters |
| [../ai-docs/notes/2026-10-02-galaxy-generator-improvement-ideas.md](../ai-docs/notes/2026-10-02-galaxy-generator-improvement-ideas.md) | the first research pass: games, libraries, astronomy, the planet and universe levels, API rules |
| [../ai-docs/notes/2026-10-02-galaxy-generator-library-or-web-tool.md](../ai-docs/notes/2026-10-02-galaxy-generator-library-or-web-tool.md) | why a web page as well as a library, and generator-site traffic |
| [../ai-docs/notes/2026-10-02-galaxy-stage0-survey-and-capture.md](../ai-docs/notes/2026-10-02-galaxy-stage0-survey-and-capture.md) | what the game's generator really does, the legacy capture and its statistics |
| [../ai-docs/notes/2026-10-02-package-size-budget.md](../ai-docs/notes/2026-10-02-package-size-budget.md) | any change that adds code or data (the D19 budget) |

Related: [the plan](../ai-docs/plans/2026-10-02-galaxy-generator-extraction.md).
