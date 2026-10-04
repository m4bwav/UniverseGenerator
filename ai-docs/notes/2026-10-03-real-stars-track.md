---
title: "Real stars track: maps that approximate the real neighbourhood (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-01-03
tags: [universegenerator, real-stars, catalogue, solar-neighbourhood, milky-way, local-group, research, 1.x]
summary: "read before researching or building anything from real astronomy data: Mark's request (2026-10-03) for maps of the real stars around Sol, which reverses the 2026-10-02 rejection of real-star-catalogue in a bounded form; what to research first (catalogues, licences, sizes, competitors), the design sketch (a Neighbourhood level, real near and generated far, catalogue versions in addresses), the size split between core and an add-on, and the new 1.x feature rows"
---

# Real stars track

## The request (Mark, 2026-10-03, during N1)

> One thing I would like to be able to add was the ability to construct a universe based on what we know of the universe. Like we know the distances and names of our immediate neighbors. Like if I wanted to I could generate the 30 or so stars around Sol for 30 star system. We don't know that much, and I don't want to overburden the lib with much more data, but I want to be able to generate maps that are at least approximations of real stars and their relationships. This wouldn't be the only mode, maybe a module if it's too big. Think about it and update the plans with the idea of doing more research and features in that direction.

The procedural mode stays the default. Real data is an extra mode, kept small, with an add-on package when it grows.

This supersedes the rejection of the `real-star-catalogue` row on 2026-10-02 ("game and fiction scale; catalogue data is large; a hand-made-systems hook covers a few real stars"). That rejection was about whole catalogues (SIMBAD, HYG). This request asks for a small, bounded table plus generated fill-in, which fits the size budget and D18: real stars as a setting for stories, not a simulation.

## What already helps

