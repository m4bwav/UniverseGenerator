---
title: "Galaxy extras design: factions, points of interest, hazards, monuments and beacons (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, galaxy, factions, points-of-interest, hazards, monuments, beacons, d24, streams, design]
summary: "read before changing the galaxy extras (coded 2026-10-03, b8dc686) or StarName (958960b): factions grown over lanes (idea 22), points of interest with placement rules (idea 23, A6), hazard areas, one monument per region and a few beacons per galaxy (D24), each on its own stream of the galaxy seed so no existing golden file moves"
---

# Galaxy extras design

## Summary

The last code of Stage 3: five layers of story on top of the galaxy map, drawn after the map and never fed back into it. Each has its own stream of the galaxy's seed (`factions`, `points`, `hazards`, `monuments`, `beacons`), reads only the finished map (positions, lanes, regions and their themes, hops, danger, chokepoints, star classes, the galaxy's context), and writes only new fields. No system's `SystemContext` changes, so every system, and every golden file from `core.json` to `universe.json`, stays as it is. A new golden file, `tests/Golden/v1/galaxy-extras.json`, holds them.

Designed on 2026-10-03 on branch `stage3-core` before coding (96376a1), and coded the same day (b8dc686). Code: `Packages/com.m4bwav.universe-generator/Runtime/Galaxies/GalaxyExtras.cs` (the public records and enums), `Packages/com.m4bwav.universe-generator/Runtime/Galaxies/GalaxyExtrasGenerator.cs` (the five passes), called at the end of `GalaxyGenerator.Generate`. Tests: `GalaxyExtrasTests`, `GoldenExtrasTests`, the extras checks in `HierarchyTests`. Sources: plan ideas 22 (factions over lanes), 23 and A6 (points of interest with placement rules), D24 (a monument per region, a few beacons per galaxy), the galaxy level note's "Still to do".

## Rules shared by all five

- Each extra draws only from its own stream, so changing one never moves another; the order of draws inside a stream is fixed by index order (systems, regions, factions).
- They are part of the `Galaxy` record, not addressable objects: `Universe.At` of the galaxy regenerates them, and the hierarchy tests compare them for galaxies alone, in clusters and in universes. A point of interest or a monument names its system by index; `galaxy.Map[i].Address` reaches the system.
- Danger is not changed by any extra: danger feeds each system's context, and changing it would change system output (a before-beta decision, like region themes steering tags).
- Cost: linear in systems and lanes (breadth-first searches, one pass per hazard); a 2,000-system map gains a few milliseconds at most.

## Factions over lanes (idea 22)

