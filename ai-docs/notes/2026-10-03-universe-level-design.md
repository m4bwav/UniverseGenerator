---
title: "Universe level design for UniverseGenerator 1.0 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, universe, cosmic-web, filaments, voids, landmarks, merger, epoch, distance-frames, addresses, universe-at]
summary: "read before writing or changing the universe level: Universe.Generate beside Universe.At (the static class became the record), the epoch (U7), a cosmic web of clusters joined by filaments with named voids and lone void systems (U11), landmark slots (U1: home spiral, dying giant, ring galaxy, distant quasar), a merging pair with a tidal frontier and a conflict hook (U10, A12), the sky landmark every galaxy sees (A13), distance frames and travel times (U3), and how a cluster in a universe takes its kind and epoch while GalaxyCluster.Generate alone keeps its output"
---

# Universe level design

## Summary

`Universe.Generate("my-seed")` makes a small universe at game scale: four to seven galaxy groups and clusters on a map, joined by filaments (the cosmic web), with one to three named voids holding lone void systems. Four landmark slots are always filled (a home spiral, a dying giant elliptical, a ring galaxy and a distant quasar), one group is a merging pair whose facing edges are a lawless tidal frontier with a shared conflict hook, and every galaxy knows where the quasar sits in its sky. The universe's epoch sets every cluster's age. Each cluster is a map generated with the universe; its galaxies and systems are generated when read. A cluster, galaxy or system generated alone draws exactly what it drew before, so no golden file moves.

## Scope

Plan D14's last level. Ideas U1 (landmark slots), U3 (distance frames and travel times), U7's epoch, U10 (merging pairs), U11 (the cosmic web), A12 (the tidal frontier) and A13 (the sky landmark). Not here: tidal-tail geometry in the galaxy map (the colliding-pair shape, a seed question for Mark), A8 universe lore and precursors, A10 scale presets, an option for the number of clusters.

## Addresses and seeds

- The universe: `v1-my-seed/universe`, seed `Child(FromText(seed), "universe", 0)`, as every root.
- Cluster i: `v1-my-seed/universe/cluster/i`; its galaxies `.../cluster/i/galaxy/j` and everything below them as in a lone cluster.
- Void k: `v1-my-seed/universe/void/k`; its systems `.../void/k/system/j`, and their planets and moons below.
- A lone cluster stays `v1-my-seed/cluster` with the seed it had.

## The universe's own draws (streams of the universe seed)

1. **Epoch, stream `epoch` (U7).** New option `Epoch` (Auto, Young, Mature, Old); `Auto` draws Young 30, Mature 45, Old 25. Every cluster takes it as its age. A young universe has more groups and, through the cluster level's rule, twice the quasars; an old one more rich clusters. A lone cluster keeps its own draw unless the option names an epoch.
2. **The web, stream `web` (U11).** Nodes: 4 to 7. Void slots: 1 to 3, and 1 to 3 systems for each of the three slots (always drawn). Node positions on a map of radius 1,000 universe units: node 0 within 0.3 R of the centre, the rest at 0.9 R √u and a uniform angle, refused within 250 units of an earlier node, the spacing relaxed by a tenth after 200 refusals; rounded to 3 decimals.
3. **Node kinds, stream `web`.** Node 0 is the home group (Group), node 1 the great cluster (Cluster), node 2 the merging group (Group); the others by epoch: Young Group 75, Cluster 25; Mature 60, 40; Old 45, 55. Every node draws a kind, used or not. The `ClusterKind` option is ignored inside a universe, as `Shape` is inside a cluster.
4. **Names, stream `names`.** The universe "the {Root} Reach" from its own 16 roots; clusters "{Root} Group" or "{Root} Cluster" from the cluster roots, unique in the universe; voids "the {Root} Void" from 16 void roots. Galaxy names stay unique within their cluster only.
5. **The conflict hook, stream `story` (A12).** One of eight hooks for the merging pair.

## Filaments (U11), no draws

The Gabriel graph of the nodes: two nodes are joined when no third lies inside the circle on their segment as diameter. It holds the minimum spanning tree, so every cluster is reachable. Each filament ends, in each cluster, at the galaxy that is not a satellite and lies farthest towards the other node (largest x dx + y dy on the cluster map, ties to the lowest index). Inside that galaxy the filament opens at the system farthest out towards the other node, as a wormhole does. `Filament`: A < B, Length in universe units, GalaxyA, GalaxyB. `LinkTier` gains `Filament` and `Tidal` (added at the end; a filament gate carries the cluster it leads to in the new `GalaxyGate.Cluster`, -1 inside a cluster).

## Voids, no draws beyond the counts