- The star tables already embed the IAU proper names, Bayer designations, and HD and HIP numbers (17 KB gzip, `names-catalogue`). A real neighbourhood would reuse its spectral classes, physics formulas, names and story layers.
- The star system level takes a context: a name, an age, a danger and a richness. A real star would add a pinned star (class, mass, companions) and pinned known planets. That is the "hand-made systems hook" the rejection mentioned; it does not exist yet (D7 hooks, now ruled into 1.0.0).
- `Distances` and `Units` (PR #22) already turn map units into light-years and parsecs.
- `seed-url` links and the export make a real map shareable and loadable like any other map.

## Research first (one pass, written to `kb/sources/` with a "feature keys" column)

1. **Catalogues of the nearest stars.** Candidates to check:
   - the RECONS census of systems within 10 pc;
   - the 10 pc sample of Reylé et al. 2021, about 540 stars and brown dwarfs in about 340 systems (recall, to verify);
   - the Gaia Catalogue of Nearby Stars (100 pc, far too large for core);
   - HYG, AT-HYG and SIMBAD for names and cross-identifiers.
   
   For each: fields (position, distance, spectral type, multiplicity, mass), accuracy, last update, and how many systems fall within 12, 16, 20 and 33 light-years.
2. **Known planets.** The NASA Exoplanet Archive and exoplanet.eu entries for those systems (Proxima b, the planets of epsilon Eridani, tau Ceti, Barnard's star, Wolf 359, YZ Ceti and so on), and which fields are firm or only minimum masses.
3. **Licences.** What each source allows in an MIT package:
   - Gaia is free with acknowledgement;
   - HYG is CC BY-SA, a share-alike problem for MIT;
   - NASA archive data is public with acknowledgement.
   
   Also: whether a hand-built table of facts (names, positions, classes) is safe to publish, and the EU database right. This decides which sources the table may be built from. Ask Mark before using any share-alike data.
4. **Larger structures, as approximations.**
   - The Milky Way as a preset: a barred spiral, four major arms, the Sun about 26,000 light-years from the centre (all to verify).
   - The Local Group as a real cluster: the Milky Way, Andromeda, Triangulum, the Magellanic Clouds and about 80 dwarfs (McConnachie 2012 and its updates).
   - The Virgo cluster and the Laniakea supercluster for a universe map.
   
   Small tables of tens of rows, not catalogues.
5. **Who does this, and how games use it.** Competitors:
   - SpaceEngine, Elite Dangerous (real stars near Sol, generated far away), Celestia and Universe Sandbox;
   - Star System Explorer, which imports the real sky from SIMBAD;
   - 3D starmaps such as the Atomic Rockets maps.
   
   Tabletop settings built on nearby stars: 2300 AD's near-star map, GURPS Space, Traveller's "Near Space". What players expect: real names, real distances, a 2D map that keeps neighbours near.
6. **Maps from 3D positions.** How others flatten the neighbourhood into a 2D game map: a galactic-plane projection with height kept as a field, force-directed layouts that keep near neighbours near, or an honest 3D option. Plus how lanes look between real stars (the Gabriel graph in 3D, then projected).

## Design sketch (to confirm after the research; nothing is built yet)

- **A `Neighbourhood` level** (working name): `Neighbourhood.Generate(seed, options)` with `options.Systems` (default about 30) gives the nearest real systems around Sol.
  - Real positions (light-years, galactic coordinates, with a 2D map projection), real star classes, companions and names, and known planets placed as known.
  - Everything not known (most planets, moons, belts, stations, story tags, factions) is generated from the seed, consistent with the known data: a known planet keeps its orbit, and generated planets fill the gaps around it.
  - The same seed gives the same fill-in. A different seed gives the same stars with different unknowns.
- **Real near, generated far.** A galaxy option that seats Sol's neighbourhood at the home position of a generated (or Milky Way preset) galaxy: the systems within N light-years are real, the rest procedural. This is Elite Dangerous's model, and it fits D17: a level above never changes the levels below.
- **Addresses and the seed promise.** Real data changes when catalogues update (a new Gaia release moves distances), so the data has its own version in the address, for example `v1-my-seed/near1/system/4`. A golden file per data version, and a new data version never changes an old one.
- **Lanes and gameplay.** The same lane, chokepoint, region and danger machinery on the real positions. Danger could come from distance to Sol rather than to a galactic core.
- **Exports and units.** `ToJson` and `Units` work unchanged. Distances in light-years are the natural text unit here.

## Size (D19 budget, to measure in the research pass)

- The nearest 30 to 60 systems, with names, positions, classes, multiplicity and known planets: estimated 3 to 10 KB. That can live in core and stays green.
- The full 10 pc census (about 340 systems): estimated 20 to 40 KB, probably still green in core. Decide on measured numbers.
- Anything from Gaia's 100 pc catalogue, or a real Milky Way star field: an add-on package (`UniverseGenerator.RealStars`, working name), as D21 does for Names and Text.
- The Milky Way preset, the Local Group and Laniakea are tens of rows each: core.

## Feature rows (status.json, all 1.x, additive: no existing seed changes)

`real-neighbourhood`, `real-exoplanets`, `real-and-generated`, `milky-way-preset`, `real-local-group`, and `real-star-catalogue` (moved from rejected to 1.x as the add-on for larger catalogues). `pinned-systems` (hand-made or real systems placed into a generated map) is the hook they all need.

## Where it sits

After 1.0.0 (N1), which Mark is finishing first. The research pass can run any time, since it is docs only. The features come as 1.x minors after Stage 5's OpenUPM release, unless Mark moves them earlier. Pinned systems come first, because the real neighbourhood and any hand-made system need them. When the research is in, stop for Mark's choices: sources, licences, core or add-on, and the default count.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md); contradicts the 2026-10-02 rejection of `real-star-catalogue` in [the feature matrix](../../kb/features/feature-matrix.md); see also [the package size budget](2026-10-02-package-size-budget.md), [the Stop 2 questions](2026-10-03-stop-2-questions.md).
