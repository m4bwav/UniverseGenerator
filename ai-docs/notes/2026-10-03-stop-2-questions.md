---
title: "Stop 2 questions for UniverseGenerator 1.0.0-beta.1 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, stop-2, review, tuning, seed-promise, golden-files, beta]
summary: "read at Stop 2 or before changing any tuning before 1.0.0-beta.1: every seed-changing question (current value, alternative, which golden files move), the questions that change only galaxy-extras.json, the non-seed questions (matrix 1.0 rows not built, awkward APIs found by the README, CI and test items), gathered from the level notes' Open for tuning sections"
---

# Stop 2 questions

PR #1 (https://github.com/m4bwav/UniverseGenerator/pull/1) holds all of Stage 3. These are the decisions for Mark before 1.0.0-beta.1. After the first release tag, a seed-changing answer needs a new generator version instead, so these are cheapest now. The recommendation comes first in each row. Sources: the "Open for tuning" parts of the [star system](2026-10-02-star-system-level-design.md), [galaxy](2026-10-02-galaxy-level-design.md), [planet](2026-10-02-planet-level-design.md), [moon and belt](2026-10-03-moon-and-belt-level-design.md), [cluster](2026-10-03-galaxy-cluster-level-design.md), [universe](2026-10-03-universe-level-design.md) and [galaxy extras](2026-10-03-galaxy-extras-design.md) notes.

What the golden files hold decides what moves: `system.json` writes lone systems; `galaxy.json`, `cluster.json` and `universe.json` write maps plus a few systems in full; `planet.json` and `moon-belt.json` write planets and moons of lone systems and of the `my-seed` galaxy; `galaxy-extras.json` writes the extras of galaxies, a cluster and a universe. "Every system file" below means system, galaxy, cluster, universe, planet and moon-belt. A changed golden file is deleted and rewritten on purpose, said in the commit message (AGENTS.md); `core.json` and `star-name.json` move in none of these.

## Seed-changing

