---
title: GitHub wiki written and published for 1.0.0
kind: note
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [wiki, docs, 1.0.0, github, universegenerator]
summary: "the wiki's pages, where their git working copy is, how every example was verified against the published 1.0.0 package, the facts found on the way, the inaccuracies in the shipped docs, and how to update the wiki; read before touching the wiki or the README sentences listed under inaccuracies"
---

# GitHub wiki for 1.0.0

## Summary

The maintainer asked on 2026-10-03 for the GitHub wiki to be filled out for 1.0.0 (plan Stage 4, decisions D13 and D22). The wikiwright skill wrote 11 pages plus sidebar and footer, from the README, CHANGELOG, AGENTS.md, `kb/`, the tests, the workflows, nuget.org and the published 1.0.0 package itself. Every example was run against UniverseGenerator 1.0.0 from nuget.org on .NET 10 and .NET Framework 4.8 and printed the same output on both, apart from the runtime line and the layout of exception messages. Wiki commit 170339b; `live`: 11 pages, 0 failures, sidebar and footer render, 20 anchor links to 5 pages, 0 broken. The everwrite checker reported 1 strong finding, a false positive ("a quasar at its core" is a galaxy descriptor quoted from the XML docs), and 42 weak ones (long sentences, mostly in generated tables).

Pages: Home, Getting-Started, API-Reference, Same-Seed-Same-Galaxy (the reproducibility page), Plausibility-Rules, Errors-and-Edge-Cases, Recipes, Feature-Matrix, FAQ, Versions-and-Upgrading, Development, `_Sidebar`, `_Footer`. No Unity or OpenUPM page: those come in Stage 5, when the OpenUPM listing is live (Stage 7 updates the wiki).

## Where the pages are

The working copy is the clone of `https://github.com/m4bwav/UniverseGenerator.wiki.git`, kept as a sibling of this repository's clone named `UniverseGenerator.wiki` (outside this repository), branch `master`. Plain markdown links between pages (`[Recipes](Recipes)`), no wikilinks, LF line endings, no byte order mark.

Two pages are generated, not written by hand:

- `API-Reference.md` by `2026-10-03-wiki-gen-api.py` (beside this note). Inputs, in one base folder: `nz/` (the 1.0.0 nupkg from nuget.org, unzipped), `api/v100.tsv` and `api/b1.tsv` (the public surfaces of 1.0.0 and 1.0.0-beta.1, written by `2026-10-03-wiki-api-surface.cs`, a .NET 10 file-based app run as `dotnet run 2026-10-03-wiki-api-surface.cs -- <lib/net10.0/UniverseGenerator.dll>`, LF output). Run `python 2026-10-03-wiki-gen-api.py <base> <wiki>/API-Reference.md`. It reads summaries, remarks and exceptions from the shipped XML docs, drops the "(plan X)" references they carry, and marks a member "1.0.0" when beta.1 lacks it. For a new version, compare against the previous release instead of beta.1 and change the "Since" wording.
- `Feature-Matrix.md` by `2026-10-03-wiki-gen-matrix.py`: `python 2026-10-03-wiki-gen-matrix.py kb/features/feature-matrix.md <wiki>/Feature-Matrix.md`, after `kb/features/build_matrix.py`. It keeps feature, meaning, the count of other generators and the status, adds notes for later and rejected rows, and strips plan IDs, idea numbers, ai-docs paths, the game's name and the maintainer's name.

## How it was published

