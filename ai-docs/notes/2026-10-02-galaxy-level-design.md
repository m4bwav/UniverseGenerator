---
title: "Galaxy level design for UniverseGenerator 1.0 (2026-10-02)"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [universegenerator, galaxy, design, shapes, lanes, gabriel-graph, chokepoints, regions, danger, hierarchy, addresses, universe-at]
summary: "read before changing the galaxy level or Universe.At: what was ported from the prototype and what changed, the public shape (Map, Lanes, Regions, Systems generated on demand), the skeleton pass that gives systems their context, the address walk, the statistics and timings, and what the level still lacks (factions, points of interest, hazards, monuments and beacons, the colliding pair)"
---

# Galaxy level design

## Summary

The galaxy level is the prototype's map, made fast and exact, plus the pass that hands each system its name, age and danger; `Universe.At` walks any address back to its object. Factions, points of interest, hazards, monuments and beacons are still to come.

Built on 2026-10-02 on branch `stage3-core` (commits 9cc6ad0 and 4e63c06). Code: `Packages/com.m4bwav.universe-generator/Runtime/Galaxies/GalaxyLayout.cs` (the map), `Packages/com.m4bwav.universe-generator/Runtime/Galaxies/Galaxy.cs` (records, the skeleton pass, the on-demand system list), `Packages/com.m4bwav.universe-generator/Runtime/Universe/Universe.cs` (`Universe.At`). Tests: `GalaxyTests`, `GoldenGalaxyTests` (`tests/Golden/v1/galaxy.json`), `HierarchyTests`.

## What was ported, and what changed

