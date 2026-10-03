---
title: "Stop 2 questions for UniverseGenerator 1.0.0-beta.1 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, stop-2, review, tuning, seed-promise, golden-files, beta]
summary: "ruled 2026-10-03 (PR #1 merged with no rulings: every question keeps its current value; N1 ruled: every unbuilt 1.0 row is built for 1.0.0, with a cost table); read before changing any tuning, or before 1.0.0 for the open N1 rows: every seed-changing question (current value, alternative, which golden files move), the questions that change only galaxy-extras.json, the non-seed questions (matrix 1.0 rows not built, awkward APIs found by the README, CI and test items), gathered from the level notes' Open for tuning sections"
---

# Stop 2 questions

## Ruled (2026-10-03)

Mark merged PR #1 into `master` on 2026-10-03 at 16:46 UTC (merge commit 32df421). He left no comments or review, and his reply when asked was "I merged the pr". The session prompt's rule for a question with no ruling is "keep the current value", so the outcome is:

- S1 to S16 and E1 to E4 keep their current values. No rule changed, no golden file in `tests/Golden/v1/` changed, and 1.0.0-beta.1 ships the output on `master` as merged.
- N1: the matrix rows marked 1.0 but not built stay as they are in `kb/features/status.json`. That is still open for 1.0.0, not for the beta. Before 1.0.0, either build them or move them to 1.x through `status.json` and `build_matrix.py`.
- N2: no API was added. `StarSystem.Planet(int)`, a typed `Universe.At` and a `StarName.Generate(long)` without a count would all be additive, and any of them can land in a 1.x minor without changing output.
- N3 to N6 stand as written. N7 (Unity compile check, scale tests, BenchmarkDotNet) waits for Stage 5. N8 (the history scan) is the first step of Stage 4.

After the first release tag, any S or E change needs a new generator version (AGENTS.md, the seed promise).

## N1 ruling table (ruled 2026-10-03: every row in 1.0.0)

**Ruled 2026-10-03:** Mark put every row below in 1.0.0, the four partial rows and the Unity float adapter included ("Do all the things"); nothing moves to 1.x. He also rejected the effort column: it was in human working days, while this run built all six levels in about a day and a half of agent time. Read it as relative size only; at agent pace the whole table is a few sessions. Each row lands as its own additive pull request (or a small group), in the order of the table.


Checked against `kb/features/status.json` and the Runtime folder on `master` a3bedd9. The note's list is right: none of its rows has code (no `ToCode`, no Pocket, Roguelike, Cozy or Epic preset, no hook, unit, warning or user-field type, no syllable names, the JSON writer is `internal`, no lane or arm option). `status.json` has 58 rows at "1.0" and most are built; the status words are have (the game does it), 1.0, 1.x and rejected, so built 1.0 rows keep "1.0".

The check found four more 1.0 rows that are only partly built. N1 did not list them:

- `orbital-elements`: `Planet.Period` exists; eccentricity and inclination do not. The matrix marks it seed-changing, but new draws on a new stream name of the planet's seed change nothing else, so it can still be additive (one new golden file).
- `galaxy-shapes`: five shapes exist; the row's "add cluster, colliding pair, starburst" does not. S1 ruled: colliding pair later, as an explicit `GalaxyShape` only (`Auto` unchanged).
- `constraints`: only the landmark guarantee ("always at least one"); there is no general "at least one X" option.
- `diagnostics` also covers D16's `GenerationBudget`, which does not exist; the `MaxSystems` constant (2,000) caps a galaxy instead.

Size now (ci on `master` a3bedd9, run 37159609289): nupkg 373.2 KB, largest DLL 499.0 KB (green under 750 KB), 35 Runtime files, 9,736 lines, all green. The DLL is the metric to watch: building every row below adds roughly 3,000 to 4,500 lines and could take it to about 600 to 700 KB, green but near the yellow line. Building only the "1.0" recommendations adds about 1,000 lines, about 30 to 50 KB.

