---
title: "Moon and belt detail design for UniverseGenerator 1.0 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2027-04-03
tags: [universegenerator, moon, belt, design, physics, tidal-heating, jeans-rule, subsurface-ocean, life, hazards, habitability, asteroid-composition, resources, addresses]
summary: "read before writing or changing moon or belt detail: what each moon gains (physics from its kind and radius, orbit and period around its planet, temperature from the planet's light plus tidal heat, air by the planet level's Jeans rule, hidden oceans, life, traits, hazards, habitability, descriptor and summary), where a moon's kind wins over the physics and why, belt addresses and seeds, belt composition, mass, largest body, temperature and resources, and the streams each draws from"
---

# Moon and belt detail design

## Summary

Every moon gains its physics, heat, air, life, story and danger, and every belt its address, composition and resources, all drawn from new streams of each moon's and belt's own seed, so no earlier golden file moves. Where the kind the star system level already gave a moon promises more than the physics allows (a volcanic moon far from its planet, a garden moon too small to hold its air), the kind wins and the note says how.

## How it was built

Plan D14's fourth level ("moon physics by parent, belt composition and resources", about 1.5 days), worked out on 2026-10-03 on branch `stage3-core` before coding. Games and fiction first (D18): cheap formulas that keep the text and the numbers agreeing, checked against Io, Europa, Titan and the Moon. The planet level's code is reused where a moon behaves like a small planet: the greenhouse and pressure solver, climate bands and biomes, hazards, the Earth Similarity Index, habitability per species and the summary line.

## The rule that keeps the other golden files still

A moon's existing fields (address, index, name, kind, orbit in planet radii, radius in km) are made exactly as before, on its planet's `moons` stream. Everything new comes from streams of the moon's own seed, `Child(planet, "moon", j)`, the seed its address already gives, and is applied after the planet's own detail, so `planet.json` cannot move. Belts gain an address and a seed, `Child(system, "belt", k)`; their existing fields are drawn on the system's `belts` stream as before. `system.json`, `galaxy.json` and `planet.json` write their fields explicitly, so new fields cannot reach them. Each stream draws a fixed number of values whatever it decides; decisions are made on rounded values.

## Moons: order of derivation