Candidate points on a 41 by 41 grid of step 50 within 0.9 R. A point's clearance is the least of its distance to a node less 20 (a cluster's radius in universe units), its distance to a filament segment, and its distance to an earlier void's centre less that void's radius. Void k takes the point of greatest clearance (ties to the first in row order) and that clearance as its radius; a slot whose best clearance is under 50 makes no void. Each void holds its slot's 1 to 3 systems, generated as lone systems at their own addresses with an Old age, danger 1 to 3 from the system's own draw, rescaled, and Poor richness: far from any galaxy, quiet and metal-poor.

## Landmark slots (U1) and the merging pair (U10, A12)

The universe hands each cluster a `ClusterContext`: its kind, its age, its name and which slots it fills. A slot changes only values, never draws, so every stream of the cluster draws as before.

| Landmark | Where | What the slot sets |
|---|---|---|
| Home spiral | node 0, galaxy 0 (a group major, always a spiral) | core Quiet |
| Dying giant | node 1, galaxy 0 (the `cD`) | age Old, core Quiet |
| Ring galaxy | node 1, the first member | type `Ring`, shape Ring |
| Distant quasar | the node of index 3 or more farthest from node 0 (ties to the lowest), galaxy 1 (a major) | core Quasar |
| Merging pair | node 2, galaxies 0 and 1 (the two majors) | placed touching (centres one room-sum apart); their link becomes `Tidal`; both Young with starburst region weights (Young 70, Mature 25, Old 5) |

Inside each galaxy of the pair, the tidal link opens at the system farthest towards the partner; every system within 300 game units of it gains 2 danger (at most 10), and the region holding it is themed "tidal frontier" (A12). `UniverseMerger` names the collision "the {A}–{B} Collision" and carries the hook. A satellite of a merging major still tethers to it.

## The sky landmark (A13)

The distant quasar is the universe's sky landmark. Every galaxy of the universe but the quasar itself gets `Galaxy.Landmark`: its name, address, bearing in whole degrees on the universe map (0 along +x, counter-clockwise) and distance in light-years. A galaxy's universe position is its node's position plus its cluster position times 0.02. Alone, `Landmark` is null.

## Distance frames and travel (U3), no draws

`Distances` holds the light-years per unit of each map and a stylised travel time per link: a galaxy unit is 50 ly (radius 50,000 ly), a cluster unit 2,000 ly (radius 2 Mly), a universe unit 100,000 ly (radius 100 Mly, a cluster 20 units across). Travel days: a lane ceil(length / 100), a gate 7, a wormhole 3, a tether 2, a tidal link 1, a filament 30 + ceil(length / 50). `Distances.LightYears(Level, units)` and `Distances.TravelDays(kind, length)` give them; game units stay float-safe because every map keeps its own frame.

## Records

- `Universe` (was a static class): Address, Name, Epoch, Radius, `Nodes` (`UniverseNode`: Index, Address, Name, Kind, Role (Home, GreatCluster, Merger, Field), X, Y, Galaxies), `Filaments`, `Voids` (`CosmicVoid`: Index, Address, Name, X, Y, Radius, Systems generated on demand), `Landmarks` (`UniverseLandmark`: Kind, Name, Address), `Merger`, `Clusters` (generated with the universe; their galaxies on demand), `Cluster(i)`; static `Generate` and `At`.
- `Galaxy` gains `Landmark` (`SkyLandmark`); `GalaxyGate` gains `Cluster`. Neither `GalaxyJson` nor `ClusterJson` writes them, so their golden files stay.
- Options: `Epoch`.
- `Universe.At` reads `v1-seed/universe`, its clusters, voids and everything below.

## Tests

Golden `universe.json` (four universes of several seeds and options: the universe fields, nodes, filaments, voids with their systems, landmarks, the merger; every cluster's map through `ClusterJson`; the merging pair's and the quasar's galaxies with their cluster and universe fields) on net10.0 and net48, and the six existing golden files unchanged. Properties over thousands of seeds: filaments connect every node; nodes 250 apart; voids clear of nodes and filaments; every landmark slot filled with the type it promises; the pair touches and is linked `Tidal`; frontier danger only near the tidal gate; every galaxy but the quasar has a sky landmark. Distributions: epoch shares, node kinds by epoch, quasars by epoch. Hierarchy: every object of a few universes regenerates from its address.

Related: builds on [the galaxy cluster level design](2026-10-03-galaxy-cluster-level-design.md); see also [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md), [the improvement ideas](2026-10-02-galaxy-generator-improvement-ideas.md) (U1, U3, U7, U10, U11, A12, A13), [determinism rules](../../kb/rules/determinism.md).
