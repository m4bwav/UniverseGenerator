---
title: "Star system level design for UniverseGenerator 1.0 (2026-10-02)"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [universegenerator, star-system, design, stars, planets, moons, belts, stations, landmarks, story-tags, names]
summary: "read before writing or changing the star system level: records, the mass bands that keep spectral classes consistent, two-chain orbits, size classes with the radius valley and hot Neptune desert built in, moons by Hill radius, belts, stations, landmarks, tags and descriptors; nothing here is coded yet"
---

# Star system level design

Worked out on 2026-10-02 for the C# code on branch `stage3-core`. The prototype (`Tests/GalaxyPrototype/SystemProto.cs` on the game repository's capture branch) is the starting point; changes from it are marked.

## Context, so a system alone equals the same system in its galaxy

A system's own content comes from its own streams. What depends on its neighbours (name unique in the galaxy, region age, danger, position) comes in as a context. The galaxy computes that context in a cheap skeleton pass (positions, lanes, regions, star rolls, names in index order). An address regenerates the skeleton, then the one system. A system generated alone (`StarSystem.Generate(seed)`, root `v1-seed/system`) draws its context from its own streams: age from "age" (Young 30, Mature 45, Old 25), danger from "danger" (1 to 10), name from "names".

## Stars

- Mix weights by age, unchanged from the prototype (Game). Plausible, per 10,000: M 7250, K 1290, G 590, F 310, A 60, B 4, O 0, white dwarf 590, giant 40, supergiant 1, neutron star 10, black hole 2.
- **Changed: mass bands chosen so the derived temperature lands in the class's band** (L = 0.23 M^2.3 below 0.43, M^4 to 2, 1.4 M^3.5 above; R = M^0.8; T = 5778 (L/R²)^0.25, so T = 5778 M^0.6 from 0.43 to 2): M 0.08 to 0.47, K 0.48 to 0.83, G 0.84 to 1.06, F 1.07 to 1.54, A 1.55 to 2.65, B 2.7 to 26, O 27 to 60. Log-uniform within a band (DMath.Exp and Log). The prototype's bands gave G stars at 5,050 K.
- Spectral type: main sequence `{letter}{digit}V`, digit = floor(10 (Thigh - T) / (Thigh - Tlow)) clamped 0 to 9 (O's top 50,000 K); giants `{letter from T}{digit}III`, supergiants `...I`; white dwarfs `DA{round(50400 / T)}` clamped 1 to 9; neutron stars and black holes empty. Sun-like check: M 0.99 gives about 5,740 K, G2V.
- Description: "blue star", "blue-white star", "white star", "yellow-white star", "yellow star", "orange dwarf", "red dwarf", "white dwarf", "red giant" (or by temperature), "blue supergiant" or "red supergiant" by temperature, "neutron star", "black hole". Colour from temperature as in the prototype.
- Companion (binary percent by class as the prototype): class M 60, K 25, G 10, white dwarf 5, never heavier than the primary (redraw as an M dwarf under the primary's mass). Close (45%): 0.02 to 0.3 au, planets circle both, first orbit at least 3 times the separation, zones from the summed luminosity. Wide (55%): 50 to 2,000 au log-uniform; under 200 au, planets stop at 0.3 times the separation and the count drops by one.
- Zones live on the system (habitable inner √(L/1.1), outer √(L/0.53), frost line 2.7√L, L summed for a close pair). Rounding: mass 3 decimals, luminosity 5, radius 6 (neutron stars are 0.000015 Suns), temperature 0.

## Planets: two chains, then size class, then kind

- Counts (inner min to max, outer min to max): O 0-1, 0-3; B 0-2, 0-3; A 0-3, 1-4; F and G 1-5, 0-4; K 1-5, 0-3; M 1-5, 0-2; white dwarf 0-1, 0-3; giant 0-1, 1-4; supergiant 0, 0-2; neutron star 0-2, 0; black hole 0-1, 0-2. Capped by MaxPlanetsPerSystem (truncate at the end, so earlier planets keep their draws).
- **Changed: two chains.** The prototype's single chain from about 0.1 au with period ratio 1.8 rarely reached the frost line, so outer giants were rare. Inner chain: first orbit max(U(0.04, 0.3) × M^(1/3), 3 × the star's radius in au (0.00465 au per solar radius), 3 × a close separation); period ratio 1.8 × exp(0.28 z), z the sum of two U(-1, 1), clamped 1.25 to 4 (Weiss 2018); a = ratio^(2/3); the chain stops past the frost line. Outer chain: start max(last inner × 1.4, frost line × U(0.8, 1.5)); au ratio U(1.5, 2.3). Neighbours where either is a giant keep a period ratio of at least 1.6.
- Size class weights per zone [dwarf, terrestrial, sub-Neptune, ice giant, gas giant], g = 30 × clamp(M, 0.15, 1.6) (M dwarfs host about 4 times fewer giants): hot [0, 70, 12, 0, g/6]; warm [0, 65, 28, 0, g/8]; temperate [0, 70, 22, 0, g/10]; cold [8, 45, 15, 10, g]; outer [20, 12, 6, 30, 1.6 g], with outer giant weights doubled when an inner planet has 2 Earth masses or more (Zhu and Wu 2018).
- Mass bands (Earth masses): dwarf 0.001 to 0.05; terrestrial 0.05 to 2.8; sub-Neptune 4.7 to 20; ice giant 10 to 50; gas giant 50 to 4,000. **The gap between 2.8 and 4.7 is the radius valley** (Chen-Kipping radii 1.33 and 2.0). Peas in a pod: one pod mass per system (log-uniform 0.3 to 8), each small planet pod × exp(0.35 z), clamped to its band.
- Radius from mass (Chen and Kipping 2017, adjusted): terrestrial M^0.279; 2 to 95 Earth masses 0.808 M^0.589; above 95, 11.2 (M/318)^-0.044 (Jupiter exactly 11.2; continuous at about 95); hot Jupiters × 1.2.
- **Hot Neptune desert:** a sub-Neptune with a period under 10 days becomes a terrestrial core (mass × 0.3, stripped). A gas giant in the hot zone is a HotJupiter. Period days = 365.25 √(a³ / M), M the summed mass of a close pair.
- Kinds: SubNeptune is new (the commonest exoplanet). Terrestrial kinds by zone: hot Lava 30, Iron 20, Barren 40, Greenhouse 10 (mass at least 0.5); warm Barren 25, Desert 30, Greenhouse 20, Rocky 25; temperate Rocky 25, Ocean 22, Garden 18, Desert 15, Barren 12; cold Barren 30, Rocky 15, Ice 40, Desert 10; outer Ice 70, Barren 30. Under 0.1 Earth masses only Barren or Ice (Iron and Lava in the hot zone); under 0.2 no Ocean or Garden; no Garden around O, B, giants, supergiants or remnants (Rocky instead).
- Rounding: orbit 4 decimals (au), period 2 (days), mass and radius 3.

## Moons, rings, belts, stations

- Moons on the planet's own stream (planet seed = Child(system, "planet", index)). Counts: gas giant 1 to 8, ice giant 1 to 5, sub-Neptune 0 to 2, terrestrial 0 to 2 (0 to 1 under 0.3 Earth masses), dwarf 0 to 1, hot Jupiter 0. Orbits in planet radii: first U(3, 6) (outside the Roche limit), then ratio U(1.3, 2.0); a moon past half the Hill radius (a (m / 3M)^(1/3), m in solar masses = Earth masses × 3.003e-6, 1 au = 1.496e8 km, Earth radius 6,371 km) is dropped. Kinds: giant moons Ice 45, Barren 20, Volcanic 10, Ocean 15, Hazy 10, and Garden (25%) for one moon of a giant in the temperate zone; terrestrial moons Barren 85, Ice 15. Radius km: giant moons 200 to 2,700 log-uniform; others 100 to 0.3 of the planet's radius. Names `{planet} I`, `II` in orbit order.
- Rings: gas giant 40%, ice giant 30%, sub-Neptune 5%, terrestrial 2%.
- Belts on "belts": an asteroid belt in the widest au gap over 1.6 (55%), centred at √(a b), half width 0.15 (b - a), so never over a planet; a Kuiper (icy) belt from 1.6 to 2.1 times the last orbit (35%); with no planets, one belt near the frost line.
- Stations on "stations": count weights 0: 60, 1: 30, 2: 10; kinds TradeHub, Shipyard, MiningPlatform, ResearchStation, NavalBase, PirateDen, Refinery, Relay, drawn without repeats; names `{word} {kind}` from 64 words, or `{system} {kind}` (20%); each orbits a planet that exists (`int? Planet`, null = the star, 20%). Addresses `.../station/k`; no Guid.

## Landmarks, tags, descriptors (D23, D24)

- Natural landmarks from the data, no draws: UnusualStar (O, B, giant, supergiant, remnant), BinaryStar, LivingWorld (a Garden planet or moon), RingedGiant, HotJupiter, AsteroidBelt. None: one from "landmarks": Ruins, Derelict, Nebula, Comet, Anomaly. Outliers on "weird", Chance(Weirdness, 100): Megastructure, RoguePlanet, AncientBeacon, ShatteredWorld, TemporalAnomaly, DerelictFleet. Landmarks add facts and never contradict data. Record: Kind, Planet (optional index), Outlier, Text.
- System tags (1 to 2, the second at 35%, on "tags"), each with a condition so text agrees with data: frontier colony (habitable world), mining boom (belt or barren or iron world), pirate haven (PirateDen or danger 7+), quarantine zone (life), abandoned colony (Ruins or a temperate rocky world), research outpost (ResearchStation or unusual star), trade crossroads (TradeHub), holy site (any, low weight), naval stronghold (NavalBase), smuggler route (danger 4 to 8), refugee haven (habitable world), corporate enclave (Refinery, Shipyard or MiningPlatform), precursor ruins (Ruins, AncientBeacon or Megastructure), untouched wilderness (life, no stations), dead system (no world beyond barren, lava, iron or dwarf), gas mining (a giant), scientific curiosity (outlier or unusual star).
- Descriptors: system "a yellow star with 6 planets and an asteroid belt", "a red dwarf and white dwarf pair with 2 planets"; planet "a cold ocean world under a red dwarf" (zone adjective scorched, warm, temperate, cold, frozen; left out where the kind says it: lava world, hot Jupiter); "a" or "an" by the first letter.

## Names (D20)

From the embedded tables (`scripts/embed-star-tables.py`): neutron stars `PSR J{hh}{mm}{+-}{dd}{mm}`; black holes `Gaia BH{n}` or `XTE J...`; white dwarfs half `WD {hhmm}{+-}{ddd}`; bright classes (O, B, A, giant, supergiant) 35% proper name, 35% Bayer or Flamsteed, 30% HD or HIP; others 8% proper, 12% designation, the rest HD 25, HIP 15, GJ 15, Kepler 13, TOI 12, Wolf 8, Ross 6, LHS 6. HIP numbers from the gap list (walk the ascending gaps; no 118,218-entry array). Planets `{system} {letter}` with letters in a seeded discovery order. Unique per galaxy by redrawing in the skeleton pass.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md); see also [plausibility rules](../../kb/rules/plausibility-rules.md), [design rules](../../kb/rules/design-rules.md).
