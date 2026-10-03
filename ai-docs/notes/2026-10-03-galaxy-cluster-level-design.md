---
title: "Galaxy cluster level design for UniverseGenerator 1.0 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, galaxy-cluster, group, design, morphology, hubble-type, active-core, satellites, wormhole-ring, travel-network, addresses, universe-at]
summary: "read before writing or changing the galaxy cluster level: GalaxyCluster.Generate, groups and clusters (U5), type codes driving the shape (U6), age and richness (U7), active cores with a hazard radius (U8), satellites (U9), the travel network of gates, a wormhole ring and tethers (U2, N10), and how a galaxy in a cluster takes its type and flavour from the cluster while Galaxy.Generate alone keeps its output"
---

# Galaxy cluster level design

## Summary

`GalaxyCluster.Generate("my-seed")` makes a group or a cluster of galaxies as a cheap map (positions, type codes, roles, ages, richness, active cores) joined by a travel network that always connects every galaxy; each galaxy is generated in full only when read, and takes its shape, size, age, richness and core from the cluster. A galaxy generated alone draws exactly what it drew before; its new fields (name, type code, age, richness, core) are derived or drawn from a new stream, so `galaxy.json` cannot move.

## Scope

Plan D14's fifth level (cluster and universe together about 3 days; both may move to 1.1 without seed changes). Ideas U5 (group or cluster environment), U6 (type code driving the shape), U7 (age and richness), U8 (active cores with a hazard radius), U9 (satellite dwarf galaxies), U2 (travel network with long links, hub gates and tiers) and N10 (a ring of wormholes so every galaxy is reachable). Not here: U1 landmark slots, U10 merging pairs, U11 the cosmic web and U3 distance frames (the universe level); U4 flavour biasing the planet tables beyond richness (open).

## Addresses and seeds

- A cluster: `v1-my-seed/cluster`, seed `Child(FromText(seed), "cluster", 0)`, as every root.
- Galaxy i of it: `v1-my-seed/cluster/galaxy/i`, seed `Child(cluster, "galaxy", i)`. Its systems, planets and moons hang below as they do below a lone galaxy (`.../cluster/galaxy/2/system/31/planet/0`).
- A lone galaxy stays `v1-my-seed/galaxy` with the seed it had. The two are different places with different seeds, as a lone system and system 0 of a galaxy are.

## The cluster's own draws (streams of the cluster seed)

1. **Kind, stream `kind` (U5).** `ClusterKind` option: `Auto` (default) draws Group 60, Cluster 40. A group is two big spirals with their satellites and a few small galaxies (the Local Group); a cluster has a giant elliptical at its centre, ellipticals and lenticulars crowded round it and spirals at the edge (the morphology and density relation).
2. **Age, same stream (U7).** Young 30, Mature 45, Old 25: the cluster's epoch. The universe level will pass its own epoch in as context; a lone cluster draws it.
3. **Name, stream `names`.** "{Root} Group" or "{Root} Cluster" from a list of 32 roots not used by regions (Aster, Calyx, Halcyon, Helix, Kestrel, Meridian, Vesper, Zephyr ...).
4. **Members, stream `members`.** Group: 2 majors; 0 to 2 members; 1 to 3 satellites per major; 0 to 3 free dwarfs (3 to 13 galaxies). Cluster: 1 central giant (`cD`), 1 to 2 other majors, 8 to 16 members, 0 to 2 satellites per spiral major, 2 to 5 free dwarfs (12 to 28). Order of indices: majors first (the central giant is galaxy 0 of a cluster), then members, then free dwarfs, then satellites by host.
5. **Sizes, same stream.** With S = the `Systems` option (default 60): majors S × 1.4 to 2.0 (the central giant S × 2.0 to 3.0), members S × 0.6 to 1.1, satellites and dwarfs S × 0.15 to 0.4 and at least 5; rounded, clamped 1 to 2,000.
6. **Positions, stream `layout`.** The cluster map has a radius of 1,000 cluster units; each galaxy has a drawn size on it, 40 √(systems / 60) units. Group: the two majors on a random axis, 250 to 400 units either side of the centre. Cluster: the central giant at (0, 0). Members and free dwarfs at radius 0.9 R u^0.75 (crowded towards the centre in a cluster) or 0.9 R √u (a group), at a uniform angle; satellites 1.3 to 2.5 host sizes from their host. A place is refused when it overlaps an earlier galaxy (centres closer than 1.2 times the two sizes); after 200 refusals the spacing relaxes by a tenth. Positions rounded to 3 decimals before any decision.
7. **Type codes, stream `types` (U6).** A small vocabulary that reads like the real one and maps onto the five map shapes:

   | Code | Shape | Who gets it |
   |---|---|---|
   | `Sa`, `Sb`, `Sc` | Spiral (pitch band a 0.25 to 0.31, b 0.31 to 0.38, c 0.38 to 0.45) | majors and members |
   | `SBa`, `SBb`, `SBc` | Barred, same bands | majors and members (30% of spirals are barred) |
   | `E0` to `E7` | Elliptical, axis ratio 1 - n / 10 | members; `E` the cluster's other big galaxies |
   | `cD` | Elliptical, round (E0 to E2) | the central giant of a cluster |
   | `S0` | Elliptical, flattened (E3 to E6): a disc that lost its gas | members |
   | `Ring` | Ring | members, rarely (a collision ring) |
   | `Irr` | Irregular | members |
   | `dE`, `dSph` | Elliptical | satellites and dwarfs |
   | `dIrr` | Irregular | satellites and dwarfs |

   Group majors: Sb 35, Sc 35, SBb 15, SBc 15. Cluster majors after the giant: E 50, Sb 25, SBb 25. Members by distance from the centre, in a cluster: inner third E 45, S0 40, spiral 10, Irr 5; middle E 25, S0 35, spiral 30, Irr 10; outer E 10, S0 20, spiral 50, Irr 15, Ring 5; in a group: E 15, S0 25, spiral 30, Irr 25, Ring 5. Satellites and dwarfs: dSph 40, dE 25, dIrr 35 (free dwarfs dIrr 60). **A spiral or barred type under 80 systems becomes S0** (plan D16: arms do not read under 80; a spiral that lost its arms is what an S0 is). Spiral letters and elliptical digits are drawn on the same stream.
