---
title: "Plan: extract the SpaceDeckBuilder galaxy generator into a public NuGet package, then OpenUPM, then a web page and blog post on markdavidrogers.com"
kind: plan
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2026-12-02
tags: [galaxy-generator, nuget, openupm, unity, markdavidrogers-web, blog, spacedeckbuilder, new-package, procedural-generation]
summary: "read before starting or resuming the galaxy generator extraction: stages from golden capture and improvement study to NuGet, OpenUPM, the game's switch to the package, the /labs page and the blog post, with the decisions Mark rules on and his own tasks"
---

# Plan: galaxy generator to NuGet, OpenUPM and markdavidrogers.com

Written 2026-10-02 at Mark's request. Kickoff prompt: ../../prompts/2026-10-02-galaxy-generator-kickoff.md (in the maintainer's private run record). Background:
- [../notes/2026-10-02-galaxy-generator-library-or-web-tool.md](../notes/2026-10-02-galaxy-generator-library-or-web-tool.md): who else does this, and why a web page.
- [../notes/2026-10-02-galaxy-generator-improvement-ideas.md](../notes/2026-10-02-galaxy-generator-improvement-ideas.md): what other generators do that this one could learn from.

Owner: Mark rules on decisions, approves every registry push and merges. The agent builds on branches and stops where this plan says.

Once the run creates the new repository, it copies this plan into that repository's `ai-docs/plans/` and keeps the copy there current. This file then only records the outcome.