1. **Physics, stream `body`.** Density by kind (g/cm³, uniform): barren 2.8 to 3.6 (the Moon 3.34), ice 1.2 to 2.0, volcanic 3.3 to 3.7 (Io 3.53), ocean 2.6 to 3.1 (Europa 3.01), hazy 1.7 to 2.0 (Titan 1.88), garden 4.0 to 5.5. With R = radius / 6,371 in Earth radii: gravity ρ R / 5.514 g, escape velocity 11.186 R √(ρ / 5.514) km/s, mass ρ R³ / 5.514 Earths (6 decimals). Composition: barren, volcanic and garden Rock; ice, ocean and hazy IceAndRock.
2. **Orbit, no draws.** Distance = orbit × the planet's radius in km; period 2π √(a³ / (398,600 M)) seconds, in days (Io 1.77). Every moon is locked to its planet, so its rotation is its period (hours) and its day the same length; its tilt to its star is its planet's.
3. **Tidal heat, stream `tides`.** Peale's formula for the surface heat flow, (21/2)(k₂/Q) G M² R³ n e² / (4π a⁶) W/m², with k₂/Q by kind (rock 0.001, ice and haze 0.003, ocean 0.01, volcanic 0.015) and e log-uniform 0.0001 to 0.003 (orbits circularise). Checked: Io 2.25 W/m² (measured about 2.4), Europa 0.15, Enceladus 0.019. **The kind bounds it:** a volcanic moon has at least a drawn 1 to 4 W/m² and at most 10; an ocean moon at least 0.02 to 0.3 and at most 0.5; every other moon at most 0.5 (more would melt it into a volcanic moon). Resonances with neighbouring moons keep Io and Europa heated, so the floor is the story; the formula alone would leave half the volcanic moons cold, because the system level draws kinds at any distance (see "Open for tuning").
4. **Light and temperature, stream `surface`.** Insolation is the planet's (same distance from the star). Albedo by kind ± 0.05: barren 0.12, ice 0.5, volcanic 0.6, ocean 0.65, hazy 0.22, garden 0.30. T_eq = 278.6 (S (1 - A))^¼ as for planets; tidal heat adds as T₀ = (T_eq⁴ + F / σ)^¼. The same stream draws the water inventory (ice 0.6 to 1 as ice, ocean 1 as an ice shell, garden 0.3 to 0.85) and a garden's target temperature.
5. **Air by the planet level's Jeans rule, stream `atmosphere`.** The same `LightestGasKept` = 0.8976 × 4 T_eq / v_esc² (stellar T_eq, not the tidal one). Candidates by kind: barren and ocean none; ice 25% a breath of nitrogen and methane (0.0001 to 0.01 bar); volcanic sulphur dioxide from its volcanoes (0.0001 to 0.001 bar); hazy nitrogen and methane 0.5 to 5 bar, log-uniform (Titan 1.5); garden nitrogen and oxygen solved for a target of 275 to 300 K within 0.5 to 3 bar, adding carbon dioxide when short, as planet gardens do. Barren, ice and volcanic moons lose what the rule says they lose; a trace that cannot stay is "leaking away to space". **Hazy and garden moons keep their air although the rule says it leaks** (only a moon over about 3,300 km could hold nitrogen in a habitable zone, and the system level makes moons up to 2,700 km): `LightestGasKept` still says the truth, and `Why` says what renews it ("cryovolcanoes renew the nitrogen and methane as fast as they leak"; "life and volcanoes renew it as fast as it leaks"). Surface temperature by the planet level's grey greenhouse (Titan 97 K against 94 measured).
6. **Hidden ocean, no draws.** `SubsurfaceOcean`: every ocean moon; an ice or hazy moon under 260 K with tidal heat of at least 0.01 W/m² or a radius of at least 1,200 km (Enceladus, Ganymede, Callisto, Titan).
7. **Life, stream `life`.** Garden moons as garden worlds (complex, flora 2 to 5, fauna 1 to 5). Hidden oceans: ocean moons none 45, prebiotic 20, microbial 25, simple 10 (no plants under ice); ice and hazy moons with a hidden ocean microbial 5, prebiotic 10. Hazy moons without one prebiotic 30 (tholins). Barren and volcanic moons never.
8. **Climate bands and biomes, no draws.** The planet level's six bands, turning freely once per orbit of its planet with its planet's tilt. Kinds map onto the planet table: barren to barren, ice and ocean to ice, volcanic to lava, hazy to barren, garden to garden.
9. **Traits and anomaly, streams `traits` and `anomaly`.** One trait from about 25 moon traits, each with a condition on the data (active volcanoes, sulphur plains, ice geysers where tides heat an icy moon, a cracked ice shell, methane lakes, an orange haze, the giant fills its sky within 10 planet radii, eclipses every orbit, a sublimating crust on an icy moon above 200 K, a captured asteroid under 300 km, and the garden traits); a second at 40%. Anomalies from the planet level's surface list at the Weirdness percentage.
10. **Resources, stream `resources`.** The planet level's grades 0 to 5 with a base by kind ± 1: barren 3, 2, ice 1 far out, 0; ice 1, 1, 5, 1; volcanic 3, 4, 0, 1; ocean 1, 1, 5, 1; hazy 1, 1, 4, 4; garden 3, 2, 4, 1 (metals, rare elements, ices, gases); organics from life, methane, and 3 for a hazy moon's hydrocarbons.
11. **Hazards, no draws.** The planet level's list (vacuum, cold, molten rock for volcanic moons ...) plus the giant's radiation belts (tier 4 inside 15 radii of a gas giant; tier 3 inside 30, or 10 of an ice giant), eruptions on volcanic moons, geysers where the trait says so, and moonquakes from tidal heat of at least 0.1 W/m².
12. **Habitability, similarity, descriptor and summary.** As for planets (`HabitabilityFor(Species)` on `Moon` too). Descriptor: "a hazy moon of a ringed gas giant". Summary as a planet's, with gravity to two decimals and "an ocean under the ice" where there is one: "0.13 g, crushing nitrogen air at 1.5 bar ...; hazard 3".

