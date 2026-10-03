---
title: "UniverseGenerator 1.0: the plan for the new package (Stage 2 of the galaxy generator extraction)"
kind: plan
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2026-12-02
tags: [universegenerator, plan, nuget, openupm, unity, galaxy-generator, procedural-generation, stage-2, stop-1]
summary: "the living plan for UniverseGenerator 1.0, in the package-modernize skeleton: survey, what the game's generator gets wrong, decisions D1 to D25 with Stage 0 and 1 corrections, the Stage 1 idea table, effort per level, the README's first example, API, tests, size budget and the stages to release; read before Stop 1 rulings or any Stage 3 work"
---

# UniverseGenerator 1.0 plan

The plan for the first brand-new package of the package-modernize runs: the galaxy and star-system generator of SpaceDeckBuilder2, extracted, rebuilt on stable seeds and released as a public MIT C# library on NuGet and OpenUPM, with a lab page and a blog post on markdavidrogers.com. It follows the package-modernize skill (NuGet reference) where a modernization's phases apply, and the extraction plan [2026-10-02-galaxy-generator-extraction.md](2026-10-02-galaxy-generator-extraction.md) for the stages. This file is copied into the new repository's `ai-docs/plans/` when Stage 3 creates it, and kept there. Evidence goes to `ai-docs/log.md`.

Knowledge base: [../../kb/INDEX.md](../../kb/INDEX.md) (feature matrix, sources, rules). Stage 0 record: [../notes/2026-10-02-galaxy-stage0-survey-and-capture.md](../notes/2026-10-02-galaxy-stage0-survey-and-capture.md).

## Status

Active. Stages 0, 1 and 2 done on 2026-10-02. **Stop 1 ruled on 2026-10-02:** Mark chose the recommendation on all eight questions asked (D20 embed the star tables; D18 game-tuned star mix; D16 60 systems with automatic shapes; D21 core plus add-on packages; D14 all six levels, cluster and universe to 1.1 if Stage 3 runs long; D1 UniverseGenerator; D10 the game chooses, saves and shows its galaxy seed in Stage 5; D23 descriptor and tags in the 1.0 core, paragraphs in the Text add-on). The decisions not asked stand as recommended. Stage 3 began 2026-10-02: the repository exists (private) with this plan, `kb/`, the legacy recordings, AGENTS.md and the everlast doc set. The core (seeds, DMath, addresses, JSON writer; golden test identical on .NET 10 and .NET Framework 4.8; CI on the self-hosted runner) is on branch `stage3-core`, draft PR #1, followed the same day by the star system level, the galaxy level with `Universe.At` and the hierarchy test, and the planet level with `Planet.Generate`, and on 2026-10-03 by moon and belt detail and the galaxy cluster level. An F# and Fable version (for npm too) was planned and dropped the same evening; see "Deferred: F# and Fable" below.

## Goal

- The best seeded space generator anyone can install for games and fiction: universe, galaxy cluster, galaxy, star system, planet, moons and belts, every level usable alone or nested.
- Easy start (one line with defaults), deep options (presets, options, tables, hooks), game scale by default.
- One seed, one result, on every runtime: Windows, Linux and macOS on .NET 10 and .NET Framework 4.8, Unity Mono and IL2CPP, for the whole 1.x line, enforced by golden tests.
- Small: green on the D19 budget (nupkg under 1 MB, UPM under 2 MB unpacked, under 300 KB added to a WebGL build, a default galaxy under 50 ms).
- Released through nuget.org Trusted Publishing with Mark's approval, then OpenUPM; the game uses the package; the lab page and post link everything to SpaceDeckBuilder2.

## Where it stands (survey 2026-10-02)