| Row | What building it takes | Effort | Recommendation |
|---|---|---|---|
| `seed-url` | `GeneratorOptions.ToCode()` and `FromCode(string)`: a short text code of the settings that differ from the defaults, plus `Universe.At(address, code)` or a link form (address plus code) so an address can carry its options (fixes N2's third point). One new file in `Options/`, about 150 lines, about 2 KB; tests for round trips and readable errors; README and Examples.cs | half a day | **1.0**: most wanted, small, fixes N2 |
| `export-json` | Make the per-level JSON public: `ToJson()` on each level (or a `Json` class) with a versioned schema field, written by the existing hand-written `JsonWriter`, no reflection. The test project's seven writers (618 lines) show the shape. 2 to 3 files, about 700 lines, about 6 KB; one new golden file per level for the export format; README and Examples.cs | 1 day | **1.0**: most wanted; the schema becomes a promise, so it needs its own version |
| Preset `Pocket` | A record in `Preset` with fewer systems and planets (for example 20 systems, 6 planets). 5 lines, one golden file | 1 hour | 1.0 with Epic, or 1.x |
| Preset `Epic` | A record with many systems (for example 300, inside `MaxSystems`). 5 lines, one golden file | 1 hour | 1.0 with Pocket, or 1.x |
| Preset `Roguelike` | Needs settings that do not exist yet (more danger, more hazards, fewer safe systems); with today's options it would be only "more weirdness" | 1 hour thin, 1 to 2 days real | **1.x**, with danger and hazard options |
| Preset `Cozy` | The same: needs fewer hazards and less danger, which no option controls | as Roguelike | **1.x** |
| `lane-density` | An option for the extra-lane share (today 25%, `LaneExtraPercent`); null keeps the drawn default, so no seed moves. About 30 lines, validation, a golden file for one non-default value | 2 hours | 1.0 (cheap) or 1.x |
| `spiral-arm-count` option | An `Arms` option (2 to 4, null = drawn); the arm count is drawn whatever the option, so default output is unchanged. About 30 lines, a golden file | 2 hours | 1.0 (cheap) or 1.x |
| `typed-units` | A `Units` option (game units, AU, light years, parsecs) applied to every distance and size field at every level, or `In(Units)` helpers. Touches most records | 1 to 2 days | **1.x** |
| `diagnostics` | Typed warnings: `Validate()` returning soft problems and each level reporting trims (a shape that could not hold every system, `GenerationBudget`). A new `Warnings` list per level, about 300 lines | 1 day | **1.x** |
| `custom-fields` | A user dictionary on each object (`Custom`, since `StarSystem.Tags` already holds story tags). Small alone, useful only with hooks | 2 to 3 hours | **1.x**, with hooks |
| `hooks-plugins` | Post-processors (`OnSystemGenerated` and so on) that also run when `Universe.At` regenerates an object, so they belong in the options; delegates in an options record break value equality and `ToCode` | 1 to 2 days, with design | **1.x** |
| `data-tables-editable` | Public table records, JSON tables by id read by a hand-written parser (no reflection, nothing parsed in a static constructor), and a ScriptableObject adapter in the Unity package. The largest row and the biggest size risk | 3 to 5 days | **1.x** |
| `names-fantasy` | A syllable-set name generator for systems and planets on its own stream, one set in core (about 5 KB), culture sets in the Names add-on | 1 day | **1.x** |
| Unity float adapter | `Vector2`/`Vector3` helpers in a second asmdef that references UnityEngine, excluded from the NuGet build; it can only be compiled in Unity | half a day, in Stage 5 | **Stage 5** (UPM only; nothing in the NuGet package) |
| `orbital-elements` (partial) | Eccentricity and inclination on a new stream of the planet's seed; one new golden file | 3 to 4 hours | 1.x |
| `galaxy-shapes` (partial) | Colliding pair, starburst and cluster as explicit shapes; `Auto` unchanged (S1) | 1 day | **1.x** (S1) |
| `constraints` (partial) | A general "at least one X" option with a repair pass on its own stream | 1 to 2 days | **1.x** |

Totals, as first written in human working days (relative size only, see the ruling above): everything about 12 to 18; seed-url and export-json about 1.5.

Every row is additive: none changes an existing seed's output or an existing golden file, so any of them can also land in a 1.x minor after 1.0.0.

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
