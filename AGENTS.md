# AGENTS.md

Rules for any AI agent (Claude Code, Copilot, Cursor, Codex) working in this repository. `CLAUDE.md` and `.github/copilot-instructions.md` only point here.

## What this is

UniverseGenerator: a seeded universe, galaxy cluster, galaxy, star system, planet and moon generator for games and fiction, as the NuGet package `UniverseGenerator` (namespace `UniverseGeneration`) and the Unity package `com.m4bwav.universe-generator` (OpenUPM), built from one source folder. It was extracted from the game SpaceDeckBuilder2. 1.0.0-beta.1 is on nuget.org (2026-10-03, Trusted Publishing) and the repository is public; 1.0.0 and OpenUPM are next. The plan is `ai-docs/plans/2026-10-02-universegenerator-1.0-plan.md` (decisions D1 to D25, ruled 2026-10-02); start with `ai-docs/HANDOFF.md`. Why it does what it does: `kb/INDEX.md`.

## Rules

- **The seed promise.** From 1.0, a seed and generator version give the same output on every runtime (Windows, Linux, macOS on .NET 10 and .NET Framework 4.8; Unity Mono and IL2CPP) for the whole major version. The golden files in `tests/Golden/v1/` are the proof; never regenerate them to make a test pass. A change that would alter any seed's output adds a generator version and keeps the old one selectable. Once a golden file is in a release tag, CI refuses any change to it; before the first release (1.0.0-beta.1), a level's golden file may be deleted and rewritten (`UG_WRITE_GOLDEN=1 dotnet test`) only when its output is changed on purpose, said in the commit message. Golden tests write an explicit list of fields per level, so adding a field adds a golden file and never changes an existing one.
- **Determinism rules** (`kb/rules/determinism.md`): our own PCG32 and SplitMix64 streams, one stream per purpose, derived from the object's address; never `System.Random`, `UnityEngine.Random`, `string.GetHashCode` or `Guid.NewGuid` in generation; decisions on integer draws; maths from + - * / and sqrt only (the `DMath` functions); values rounded before they are stored or written.
- **Games and fiction first** (D18): story hooks, names, readable maps and gameplay structure lead; astronomy only as far as it keeps descriptions consistent (`kb/rules/plausibility-rules.md`).
- **Easy start.** `Galaxy.Generate("my-seed")` must keep working with good defaults; presets and options add, never require.
- **Size budget** (`ai-docs/notes/2026-10-02-package-size-budget.md`): nupkg under 1 MB, compressed data under 500 KB, UPM package under 2 MB unpacked, under 150 Runtime .cs files, under 300 KB added to a WebGL build, a default galaxy under 50 ms. Say so first in the pull request and to the maintainer when a change moves any metric into yellow; never cross red. Larger content goes to the add-on packages (Names, Text, Exports).
- **Targets.** The Runtime folder compiles as `netstandard2.0` and `net10.0` with LangVersion 9, and in Unity 6 with no engine references: no net5+ APIs without `#if` or a polyfill, no reflection, no data parsed in a static constructor.
- **Every file under `Packages/com.m4bwav.universe-generator/` has a committed `.meta`, and a GUID never changes.**
- **Nothing reaches a registry without the maintainer.** No API key anywhere; `release.yml` publishes through nuget.org Trusted Publishing from a job that waits at the `nuget` environment. OpenUPM builds from tags; the maintainer opens its submission.
- **Releases follow one ritual.** CHANGELOG with the date and the size numbers, `<Version>` set, merged, `ci` green on `master`, then tag `v<version>` and push it. Tag only after green.
- **Knowledge base.** Research goes into `kb/` (reports in `kb/sources/` with a "feature keys" column, statuses in `kb/features/status.json`), and `python kb/features/build_matrix.py` regenerates the feature matrix; never edit the matrix by hand. The wiki publishes the public parts.
- **Research beats recall.** Re-verify any version or registry fact older than three months.
- **Document for handoff.** Anything learned, decided or built goes into `ai-docs/` before you finish; rewrite `ai-docs/HANDOFF.md` when work is left unfinished. Before stopping, also rewrite `ai-docs/next-session-prompt.md`: the prompt that starts the next session (what to read first, the next step and its limits, the rules, when to stop and ask), ending with this same instruction to rewrite the file, so every session hands the next one its prompt; give the prompt in your final message too.
- **No AI attribution anywhere.**
- **Line endings.** Files are LF; count byte 13 after writing on Windows.
- **Before this repository goes public,** scan the whole history for secrets, private names and local paths.

## Commands (once Stage 3 adds the projects)

```
dotnet restore --locked-mode
dotnet format --verify-no-changes
dotnet build -c Release
dotnet test -c Release                              # net10.0 and net48 (net48 executes only on Windows)
dotnet pack src/UniverseGenerator -c Release -o artifacts
python kb/features/build_matrix.py                  # regenerate the feature matrix
```

## Layout

- `Packages/com.m4bwav.universe-generator/Runtime/`: the one source folder (Unity asmdef, noEngineReferences); `src/UniverseGenerator/` compiles it for NuGet (plan D8).
- `tests/`: unit, golden (`tests/Golden/v1/`), property, distribution, hierarchy and size tests; `tests/Golden/legacy/` holds the game's output before the extraction, a record that no test compares against (D25).
- `kb/`: the knowledge base. `ai-docs/`: plans, notes, log, handoff.

## everlast (session knowledge, load on demand)

- `ai-docs/INDEX.md` lists what past sessions learned here (solutions with verified commands, decisions with reasons, plans). At the start of a task, scan it and open only the entries whose title or tags match; no line matches: `everlast.py search "<key terms>"` before concluding nothing was recorded. Read `ai-docs/HANDOFF.md` when continuing unfinished work (everlast-resume skill).
- Before acting on an entry marked `(recheck due)`, run `everlast.py recheck <entry>`, re-run its Verified-by command only when that is read-only or safe (a build, a test, a version query), then record `everlast.py verify <entry>` or `verify <entry> --failed "what broke"`; a fix that changed is superseded, never reused blindly.
- Before finishing a task that hit a dead end, verified a non-obvious command, made a design choice, or taught you something about the user, record it (everlast-capture skill, or `everlast.py note` / `handoff`); rewrite `HANDOFF.md` when work is left unfinished. Say "nothing to record" when that is true.
- Anything naming a person, an internal host or name, a credential, or an opinion about people goes to the private sidecar (`--private`), never here. Lessons about the user or this machine go to the user tier (`--user`).
- Rules go in this file, system layout in CODEMAP.md; the doc set holds only what could not be re-derived from the code in a minute.
- Link documents together with relative markdown links: every markdown folder is reachable from an index whose lines say when to read each file (`ai-docs/INDEX.md` is generated from frontmatter; give entries a one-line `summary`), and an entry links the entries it relates to on a typed `Related:` line (`supersedes`, `contradicts`, `builds on`, `see also`). The set then reads as a graph for people in Obsidian and for agents alike. No wikilinks in the repo.
