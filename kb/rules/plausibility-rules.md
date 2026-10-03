---
title: "Plausibility rules: cheap astronomy that keeps generated descriptions consistent"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, astronomy, exoplanets, habitable-zone, frost-line, orbit-spacing, plausibility]
summary: "read when choosing or reviewing a formula for stars, orbits, planets or moons: each rule in one line with its source and whether the prototype uses it; the generator is for games and fiction (D18), so these keep text consistent rather than simulate"
---

# Plausibility rules

Under D18 the generator serves games and fiction first. These rules are cheap formulas that keep a description consistent (a red dwarf is small and dim, its habitable zone close in). Numbers are defaults, not truths. "Proto" marks rules the Stage 1 prototype already applies.

## Stars

| Rule | Source | Proto |
|---|---|---|
| Real shares: M 72.5%, K 12.9%, G 5.9%, white dwarfs 5.9%, F 3.1%, A 0.6%, B 0.04%, O 0.00005%. The default preset uses a game-tuned mix instead (M 31%, K 21%, G 14%, F 10%, A 6%, white dwarf 7%, giant 5%, B 3%, others 1% or less) | Mamajek star counts (pas.rochester.edu/~emamajek/memo_star_dens.html) | game mix |
| Main-sequence luminosity: L = 0.23 M^2.3 (M < 0.43), M^4 (0.43 to 2), 1.4 M^3.5 (2 to 55) | Univ. of Oregon mass-luminosity lecture | yes |
| Radius ≈ M^0.8 on the main sequence; temperature = 5778 K × (L / R²)^0.25 | Stefan-Boltzmann; Artifexian | yes |
| Lifetime ≈ 10 Gyr × M^-2.5 | lecture above | no |
| Habitable zone: conservative inner √(L / 1.1) au, outer √(L / 0.53) au; Kopparapu 2014 gives the Teff-dependent polynomial | Kopparapu et al. 2014 (arXiv 1401.3349) | simple form |
| Frost line ≈ 2.7 au × √L | Wikipedia, frost line (astrophysics) | yes |
| Multiplicity: M about 27%, G about 46%, A/B/O 70% or more | Duchêne and Kraus 2013 | yes (game values) |
| Close binaries (inside 1 au) leave no planets around either star; at about 10 au the planet rate is about 15% of normal; past about 200 au no effect | Moe and Kratter 2021 (MNRAS 507, 3593) | partly (close pairs lose inner orbits) |
| Stable planet limit in a binary: a_c = (0.464 − 0.38μ − 0.631e + …) × a_bin | Holman and Wiegert 1999 | no |
| Blackbody colour from temperature | Mitchell Charity's blackbody table | coarse bands |

## Planetary systems

| Rule | Source | Proto |
|---|---|---|
| Neighbour period ratio rarely under 1.2; about 20 mutual Hill radii typical, 93% of pairs at least 10 | Weiss et al. 2018 (AJ 155, 48) | ratio 1.25 to 4, centred on 1.8 |
| Peas in a pod: planets in one system are similar in size and evenly spaced; the outer one of a pair is larger about 65% of the time | Weiss 2018 | no (1.0 candidate) |
| Radius valley: few planets of 1.5 to 2.0 Earth radii (rocky below, gassy above) | Fulton et al. 2017; Cloutier 2024 review | no |
| Hot Neptune desert: inside periods of about 3 to 10 days, 2 to 9 Earth-radius planets are rare | TESS demographics (search summary) | no |
| Hot Jupiters around about 0.5% of Sun-like stars | Kepler studies (search summary) | rarer than default |
| Cold giants: about 14 per 100 Sun-like stars at 2 to 8 au, peaking near 3 au just past the frost line | Fulton et al. 2021 | giants mostly past the frost line |
| Inner super-Earths make an outer cold giant about 3 times as likely (about 30% against 10%) | Zhu and Wu 2018; Bryan et al. 2019 | no |
| M dwarfs: 2 to 3 times more small planets inside 1 au, giants about 4 times rarer | Kepler M-dwarf studies (search summary); Dressing and Charbonneau 2015 | yes (giant weight scales with √M) |
| Giant chance rises with metallicity: 0.03 × 10^(2[Fe/H]) | Fischer and Valenti 2005 | no (region metallicity is a 1.0 candidate) |
| Rocky planet in the habitable zone around 37% to 60% of Sun-like stars | Bryson et al. 2021 | no |
| Regular moons of giants total about 10^-4 of the planet's mass; rocky planets 0 to 2 moons | Canup and Ward 2006 | counts only |
| Rings inside the Roche limit; moons between Roche and about half the Hill radius | Hill sphere, Roche limit | no |

## Planets

| Rule | Source | Proto |
|---|---|---|
| Radius from mass (Chen-Kipping forecaster) | NASA Exoplanet Archive calculator notes | no |
| Gas retention: Jeans "1/6 rule" (escape velocity at least 6 times the gas's thermal speed) | scientific-speculation.codidact.com | no |
| Tidal lock time ∝ a^6 | same | no |
| Biome from temperature and rainfall (Whittaker diagram) | SERC Carleton | no |
| Earth Similarity Index | Schulze-Makuch et al. 2011 | no |

## Galaxies and beyond (story level, not simulation)

| Rule | Source | Proto |
|---|---|---|
| Metallicity gradient about −0.06 dex per kpc | arXiv 1403.6128 | region age only |
| Galaxy morphology codes (SB(r)bc) | de Vaucouleurs (NED) | no |
| Groups of up to about 50 galaxies, two big spirals plus satellites; clusters with a giant elliptical at the centre | Local Group notes (Case Western) | no |

Related: [determinism rules](determinism.md), [design rules](design-rules.md), [INDEX](../INDEX.md).