**Outcome so far (2026-10-02):** Stages 0, 1 and 2 done; Stop 1 ruled the same day (every recommendation stands; the name is UniverseGenerator). The Stage 2 plan is [2026-10-02-universegenerator-1.0-plan.md](2026-10-02-universegenerator-1.0-plan.md) (decisions D1 to D25, the Stage 1 idea table, effort per level, the README's first example). Stage 0 record: [../notes/2026-10-02-galaxy-stage0-survey-and-capture.md](../notes/2026-10-02-galaxy-stage0-survey-and-capture.md). The research is a knowledge base at [../../kb/INDEX.md](../../kb/INDEX.md) (Mark, 2026-10-02: "The AI should store a knowledge base in md files and/or the repo wiki"). Corrections to the survey below: the game never regenerates a galaxy from a seed (saves hold copies and no seed), the repository pins Unity 6000.6.0f1 while the open Editor has a local 6000.6.4f1 upgrade, and a local clone of the game exists (not where the prompt looked).

## Goal

Turn the galaxy and star-system generator from SpaceDeckBuilder2 into the best seeded space generator anyone can install, and keep it easy for a newcomer. It covers every level, from a small universe down to planets and moons. It is released as:
1. a public, MIT-licensed C# library on nuget.org;
2. the same source as a Unity package on OpenUPM;
3. a page on Mark's site where anyone can roll a universe, galaxy or system from a seed, share the URL, open any object and download it as JSON;
4. a blog post about it.

Every surface links to SpaceDeckBuilder2.

Mark's requirements (2026-10-02, in his words where quoted):
- "Before publishing the repo, research all existing star and galaxy generator for ideas on how to improve mine."
- "More features and more options are good. I want to out do every existing generator but still allow it to be simple and easy to use for new users ... customizable but it can also be easy to start with. Maybe we need a universe generator or a galaxy cluster generator or a planet generator as well."
- "I'm not going to want to generate a true universe because of the massive data involved, so it has to be easy to make small universes, galaxies, or solar systems that will fit in game engines."

- "The focus is for use in gaming or writing rather than science." (Mark, 2026-10-02.)

What that means for this plan:
- **Games and writing first (D18).** Defaults are tuned to be varied, readable and full of story hooks, not statistically realistic. Science is a tool for plausibility: cheap formulas that keep a description consistent (a red dwarf is small and dim, with the habitable zone close in). Heavier astronomy is an optional "Plausible" preset or is left out.
- **Out-do, measurably.** Stage 1 builds a feature matrix of every generator found (games, tabletop tools, web tools, libraries). Every row is marked have, 1.0, 1.x or rejected-with-reason, and the README publishes the matrix.
- **Easy start, deep options.** Three layers of use, and the first must need no reading:
  - one line with defaults;
  - a preset plus a few named options;
  - full data tables and hooks.
- **Game scale by default.** Small counts, budgets the caller sets, generation on demand by address, and coordinates in engine-friendly units. Realism controls what objects look like, not how many there are.

Anything that changes what a seed produces must land before 1.0, because the 1.0 release promises that seeds keep their output. The hierarchical seed scheme (D17) lets new levels and fields arrive in 1.x without changing existing seeds.

## What exists (survey 2026-10-02, read from GitHub; verify before relying on it)

- **Repository:** the game's private repository. It is private, on Unity 6000.6.0f1, and its default branch is `main`. Its AGENTS.md sets the rules: new files under `Assets/` need a `.meta` from `tools/new-meta.ps1`, and the sibling `SpaceDeckPrototype/` must not be edited. No local clone was found in the usual folders.
- **Generator** (`Assets/Scripts/Game/Generators/`, about 1,400 lines):
  - `GalaxyGenerator`: 40 systems on a disc of radius 1000; danger 1 to 10 and region (Core, MidRim, OuterRim) by radius.
  - `StarSystemGenerator`, `PlanetGenerator`, `AsteroidBeltGenerator`, `StationGenerator`.
  - `CelestialNaming` (23 KB): catalog designations, planet discovery letters, Roman-numeral moons, on separate seeded streams.
  - Type files: `PlanetArchetype`, `AtmosphereProfile`, `ResourceDistribution`, `OrbitType`, `StarSystem`, `StarSystemTemplateLibrary`. `ShipFactory` is game-only.
- **Models** (`Assets/Scripts/Game/WorldModels/`): `Galaxy`, `StarSystemData`, `StarData` (StarType: RedDwarf to Multiple), `PlanetData`, `StationData` (StationType, StationService), `AsteroidBeltData`, the archetype libraries and `StartingModels/StarSystemTemplate`.
- **Coupling:**
  - `CelestialNaming` uses no UnityEngine code.
  - The other files use `Vector2`, `Mathf` and the global `UnityEngine.Random`. `GalaxyGenerator.Generate` calls `Random.InitState(seed)`, which resets the host's global RNG; a library must not do that.
  - Systems use `System.Random(seed)`.
  - Star mass (0.8 to 2.5) and luminosity (0.5 to 3.0) are drawn regardless of star type.
  - The only game-specific fields are `StarSystem.BaseEnemySpawnRate` and `StationData.IsHostileToPlayer`.
- **Consumers** (code search for "galaxy"): `Core/GameManager.cs`, `Game/StartingScenario.cs`, `PlayerModels/PlayerLocation.cs`, `PlayerModels/WorldStateService.cs`, `Run/GalaxyChartLayout.cs`, `Run/GalaxyChartView.cs`, `Run/RunState.cs`, `Run/SightingRules.cs`, `Run/SightingState.cs`, `Run/SightingWorldAdapter.cs`, `SaveSystem/SaveData.cs`, `SaveSystem/SaveGame.cs` and `UI/Gameplay/GameplayUIController.GalaxyChart.cs`.
- **Saves:** format version 3 stores sighting records with system centres and designations, and regenerates the rest from seeds. A change in what a seed produces therefore changes the galaxy behind an existing save.
- **Tests:** `Tests/HumbleLogicTests/` is NUnit, run with `dotnet test`. It links the game's source files with `<Compile Include>` and compiles against a hand-written `UnityEngineStub.cs`, and it already has `CelestialNamingTests`, `GalaxyChartLayoutTests` and `GalaxyChartViewTests`. The stub's `Random` is not Unity's algorithm, so galaxy layouts recorded through it are not the game's.
- **Site:**
  - m4bwav/markdavidrogers-web: ASP.NET Core .NET 10 Razor Pages, Markdown posts in `content/posts/`, React islands.
  - The pattern for experiments is `/labs/<name>`: a Razor page or minimal API group, an optional island, tests, and a rate-limit policy if it takes input.
  - The CSP allows no `unsafe-inline` or `unsafe-eval`.
  - SpaceDeckBuilder2 is already in `content/projects.json` (group games).
  - CI and Preview run on Mark's self-hosted runner `mdr`, and each pull request gets a preview deploy.
  - Posts an agent wrote carry `ai: generated` (or `ai: assisted`) and go in as `draft: true`.
- **Names:** GalaxyGenerator, StarSystemGenerator, SeededGalaxy, ProceduralGalaxy, StarCatalog, GalaxyForge, Starwright and CelestialNaming were all free on nuget.org and as m4bwav/<name> on GitHub (2026-10-02).
- **OpenUPM** (https://openupm.com/docs/adding-upm-package.html, read 2026-10-02):
  - The repository must be public on GitHub. `package.json` can sit at the root or in a subfolder (for example `Packages/com.x.y`).
  - Names are reverse-domain with at least three segments; `com.unity` and `com.github` are not allowed.
  - OpenUPM builds a version from each semver git tag (`v1.2.3` is fine) in 15 to 30 minutes, and the tag must match `package.json`.
  - Submission is a pull request to openupm/openupm made with the package-add form (first-time contributors wait for a moderator, usually within a day).
  - Precompiled DLLs need workarounds, so ship source. There must be a release within 3 months of submission.
- **Competition:** see the background note. The data niche is open: no maintained, engine-neutral, deterministic generator covers everything from galaxy to moons with astronomy naming.

## Decisions (recommendation first; Mark rules at Stop 1, and silence means the recommendation stands)

| ID | Question | Recommendation | Alternatives |
|---|---|---|---|
| D1 | Names | `UniverseGenerator`: repository `m4bwav/UniverseGenerator`, NuGet id `UniverseGenerator`, UPM `com.m4bwav.universe-generator`, root namespace `UniverseGeneration` (a namespace must not share a type's name). It covers every level. Put "galaxy, star system and planet generator" in the description, topics and tags, because "galaxy generator" is the bigger search term. Every name listed was free on nuget.org and GitHub on 2026-10-02. | `GalaxyGenerator` (the search term, but too narrow now), `ProceduralUniverse`, `SeededUniverse`. Avoid `StarForge`: a 2014 game already has the name. |
| D2 | What goes public | The generators, naming, models and the archetype, template and station data. `BaseEnemySpawnRate`, `IsHostileToPlayer` and `ShipFactory` stay in the game, attached through the extension point (D7). | Keep the data tables private and ship only built-in defaults |
| D3 | RNG and existing saves | The package uses its own portable PRNG, specified exactly (for example PCG32, with SplitMix64 to derive a stream per purpose and per object), never `UnityEngine.Random` or `System.Random`. The game takes the package's output, so existing seeds give new galaxies. The game is unreleased: bump the save format and accept that development saves regenerate differently. | Reimplement `UnityEngine.Random` (Xorshift128 and its InitState seeding) to keep today's galaxies. Fragile and Unity-specific. |
| D4 | Seed promise | From 1.0, a given seed and generator version gives the same output on every runtime for the whole major version. Seeds carry the version (for example `v1-8F3K2Q`), and golden-seed tests enforce it. A change that alters output adds a generator version and leaves the old one selectable. | Promise only within a minor version |
| D5 | Float determinism | Decisions come from integer RNG draws. Positions and physical values are computed in `double` and rounded to fixed precision before they are stored or compared. No `Math.Cos`/`Math.Sqrt` result feeds a later random decision. Golden tests compare rounded values on Windows, Linux and macOS, and in Unity (Mono and IL2CPP). | Fixed-point everywhere |
| D6 | Improvements for 1.0 | Settled from the improvement study: Stage 1, the ideas note and the feature matrix. Mark wants more features and options, so the default is to adopt. Everything that changes what a seed produces goes into 1.0 whatever its size. Purely additive features (new optional fields, exports, extra presets) may follow in 1.x, each with a row in the matrix. | Adopt only the cheap output-changing ideas |
| D14 | Levels | Six generators sharing one seed scheme (D17):<br>- Universe: a small set of galaxy clusters and filaments.<br>- Galaxy cluster: a group of galaxies.<br>- Galaxy: shape and type (spiral, barred, elliptical, irregular, ring, cluster), arms, regions, sectors and lanes.<br>- Star system: stars including binaries, orbits, belts, stations, hazards and points of interest.<br>- Planet: physical data, atmosphere, hydrosphere, climate, biomes, resources, rings, day length, tilt, habitability, and surface-map parameters (noise seeds and biome rules for an engine to render; no meshes or textures).<br>- Moon and belt detail.<br><br>Each level works on its own (`StarSystem.Generate(seed)` needs no galaxy) and nested under its parent. Ship all six in 1.0 if Stage 1's estimate fits. Otherwise 1.0 ships galaxy, system and planet, and universe and cluster follow in 1.1 without changing any 1.0 seed. | Galaxy and system only, as in the game today |
| D15 | Ease of use | Three layers:<br>- `Galaxy.Generate("my-seed")` with game-scale defaults, documented in the README's first five lines.<br>- Presets (`Preset.SpaceOpera`, `Preset.Realistic`, `Preset.Pocket`, `Preset.Roguelike`) plus an options object with named properties and XML docs. Every option has a sensible default and a validated range, with an error message that says what to change.<br>- Data tables (C# records, or JSON loaded at run time) and hooks for full control: star types, planet archetypes, naming lists, station types, faction or tag tables.<br><br>The samples, README and wiki teach in that order. An API review in Stage 3 checks that a new user can roll and print a galaxy in under a minute. | Options object only |
| D16 | Game scale and engine fit | Defaults are small:<br>- a universe of 3 to 12 galaxies;<br>- a galaxy of 20 to 200 systems;<br>- a system of up to about 12 planets and their moons.<br><br>Every count is an option with a cap, and `GenerationBudget` (maximum objects, maximum depth) refuses or trims, saying which. Generation is lazy by address: a universe lists its galaxies as lightweight stubs (seed, position, type, name), and a galaxy, system or planet is generated only when asked for, so memory follows what the game opens.<br><br>Coordinates come in a chosen unit (`Units.GameUnits` with a scale factor, or AU, light-years and parsecs) and are kept within float-safe ranges for engines: each level has its own local frame, so no value needs float precision beyond about 1e5. Output is plain data (no Unity types), with a compact serializer and a JSON one. Benchmarks: generating a default galaxy and system stays under a stated time and allocation budget. | Real counts and real units |
| D18 | Purpose: games and writing, not science (Mark, 2026-10-02) | This decision ranks every Stage 1 idea.<br><br>**First:**<br>- story hooks (tags with Enemies, Friends, Complications, Things and Places; points of interest; factions; history events);<br>- names and flavour text (themed pools, grammar-based descriptions);<br>- varied, readable shapes and maps;<br>- gameplay structure (lanes, chokepoints, run-map extraction, danger and reward);<br>- exports writers and game masters use (Markdown for Obsidian, JSON, a player-safe view).<br><br>**Second, only as far as it keeps descriptions consistent:** cheap plausibility formulas (properties from star type, habitable zone, frost line, planet type by zone, believable orbit spacing).<br><br>**Optional:** a `Preset.Plausible` with real star shares and multiplicity.<br><br>**Out:** physics simulation (accretion, n-body, chi-square fits to survey data).<br><br>The default preset uses a documented game-tuned star mix rather than about 73% red dwarfs. Distribution tests check the preset's own targets, not astronomy. The improvement table's "kind" column gains a "for games/writing" value, and the README says plainly that this is a game and fiction generator. | Science-first, with a "fun" preset |
| D17 | Seed scheme | Hierarchical, addressable seeds. Every object's seed comes from its parent's seed plus its path (`universe/2/galaxy/7/system/31/planet/3`) through a hash (SplitMix64 or similar). Every purpose (layout, naming, physics, content) has its own stream, so adding a new level above, a new field or new names never shifts existing results. A seed string carries the generator version (D4) and is URL-safe, so the web page's URLs are addresses: `/labs/universe/v1-8F3K2Q/galaxy/7/system/31`. | One flat seed per call |
| D19 | Size budget (Mark, 2026-10-02: "I love adding tools, but it should remain within the size range that wouldn't make it difficult to use") | Hold the budget in [../notes/2026-10-02-package-size-budget.md](../notes/2026-10-02-package-size-budget.md). Green means under 1 MB nupkg, under 500 KB of compressed data, a UPM package under 2 MB unpacked, under 150 Runtime .cs files, under 300 KB added to a WebGL build, and a default galaxy in under 50 ms. Yellow means warn Mark in the reply with the numbers and a proposed split before adding more. Red fails CI.<br><br>To keep growing without bloat:<br>- **Core plus satellite packages:** `UniverseGenerator` holds the generators and compact default tables. Larger content goes in satellites, for example `UniverseGenerator.Names` (culture name tables), `UniverseGenerator.Grammars` (description text) and `UniverseGenerator.Exports` (Markdown, Obsidian, CSV). An optional `UniverseGenerator.All` metapackage brings everything. Unity gets the same split as separate UPM packages.<br>- **Data:** gzip-compressed and loaded lazily per table, never parsed in a static constructor, never giant C# array initializers.<br>- **Targets:** only netstandard2.0 and net10.0, with `IsTrimmable` and `IsAotCompatible`, and no reflection, so nothing needs `link.xml`.<br>- **Layout:** demos in `Samples~`.<br><br>Open question for Stage 1, because D8 shares one source folder between NuGet and Unity: how tables reach Unity. Embedded resources in a prebuilt data DLL, or `.bytes` TextAssets behind a small loader interface with a Unity adapter asmdef. Recommend the data DLL if OpenUPM accepts it (its docs say precompiled DLLs need workarounds), otherwise TextAssets. | No budget, react when someone complains |
| D7 | Extension point | Data-driven tables (C# records plus optional JSON) for star types, archetypes, templates, station types and naming lists, plus a per-object `Tags` dictionary or a typed hook, so a game can add fields such as spawn rate without forking. | Inheritance hooks only |
| D8 | Targets and source layout | One source folder, `Packages/com.m4bwav.galaxy-generator/Runtime/`, with `.meta` files committed and an asmdef. The NuGet project (`src/GalaxyGenerator/GalaxyGenerator.csproj`) compiles that folder with `<Compile Include>`, targeting `netstandard2.0;net10.0` with LangVersion 9 so Unity 6 can compile the same files. Tests target `net10.0;net48`. One git tag `vX.Y.Z` releases both. | A separate `upm` branch made with `git subtree split` |
| D9 | Repository visibility and timing | Create the repository private, make it public only after Mark approves the 1.0 plan pull request, then release 1.0.0-beta.1. The public history starts clean, with no game history and no copied secrets (scan before going public). | Public from the start |
| D10 | The game's switch to the package (Stage 5) | Yes, after OpenUPM 1.0.0, on a branch of the game repo with a pull request for Mark. The game uses the OpenUPM scoped registry, deletes its copies, moves its own fields to the extension point and updates HumbleLogicTests. This makes "same seed as the game" true, which the web page promises. | Leave the game on its copy for now; drop the promise from the page |
| D11 | Web page (Stage 6) | Server-side, in markdavidrogers-web, with the NuGet package as a normal PackageReference:<br>- `/labs/galaxy`: an intro, buttons to roll a universe, galaxy, system or planet, a seed box and the presets.<br>- One page per D17 address, for example `/labs/galaxy/v1-8F3K2Q`, `.../system/31` and `.../system/31/planet/3`, with universe addresses above them. Each is a Razor page with inline SVG (galaxy map with lanes; system orbits with habitable zone and frost line), so no script is needed and it is CSP-safe; tables of the objects; and the profile codes.<br>- `.json` exports of each.<br>- Presets and main options as query parameters that appear in the canonical URL.<br>- A rate-limit policy and long cache headers (output is deterministic).<br>- Canonical URLs and an `llms.txt` entry.<br>- A link to SpaceDeckBuilder2 on `/games` and to the repository, NuGet and OpenUPM.<br><br>An island for pan and zoom is optional. A TypeScript port is out of scope. | A client-side island with a TypeScript port (needs D3's PRNG in TypeScript and cross-language golden seeds) |
| D12 | Blog post (Stage 6) | `content/posts/<date>-galaxy-generator.md`, in as `draft: true` with `ai: generated` (or `assisted` if Mark rewrites most of it). It covers why the generator was pulled out of the game, how seeds work, pictures from the lab page, links to the repository, NuGet, OpenUPM and the lab, and SpaceDeckBuilder2. Run the everwrite check; Mark edits and sets `draft: false`. | Mark writes it from an outline |
| D13 | Docs | README with badges and a picture (an SVG map from a fixed seed), CHANGELOG, AGENTS.md, SECURITY.md, a `Samples~` folder in the UPM package (a scene that draws a galaxy with Gizmos), and a GitHub wiki through wikiwright (the standing decision for public package repositories). | |

## Stages and stops

### Stage 0: survey and golden capture (the game repository, read-only on `main`)

- [x] Find the local clone of SpaceDeckBuilder2 or clone it. Work in a git worktree on a branch of your own (`capture`), never on the branch a clone is on.
- [x] Read every generator, model and consumer listed above. Record the exact public surface the game uses, every UnityEngine symbol, every place the game stores or compares generated values, and the save fields.
- [x] Golden capture of today's output, the contract for whatever D3 keeps:
  - Galaxy layouts for 100 seeds, recorded inside Unity (batchmode or an EditMode test through the unity-agent-cli skill), because the stub's Random differs.
  - Systems for 200 seeds per template, recorded both in Unity and through HumbleLogicTests, with the two compared (`System.Random` and `Mathf.Lerp` should agree).
  - Names for the same seeds.
  - JSON files committed in the new repository's `tests/Golden/legacy/`, never regenerated.
- [x] Statistics from the capture: star-type shares, planets per system, orbit spacing, station and hazard rates, name collisions. These feed Stage 1.

### Stage 1: improvement study (before anything is public)

- [x] Start from [the ideas note](../notes/2026-10-02-galaxy-generator-improvement-ideas.md). Check each idea against the code and the Stage 0 statistics, and search for anything the note missed (newer tools, papers, talks).
- [x] Prototype the cheap output-changing ideas behind a flag on the extraction branch, and compare before-and-after statistics and pictures (SVG maps of 6 fixed seeds).
- [x] Survey what writers and game masters use, which the 2026-10-02 research did not cover: World Anvil, Campfire, Worldbuilding School, the planet and system description generators on fantasynamegenerators.com and thestoryshack.com, and Obsidian worldbuilding templates. Record what they expect from output: description paragraphs, history, cultures, and export formats.
- [x] Build the feature matrix. Rows are every feature found in any surveyed generator (games, tabletop and web tools, libraries, Unity and Godot assets). Columns are which generators have it, plus this one's status: have, 1.0, 1.x or rejected-with-reason. "Out-do every existing generator" (Mark) means no row stays unaddressed without a reason. A short form of the matrix goes in the README.
- [x] Write the decision table in the plan: each idea adopted for 1.0, deferred to 1.x, or rejected, with effort, whether it changes seeds, and one line of why. Mark wants more features, so rejecting needs a reason (does not fit game scale, is visual rather than data, or is a duplicate).
- [x] Estimate effort per level (D14) and say whether all six fit in 1.0.
- [x] Check the API sketch for D15 by writing the README's first example before any code.

### Stage 2: the plan. **Stop 1: Mark rules on D1 to D19, the Stage 1 table and the feature matrix.**

- [x] Write the plan in the new repository's skeleton (package-modernize `references/plan-skeleton.md`), with this file's decisions and the Stage 1 table. Ask Mark to rule.

### Stage 3: extraction and 1.0 code (new repository, private until D9 says otherwise)

- [ ] Create `m4bwav/<D1 name>` after the ruling. Set up everlast (ai-docs/, AGENTS.md, CLAUDE.md beginning with the @AGENTS.md import) and the package-modernize NuGet templates.
- [ ] Port the code: drop UnityEngine, then add the D3 PRNG, the D17 seed scheme, the D5 numeric rules, the D7 extension point, the D16 budgets, units and lazy addressing, and the adopted Stage 1 improvements.
- [ ] Build the levels in this order, each merged with its own tests before the next starts: star system (today's core), galaxy, planet, then moon and belt detail, then galaxy cluster and universe. Each level's entry point is one line with defaults (D15), for example `Universe.Generate("seed")`, `Galaxy.Generate("seed")`, `StarSystem.Generate("seed")` and `Planet.Generate("seed")`, with an overload taking a preset and an options object.
- [ ] Use the 1.0.0-beta.N versions to ship levels to NuGet as they land (see Stage 4), if Mark wants early feedback.
- [ ] Tests:
  - New golden seeds for generator version 1.
  - Legacy golden tests for whatever D3 keeps.
  - Property tests: names unique per galaxy, orbits increasing, no NaN, determinism across two runs and two threads.
  - Distribution tests: star-type shares within tolerance over 10,000 seeds.
  - A Unity compile check: the Runtime folder compiles with LangVersion 9 and no UnityEngine reference.
  - Scale tests: every preset's defaults stay within D16's object counts and coordinate ranges; a budget overrun gives the documented error; lazy access to `galaxy/7/system/31` produces the same object as full generation.
  - Hierarchy tests: a system generated alone equals the same system generated inside its galaxy, and adding a universe above a galaxy never changes the galaxy.
  - BenchmarkDotNet numbers for D16's budgets, recorded in the CHANGELOG.
  - The D19 size gate in CI: nupkg, each DLL, compressed resource bytes, UPM tarball and unpacked size, Runtime .cs count and lines, and the default-galaxy benchmark, with the build-size delta of an IL2CPP WebGL player in Stage 5. Warn on yellow, fail on red, and write the numbers in each release's CHANGELOG entry. Whenever a pull request moves any metric into yellow, its description says so first, and the run tells Mark in its reply.
- [ ] CI on GitHub-hosted runners (free for a public repository; while it is private, use the self-hosted runner rule in package-modernize `references/private-repo-ci.md`): Windows, Linux and macOS, format check, pack, package validation.
- [ ] `.meta` check: every file under the package folder has one, and GUIDs never change.
- [ ] README, CHANGELOG, samples. **Stop 2: pull request review by Mark.**

### Stage 4: NuGet release

- [ ] **Mark:** make the repository public (D9), after the agent's secret and history scan.
- [ ] **Mark:** create the Trusted Publishing policy on nuget.org for the new id. It needs the scope that allows new packages, because the id does not exist yet. Also create the `nuget` environment's reviewer.
- [ ] Rehearsal with `1.0.0-beta.1`: the version change goes through a pull request, the tag goes on only after CI is green, Mark approves the deployment, then the agent verifies: flat-container index, `listed`, `dotnet nuget verify`, a fresh console project gets the golden output, and the attestation.
- [ ] Then 1.0.0 the same way, and set `PackageValidationBaselineVersion` 1.0.0.

### Stage 5: OpenUPM, then the game's switch

- [ ] Check the package in a scratch Unity 6 project with the unity-agent-cli skill: install by git URL (`...git?path=Packages/com.m4bwav.galaxy-generator#v1.0.0`), open, run the sample, run EditMode tests over the golden seeds in Mono and in an IL2CPP player build.
- [ ] Prepare the openupm/openupm submission with the package-add form. **Mark** opens or approves the pull request from his account; the agent never posts it unasked. After the build, install through the scoped registry and verify.
- [ ] D10: the game's switch, on a branch of SpaceDeckBuilder2. The game repository's CI runs on the self-hosted runner, and its Unity build workflow still needs moving there (memory note, 2026-09-29). Steps: HumbleLogicTests green, a local build with `tools/build.ps1`, and a play check that a new run's galaxy chart matches the lab page for the same seed. **Stop 3: pull request for Mark.**

### Stage 6: the lab page and the blog post (markdavidrogers-web)

- [ ] On a branch, following the site's AGENTS.md rules:
  - D11 pages, endpoints, a rate-limit policy and tests (SiteTests, AgentSurfaceTests).
  - An `llms.txt` entry and a `projects.json` entry (group packages, with a checkable version and date).
  - A link from the SpaceDeckBuilder2 card to the lab.
  - sitewright `mirror`, `audit` and `verify` with screenshots, in both themes.
- [ ] D12 post as `draft: true`, with SVG or WebP images rendered from the lab page for fixed seeds.
- [ ] Pull request, preview deploy, and the preview link checked. **Stop 4: Mark reviews, merges and publishes the post.**

### Stage 7: wrap-up

- [ ] Write the wiki with wikiwright (ask Mark for the first page click at `/wiki/_new` early, and keep working meanwhile).
- [ ] Inventory: a "New packages" row with the NuGet and OpenUPM versions, and the wiki row.
- [ ] HANDOFF.md in the new repository, and a log line here.
- [ ] Lessons for package-modernize: the first brand-new package id, the first OpenUPM release and the shared-source layout. Propose a `references/openupm.md` to the skill in a pull request Mark merges.
- [ ] Launch list for Mark to post himself, never posted by the agent: r/proceduralgeneration first, then r/Unity3D (Showcase or Resources flair as its rules say), r/csharp, r/worldbuilding (Resource flair), then the system subreddits, and the Unity Asset Store free listing as a later option. Each link carries UTM tags.

## Mark's tasks

1. Rule on the decisions at Stop 1.
2. Review and merge the 1.0 pull request (Stop 2), then make the repository public.
3. Create the nuget.org Trusted Publishing policy and approve each deployment.
4. Open or approve the OpenUPM submission.
5. Review the game's pull request (Stop 3).
6. Review the site pull request and edit and publish the post (Stop 4).
7. Click the first wiki page.
8. Make the launch posts.

## Acceptance

- GalaxyGenerator 1.0.0 is on nuget.org and OpenUPM, verified from both.
- Golden seeds give the same output on Windows, Linux and macOS (.NET 10 and .NET Framework 4.8) and in Unity (Mono and IL2CPP).
- The game uses the package, and its galaxy for a seed matches the lab page for that seed.
- `/labs/galaxy/{seed}` is live with JSON export and the blog post is published.
- The wiki, inventory and handoff are written.
