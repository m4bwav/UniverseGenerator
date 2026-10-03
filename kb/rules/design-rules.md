---
title: "Design rules for a game and fiction space generator: variety, structure, scale and API"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, design, oatmeal, landmarks, lanes, game-scale, api, writers]
summary: "read when deciding what the generator should output or how its API should feel: the oatmeal problem and landmark guarantees, map structure, game scale, fairness, text that agrees with data, and the ten API rules, each with its source and what the Stage 1 prototype showed"
---

# Design rules

## Variety that people notice

- **Oatmeal.** Ten thousand bowls of oatmeal are all different and all the same: aim for differences a person notices (Kate Compton, 2016). Rabii and Cook 2023 ("Why Oatmeal is Cheap", arXiv 2305.02131) show why: knowledge has to be put into the generator; more random dimensions do not help. Kreminski 2023 ("Generator's Haunted"): users build a model of the generator's range from the first few outputs, and "perceptual collapse" follows when they think they have seen it all.
- **Landmark guarantee.** Every scale gets something memorable: a landmark per system, a monument per region, beacons per galaxy (Hervé, Warpefelt, Salge 2025, arXiv 2509.19030).
- **Outlier quota.** Let a small share (about 5%) of outputs break the usual rules on purpose (Civilization VII 1.2.5 map generation, 2025).
- **Prototype evidence (2026-10-02):** the 50-galaxy contact sheet is clearly varied (five shapes, lanes, regions), but the 50-system sheet still looks alike at thumbnail size although the data differs. Systems need visible identity: an unusual star, a ringed giant, a living world, a belt, a landmark.

## Map structure for play

- Lanes from the relative neighbourhood graph (connected, sparse, no crossings) plus a tunable share of Gabriel edges; chokepoints are bridges and articulation points (Red Blob Games; Stellaris dev diaries). Prototype: 2.55 lanes per system, 4.7 bridge lanes and 3.2 dead ends per 40-system galaxy.
- Minimum spacing between systems (Bridson Poisson disc). The game today has neighbours 2.7 units apart on a radius of 1,000; the prototype's minimum is 63.
- Danger from hop distance to the core plus regional noise covers 1 to 10 (the game today never reaches 10).
- Start fairness and contested richness: keep special systems out of start areas, spread habitable worlds, score contested ground by the ratio of distances to the two nearest starts (Star Ruler 2, MIT source; Stellaris 4.0).
- Travel terrain as an alternative to lanes: nebulae that slow, damage or hide (Distant Worlds 2; Galactic Civilizations IV 3.0).
- Run maps: an FTL or Slay the Spire path from entry to boss over the lane graph; one-way main edges and two-way fuel-cost side edges (Breachway).

## Game scale

- Small by default: 20 to 200 systems per galaxy, 3 to 12 galaxies per universe, up to about 12 planets per system; every count an option with a cap; lazy generation by address.
- **Shapes need numbers.** At 40 systems, ellipticals, rings and irregulars read clearly, spirals do not (prototype contact sheet). Either default spirals to 80 or more systems, string systems along arms, or let the renderer draw dust that is not systems.
- Make a small universe feel big: landmarks visible from everywhere, gated travel, density over size, a thread across galaxies (Outer Wilds; space compression).

## Writers and game masters

- Text must agree with data: tag-filtered grammar snippets (Improv, Voyageur).
- History: generate events, then explain them, told by sources that may disagree (Caves of Qud).
- Fill around fixed facts: the user pins a world they already imagined, the generator builds the rest consistently (Traveller World Builder's Handbook).
- Exports into the tools they use: Markdown with YAML properties for Obsidian, fields that map to World Anvil, Campfire and Kanka, GM secrets kept apart from a player-safe view (see the writers' sources).

## API (from Bogus, FastNoise2, DunGen, Edgar, DeBroglie)

1. One call with good defaults. 2. Presets as immutable records adjusted with `with`. 3. Levels of control: preset, options, tables, hooks, constraints. 4. Data tables swappable by id. 5. A shareable config code. 6. Determinism as contract. 7. Global sliders (Density, Weirdness, Realism). 8. Validate before generating. 9. Lazy, addressable access (`u.Galaxy(2).System(17).Planet(3)`). 10. Explainable output.

Related: [determinism rules](determinism.md), [plausibility rules](plausibility-rules.md), [INDEX](../INDEX.md).