Preflight on 2026-10-03: state `placeholder` (only GitHub's first Home page, commit 857ebaa). The pages were committed on top of the cloned placeholder and pushed as a plain fast-forward (170339b), then `wikiwright.py live m4bwav/UniverseGenerator <wiki>`: 11 pages, 0 failures.

## Updating the wiki later

1. `git -C <wiki> pull --ff-only`, then edit the pages. Page names are the file names with hyphens; links are `[Text](Page-Name)`.
2. Re-verify: bump the version in `2026-10-03-wiki-verify.cs` (next to this note: the `#:package` line and the children's `UniverseGenerator@` line), run it from a scratch folder outside the repository with TEMP and TMP pointing into that folder (`dotnet build wiki-verify.cs && dotnet run --no-build wiki-verify.cs | tr -d '\r' > out.txt`). It runs every example on net10.0 and net48 (Windows only) and the versions example on the previous release too; point that config at the release before the new one. Then `python <wikiwright>/scripts/wikiwright.py diffout 2026-10-03-wiki-verify.out.txt out.txt`: every difference is a page to fix. `diffout ... --save` saves the new output over the old.
3. `wikiwright.py outputs <wiki> <new output>`, `wikiwright.py snippets <wiki> 2026-10-03-wiki-verify.cs`, `wikiwright.py check <wiki> --version <new>`, and the everwrite checker.
4. Regenerate API-Reference and Feature-Matrix as above.
5. Commit, `git push`, then `wikiwright.py live m4bwav/UniverseGenerator <wiki>`. Pages that name the version: `_Footer` (version and date), Home (the version, its date, and the install line has none), Getting-Started (`--version 1.0.0` and the PackageReference), API-Reference (intro and "Since"), Versions-and-Upgrading (the releases table, registry read time, the 1.0.0 section and the comparison), Development (test counts and size numbers), FAQ (the upgrade answer).

## How the examples were verified

`2026-10-03-wiki-verify.cs`, scaffolded by `wikiwright.py scaffold nuget UniverseGenerator 1.0.0 --namespace UniverseGeneration --type Galaxy --children net48`, holds every page's C# block as a whole program, run in child apps: net10.0 (`.NET 10.0.12`, lib net10.0) and net48 (`.NET Framework 4.8.9345.0`, lib netstandard2.0), each example in its own process, all from the published package (`#:package UniverseGenerator@1.0.0`). The versions example also ran on 1.0.0-beta.1 (net10.0). Output: `2026-10-03-wiki-verify.out.txt` (LF). Two runs gave byte-identical output. net10.0 and net48 differ only in the `installed` lines and in exception messages (.NET Framework prints "Parameter name: X" on its own line). 1.0.0-beta.1 and 1.0.0 print the same text for every system and planet of `my-seed`.

`outputs`: 11 pages, 25 outputs checked, 0 missing. `snippets`: 25 blocks checked, 0 missing, 3 commands. Not tested: Unity, Mono, trimmed or native AOT builds, Linux and macOS (CI covers those for the golden files; the wiki's examples ran on Windows only), timing. The repository's tests on the `chore/package-validation-baseline` branch the same evening: net10.0 272 passed, net48 271 passed.

The public surfaces were compared from reflection over the two published DLLs: 635 signatures in 1.0.0-beta.1, 763 in 1.0.0, none removed or changed; 128 members are new.

## Facts verified while writing (not in the README)

- An empty seed is accepted; `null` and seeds over 200 characters are refused.
- Seeds are exact text: "My-Seed" and "my-seed " (trailing space) are different seeds; a space is escaped as `%20` in the address.
- Galaxy names repeat across seeds ("my-seed" and "hello world" both make a Wyvern Galaxy).
- On .NET Framework 4.8 every `ArgumentException` message ends with a "Parameter name: X" line instead of "(Parameter 'X')".
- The JSON `type` values are camelCase: `universe`, `galaxyCluster`, `cosmicVoid`, `galaxy`, `starSystem`, `planet`.
- A full 60-system galaxy with `children: true` is 909,995 characters of JSON (SHA-256 starts D486B72BA56ACAEF), the same on .NET 10 and .NET Framework 4.8.
- The habitable zone is √(L / 1.776) to √(L / 0.32) au (optimistic limits); the frost line is exactly 2.7 × √L; temperature follows Stefan-Boltzmann; luminosity and radius are within 0.2% of the mass formulas.
- Planets are listed by orbit, innermost first; their letters are not in orbit order.
- `DangerShift` keeps every system's name and position and clamps danger to 1 to 10.
- `Distances.LaneDays` is one day per 100 game units or part of one, at least one.
- The presets' codes: Pocket `systems=20&planets=6`, Roguelike `systems=30&weirdness=15&lanes=10&danger=2`, Cozy `systems=40&weirdness=3&lanes=50&danger=-3`, Epic `systems=300`, SpaceOpera `weirdness=10`, Plausible `starmix=plausible&weirdness=1`.
- `GeneratorTables.FromJson` throws `FormatException`, not `ArgumentException`.
- `Universe.At` for a system beyond the galaxy's count asks in its message whether the same options were passed.
- nuget.org dates 1.0.0 2026-10-04 (UTC); the CHANGELOG says 2026-10-03 (US Central).

## Inaccuracies found in the shipped docs

1. README, Install: "Unity 6: add `com.m4bwav.universe-generator` from OpenUPM" reads as available now, while the status line above it says the OpenUPM listing comes next. True on 2026-10-03: not on OpenUPM yet. Fix in the README with the next release, or at the Stage 5 listing.
2. XML docs (shipped in the nupkg, so in every user's IntelliSense): about 30 summaries cite internal plan IDs, such as "(plan D17)", "(plan D24)", "(plan U6)" and "(Stop 2, S1)", that mean nothing outside the repository. The wiki strips them. Fix in the source with a later release.
3. XML docs of `ToJson` on the generated records: "This galaxyCluster as JSON", "This starSystem as JSON" use the JSON type key as an English noun. Cosmetic; fix with item 2.
4. `kb/rules/plausibility-rules.md`, Stars: lists the conservative habitable zone (√(L / 1.1) to √(L / 0.53)) with "simple form" as what the prototype uses; 1.0.0 uses √(L / 1.776) to √(L / 0.32). The "Proto" column in general reflects the Stage 1 prototype, not 1.0.0 (it says "no" for the radius valley, the hot Neptune desert and the Jeans rule, which the CHANGELOG lists in 1.0.0-beta.1). A repository doc, fixable any time without a release.

## Gotchas

- `dotnet add package --version 1.0.0` failed with NU1102 minutes after the flat container listed 1.0.0: NuGet's HTTP cache still held the old index. Use an empty `NUGET_HTTP_CACHE_PATH` for fresh-consumer checks.
- A heredoc in Git Bash turned `"\\n"` in a C# raw string into a real newline (the known heredoc quirk); edit such lines with an editor tool.
- The prose checker's "setup" pattern fires on "a quasar at its core", a generated descriptor; leave it.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (Stage 4); see also [the Stage 4 checklist](2026-10-03-stage-4-checklist.md), [../HANDOFF.md](../HANDOFF.md), [../log.md](../log.md).