The Stage 1 prototype (GalaxyProto.cs in Tests/GalaxyPrototype of the game repository's capture branch, worktree `Ai/labs/SpaceDeckBuilder2.1-galaxy`) was the starting point. Kept as it was: the five density functions and their parameters, the integer density decision (`NextInt(4096)` against the density times 4096), minimum spacing `0.4 R / sqrt(n)` relaxed by a tenth every 5,000 crowded attempts, lanes as the relative neighbourhood graph plus 25% of the remaining Gabriel edges (one roll per Gabriel pair in index order), bridges and articulation points as chokepoints, regions by farthest-point sampling over hops (2 to 8, n / 8), region names from 20 roots and 8 kinds, ages 30/45/25, eight themes, danger from hops plus regional noise (-2 to 2) and local noise (-1 to 1).

Changed:

| Change | Why |
|---|---|
| A spatial grid (cell = the starting spacing) replaces the all-pairs scans in placement and lanes; empty-circle and lune checks search rings of cells outward from the centre | the prototype's lanes were O(n³): fine at 40, hopeless at the 2,000 cap. `The_lanes_are_the_prototype_lanes` checks the grid's lanes equal the brute-force definition, with the same rolls |
| Only Gabriel pairs are tested for the relative neighbourhood rule | every relative-neighbourhood edge is a Gabriel edge (strictly, with these inequalities), so the result is identical and the roll order unchanged |
| Tarjan's bridges and articulation points run iteratively | a chain of 2,000 systems would recurse 2,000 deep; a worker thread with a small stack could overflow |
| `Shape = Auto` draws from {Elliptical 15, Ring 10, Irregular 15} under 80 systems, from all five {40, 20, 15, 10, 15} from 80 | plan D16: spirals and bars do not read at 60 |
| The shape roll and every shape parameter are drawn even when a shape is chosen | choosing a shape keeps the seed's arms, twist and blobs |
| Danger's base is `1 + (18 h + m) / (2 m)` on integers (h hops, m the most hops) | the prototype used `Math.Round`, which rounds half to even and is banned in Runtime (AGENTS.md) |
| Ties go to the lowest index everywhere (core, farthest point, nearest centre) | the prototype used LINQ `OrderBy(...).First()`, which also does, but loops say it plainly |
| Arms are reported as 0 for shapes without arms | the drawn value means nothing there |
| Placement tries `max(200,000, 500 n)` times | the 2,000-system ring and elliptical fill completely; tested for all five shapes at 2,000 |

## The public shape

- `Galaxy.Generate(seed, options)` returns a `Galaxy` record: `Address` (`v1-my-seed/galaxy`), `Shape`, `Arms`, `Radius` (1,000 game units), `Core`, `Map`, `Lanes`, `Regions`, `Systems` and `System(i)`.
- `Map` holds one `MapEntry` per system: index, address, name, X, Y, star class, region, hops, danger, chokepoint. It is everything a map screen needs without generating any planets.
- `Systems` is generated on demand: system i is generated with its context the first time it is read and kept (a race at worst generates it twice; generation is deterministic). The README's first example (`foreach (var system in galaxy.Systems)`) therefore works as written, and a 2,000-system map costs nothing until a system is opened.
- `Lane` (A < B, `Bridge`) and `GalaxyRegion` (index, name, age, theme, centre). The region type is not called `Region` because `System.Drawing.Region` would clash for WinForms users.
- New options: `Systems` (1 to 2,000, default 60; "Systems must be 1 to 2000; you asked for 0.", the plan's message) and `Shape` (default `Auto`).

## The skeleton pass (SystemContext)

For each system in index order: its seed is `Child(galaxy, "system", i)`, the same seed its address gives; its age is its region's; its danger is the map's; its star is rolled from its own `star` stream with that age and the options' star mix (the same roll the full system makes, so the map's star class always equals the system's); its name is drawn from its own `names` stream for that class and redrawn from the same stream while an earlier system holds it (after 100 clashes, a number is appended; never reached in practice). The three facts go into `SystemContext(name, age, danger)`, which `StarSystemGenerator` already accepted.

## Universe.At

`Universe.At(address, options)` parses the address, checks the version and seed, regenerates the root level (`galaxy` or `system`) and walks the path: `system/i` below a galaxy, then `planet/j` (then `moon/k`) or `station/k`. It returns `object`; callers cast or pattern-match. An address does not carry options, so the caller passes the ones used; an out-of-range index says so and asks whether the same options were passed. `Universe` is a static class for now; when the universe level arrives it becomes the record with `Generate` beside `At` (nothing is released, so the change is free).

`HierarchyTests` proves the promise: every object of seven galaxies (4,929 objects, including a 200-system spiral, the Plausible preset and a seed with `/` and accents) and of 300 lone systems equals the object regenerated from its address.

## Statistics (500 default galaxies, 30,000 systems; `Prints_the_statistics_and_timing_for_review`)

- Shapes at 60 systems: Elliptical 36%, Irregular 35.8%, Ring 28.2%. At 80 (200 seeds), Spiral 30 to 50% and all five occur.
- Stars: M 31.4%, K 20.4%, G 15.5%, F 9.8%, white dwarf 6.1%, A 5.8%, giant 5.5%, B 2.3%, neutron star 1.2%, supergiant 0.8%, black hole 0.7%, O 0.5% (the game mix, D18).
- Danger 1 to 10: 1840, 1822, 2691, 3457, 4069, 4130, 3893, 3227, 2335, 2536; the core is at most 4.
- 2.63 lanes per system; 5.8 bridges and 5.8 chokepoints per galaxy; 7.0 regions per galaxy; region ages Mature 46.5%, Young 28.6%, Old 24.8%.
- No two systems closer than 30 units in 500 galaxies; names unique in every galaxy, also at 2,000 systems.
- Timing, warm, Release, this machine: default galaxy map 0.41 ms; with every system in full 1.72 ms (D19: under 50 ms); a 2,000-system map 210 ms, most of it the n² Gabriel pre-check.

## Still to do in the galaxy level

- Done 2026-10-03 (b8dc686): factions over lanes, points of interest, hazards, a monument per region and beacons, each on its own stream; see [the galaxy extras note](2026-10-03-galaxy-extras-design.md).
- Gate requirements on links (A9): 1.1, no seed change.
- The colliding-pair shape (idea 7, with U10). If it joins `Auto`'s tables, that changes seeds and must happen before 1.0.0-beta.1; as an explicit shape only, any time.
- Region themes do not yet steer the systems' tags or landmarks; doing that changes system output, so it is a before-beta decision too.
- 2,000-system maps take 210 ms: a Delaunay triangulation would cut the candidate pairs to about 3n, if a larger cap is ever wanted.

Related: builds on [the star system level design](2026-10-02-star-system-level-design.md); see also [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) and [the Stage 0 survey](2026-10-02-galaxy-stage0-survey-and-capture.md).
