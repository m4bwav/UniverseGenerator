---
title: "Engine assets and libraries that generate galaxies, systems or planets"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, sources, unity, openupm, godot, unreal, libraries, competition]
summary: "read when comparing the package with Unity Asset Store, OpenUPM, Godot, Unreal and library rivals: 36 data generators with prices, dates, licences, popularity and feature keys, reviewers' complaints, and what nobody offers"
---

# Engine assets, packages and libraries that generate space

Research date: 2026-10-02. Scope: Unity Asset Store, OpenUPM, GitHub Unity projects, Godot Asset Library and Godot Asset Store, Unreal Fab, and engine-neutral libraries (C#/NuGet, Rust crates, npm, Python) that generate galaxies, star systems, planets or moons.

## How this was gathered

- Unity Asset Store pages were downloaded and the product JSON embedded in each page was parsed (name, description, key features, price, rating count, current version and its publish date). Review text came from the server-rendered `/reviews` page of each asset. Favourite counts came from WebFetch of the store page and exist only for four assets.
- Fab listings were read through Fab's own listing JSON (`/i/listings/<uid>`), which gives the description, price, rating and changelog dates.
- Godot Asset Library through its public API; one Godot Asset Store page through WebFetch.
- GitHub READMEs fetched raw; stars, last push date and licence from the GitHub API. npm, crates.io, NuGet and OpenUPM data from their registry APIs.
- "Last update" is the asset's current-version publish date (Unity), the newest changelog entry (Fab), the last push (GitHub) or the newest registry publish (packages).
- A feature key is marked only when a primary page says so. Claims that come only from search-result snippets are labelled "(snippet only)".
- Licence warnings: GPL-3 (artak10t/ProceduralGalaxy, wisepythagoras/galaxy-gen, rhogen, Infinite Discoveries), AGPL-3 (coilyco/galaxy-gen), PolyForm Noncommercial (The Atlas), "all rights reserved" (GalexJS) and repositories with no licence at all (StarformNET, LilMako17, Jimbly, notakamihe, Retalyx, BenedictVig) must never be copied from. Take ideas only. MIT, BSD-3 and Apache-2.0 code could be reused with attribution, but the plan is still ideas only.

## Feature matrix

Popularity: Unity = rating count (average) and favourites where known; Fab = rating count (average); GitHub = stars; packages = downloads.

### Data generators (they output systems, stars, planets or maps as data)

| Generator | Kind | URL | Price / licence | Last update | Popularity | Feature keys |
|---|---|---|---|---|---|---|
| Complete Galaxy Generator (Much Coffee Studios) | Unity asset | https://assetstore.unity.com/packages/templates/systems/complete-galaxy-generator-277304 | $15.99, Asset Store EULA | v1.1.0, 2025-11-24 (first 2024-04-10) | 1 review (5 stars), 26 favourites | galaxy-shapes, spiral-arm-count, min-spacing, options-sliders, lanes-graph, lane-density, connected-graph, export-json, editor-preview, map-3d, unity-integration, price |
| Persistent Galaxy Generator with Solar Systems and Planets (KassaK) | Unity asset | https://assetstore.unity.com/packages/templates/systems/persistent-galaxy-generator-with-solar-systems-and-planets-114205 | $19.99, Asset Store EULA | v3.0, 2022-08-22 (first 2018-09-14) | 7 reviews (avg 4), 247 favourites | galaxy-shapes, spiral-arm-count, options-sliders, planet-types, data-tables-editable, names-fantasy, export-json, map-3d, unity-integration, price |
| Galaxy Constructor (Dmitry Andreev) | Unity asset | https://assetstore.unity.com/packages/vfx/particles/galaxy-constructor-329053 | $19, Asset Store EULA, full C# and HLSL source | v2.0 listing, 2026-09-24 (first 2025-09-10) | 5 reviews (5 stars) | seed-input, custom-shape-map, lazy-generation, options-sliders, data-tables-editable, regions-sectors, galaxy-age-metallicity, spectral-classes, star-physics, star-remnants, star-age, star-colour, moons, asteroid-belts, comets, names-catalogue, names-fantasy, sub-seeds, visited-cache, export-csv, editor-preview, map-3d, unity-integration, price |
| Galaxy Creator 1.3 (Patryk Łazowski) | Unity asset | https://assetstore.unity.com/packages/tools/video/galaxy-creator-1-3-194518 | $7, Asset Store EULA | v1.3, 2021-09-01 | 3 reviews (avg 4) | options-sliders, asteroid-belts, rings, unity-integration, price |
| Solar System Generator (Thunder Entertainment) | Unity asset | https://assetstore.unity.com/packages/3d/environments/sci-fi/solar-system-generator-130882 | $19, Asset Store EULA | v1.0, 2018-10-18 | 1 rating, 25 favourites | options-sliders, moons, rings, asteroid-belts, unity-integration, price |
| 3D Volumetric Procedural Galaxy with Selectable Stars (Aquinos creations) | Unity asset | https://assetstore.unity.com/packages/3d/environments/sci-fi/3d-volumetric-procedural-galaxy-with-selectable-stars-102961 | $9, Asset Store EULA | v1.1, 2022-01-21 (first 2017-11-08) | 1 review (4 stars), 104 favourites | sub-seeds, map-3d, unity-integration, price |
| Volumetric Galaxy Pack (Displacementality) | Unity asset (mostly visual) | https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/volumetric-galaxy-pack-92694 | $39, Asset Store EULA | v1.8, 2019-02-13 | 4 reviews (avg 3) | sub-seeds, custom-shape-map, map-3d, export-image, unity-integration, price |
| Random Generator 2.0 (names) | Unity asset | https://assetstore.unity.com/packages/tools/input-management/random-generator-118768 | $4.99, source included | v2.0, 2026-09-25 | 2 ratings | names-fantasy, seed-input, data-tables-editable, unity-integration, price |
| Easy Name Generator (NVJOB) | Unity asset | https://assetstore.unity.com/packages/tools/gui/easy-name-generator-171368 | Free | v1.1, 2020-06-25 | 7 reviews (5 stars) | names-fantasy, unity-integration, price |
| Lexic: A Procedural Name Generator | Unity asset | https://assetstore.unity.com/packages/tools/lexic-a-procedural-name-generator-33221 | Free | v1.0, 2016-06-01 | 15 ratings (5 stars) | names-fantasy, data-tables-editable, unity-integration, price |
| Celestial Mechanics Toolkit | Unity asset | https://assetstore.unity.com/packages/tools/celestial-mechanics-toolkit-42607 | $15 | v1.2, 2017-08-23 | 17 ratings (5 stars) | orbital-elements, rings, rotation-tilt, unity-integration, price |
| Procedural-Galaxy-Gen (LilMako17) | GitHub Unity project | https://github.com/LilMako17/Procedural-Galaxy-Gen | No licence (all rights reserved by default) | 2023-10-24 | 1 star | min-spacing, lanes-graph, data-tables-editable, moons, export-json, map-3d, unity-integration |
| Galaxia Runtime (Simeon Radivoev) | GitHub Unity project (ex-paid asset) | https://github.com/simeonradivoev/Galaxia-Runtime | BSD-3-Clause | 2020-12-01 (tested to Unity 2017.1) | 59 stars | custom-shape-map, options-sliders, hooks-plugins, map-3d, open-source-licence, unity-integration |
| starstuffer (nihilocrat / Fractal Phase) | GitHub Unity library | https://github.com/nihilocrat/starstuffer | MIT; itch.io pay-what-you-want | 2016-11-16 | 8 stars | min-spacing, moons, orbital-elements, rotation-tilt, export-json, open-source-licence, unity-integration |
| Unity Star Systems and Galaxies (notakamihe) | GitHub Unity game | https://github.com/notakamihe/Unity-Star-Systems-and-Galaxies | No licence | 2021-08-27 | 16 stars | galaxy-shapes, star-remnants, star-physics, moons, asteroid-belts, map-3d, unity-integration |
| ProceduralGalaxy (artak10t) | GitHub Unity project | https://github.com/artak10t/ProceduralGalaxy | GPL-3.0 (ideas only) | 2021-09-18 | 15 stars | orbital-elements, map-3d, unity-integration |
| StarGen (jazhikho) | Godot .NET (C#) standalone tool | https://github.com/jazhikho/star_gen and https://jazhikho.itch.io/stargen | MIT; itch.io pay-what-you-want | 2026-09-16 (internal 0.11.9, itch v0.10 on 2026-05-02) | 0 stars | seed-input, presets, options-sliders, galaxy-shapes, regions-sectors, hierarchical-address, lazy-generation, lanes-graph, galaxy-age-metallicity, spectral-classes, star-physics, star-remnants, multiple-stars, habitable-zone, frost-line, star-age, planet-types, moons, asteroid-belts, atmosphere, habitability-score, life-ladder, population-society, stations, profile-code, rpg-system-profiles, explain-trace, cited-assumptions, edit-after-generate, export-json, map-3d, open-source-licence, price |
| star-system-sim (dexmoh) | GitHub Godot 4.5 project | https://github.com/dexmoh/star-system-sim | MIT | 2025-12-25 | 1 star | open-source-licence, godot-integration (README says only "a procedural 2D star system generator") |
| Starlight (tiffany352) | Godot Asset Library addon (renderer with a star generator) | https://godotengine.org/asset-library/asset/2221 | MIT | v1.3, 2025-01-03 | n/a | spectral-classes, star-physics, star-colour, map-3d, open-source-licence, godot-integration |
| Planet Generator (Jujedie) | Godot Asset Store addon | https://store.godotengine.org/asset/jujedie/planet-generator/ | Free, MIT | v1.0.0 (unstable), 2026-09-01 | no reviews; GitHub 5 stars | seed-input, presets, planet-types, temperature-climate, biomes, hydrosphere, resources, surface-map, open-source-licence, godot-integration |
| Stellar System (Theo Unreal) | Unreal Fab plugin (C++) | https://www.fab.com/listings/f19a942e-fb00-4584-a7f9-2d862653b438 | $9.99, Fab standard licence | 2026-06-24 (first 2024-07-24) | 2 ratings (5.0) | orbital-elements, planet-types, temperature-climate, moons, rotation-tilt, multiplayer-determinism, price |
| Solar System Generator (InflatGames) | Unreal Fab blueprint | https://www.fab.com/listings/0e1e23e9-33f7-42d4-9b0e-4384192e8ed0 | $6.99 | 2024-09-19 (first 2020-09-04) | 1 rating (4.0) | options-sliders, editor-preview, price |
| Stargen.Net (tofi92) | NuGet library (C#, .NET Standard) | https://github.com/tofi92/Stargen.Net | MIT | 1.0.0.1, 2025-11-05 | 510 NuGet downloads, 2 stars | seed-input, options-sliders, orbit-spacing, planet-physics, habitable-zone, moons, typed-units, engine-neutral-data, open-source-licence |
| Starform.NET (glegeza) | C# library (GitHub) | https://github.com/glegeza/StarformNET | No licence file (all rights reserved by default) | 2018-03-31 | 18 stars | orbit-spacing, planet-physics, atmosphere, habitable-zone |
| galaxy-gen (wisepythagoras) | C# .NET library | https://github.com/wisepythagoras/galaxy-gen | GPL-3.0 (ideas only) | 2020-02-06 | 15 stars | (README gives no features) |
| planet_generator | Rust crate | https://crates.io/crates/planet_generator | MIT | 0.0.8-pre-alpha, 2026-08-02 | 8,129 downloads | seed-input, determinism-tests, universe-level, galaxy-shapes, custom-shape-map, regions-sectors, galaxy-age-metallicity, spectral-classes, star-age, names-catalogue, multiple-stars, orbital-elements, moons, temperature-climate, diagnostics, engine-neutral-data, open-source-licence |
| accrete / accrete-wasm (LeonidGrr) | Rust crate + npm (WASM) | https://github.com/LeonidGrr/accrete | MIT | repo 2026-05-13; crate 0.2.0 2022-06-01 | 22 stars, 24,639 crate downloads, 61 npm/month | seed-input, options-sliders, orbit-spacing, orbital-elements, planet-physics, atmosphere, moons, rings, engine-neutral-data, open-source-licence |
| GalexJS (Cédric Pouilleux) | npm library (TS) | https://github.com/cedric-pouilleux/galex-js | "All rights reserved" (published on npm, no use licence granted) | 0.4.0, 2026-06-10 | 230 npm/month, 0 stars | seed-input, deterministic-cross-platform, multiplayer-determinism, determinism-tests, spiral-arm-count, options-sliders, spatial-index, pathfinding-routes, visibility-helpers, engine-neutral-data, map-3d |
| Stellar Dream (irskep) | npm library (TS) | https://github.com/irskep/stellardream | MIT | npm 0.1.5 2019-02-09; fork @unsou/stellardream 0.2.0 2025-06-05; repo 2023-01-04 | 30 stars, 42 npm/month | seed-input, spectral-classes, star-physics, star-colour, multiple-stars, habitable-zone, planet-types, planet-physics, export-json, engine-neutral-data, open-source-licence |
| @xenocide/world-generator (duchu-net) | npm library (TS) | https://github.com/duchu-net/xenocide-world-generator | ISC on npm (no licence file in repo) | 0.0.4, 2024-09-14 | 20 stars, 17 npm/month | seed-input, galaxy-shapes, hierarchical-address, lazy-generation, surface-map, names-fantasy, export-json, engine-neutral-data |
| Yudha's Starmap | JS browser project | https://github.com/yudhanjaya/Starmap | MIT | 2025-04-24 | 8 stars | galaxy-shapes, options-sliders, spectral-classes, star-physics, star-age, star-remnants, multiple-stars, planet-types, planet-physics, atmosphere, hydrosphere, temperature-climate, moons, rings, habitability-score, cultures-species, population-society, description-text, names-fantasy, map-3d, web-tool, open-source-licence |
| The Atlas (SurceBeats) | Python + React/Three.js web app | https://github.com/SurceBeats/Atlas | PolyForm Noncommercial 1.0.0 (ideas only) | 2026-04-09 | 63 stars | seed-input, seed-url, universe-level, galaxy-shapes, lazy-generation, planet-types, rings, rotation-tilt, temperature-climate, life-ladder, planet-traits, resources, export-image, map-3d, web-tool |
| StarGen II (Sphinkie) | C++ library + CLI | https://github.com/Sphinkie/StarGen-II | MIT | 2023-03-27 | 32 stars | seed-input, planet-physics, orbit-spacing, export-text-html, open-source-licence |
| Accrete/Starform/Stargen archive (zakski) | C/Java code archive | https://github.com/zakski/accrete-starform-stargen | Apache-2.0 | 2025-05-22 | 46 stars | orbit-spacing, planet-physics, open-source-licence |
| rhogen (ofasgard) | Python CLI | https://github.com/ofasgard/rhogen | GPL-3.0 (ideas only) | 2022-08-03 | 10 stars | star-physics, export-json, export-markdown, export-image |
| Infinite Discoveries (Sushutt) | Python generator for Kerbal Space Program | https://github.com/Sushutt/Infinite-Discoveries | GPL-3.0 (ideas only) | 2024-10-28 | 19 stars | options-sliders, surface-map |
| galaxy_map_generator (BenedictVig) | Python script | https://github.com/BenedictVig/galaxy_map_generator | No licence | 2025-06-11 | 1 star | planet-types, resources, export-json |

### Adjacent: models, editors and visual generators (no galaxy or system data, or very little)

| Generator | Kind | URL | Price / licence | Last update | Popularity | Feature keys |
|---|---|---|---|---|---|---|
| Space Graphics Toolkit | Unity asset (visual) | https://assetstore.unity.com/packages/tools/level-design/space-graphics-toolkit-4160 | $49.97 at fetch (reviews mention $99) | v4.2.14, 2025-07-23 | 390 ratings (5 stars), 151 reviews | surface-map, map-3d, unity-integration, price; starfield generator with "Elliptical Galaxy" distribution (snippet only) |
| ORION Space Scene Generation Framework (ARTnGAME) | Unity asset (visual) | https://assetstore.unity.com/packages/tools/level-design/orion-space-scene-generation-framework-206113 | $119 | v3.2.0, 2026-09-25 | 13 ratings (5 stars) | surface-map, map-3d, unity-integration, price |
| Planet generator class on the Unity Asset Store: Planets Generator (143223, $19.99, 2019), Simple Planet Generator (104279, $9.99, 2018), PlanetGen (163535, $25, 2020), Planet Generator by SKAVA (315528, $60, 2025-04-16, has presets and a biome system), Procedural Planet Generator Tool Idealplay (291049, $92.99, 2024-11-07), Procedural Planets and Stars (106662, free, 2019), Planets & Stars (145836, $7, 2020) | Unity assets (visual) | https://assetstore.unity.com/packages/tools/level-design/planet-generator-procedural-planet-creation-tool-315528 (example) | $0 to $93 | 2018 to 2025 | 0 to 8 ratings each | surface-map, biomes (SKAVA only), presets (SKAVA only), unity-integration, price |
| Sky and galaxy visual class on the Unity Asset Store: Cosmic StarBox Generator (314631, $15, 2026-08-14, JSON presets), Space Galaxy And Nebula Modular Creator (251656, $79.99, 2026-08-18, random generator), Procedural Spiral Galaxy Starfield (403622, $4.99, 2026-09-04), Asteroids Belt procedural generator URP (281176, free, 2024-11-15) | Unity assets (visual) | https://assetstore.unity.com/packages/tools/utilities/cosmic-starbox-generator-314631 (example) | $0 to $80 | 2024 to 2026 | 0 to 3 ratings | export-image, presets, asteroid-belts (belt asset only), unity-integration, price |
| Infinity Square/Space (NVJOB) | Unity asset / open source game prototype | https://assetstore.unity.com/packages/templates/packs/infinity-square-space-procedurally-generated-space-with-a-destru-149923 | Asset Store, "open source" prototype | v1.0, 2019-08-06 | 10 ratings (5 stars) | lazy-generation (coordinate-based procedural world), unity-integration |
| Universe (Vindemiatrix Collective, com.vindemiatrixcollective.universe) | OpenUPM package (model and orbital maths, no generator) | https://github.com/TheWand3rer/Universe | MIT | 1.3.0, 2026-06-03 | 91 stars | orbital-elements, multiple-stars, typed-units, export-json, unity-integration, open-source-licence |
| @industrieh/starmap + starmap-render | npm library (model, explicitly not a generator) | https://github.com/Industrieh/starmap | MIT | 0.1.0-beta.0, 2026-07-31 | 1 star, 16 npm/month | lanes-graph, pathfinding-routes, orbital-elements, export-image, engine-neutral-data, open-source-licence |
| Novolis.Astro.* (Assessment, Catalog, Routing, Plotting...) | NuGet packages | https://novolis-platform.github.io/.github/novolis-astro/ | MIT | 2026.1.0.24, 2026-10-03 | 0 downloads | NuGet description: "deterministic SystemProfile generation (elements + economic potentials) for worldbuilding and games"; docs page lists no generator features. No keys marked. |
| Procedural Galaxy System (Athian Games) | Unreal Fab (Niagara, visual) | https://www.fab.com/listings/fc525aa7-eb3d-4e69-8d85-30eca12988d9 | $34.99 | 2026-06-18 | 0 ratings | galaxy-shapes, galaxy-blend, options-sliders, map-3d, price |
| Interactive Star Systems (ProMaxx Studio) | Unreal Fab (meshes + blueprint designer) | https://www.fab.com/listings/89804ccf-4ab7-4d24-900b-0acd69bb3500 | $29.99 | 2026-06-28 | 0 ratings | rings, asteroid-belts, editor-preview, price |
| Space Creator Pro / Solar System (Makemake) | Unreal Fab (visual, real Solar System) | https://www.fab.com/listings/9680ec0b-64ce-4857-bc55-1585d779a7e3 | $299.99 / $99.99 | 2026-09-09 / 2026-08-20 | 18 ratings (4.83) / 3 (4.33) | surface-map, price |
| Godot visual planet class: PixelPlanets (Deep-Fold, MIT, 1,394 stars, 2024-03-29, ports to Unity, MonoGame, Defold, JS), Planet-Generator (Hoimar, 264 stars, 2023), stylized-planet-generator (Bauxitedev, MIT, 229 stars, 2020), 3D Planet Generator (Naejimer, asset 1615, 2025-12-10), Pixel space background generator (asset 3552), Celestial Bodies (asset 3683, 2025-02-13) | Godot addons / projects | https://github.com/Deep-Fold/PixelPlanets (example) | MIT mostly | 2020 to 2025 | up to 1,394 stars | surface-map, godot-integration, open-source-licence |
| Nro Galaxy Editor | Godot app (Stellaris save editor, not a generator) | https://github.com/Nro001/Nro-Galaxy-Editor | custom licence | 2025-10-04 | 61 stars | lanes-graph (edit), chokepoints (bridges shown), nebulae-hazards (edit), edit-after-generate |
| @eluvade/cosmos | npm (WebGL renderer for seeded bodies) | https://github.com/Eluvade/cosmos | MIT | 1.3.0, 2026-08-07 | 10 stars, 41 npm/month | seed-input, planet-types, export-image, web-tool, open-source-licence |
| coilyco/galaxy-gen | Rust to WASM galaxy evolution simulation | https://github.com/coilyco/galaxy-gen | AGPL-3.0 (ideas only) | 2026-10-02 | 33 stars | star-remnants, galaxy-age-metallicity, web-tool |
| Jimbly/galaxy-gen | JS browser toy | https://github.com/Jimbly/galaxy-gen | No licence | 2025-09-29 | 38 stars | web-tool, map-render (100,000,000,000 stars "exactly") |
| gemini (holmgr) | Rust game with galaxy generation | https://github.com/holmgr/gemini | MIT | 2022-12-14 | 38 stars | (README describes a trading game, no generator features) |

Bevy and MonoGame: no galaxy or star system data generator crates or packages were found. Bevy crates in this space are rendering helpers (big_space floating origin, bevy_weather and bevy_sky_gradient procedural skies). MonoGame has only a PixelPlanets port and an old XNA project (GenesisEngine, snippet only).

OpenUPM: a registry search for galaxy, star, planet, space, universe, solar and procedural found no generator package. The only space package is Vindemiatrix "Universe" (model and orbits, above).

## Notes per generator

**Complete Galaxy Generator** (the known $16 asset). Four shapes: ring, wheel, spoked, spiral (with arm count and spread, and burst count and angle for spoked). Builds a neighbour graph with distances as weights, a maximum connection count, and promises full connectivity ("every star system can be accessed from any other"). Saves and loads JSON including planets and connections. Weak on content: planets exist but the page names no planet properties, no star types, no names and no seeds. One review (2024) reported a null-reference bug in `GetNeighbors`, since patched.

**Persistent Galaxy Generator** (KassaK). The most-favourited data generator on the store (247). Circle, spiral and irregular galaxies, planet type and rarity tables, a name generator for systems and planets, JSON export and import, in-and-out navigation, two nebula shaders. Reviews (2018 to 2025) complain that every star is its own GameObject and particle emitter, code has no namespace or a clashing `Entity` namespace, it needs project settings that break existing projects, and one reviewer had to rename a Rider plugin DLL to make it compile. Praised as the quickest "starter" for a Stellaris-sized map of 1,000 to 10,000 stars.

**Galaxy Constructor** (2025). The strongest Unity data generator found. GPU compute draws a million-plus stars in one draw call; galaxy shape comes from a face-on and a side-profile density map plus a colour map (real Hubble/Spitzer/WISE images or hand-painted). Star data is made on click (lazy): spectral and luminosity class, temperature, luminosity, radius, mass, age, metallicity, with young and old populations mixed by position, plus brown dwarfs, white dwarfs, neutron stars and black holes. Names are catalogue style by galactic sector ("Kelsaemkrox RC V d14", Elite Dangerous style) with proper names for bright giants. Planets, moons, belts and comets come from editable "Body Type" assets with chance, orbit preference and size range; orbits scale with star luminosity. Stand-out idea: visited stars are written to a CSV database on first visit and read back, "so tuning the tables later does not change a map your players already know". Each system and body carries its own seed. No hyperlanes, no regions with traits, no factions, no JSON.

**Galaxy Creator 1.3**. Thousands of orbiting stars, planets, belts and rings, $7. Reviews: HDRP-only VFX Graph without warning on the store page, broken prefabs on an early version (2020).

**Solar System Generator** (Thunder, 2018). Counts of planets, moons per planet, suns and asteroids; random sizes and positions in a range; 20 textured planets. Visual randomiser, no data model.

**3D Volumetric Procedural Galaxy with Selectable Stars**. 250,000 selectable stars, 10 star types, each star with an ID and a 6-digit random number "for generating further procedural data". A 2023 review: a GameObject per star, should be VFX for performance.

**Volumetric Galaxy Pack**. Visual first, but its star field generator keeps star IDs and properties persistent across sessions for a given setting, with picking. Reviews: thread deadlocks in the demo scene, only one material and galaxy, "very expensive for what it is".

**Name generators on the Unity store**. Random Generator 2.0 (updated 2026-09-25) is the richest: 124 generators including planets, stars, galaxies, nebulae, aliens and spaceships, Markov or whole-entry modes, custom seed data, and "replay a sequence" with a seed. It notes that repeatability depends on "the same runtime, asset version, data, settings, and calls" (honest about seed versioning, which no galaxy asset mentions). Easy Name Generator has a 18.3 million combination Sci-Fi mode. Lexic has editable dictionaries.

**Celestial Mechanics Toolkit**. Not a generator of content but "designed for quick procedural generation of orbits and orbital systems": circular, elliptical, parabolic and hyperbolic paths, nested orbits, ring meshes, orbital decay and precession. Reviews: "covers only the basic".

**Procedural-Galaxy-Gen** (LilMako17). The only Stellaris-style hyperlane project found for Unity: Poisson-disc star placement, hyperlanes, ScriptableObject parameters, planets and moons, deterministic and serialisable systems. Deliberately "non-realistic, no orbital mechanics". Depends on Space Graphics Toolkit and Odin Serializer; no licence.

**Galaxia**. A former paid Asset Store galaxy generator, now BSD-3 source. Curve-driven particle "distributors" (custom distributors allowed), image-based galaxies, a simple galaxy map example. Last tested on Unity 2017.1.

**starstuffer**. 2016 jam library: somewhat regularly spaced systems, one star per system with magnitude and temperature, planets and moons with period, distance and day length, XML and JSON serialisation, generator and renderer separated. Old, but the generator/renderer split is the right idea.

**StarGen (jazhikho)**. The closest competitor in spirit and stack: C#, MIT, Godot .NET, aimed at "science-fiction worldbuilding, setting design". Galaxy, System, Object and Station studios; named presets (Milky Way, Andromeda, LMC analog); quadrant, sector and local coordinates; lazy subsector placement; jump routes; brown dwarfs, evolved stars, white dwarfs and multi-star hierarchies; snow line and habitable zone; life model (abiogenesis, complex life, civilisation); settlements and population; "clean-room RPG compatibility" profiles for Space Opera, Cepheus (Traveller-like mainworld data), Starfinder and Starforged; save and load; JSON export for stations; edit with recipe preservation. Weaknesses: it is a standalone app, not a library or NuGet package; very science-heavy (scale lengths, IMF, MIST/PARSEC, dark matter halo mass) and the changelog spends its effort on scientific audits and lay relabelling; no stars on GitHub yet.

**Starlight (Godot)**. A 100,000-star renderer with a "random star generator based on main sequence stars (classes M through O)"; position, luminosity and temperature per star.

**Planet Generator (Jujedie, Godot Asset Store, 2026-09-01)**. Seven world classes (Terran, Toxic, Volcanic, Airless, Dead/irradiated, Sterile rocky, Gas giant), GPU tectonic plates, erosion, climate, biomes, water bodies and resources; runtime API with templates such as "Earth-like" and a seed. Planet surface only, no systems.

**Stellar System (Unreal, Fab)**. A procedural generator actor makes a whole system at BeginPlay: suns, planets classified by temperature (Lava, Rocky, Desert, Ocean, Ice, Gas Giants) and satellites with "collision-safe non-overlapping orbits", every orbit from six Keplerian elements, axial tilt, and multiplayer positions computed deterministically from server time. No galaxy level, no names.

**Other Fab listings**. Procedural Galaxy System (Athian, 2026) is Niagara visuals but notably blends spiral, barred spiral, elliptical and irregular types. Solar System Generator (InflatGames) offers 40+ planet parameters in Blueprints. Everything else on Fab is meshes, skyboxes or nebula renderers.

**Stargen.Net**. The only modern NuGet star system generator found. A .NET Standard rewrite of Starform.NET (Accrete, Dole 1970) with seed and options, values typed with UnitsNet. Its README reports 10,000 systems at seed 1337 give 2,310 habitable planets and 29,282 planets with moons. 510 downloads. Plausible, not story-oriented: no names, no galaxy, no tags.

**planet_generator (Rust)**. The most ambitious engine-neutral library: universe age and era, galaxy neighbourhood, shapes and "peculiarities", the Local Group, sectors and subsectors with hex divisions and region mapping, star generation by population, star names, multiple-star eccentricity, orbital zones, planets, moons, climate. SVG map layers set galaxy shape and density. Typed warnings instead of panics, golden determinism tests. Inspired by RTT, Instant Universe, GURPS Traveller, Stars Without Number, Rogue Trader and Alternity. Still pre-alpha: resources, life, points of interest, species and history are unchecked roadmap items.

**accrete (Rust/WASM)**. Accrete plus Starform/Stargen environments, moons, rings, stand-alone planet generation, seed, on crates.io and npm. Scientific plausibility only.

**GalexJS**. A deterministic galaxy "board" for multiplayer: the same seed and options give byte-identical `Float32Array`s on V8, SpiderMonkey and JavaScriptCore, checked by a static linter that bans `Math.sin` and friends in `core/` and by a hash test on Chromium, Firefox and WebKit. Server validates player actions against the seed without stored state. Cube grid spatial index, pathfinding, visibility helpers. Licence is "all rights reserved"; ideas only.

**Stellar Dream**. Exoplanet-statistics-based systems (Kepler era): star type, colour hex, luminosity, mass, radius, metallicity; Terran, Neptunian and Jovian planets; habitable zone; planet-metallicity correlation and Titius-Bode in its references. JSON-serialisable. Unmaintained since 2019 on npm.

**@xenocide/world-generator**. Generator objects that yield systems, then stars, planets and surface regions on demand, each with a `path`; spiral and grid galaxies; Markov names in an older alpha. Small and stalled (0.0.4, 2024).

**Yudha's Starmap**. Built for a novelist ("Salvage Crew", "Pilgrim Machines"): spiral, elliptical and irregular galaxies, star evolution stages and binaries, planets with atmosphere, water, moons, rings and habitability, then civilisations with traits, governments, tech levels, relationships and short narrative descriptions. Uses `Math.random`, so no seeds; export but no import.

**The Atlas**. A seeded "everything from one primordial seed" universe with up to 10^21 galaxies, 26+ planet types, 10 life forms, planetary anomalies, Roche-limit rings, tidal locking, seasons, and "Stargate links" (encoded URLs that share a location). Non-commercial licence.

**StarGen II, zakski archive, rhogen, Starform.NET**. The Accrete/Stargen family that most science-leaning generators descend from. rhogen (GPL) outputs JSON, a Markdown report and a system diagram. StarGen II outputs XML or commented TXT and can start from real Celestia catalogue stars.

**Vindemiatrix Universe (OpenUPM)**. Not a generator, but the closest thing to the owner's package shape on OpenUPM: an engine-agnostic C# galaxy / n-ary star system / star / planet object model with UnitsNet units, Kepler propagation, binary-star propagator, transfer planner and relativistic rocket maths.

**@industrieh/starmap**. Not a generator by design ("the shape of a galaxy is game design"), but a clean zero-dependency TS model with hyperlanes, route finding, planet positions over time and an SVG renderer. A good model for what consumers of generated data want to query.

## New feature keys added

- **connected-graph**: the lane network is guaranteed connected (every system reachable from every other).
- **comets**: comets generated as bodies.
- **sub-seeds**: every system or body exposes its own derived seed or stable ID so users can hang their own procedural content on it.
- **visited-cache**: records are persisted the first time a place is generated or visited, so later table or version changes do not alter places players have already seen.
- **multiplayer-determinism**: server and every client derive the same world (or the same positions) from the seed or clock, without sending map data.
- **determinism-tests**: determinism is checked by automated golden or hash tests in CI.
- **spatial-index**: the galaxy is indexed by a grid or cube structure that can be queried ("what is in this cell").
- **pathfinding-routes**: built-in route or shortest-path queries over the lane graph.
- **visibility-helpers**: fog-of-war or visibility-field helpers in the library.
- **rpg-system-profiles**: generation biased by named tabletop RPG rule sets (Traveller/Cepheus, Starfinder, Starforged, Stars Without Number).
- **cited-assumptions**: each scientific assumption shown with its source, beyond a per-value explanation.
- **typed-units**: values carry explicit physical units (for example UnitsNet types) instead of bare doubles.
- **diagnostics**: soft problems (clamped values, unparsable maps) returned as typed warnings on the result instead of exceptions.
- **editor-preview**: generate and preview inside the engine editor without entering play mode.
- **galaxy-blend**: several galaxy types blended into one structure.

## What nobody offers

No engine asset or library combines a story-first galaxy with a plain-data, engine-neutral API. Unity assets build GameObject scenes (reviewers repeatedly complain about one GameObject per star, namespace clashes and broken existing projects), offer JSON at most, and none mention seeds except Galaxy Constructor; none ship on NuGet or OpenUPM. The libraries that are engine-neutral (Stargen.Net, accrete, Stellar Dream, planet_generator) are science-plausible or pre-alpha and stop at physics. Nobody found offers versioned seeds, a shareable seed code, or hierarchical addresses that regenerate one object alone, with the honest exceptions of Random Generator's caveat and Galaxy Constructor's visited-cache workaround. Nobody pairs hyperlanes with chokepoints or wormholes as data, regions with shared traits, factions or territory, danger levels, story tags, points of interest, history, GM secrets, Markdown or VTT export, or "lock this and reroll the rest" in a game-engine package. StarGen (Godot, MIT, C#) is the only one reaching into population, stations and RPG profiles, and it is a standalone science-heavy app, not a library. A free MIT C# package on NuGet and OpenUPM with deterministic plain data, story hooks and fiction-friendly output would have no direct competitor in the engine space.

Related: [../INDEX.md](../INDEX.md), [../features/feature-matrix.md](../features/feature-matrix.md).