- Count: 1 under 8 systems; otherwise drawn from 2 to `min(6, 2 + n / 25)` (2 to 4 at 60, 2 to 6 from 100).
- Kinds (`FactionKind`): Empire, Republic, Corporate, Theocracy, Guild, Pirates. When a region has the theme "pirate haven" or "tidal frontier", the last faction is the pirates and its capital is that region's centre.
- Capitals: the first drawn among systems of danger 1 to 4 (the core's neighbourhood); each next among the systems at least 3 hops from every earlier capital (falling back to the farthest), weighted towards low danger for lawful kinds and high danger for pirates.
- Growth: each faction has a reach of 2 to 5 hops (empires 3 to 6), times the map's depth over 10 when its deepest system is more than 10 hops from the core (added while coding: on a 2,000-system map a reach of 5 hops held small bubbles). Breadth-first, one ring per round, factions in index order within a round, so a system goes to the first faction to reach it. Lawful factions never claim systems of danger 9 or 10; pirates claim anything. Unreached systems are independent.
- Output: `galaxy.Factions` (index, name, kind, capital, systems held, description) and, on each map entry, `Faction` (index or null) and `Contested` (a lane to a system of another faction).
- Names from the capital: "Vega Hegemony", "Free Worlds of Vega", "Vega Combine", "Synod of Vega", "Vega Compact", "Vega Raiders"; unique because capital names are unique.

## Points of interest (ideas 23, A6)

- `galaxy.PointsOfInterest`: kind, system, name, text, and for a precursor chain its step and length.
- Count: about one per 8 systems (drawn from n/10 to n/6, at least 1), plus a precursor chain of 3 to 5 sites from 30 systems up.
- Placement rules: ruins and precursor sites only in Old or Mature regions (history allows), three times as likely in a "precursor ruins" region; wrecks prefer bridges' ends and chokepoints (battles happen there); caches never at a chokepoint and never in the core region (fair starts); anomalies prefer "nebula maze" and "quarantine zone" regions; outposts prefer the edge (hops at least half the most). One point per system at most.
- The precursor chain spreads across regions: its first site in one region, each next in the region farthest (by hops between centres) from those already used, so following it crosses the map.
- Kinds (`PointOfInterestKind`): Ruins, PrecursorSite, Wreck, Cache, Anomaly, Outpost, Shrine. They can sit beside a system's own landmarks: a system landmark is what is in the system; a point of interest is a site the galaxy's story places there.

## Hazards

- `galaxy.Hazards`: kind, name, centre X and Y, radius, the systems inside, and an effect line.
- Count: 1 to `1 + n / 40` areas (1 or 2 at 60), plus one centred on every "nebula maze" region's centre, plus the core's radiation when the core is active (its existing `CoreHazardRadius`, no draw).
- Kinds (`HazardKind`): Nebula (sensors blind), IonStorm (shields and drives fail), GravityRift (lanes shift; travel slower), DarkCloud (no starlight; navigation by beacon), RadiationZone (the active core). Radius 80 to 220 game units, centred on a drawn system.
- Effects are text and system lists only; they do not change danger (see shared rules).

## Monuments (D24): one per region

- `galaxy.Monuments`: region, system, kind, name, text. Exactly one per region, in a system of that region (drawn, avoiding the core and points of interest when another system is free).
- Kind weighted by the region's theme (`MonumentKind`): ColossalStatue, Necropolis, Archive, Battlefield, GreatGate, Observatory, Shrine, Market. "old empire" favours statues and gates, "precursor ruins" necropolises and archives, "trade corridor" markets, "frontier" battlefields, "pirate haven" battlefields and markets.
- Names from the region's root: "the Vela Colossus", "the Archive of Vela".

## Beacons (D24): a few per galaxy

- `galaxy.Beacons`: system, kind, name, text: what a pilot sees from anywhere in the galaxy.
- Count: 2 to 4 (1 for under 10 systems, none for one system).
- Placement: the first at a bright star (giant, supergiant, neutron star or black hole) drawn among them when the map has one, then spread by farthest point over positions (added while coding: with the first drawn among all systems, 92% of beacons were artificial).
- Kind from the star where it fits, so the map and the beacon agree: a neutron star is a Pulsar, a supergiant or giant a BeaconStar, a black hole an AccretionGlow; any other star holds an artificial NavigationBeacon or SignalTower.

## Tests

- Golden: `tests/Golden/v1/galaxy-extras.json`, the extras of the eight galaxies of `galaxy.json`, two galaxies of a cluster with an active core, and the merging pair's frontier galaxy.
- Properties over 500 default galaxies and some larger ones: capitals owned by their factions; every held system joined to its capital through systems of the same faction; lawful factions hold no system of danger 9 or 10; `Contested` exactly when a lane crosses to another faction; one monument per region inside it; beacons on distinct systems; points of interest one per system, ruins only in Old or Mature regions, a chain's sites in distinct regions; hazard systems within the radius.
- Distribution: printed shares (claimed, contested, kinds), checked against broad bands.
- Hierarchy: the extras of every galaxy regenerated from its address equal the walked ones.

## Statistics (500 default galaxies; `The_shares_are_in_their_bands`)

- Factions: 3.03 per galaxy; 59.3% of systems held, 13.6% on a border. Kinds: Pirates 22.7% (a "pirate haven" region is in most galaxies of 7 regions), Empire 19.1%, Corporate 15.9%, Republic 15.7%, Guild 15.7%, Theocracy 11.0%.
- Points of interest: 11.8 per galaxy, a precursor trail in 499 of 500. PrecursorSite 32.3%, Wreck 13.8%, Ruins 13.5%, Cache 10.4%, Outpost 10.2%, Anomaly 10.0%, Shrine 9.7%.
- Hazards: 2.44 per galaxy, 3.1 systems each. Nebula 59.4%, DarkCloud 16.1%, IonStorm 14.7%, GravityRift 9.9%.
- Monuments: every kind 11 to 15%. Beacons: 3.03 per galaxy; NavigationBeacon 31.6%, SignalTower 31.0%, BeaconStar 28.8%, Pulsar 5.7%, AccretionGlow 2.9%.
- A 2,000-system map with its extras: 121 ms warm (the map alone was 210 ms on 2026-10-02, so the extras cost little). The golden file `galaxy-extras.json` is 88.6 KB. Size gate after the extras and StarName: DLL 499.0 KB, nupkg 370.7 KB, 35 Runtime files.

## Open for tuning (changes only galaxy-extras.json, before beta; no map or system moves)

- Pirates are 23% of factions because a "pirate haven" or "tidal frontier" region always seats them; a cap (pirates in at most half the galaxies) is one line.
- Almost every default galaxy has a precursor trail; it could be rarer (a draw) so it feels special.
- Factions hold 59% of systems; independents are the rest. A wider reach would fill more of the map.
- Points of interest name their system, so they are unique, except wrecks, which draw from 8 ship names.

## StarName (the public star-name entry point)

Added after the extras (958960b) for the website's star name tool (plan Stage 6). `StarName.Generate(seed)` returns one `StarName` (Name and Class); `StarName.Generate(seed, count)` returns 1 to 1,000 different names, the first equal to the single one; an optional `StarClass` fixes the class, otherwise it is drawn with the options' star mix (Mature age). Seeds: root `starname` under the seed text, child `name/i`, streams `star` and `names`; `StarNames` and its tables are unchanged, so no other output moves. The names are not the names of the systems of the same seed. Golden: `tests/Golden/v1/star-name.json`.

## Not here

- Gate requirements on links (A9) and the run map (idea 24): 1.1, no seed change.
- Stations of factions (pirate dens, naval bases) would change system output; the stations stay the system level's.

Related: builds on [the galaxy level design](2026-10-02-galaxy-level-design.md); see also [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) and [the improvement ideas](2026-10-02-galaxy-generator-improvement-ideas.md).