8. **Age, richness and core, stream `traits` (U7, U8).** Galaxy age: ellipticals, `cD`, S0, dE and dSph Old (Mature in a Young cluster); spirals the cluster's age; Irr, dIrr and Ring Young. Richness (metallicity): majors Rich 50, Normal 50; members Normal 70, Rich 15, Poor 15; satellites and dwarfs Poor 70, Normal 30. Core activity: majors Quiet 60, Seyfert 30, Quasar 10 (the central giant 50, 30, 20); members 85, 13, 2; dwarfs always Quiet; a Young cluster doubles the quasar weight. Every galaxy draws the same number of values whatever it gets.

## The travel network (U2, N10), no draws

Three tiers, at most one link per pair (the higher tier wins):
- **Wormhole ring (N10).** Every galaxy that is not a satellite, except a cluster's central giant, sorted by angle around the centre (ties by index), each joined to the next and the last to the first. Every galaxy is then reachable, whatever else is cut. Two galaxies make one link; one makes none.
- **Gates (U2's hub gates and long primary links).** Majors are hubs: every major joins every other major, and every member and free dwarf joins its nearest major.
- **Tethers.** Each satellite joins its host.

`ClusterLink` holds A < B, the tier and its length on the cluster map. Inside a galaxy each link ends at a system chosen without draws: a gate at the core; a wormhole or tether at the system farthest out towards the other galaxy (largest x dx + y dy, ties to the lowest index). `galaxy.Gates` lists them (other galaxy, tier, system).

## What a galaxy takes from its cluster (GalaxyContext)

The cluster's map entry for galaxy i becomes its context; `GalaxyContext.Alone` reproduces today's behaviour exactly.

| Field | In a cluster | Alone (unchanged map) |
|---|---|---|
| Systems | the entry's size | the `Systems` option |
| Shape | from the type code; the `Shape` option is ignored inside a cluster | as before (`Auto` rolls, or the option) |
| Pitch, ellipse | the drawn pitch mapped into the type's band; the ellipse 1 - n / 10 | as drawn |
| Region ages | weights by galaxy age: Young 55, 35, 10; Mature 30, 45, 25 (today's); Old 5, 35, 60 | 30, 45, 25 |
| Danger | plus 2 (Seyfert) or 3 (Quasar) within the hazard radius of the centre (150 or 300 units), at most 10 | as before |
| Richness | `SystemContext.Richness`: the gas giant weight × 50% (Poor), 100%, 160% (Rich); giant planets follow metallicity (Fischer and Valenti 2005) | 100%, which leaves the weight's integer untouched |
| Name | the cluster's | drawn from the galaxy's new `name` stream |
| Type, age, core | the cluster's | derived: Spiral S + pitch letter, Barred SB + letter, Elliptical E + round(10 (1 - ellipse)), Ring `Ring`, Irregular `Irr`; age Mature; core Quiet |

Every stream of the galaxy draws exactly what it drew before; only the values it uses change, and only inside a cluster. The shape stream still draws its roll and every parameter.

## Records

- `GalaxyCluster`: Address, Name, Kind (Group, Cluster), Age, Radius (1,000), `Map` (one `ClusterEntry` per galaxy: Index, Address, Name, Type, Shape, Role (Major, Member, Satellite, Dwarf), Host (-1 unless a satellite), X, Y, Size, Systems, Age, Richness, CoreActivity), `Links`, `Galaxies` generated on demand and `Galaxy(i)`.
- `Galaxy` gains Name, Type, Age, Richness, CoreActivity, CoreHazardRadius and Gates (empty alone). `GalaxyJson` in the tests does not write them, so `galaxy.json` stays; `tests/Golden/v1/cluster.json` writes them for lone galaxies and for galaxies of clusters.
- Options: `ClusterKind` (Auto, Group, Cluster).
- `Universe.At` reads `v1-seed/cluster`, `.../cluster/galaxy/i` and everything below.

## Tests

Golden `cluster.json` (clusters of several seeds and kinds; their maps and links; two galaxies of each in full with one system; the new fields of the lone galaxies of `galaxy.json`), and the five existing golden files unchanged. Properties over thousands of seeds: every galaxy reachable over links, satellites near their hosts, no overlaps, unique names, spirals at 80 systems or more, type matches shape, a galaxy's fields equal its map entry, gate systems valid, quasars raise danger only inside their radius. Distributions: kind shares, sizes, morphology by distance, core activity, Old galaxies holding more Old regions, Rich galaxies more giant planets. Hierarchy: every object of a few clusters regenerates from its address.

Related: builds on [the galaxy level design](2026-10-02-galaxy-level-design.md); see also [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md), [the improvement ideas](2026-10-02-galaxy-generator-improvement-ideas.md) (U2, U5 to U9, N10), [determinism rules](../../kb/rules/determinism.md).