## Belts

- **Address and seed.** `.../belt/k` in the order the system level made them, seed `Child(system, "belt", k)`; `Universe.At` reaches them. `Index`, and a `Name`: "{system} Belt" for an asteroid belt, "{system} Outer Belt" for an icy one.
- **Composition, stream `belt`.** Shares in whole percent of silicate rock, carbonaceous rock, metal and ice, summing to 100, from x = middle / frost line (the middle is √(inner × outer)). Asteroid belts: metal 4 to 12; carbonaceous 70 x - 5 ± 10, clamped 10 to 80; ice past 1.2 x, up to 30; silicate the rest, at least 5 (the Sun's belt, x about 1: carbonaceous 65%, as measured, 75% by number). Icy belts: ice 55 to 80, metal 0 to 2, carbonaceous 15 to 30, silicate the rest.
- **Mass and largest body, same stream.** Asteroid belts 0.0001 to 0.003 Earths (the Sun's 0.0004), largest body 100 to 500 km in radius (Ceres 470); icy belts 0.005 to 0.2 Earths, largest 300 to 1,300 km (Pluto 1,188). Log-uniform.
- **Temperature, no draws.** T_eq at the middle with albedo from the shares (silicate 0.20, carbonaceous 0.06, metal 0.15, ice 0.6).
- **Resources, stream `resources`.** Bases from the shares: metals 1 + metal / 4 + silicate / 40, rare elements 1 + metal / 5, ices ice / 20 + 1 (1 when there is no ice but carbonaceous rock is at least 50%, else 0), gases 2 for icy belts (methane, ammonia), organics carbonaceous / 20; each clamped and moved by ± 1 as for planets.
- **Summary.** "65% carbonaceous rock, 25% silicate rock, 10% metal; largest body 470 km in radius; -103 °C; richest in organics".

## Records

`Moon` gains Descriptor, Distance, Period, Composition, Density, Mass, Gravity, EscapeVelocity, Insolation, Albedo, TidalHeating, Temperature, DayTemperature, NightTemperature, Atmosphere, Water, Ice, SubsurfaceOcean, Rotation, Tilt, Bands, Biomes, Life, Flora, Fauna, Traits, Anomaly, Resources, Hazard, Hazards, Similarity, Habitability, Summary and `HabitabilityFor`. `Belt` gains Address, Index, Name, Composition (Silicate, Carbonaceous, Metal, Ice), Mass, LargestBody, Temperature, Resources and Summary. `PlanetResources` becomes `ResourceGrades`, shared by planets, moons and belts (nothing is released, so the rename is free). `tests/Golden/v1/moon-belt.json` writes the new fields explicitly.

## Open for tuning before beta

The star system level draws a moon's kind without its size or distance, so some kinds rest on the floors above: volcanic and ocean moons far from their planet (tides alone would need an eccentricity over 0.2 for half the volcanic moons), hazy and garden moons too small to hold their air, icy moons of giants in the warm zone. Drawing those kinds only where the physics allows would change `system.json` (seed-changing, so before 1.0.0-beta.1 only, Mark's call). The statistics test prints how often each floor is used.

Related: builds on [the planet level design](2026-10-02-planet-level-design.md) and [the star system level design](2026-10-02-star-system-level-design.md); see also [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md), [plausibility rules](../../kb/rules/plausibility-rules.md), [determinism rules](../../kb/rules/determinism.md).