| Fact | Value | Evidence |
|---|---|---|
| Source | SpaceDeckBuilder2 `main` 7bf13f2, `Assets/Scripts/Game/Generators` and `WorldModels`, about 1,400 lines; private repository | Stage 0 note |
| Published | never; a brand-new package id (L-139 `frozen-source-reference` applies: the frozen source is the reference) | |
| Unity | 6000.6.0f1 pinned; the open Editor has a local upgrade to 6000.6.4f1 | ProjectVersion.txt |
| Dependencies | UnityEngine (Vector2, Mathf, Random, ScriptableObject, Resources), RandomNameGeneratorLibrary 2.2.0 for star names | Stage 0 note |
| Game use | new game draws a random galaxy seed and copies names, positions and star types into the save; the galaxy is never regenerated; no seed is stored | GameManager, SightingWorldAdapter |
| Data in practice | 1 system template, 4 planet archetypes with moons, atmosphere and life all off, 4 belt archetypes | Resources/*.asset |
| Golden capture | 100 galaxies and 200 systems in Unity (Mono), the systems again through .NET (net10.0, net48): identical once Mono's double-precision float maths is modelled | capture branch f0c7438 |
| Prototype | the 1.0 seed and maths rules plus shapes, lanes, regions, stars, zones and orbits; 0.3 ms per 40-system galaxy | capture branch a3ede9b, `Tests/GalaxyPrototype/out/` |
| Names free | UniverseGenerator, GalaxyGenerator and the others on nuget.org and GitHub (2026-10-02; recheck before creating) | extraction plan |

## What the game's generator gets wrong, confirmed, and what 1.0 does

1. Red dwarfs weigh 0.83 to 2.49 Suns: mass and luminosity ignore the star type. 1.0 derives them from the spectral class.
2. Danger 10 never occurs and level 1 covers the inner 20%. 1.0 derives danger from hops to the core plus regional noise (1 to 10 all occur in the prototype).
3. Systems can sit 2.7 units apart on a radius of 1,000; no shape, lanes or clusters. 1.0 has shapes, minimum spacing, lanes and chokepoints.
4. 127 of 892 planets sit inside an asteroid belt's band. 1.0 places belts in gaps.
5. Five stations orbit a planet that does not exist; 27 distinct station names in 64 stations. 1.0 picks real bodies and a larger name set.
6. Moons, atmospheres and life never occur (data switches them off). 1.0 generates them by planet kind.
7. Orbits grow by random units with no habitable zone or frost line; gas giants are as likely innermost as outermost. 1.0 spaces orbits by period ratio and picks kinds by zone.
8. Seeds `s` and `-s` produce identical systems (seeded System.Random folds the sign). 1.0 uses its own PRNG on 64-bit seeds.
9. `UnityEngine.Random.InitState` resets the host game's RNG; `Guid.NewGuid` makes station ids unreproducible. 1.0 does neither.
10. Mono evaluates float expressions in double precision, so Editor and player builds may disagree. 1.0 decides on integers and rounds stored values.

None of these is kept: the legacy recordings are a record, not a contract (D3).

## Decisions (recommendation first; ruled at Stop 1 on 2026-10-02: every recommendation stands)

Stage 0 and 1 changes are marked **(changed)** or **(new)**.

| # | Question | Recommendation | Why | Alternative |
|---|---|---|---|---|
| D1 | Names | `UniverseGenerator` (repository m4bwav/UniverseGenerator, NuGet id UniverseGenerator, UPM `com.m4bwav.universe-generator`, namespace `UniverseGeneration`); "galaxy, star system and planet generator" in the description and tags | covers every level; the search term goes in the description | GalaxyGenerator, ProceduralUniverse, SeededUniverse |
| D2 | What goes public | generators, naming, models, tables; game-only fields (enemy spawn rate, enemy levels, hostility, terrain profile) stay in the game via D7 | | keep tables private |
| D3 | RNG and saves **(changed)** | own PCG32 with SplitMix64-derived streams; no legacy mode. Stage 0 found that saves store copies, not seeds, so no existing save changes and no save-format bump is needed for it | proven portable in the prototype; the legacy RNG halves the seed space | reimplement UnityEngine.Random and System.Random to keep today's galaxies |
| D4 | Seed promise | same seed and generator version, same output on every runtime for the major version; seeds carry the version (`v1-...`); golden files per version | | within a minor only |
| D5 | Float determinism **(changed)** | integer decisions; maths from + - * / and sqrt only (DMath: Exp, Log, Pow, Sin, Cos, Atan2 by series); doubles rounded before storing; test an ARM64 IL2CPP build for fused multiply-add | Mono's double-precision float maths measured in Stage 0 | fixed point |
| D6 | Improvements for 1.0 | the Stage 1 table below: everything that changes seeds is in 1.0; additive features may follow in 1.x | | cheap ideas only |
| D7 | Extension point | tables as C# records plus JSON by id, a per-object Tags dictionary, generation hooks | | inheritance only |
| D8 | Source layout | one source folder `Packages/com.m4bwav.universe-generator/Runtime/` (asmdef with noEngineReferences), compiled by src/UniverseGenerator/UniverseGenerator.csproj for `netstandard2.0;net10.0`, LangVersion 9; tests `net10.0;net48`; one tag releases both | | subtree split branch |
| D9 | Visibility | private until Mark approves the Stage 3 pull request and the secret scan; then public, then 1.0.0-beta.1 | | public at once |
| D10 | Game switch **(changed)** | yes, after OpenUPM 1.0.0; and the game starts choosing, saving and showing its galaxy seed (it stores none today), so "same seed as the lab page" can be checked | Stage 0 | leave the game on its copy |
| D11 | Lab page | server-side Razor pages with inline SVG in markdavidrogers-web; one page per address; JSON export; rate limit and long cache | | TypeScript port |
| D12 | Blog post | `draft: true`, `ai: generated`; Mark edits and publishes | | outline only |
| D13 | Docs | README, CHANGELOG, AGENTS.md, SECURITY.md, Samples~ (Gizmos scene), wiki by wikiwright | | |
| D14 | Levels **(changed)** | all six in 1.0, built in the order system, galaxy, planet, moon and belt detail, cluster, universe; if Stage 3 runs long, cluster and universe move to 1.1 (no seed changes, D17). Estimate below: about 21 working days | | galaxy, system and planet only |
| D15 | Ease of use | three layers; the README's first example is below and needs no reading | | options only |
| D16 | Game scale **(changed)** | defaults: galaxy of **60** systems (prototype: 40 is too few for a spiral to read; ellipticals, rings and irregulars read at 40); `Shape = Auto` picks only shapes that read at the requested count (spirals and bars from 80); every count capped by `GenerationBudget`; coordinates per level within float-safe ranges; lazily generated by address | Stage 1 contact sheets | real counts and units |
| D17 | Seed scheme | hierarchical: child = SplitMix64(parent, label, index); one stream per purpose; versioned, URL-safe seed strings that are addresses | prototyped | flat seeds |
| D18 | Games and writing first | default star mix game-tuned (M 31%, K 21%, G 14%, F 10%, white dwarf 7%, A 6%, giant 5%, B 3%, rest under 1% each); `Preset.Plausible` uses the real shares (M 73%); story hooks, names, readable maps first | prototyped; the game today never rolls blue stars or giants | science first |
| D19 | Size budget | as the size-budget note; CI gate; warn at yellow; never red | | react later |
| D20 | Star names **(new)** | embed RandomNameGeneratorLibrary's star tables (Mark's own MIT code; 61 KB raw, 17 KB gzipped) in the core, drawn with the package's PCG32; no dependency on that package | the whole package is 954 KB and takes a System.Random, which breaks D3 and adds about 1 MB to every install and WebGL build | depend on RandomNameGeneratorLibrary |
| D21 | Package split **(new)** | `UniverseGenerator` core at 1.0; satellites additive in 1.x: `UniverseGenerator.Names` (culture name sets, constellations), `UniverseGenerator.Text` (description grammar, history, cultures, creatures), `UniverseGenerator.Exports` (Markdown/Obsidian, CSV, SVG, Traveller, VTT); `UniverseGenerator.All` metapackage; the same split as UPM packages | keeps the core green on every metric while features grow | one package |
| D22 | Knowledge base **(new)** | the research lives in the repository's `kb/` (moved from the maintainer's private run record): the generated feature matrix, sources and rules; the wiki publishes the matrix, the rules and how-to pages | Mark, 2026-10-02: "The AI should store a knowledge base in md files and/or the repo wiki" | wiki only |
| D23 | Text for writers **(new)** | 1.0 core gives each object a one-line descriptor from its data ("a cold ocean world under a red dwarf") and story tags on their own stream; full paragraphs, history and GM secrets come in `UniverseGenerator.Text` 1.x, built as a tag-filtered grammar so text never contradicts data | Fantasy Name Generators' prose is what writers use most; filtered grammar (Improv) keeps it honest | paragraphs in 1.0 core |
| D24 | Landmarks and variety **(new)** | every system gets at least one landmark (an unusual star, ringed giant, living world, belt, ruin or hazard), every region a monument, every galaxy a few beacons; a Weirdness slider sets an outlier share (default 5%) | the prototype's 50-system sheet still looks alike at a glance; Hervé et al. 2025, Kreminski 2023 | none |
| D25 | Legacy recordings **(new)** | keep them in the new repository's `tests/Golden/legacy/` as the record of the game before the switch; no test compares against them | D3 drops legacy output | delete |

## Stage 1: every idea, with its decision

Effort: S under a day, M one to three days, L more. Seeds: "yes" means it must be in 1.0 (or behind a new generator version), because adding it later would change existing seeds' output. Size: added compressed size, estimated. The full feature list against every surveyed generator is the [feature matrix](../../kb/features/feature-matrix.md) (176 sources, 128 features found, every one with a status: 58 for 1.0, 51 for 1.x, 16 the game already has, 8 rejected with a reason).

**Determinism and API**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| 1 | Own PRNG (PCG32) | 1.0 | yes | S | 1 KB core | prototyped |
| 2 | Seeds by SplitMix64 from parent, label, index; purposes by FNV-1a | 1.0 | yes | M | 1 KB | prototyped |
| 3 | Seed plus generator version as identity | 1.0 | yes | S | 1 KB | D4 |
| 4 | Integer decisions, deterministic maths, rounded storage | 1.0 | yes | M | 3 KB | D5, Mono finding |
| 5 | A stream per optional feature | 1.0 | yes | S | 0 | prototyped |
| 30 | Hierarchy generated on request, plain records, float adapters for Unity | 1.0 | yes | M | 4 KB | D16 |
| 31 | Golden tests on every runtime, property tests, contact sheets | 1.0 | - | M | tests | prototype sheets are the review tool |
| A14 | Explain(), hooks, tables by file, options code | 1.0 (hooks, tables, code); 1.x (Explain) | no | M | 5 KB | D7, D15 |
| N2 | Detail levels on isolated streams | 1.0 | yes | S | 0 | falls out of D17 |
| N7 | Seed search (FindSeeds) | 1.x | no | S | 1 KB | additive |
| N3 | Fill around fixed facts | 1.x | no | M | 4 KB | separate entry point |

**Galaxy**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| 6 | Minimum spacing | 1.0 | yes | S | 1 KB | prototyped |
| 7 | Shapes as density functions; image density map | 1.0 (five shapes plus colliding pair); 1.x (custom map hook) | yes | M | 6 KB | prototyped |
| 8 | Lanes: RNG plus Gabriel share; clusters joined by highways | 1.0 | yes | M | 4 KB | prototyped |
| 9 | Chokepoints, hops, dead ends as data | 1.0 | no | S | 2 KB | prototyped |
| 10 | Named regions with traits; danger from hops plus noise | 1.0 | yes | M | 3 KB | prototyped |
| 11 | Age, abundance, metallicity | 1.0 (region age); 1.x (metallicity gradient in Plausible) | yes | S | 1 KB | prototyped coarsely |
| 22 | Factions grown over lanes | 1.0 | no (own stream) | M | 4 KB | story first |
| 23 | Points of interest with placement rules | 1.0 | no (own stream) | M | 4 KB | story first |
| 24 | Run-map extraction | 1.x | no | M | 4 KB | the game's first use; additive |
| 25 | Pinned hand-made systems | 1.x | no | M | 2 KB | additive |
| A9 | Gate requirements on links | 1.0 | yes | M | 2 KB | universe and galaxy links |
| A11 | Sectors with danger and power tags | 1.0 (via regions) | yes | S | 1 KB | merged into 10 |
| N4 | Start fairness, contested richness | 1.x | no | M | 3 KB | a pass that reads the galaxy |
| N11 | Travel terrain instead of lanes | 1.x | no | M | 3 KB | option LaneMode |
| N13 | Radial progression, barrier rings | 1.x | no | S | 1 KB | pure function of position |
| N5 / D24 | Landmark guarantee, outlier quota | 1.0 | yes | M | 3 KB | the oatmeal answer |

**Stars**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| 12 | Spectral classes with derived properties; game-tuned mix | 1.0 | yes | S | 2 KB | prototyped (D18) |
| 13 | Habitable zone, frost line | 1.0 | yes | S | 0 | prototyped |
| 14 | Real multiples: close or wide, planet limit, mass budget | 1.0 (close or wide pairs affect planets); 1.x (a_c limit in Plausible) | yes | M | 2 KB | prototyped partly |
| 15 | Age decides giants and white dwarfs | 1.0 | yes | S | 1 KB | region age |

**Systems**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| 16 | Orbits by period ratio, peas in a pod, monotony test | 1.0 | yes | M | 2 KB | prototyped spacing |
| 17 | Kinds by zone and star type | 1.0 | yes | S | 2 KB | prototyped |
| 18 | Coherent derived stats | 1.0 | yes | S | 1 KB | |
| 19 | Profile code | 1.x | no | S | 1 KB | derived |
| 20 | Moons and rings by planet kind, Roche and Hill | 1.0 | yes | S | 1 KB | prototyped counts |
| N1 | Exoplanet demographic rules (radius valley, hot Neptune desert, giants near 3 au, inner-outer link, binaries suppress planets) | 1.0 | yes | M | 2 KB | cheap; must be in 1.0 |
| 21 | Story tags per world or system | 1.0 (small table in core) | no (own stream) | M | 8 KB core | the stand-out for writers |

**Planets**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| P1 | Traits and anomalies | 1.0 | no | S | 6 KB | story |
| P2 | Readable descriptor; paragraph | 1.0 (descriptor); 1.x (paragraph, Text) | no | S | 2 KB | D23 |
| P3 | Life ladder, flora and fauna density | 1.0 | yes | S | 1 KB | |
| P4 | Hazard tier from biome and weather | 1.0 | no | S | 1 KB | derived |
| P5 | Society layer | 1.x | no | M | 4 KB | optional |
| P6 | Resources graded 1 to 5 | 1.0 | yes | M | 2 KB | the game uses resources |
| P7 | Flags and profile code | 1.x | no | S | 1 KB | derived |
| P8 | Composition classes | 1.0 | yes | S | 1 KB | |
| P9 | Radius, density, gravity, escape velocity from mass | 1.0 | yes | S | 1 KB | |
| P10 | Temperature from orbit, albedo, greenhouse | 1.0 | yes | S | 1 KB | |
| P11 | Atmosphere by the Jeans rule, with a why note | 1.0 | yes | M | 2 KB | |
| P12 | Rotation, tilt, tidal lock | 1.0 | yes | M | 1 KB | |
| P13 | Hydrosphere | 1.0 | yes | S | 1 KB | |
| P14 | Gas giant classes, rings, moon zone | 1.0 | yes | M | 1 KB | |
| P15 | Climate bands to biome shares | 1.0 | yes | M | 3 KB | |
| P16 | Surface parameter pack | 1.x | no | M | 3 KB | own stream |
| P17 | Habitability per species, similarity score | 1.0 | no | S | 1 KB | derived |
| A5 | Signature creature archetypes | 1.x | no | S | Text | writers |
| A6 | Points of interest on planets, lazily, with coordinates | 1.x | no | M | 3 KB | own stream |
| A7 | Day side, terminator, night side | 1.0 | yes | S | 0 | with P12 |
| N12 | Life and biomes by condition tables, colour variants | 1.x | no | M | 3 KB | own stream |
| writers | Sky view and local calendar | 1.x | no | S | 3 KB | the gap writers noticed |

**Universe and cluster**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| U1 | Landmark slots per universe | 1.0 | yes | S | 1 KB | D24 |
| U2 | Travel network: long links, hub gates, tiers | 1.0 | yes | M | 3 KB | with N10 |
| U3 | Two distance frames, travel-time tiers | 1.0 | no | S | 1 KB | D16 units |
| U4 | Galaxy flavour biasing planet tables | 1.0 | yes | S | 1 KB | |
| U5 | Group or cluster environment | 1.0 | yes | M | 2 KB | |
| U6 | Galaxy type code driving the shape | 1.0 | yes | S | 1 KB | |
| U7 | Age and richness; epoch | 1.0 | yes | M | 1 KB | |
| U8 | Active cores with a hazard radius | 1.0 | yes | S | 1 KB | story |
| U9 | Satellite dwarf galaxies | 1.0 | yes | S | 1 KB | |
| U10 | Merging pairs with tidal tails | 1.0 | yes | M | 2 KB | colliding-pair shape |
| U11 | Cosmic-web skeleton, voids | 1.0 | yes | M | 2 KB | |
| N10 | Ring of wormholes so every galaxy is reachable (Star Ruler 2, MIT) | 1.0 | yes | S | 1 KB | proven in shipped code |
| A8 | Universe lore, precursors, a mystery thread | 1.x | no | M | Text | own stream |
| A10 | Cozy and Epic scale presets | 1.0 | no | S | 1 KB | presets |
| A12 | Tidal-tail frontier as a lawless zone | 1.0 | yes | M | 1 KB | with U10 |
| A13 | One universe landmark in every skybox | 1.0 | yes | S | 1 KB | D24 |

**Names, text, exports**

| # | Idea | Decision | Seeds | Effort | Size, package | Why |
|---|---|---|---|---|---|---|
| 26 | Catalogue names (kept), themed pools per region, decodable names | 1.0 (catalogue with embedded tables, D20); 1.x (themed culture pools in Names, decodable style) | yes (default scheme) | M | 17 KB core | |
| N8 | Culture phonology names (Syllabore, MIT) | 1.x | no | M | Names | |
| N9 | Names that encode the address | 1.x | no | S | 2 KB | option |
| N14 | Constellations and Bayer letters from home | 1.x | no | M | Names | |
| N6 | Tag-filtered description grammar | 1.x | no | M | Text | D23 |
| N15, A1 | History: generate, then explain | 1.x | no | M | Text | |
| A2 | One-liner, paragraph, GM secret per object | 1.0 (one-liner); 1.x (rest) | no | M | Text | D23 |
| 27, A3 | Exports: JSON schema (1.0); SVG, Markdown/Obsidian, CSV, Traveller, VTT, player-safe | 1.0 (JSON); 1.x (the rest) | no | M | 6 KB core, Exports | |
| 28 | Show your working | 1.x | no | M | 4 KB | additive |
| 29 | Versioned content packs with a hash | 1.x | no | M | 2 KB | |
| A4 | Travel zone (green, amber, red) | 1.x | no | S | 1 KB | with society |
| N15+ | Time as a coordinate (positions at t) | 1.x | no | S | 1 KB | |

**Rejected, with the reason**

| Idea | Reason |
|---|---|
| Physics simulation (accretion, n-body), chi-square fits to astronomy | D18: game and fiction scale; test the preset's own targets instead |
| 3D rendering, meshes, textures | visual, not data; engines render |
| Real star catalogues (SIMBAD, HYG) | large data; game scale; a few real stars go in as pinned systems |
| Visited-place cache | versioned seeds keep old places without storage |
| Language-model text | not deterministic, needs a service |
| Live player view, visibility helpers, NPC generation, import of other tools' formats | belong to the game, the VTT, RandomNameGeneratorLibrary or a converter |

## Effort per level (D14)

| Level | Content | Estimate |
|---|---|---|
| Core | seeds, DMath, options and validation, presets, tables by id, budgets, units, address and lazily generated access, JSON writer, golden and property test harness, size gate | 5 days |
| Star system | stars, multiples, zones, orbits, kinds, moons and rings, belts, stations, names (embedded tables), landmarks, tags | 4 days |
| Galaxy | shapes, spacing, lanes, chokepoints, regions, danger, factions, points of interest, hazards | 4 days |
| Planet | physics, temperature, atmosphere, hydrosphere, rotation, climate and biomes, life, traits, resources, descriptor | 4 days |
| Moon and belt detail | moon physics by parent, belt composition and resources | 1.5 days |
| Cluster and universe | groups and clusters, galaxy types, active cores, satellites, mergers, cosmic web, wormhole ring, landmarks | 3 days |
| Total | about 21 working days of agent time across several sessions; cluster and universe may move to 1.1 without seed changes | |

## Proposed public API (1.0)

The README's first example, written before any code (D15). A newcomer copies it and gets a galaxy:

```csharp
using UniverseGeneration;

var galaxy = Galaxy.Generate("my-seed");
foreach (var system in galaxy.Systems)
    Console.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
```

The second layer, a preset with named changes, lazily generated access and addresses:

```csharp
var galaxy = Galaxy.Generate("my-seed", Preset.SpaceOpera with { Systems = 120, Shape = GalaxyShape.Barred });
var planet = galaxy.System(31).Planet(2);   // generated on request; same result as full generation
Console.WriteLine(planet.Descriptor);        // "a cold ocean world under a red dwarf"
Console.WriteLine(planet.Address);           // v1-my-seed/galaxy/system/31/planet/2
var same = Universe.At(planet.Address);      // any object regenerates alone from its address
```

Every level has the same pair of entry points: `Universe.Generate`, `GalaxyCluster.Generate`, `Galaxy.Generate`, `StarSystem.Generate`, `Planet.Generate`, each taking a seed text (or a 64-bit seed) and optionally a preset or options. Options validate with readable messages ("Systems must be 1 to 2000; you asked for 0"). Output is plain records (no engine types), with coordinates as doubles in the chosen units and a float adapter in the Unity package. Throws ArgumentException only for invalid options; generation itself never throws.

## Build and package specifics

- Layout per D8; the package-modernize NuGet templates (`Library.csproj.template`, `Directory.Build.props`, `global.json`, workflows) copied, not written from memory.
- Embedded data **(changed 2026-10-02)**: the star tables as string constants in a generated source file (`Runtime/Names/StarTables.cs`, written by `scripts/embed-star-tables.py`, 72 KB of source), split on first use. The same file compiles for NuGet and in Unity, so there is no resource loader, no TextAsset and no runtime-specific file reading; the size-budget note's open question is answered. The DLL grows by about 120 KB (UTF-16 strings), which the size gate measures.
- `IsTrimmable` and `IsAotCompatible` on net10.0; no reflection; a hand-written JSON writer.
- CI on the self-hosted runner while private (package-modernize references/private-repo-ci.md; runner `universe` registered 2026-10-02, first run green), GitHub-hosted Windows, Linux and macOS once public.

## Stages from here

### Stage 3: new repository and 1.0 code
- [x] Create m4bwav/UniverseGenerator (or the ruled name), private; everlast, AGENTS.md, CLAUDE.md with the AGENTS.md import line; copy this plan, the knowledge base (to `kb/`) and the legacy recordings (to `tests/Golden/legacy/`).
- [x] Core with its golden test and CI (2026-10-02; branch `stage3-core`, draft PR #1; one branch and pull request for all of Stage 3, a commit per step).
- [x] Star system level (2026-10-02): stars, companions, two-chain orbits, kinds with the exoplanet rules, moons, rings, belts, stations, names from the embedded tables, landmarks, tags, descriptors; property tests over 10,000 seeds, distribution tests, golden `system.json` identical on net10.0 and net48; size gate green (DLL 308.5 KB, nupkg 182.9 KB).
- [x] Galaxy level (2026-10-02): the prototype's shapes, spacing, lanes, chokepoints, regions and danger on a spatial grid; the skeleton pass giving each system its context (unique name, region age, danger); `Galaxy.Generate`, `galaxy.Map`, lazily generated `galaxy.Systems` and `galaxy.System(i)`; `Universe.At(address)`; hierarchy test over 4,929 objects; golden `galaxy.json` identical on net10.0 and net48. Record: [the galaxy level note](../notes/2026-10-02-galaxy-level-design.md). Still owed in this level (own streams, no seed changes): factions, points of interest, hazards, D24 monuments and beacons.
- [x] Planet level (2026-10-02, d673430): ideas P1 to P4, P6, P8 to P15, P17 and A7 on the planet's own streams (gravity, escape velocity, density, insolation, albedo, temperature with day and night sides, atmosphere by the Jeans rule with a why note, water and ice, rotation, tilt and tidal lock, six climate bands, biomes, life, traits and anomalies, resources 0 to 5, hazard tier, Earth Similarity Index, habitability per species, a summary line); `Planet.Generate` for a lone planet (`v1-seed/planet`) and `Universe.At` reaching it; golden `planet.json` identical on net10.0 and net48, `system.json` and `galaxy.json` unchanged; size gate green (DLL 378.0 KB, nupkg 252.3 KB). Record: [the planet level note](../notes/2026-10-02-planet-level-design.md).
- [x] Moon and belt detail (2026-10-03, 49c5fa5): every moon's physics from its kind and radius, orbit and period around its planet, tidal heat (Peale, bounded by the kind), temperature from the planet's light, air by the planet level's Jeans rule (hazy and garden moons keep theirs by renewal), hidden oceans, life, climate bands and biomes, traits, resources, hazards (the giant's radiation), habitability, descriptor and summary; belts gain addresses and seeds (`.../belt/k`), names, composition, mass, largest body, temperature, resources and a summary; `Universe.At` reaches belts; golden `moon-belt.json` identical on net10.0 and net48, the other four unchanged; 130 tests; size gate green (DLL 400.0 KB, nupkg 275.8 KB). Record: [the moon and belt level note](../notes/2026-10-03-moon-and-belt-level-design.md).
- [x] Galaxy cluster level (2026-10-03, 64e90de): ideas U5 to U9, U2 and N10; `GalaxyCluster.Generate` makes a group (two big spirals, satellites, dwarfs) or a cluster (a giant elliptical at the centre, ellipticals inside and spirals at the edge) as a cheap map with type codes driving each galaxy's shape, ages, richness (the gas giant weight) and active cores (danger within a hazard radius), joined by gates to the majors, a wormhole ring and satellite tethers; each galaxy is generated on demand with its context (`v1-seed/cluster/galaxy/i`), and opens its links at its core or its facing edge (`galaxy.Gates`); lone galaxies keep their maps and gain a name, type code and descriptor; `Universe.At` reaches clusters; golden `cluster.json` identical on net10.0 and net48, the other five unchanged; 144 tests; size gate green (DLL 435.0 KB, nupkg 308.5 KB). Record: [the galaxy cluster level note](../notes/2026-10-03-galaxy-cluster-level-design.md).
- [ ] The universe level (D14's last), with its tests; the prototype (`Tests/GalaxyPrototype` on the game repository's capture branch) is the starting point, and the star system design is [the 2026-10-02 note](../notes/2026-10-02-star-system-level-design.md).
- [ ] Golden seeds v1, property tests, distribution tests against the preset's own targets, hierarchy tests, the Unity compile check, scale tests, BenchmarkDotNet, the D19 size gate.
- [ ] README (the example above first, then the feature matrix's short form), CHANGELOG, samples. **Stop 2: pull request review.**

### Stage 4: NuGet release
- [ ] Mark makes the repository public after the secret and history scan; creates the Trusted Publishing policy (scope allowing new packages); approves each deployment.
- [ ] 1.0.0-beta.1 rehearsal, verified from nuget.org; then 1.0.0; PackageValidationBaselineVersion 1.0.0.

### Stage 5: OpenUPM, then the game's switch
- [ ] Scratch Unity 6 project: install by git URL, run the sample, EditMode golden tests in Mono and an IL2CPP player (one ARM64 build for the fused multiply-add check); measure the WebGL build-size delta.
- [ ] Prepare the OpenUPM submission; Mark opens it.
- [ ] The game's switch on a branch, with the galaxy seed saved and shown (D10). **Stop 3.**

### Stage 6: lab page and post (markdavidrogers-web). **Stop 4.**

### Stage 7: wiki (from `kb/`), inventory, handoff, skill lessons, launch list.

## Test strategy

| Layer | What it proves | How | Runs where |
|---|---|---|---|
| Golden v1 | a seed's output never changes within 1.x | committed JSON per level for fixed seeds, compared as text after rounding | net10.0 and net48 on Windows, Linux, macOS; Unity Mono and IL2CPP |
| Canary | the golden test can fail | a planted change in a rule turns it red | CI once, logged |
| Properties | no NaN, names unique per galaxy, orbits increasing, lanes connected, belts clear of planets, stations orbit real bodies | NUnit over 10,000 seeds | all |
| Hierarchy | a system alone equals the same system in its galaxy; a level above never changes a galaxy | address round trips | all |
| Distributions | the preset's own targets (star mix, planets per system, danger spread) within tolerance | 10,000 seeds | Linux |
| Scale and budget | presets stay inside D16 counts and ranges; budget errors are readable | NUnit | all |
| Size | D19 metrics | CI gate script | Linux |
| Speed | default galaxy under 50 ms cold | BenchmarkDotNet | Windows |

## Security

No network, no file access beyond its own embedded tables, no reflection, no dependencies. Publishing only through Trusted Publishing from `release.yml` with the `nuget` environment's reviewer. The public history starts clean: the game repository's history is never copied, and a secret and name scan runs before the repository goes public.

## Verification checklist

| Claim | Command or place | Expected |
|---|---|---|
| Same seed everywhere | the golden test on every OS, net48 and Unity Mono and IL2CPP | green |
| Size green | the CI size gate | every metric green |
| On nuget.org and OpenUPM | flat container, registration `listed`, OpenUPM package page | 1.0.0 |
| Game matches the lab page | the same seed in the game's galaxy chart and on /labs/galaxy | same names and positions |

## Risks and open points

- WebGL size: a core of about 200 KB of IL may add 120 to 250 KB Brotli to a WebGL build; green, but near the 300 KB line. Measured in Stage 5; satellites keep text and exports out of the core.
- IL2CPP on ARM64 may contract multiply-add (D5); the golden test there decides.
- OpenUPM and embedded data: TextAssets unless a data DLL proves fine.
- The effort estimate (21 days) is a guess from the prototype; cluster and universe are the release valve.

## Deferred: F# and Fable (2026-10-02)

Mark asked whether the package could be F# compiled by Fable, so one source also ships to npm (his research note `Ai/typescript-csharp-dual-packages-2026.md`, route 4), then chose the same evening to keep C# and perhaps port later. What a later port should know (checked 2026-10-02):

- Feasible: the determinism rules already avoid what differs between .NET and JavaScript. Fable 5.18.0 (2026-09-25) maps int64 and uint64 to BigInt, which wraps exactly, but compiles int32 and uint32 to JS numbers "without expected truncation to 32 bits on overflow" (fable.io compatibility page), so all wrapping arithmetic must stay in 64-bit. No float32, no BitConverter in DMath, no sprintf in the library.
- Costs: FSharp.Core becomes the NuGet dependency (netstandard2.0 DLL 2.39 MB in 10.0.100, 3.07 MB in 6.0.7); Unity cannot compile F#, so the UPM package would ship DLLs plus FSharp.Core and turn yellow on size; C#'s `with` does not work on F# records; about five more days.
- The npm name `universe-generator` was free. The C# core's `tests/Golden/v1/core.json` is the check a port must pass bit for bit in Node.

## Next single action

The universe level on branch `stage3-core` (D14's last; the release valve to 1.1 if Stage 3 runs long, without seed changes): write a short design note first, as for the cluster level, covering `Universe.Generate` beside `Universe.At` (the static class becomes the record, free while nothing is released), ideas U1 (landmark slots), U3 (two distance frames, travel-time tiers), U10 (merging pairs with tidal tails, A12 the lawless frontier), U11 (the cosmic web: nodes, filaments, named voids), A13 and the epoch setting from U7, and how a cluster inside a universe (`v1-seed/universe/cluster/i`) takes its kind and epoch from the universe while `GalaxyCluster.Generate` alone keeps its output (`cluster.json` must not move). Before beta, also put the seed-changing questions to Mark: the two galaxy ones in [the galaxy level note](../notes/2026-10-02-galaxy-level-design.md) (the colliding pair in `Auto`, region themes steering tags), the moon kinds the system level draws without size or distance ([the moon and belt level note](../notes/2026-10-03-moon-and-belt-level-design.md), "Open for tuning") and the cluster counts and shares ([the galaxy cluster level note](../notes/2026-10-03-galaxy-cluster-level-design.md), "Open for tuning").