| # | Question | Now | Alternative | Moves |
|---|---|---|---|---|
| S1 | Colliding-pair galaxy shape | not in `Auto`; recommend adding it later as an explicit `GalaxyShape` only (no seed change) | put it in `Auto`'s tables now | galaxy, galaxy-extras, cluster, universe, planet, moon-belt |
| S2 | Region themes steer system tags and landmarks | they do not (keep) | a "precursor ruins" region favours ruin tags, and so on | galaxy, cluster, universe, and planet and moon-belt where landmarks touch them |
| S3 | Giant planets' share | 30% of planets (gas 17%, ice 13%); reads well on maps (keep) | lower toward real statistics | every system file |
| S4 | "holy site" tag | always eligible, 11% of tags | make it conditional (life, ruins or a landmark) | system, galaxy, cluster, universe |
| S5 | Temperate deserts | can be cold (-37 °C at the 0.6 bar cap) | a warmer floor or a higher pressure cap for temperate deserts | planet (and moon-belt if moons read the planet's temperature) |
| S6 | Tidal lock and hazard 4 | 41% of planets locked; hazard 4 at 0.2% | a narrower lock rule; spread the hazard tiers | planet, moon-belt |
| S7 | Planets and moons by blue stars and giants | reach thousands of kelvin (true to the formula, "searing heat") | cap or push the inner orbits out | planet, moon-belt, and system if orbits move |
| S8 | Moon kinds | drawn without the moon's size or distance, so volcanic, ocean, hazy and garden moons rest on floors | draw each kind only where the physics allows | every system file |
| S9 | Ice moons of warm-zone giants | keep their ice above 273 K (8.6% of ice moons, trait "a sublimating crust") | turn them barren | moon-belt |
| S10 | Cluster counts and shares | groups of 4 to 13, clusters of 12 to 28; quasars 3.7% of galaxies (10% of majors); satellites take the `Systems` floor of 5; richness moves only the gas giant weight | other counts; richness also moves the planet tables; an option for the number of galaxies (additive) | cluster, universe, galaxy-extras |
| S11 | A galaxy alone | always Mature, Normal and Quiet | draw its age, richness and core | galaxy and everything built on galaxies |
| S12 | Universe shape | 4 to 7 clusters, no option; nodes 0, 1 and 2 have fixed roles (home, great cluster, merging group) | roles by position; a count option | universe, galaxy-extras |
| S13 | Home spiral under 40 systems | becomes an S0 (cluster spirals under 80 shrink there) | keep it a spiral at any count | universe, galaxy-extras |
| S14 | Galaxy names in a universe | unique within a cluster only (roots repeat across clusters) | unique across the universe | universe, galaxy-extras |
| S15 | Voids | large, filling most of the empty map as real voids do | smaller | universe |
| S16 | The merging pair's maps | no tidal-tail geometry (tied to S1) | tidal tails | universe, galaxy-extras |

## Changing only galaxy-extras.json (no map or system moves)

| # | Question | Now | Alternative |
|---|---|---|---|
| E1 | Pirates | 23% of factions, because a "pirate haven" or "tidal frontier" region always seats them | cap: pirates in at most half the galaxies (one line) |
| E2 | Precursor trail | in 499 of 500 default galaxies | a draw, so it feels special |
| E3 | Faction reach | factions hold 59% of systems | a wider reach fills more of the map |
| E4 | Wreck names | 8 ship names, so wrecks can repeat | a longer list |

## Not seed-changing

- N1. Rows the feature matrix marks 1.0 that are not built: `seed-url` (`Options.ToCode` / `FromCode`), presets Pocket, Roguelike, Cozy and Epic (only Default, SpaceOpera and Plausible exist), `data-tables-editable` (with a ScriptableObject adapter), `hooks-plugins`, `typed-units` (a Units option), `diagnostics` (typed warnings), `custom-fields` (a Tags dictionary), `names-fantasy`, `export-json` (the JSON writer is internal), the Unity float adapter, and options for lane density and arm count. Every one is additive (the lane roll and the arm count are already drawn whatever the option), so: build some before beta, or move them to 1.x in `kb/features/status.json`? Recommendation: move them to 1.x and rebuild the matrix; `export-json` and `seed-url` are the most wanted.
- N2. Awkward APIs the README showed (left as they are, for Mark's view):
  - The plan's `galaxy.System(31).Planet(2)` does not exist; the README uses `galaxy.System(31).Planets[1]`. A `Planet(int)` method on `StarSystem` (and `Moon(int)` on `Planet`) would match `galaxy.System(i)` and `cluster.Galaxy(i)`.
  - `Universe.At` returns `object`, so every call casts. Typed helpers (`Universe.PlanetAt(address)`) or a generic `Universe.At<T>` would read better.
  - An address does not carry options, so `Universe.At(planet.Address)` after a SpaceOpera galaxy finds a different planet, or throws "Were the same options passed...". The plan's second example had exactly that bug. `seed-url` (N1) or options in the address would fix it.
  - Records that hold lists compare by reference, so `Universe.At(a) == original` is false for the same content; the README compares `Summary`. Value equality would need custom `Equals`.
  - `StarName.Generate(42)` does not compile: the `long` overload needs a count.
- N3. The planet level: garden moons are mostly marginal for people (`Species.Human` needs 0.2 g, and most garden moons are small).
- N4. The CI golden check guards golden files from the first release tag (`v*`), not before: confirm.
- N5. `universe.json` is 813 KB, the largest golden file (every galaxy of four universes). Fine for the repository; it is not in the package.
- N6. `Distances`' travel days are stylised and tied to no game.
- N7. Test checklist items not done, listed rather than started because each takes long: the Unity compile check (needs a Unity 6 project, Stage 5 does it anyway), scale tests (presets at 2,000 systems), BenchmarkDotNet (a default galaxy under 50 ms cold; the statistics tests measure 1.72 ms warm in full).
- N8. Before the repository goes public: the secret, private-name and local-path scan of the whole history.

## Sizes at Stop 2

Size gate, all green: nupkg 373.5 KB, largest DLL 499.0 KB, UPM 456.9 KB unpacked and 116.0 KB compressed (now counting `Samples~`, which OpenUPM ships), 35 Runtime files, 9,736 lines. The README (8.0 KB) added 2.8 KB to the nupkg.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md); see also [the galaxy extras design](2026-10-03-galaxy-extras-design.md), [the universe level design](2026-10-03-universe-level-design.md), [the package size budget](2026-10-02-package-size-budget.md).
