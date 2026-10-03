---
title: Improvement ideas for the galaxy generator from existing games, tools, libraries and astronomy
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-generator, procedural-generation, improvement-study, feature-matrix, astronomy, determinism, prng, hyperlanes, planets, universe]
summary: "read in Stage 1 of the galaxy generator plan, or whenever deciding what the generator should do: ideas harvested from about 30 games and tools, the open-source libraries, the astronomy formulas, determinism rules, and the planet and universe levels, each with effort, whether it changes seeds, and sources"
---

# Improvement ideas for the galaxy generator

Mark asked on 2026-10-02 for research into all existing star and galaxy generators, to improve his before it goes public. He added that he wants to out-do every existing generator while staying simple for new users, possibly with universe, galaxy cluster and planet levels, all at game scale. The research ran in three web passes on 2026-10-02:
- games and tools;
- libraries, algorithms and astronomy;
- the planet and universe levels plus API design.

Items marked * came from the researcher's own knowledge, not a page fetched that day. Some primary pages (Stellaris dev diaries, the Starsector and FTL wikis) refused fetching and are summarised from search extracts and mirrors.

Plan: [../plans/2026-10-02-galaxy-generator-extraction.md](../plans/2026-10-02-galaxy-generator-extraction.md).

## Faults in the current generator these sources expose

These come from reading the code (the plan has the survey):
- Star mass (0.8 to 2.5) and luminosity (0.5 to 3.0) are drawn regardless of star type, so a red dwarf can be heavier than the Sun.
- Systems are placed uniformly on a disc with no minimum spacing, so they can overlap. There is no galaxy shape and no lanes.
- Danger and region depend on radius only.
- Orbits grow by a random 1.5 to 4 units, with no tie to the star's luminosity, habitable zone or frost line.
- `UnityEngine.Random.InitState` resets the host's global RNG, and `System.Random`'s seeded sequence is not promised across .NET versions (learn.microsoft.com, System.Random). Neither belongs in a library that promises stable seeds.

## Games and tools: what they do well

| Source | Key ideas |
|---|---|
| Stellaris ([wiki](https://stellaris.paradoxwikis.com/Galaxy), [dev diary 3](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-3-galaxy-generation.885267), [92](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-92-ftl-rework-and-galactic-terrain.1052958/page-11), [367](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-367-4-0-changes-part-1.1726735)) | 10 shapes (elliptical, spiral 2/3/4/6, barred, ring, starburst, cartwheel, spoked). Tightly linked lane clusters are joined by thin highways, and the chokepoints are recorded. Sliders for lane density and habitable worlds. Initializers place content with neighbour rules. |
| Endless Space 2 ([wiki](https://endless-space-2.fandom.com/wiki/Galaxy)) | Twin colliding ellipticals, spirals with 2 to 8 arms. Named constellations give a bonus for owning all of them. Age and density settings. |
| Starsector ([wiki](https://starsector.wiki.gg/wiki/Sector)) | Constellations of 2 to 7 systems share an age. Young means hot stars and more minerals; old means more habitable worlds. Hand-made core worlds sit inside a procedural sector. Themed name pools. |
| Elite Dangerous Stellar Forge ([space.com](https://www.space.com/31366-elite-dangerous-stellar-forge-interview.html), [boxels](https://forums.frontier.co.uk/threads/marxs-guide-to-boxels-subsectors.618286)) | Works from large scale to small (mass distribution, sector, boxel, system). Names derive from coordinates, so a name decodes to a place. |
| Elite 1984 ([bbcelite](https://elite.bbcelite.com/deep_dives/generating_system_data.html), [seeds](https://elite.bbcelite.com/deep_dives/twisting_the_system_seeds.html)) | Stats derived from each other, with illogical combinations blocked (an anarchy is never rich). 256 systems from 6 bytes of seed. |
| Pioneer ([wiki](https://wiki.pioneerspacesim.net/wiki/Discussion_Sysgen_and_Custom_Systems)) | A SystemPath is sector coordinates plus the universe seed. Each system has a mass budget that its bodies consume. Custom systems mixed with procedural ones. |
| No Man's Sky, Starbound ([PS blog](https://blog.playstation.com/2014/08/26/no-mans-sky-a-whole-universe-to-explore/comment-page-1), [starbounder](https://starbounder.org/Modding:Sector_(JSONObject))) | The coordinate is the seed, so any planet regenerates alone. Sectors set threat ranges. |
| SpaceEngine ([blog](https://spaceengine.org/news/blog251118)) | Real catalogue objects mixed with procedural ones. They found a monotonous giant/terrestrial alternation; test for patterns like that. |
| Aurora 4X / Accrete ([accrete crate](https://docs.rs/crate/accrete/latest)) | Dole accretion gives realistic orbits, but only for stars of 0.6 to 1.3 solar masses. |
| Artifexian ([formulas](https://scientific-speculation.codidact.com/posts/243082)) | Cheap main-sequence formulas: L=M³, R≈M^0.74, lifetime≈M^-2.5, habitable zone √L×0.95–1.37, frost line ≈4.85√L. |
| Star System Explorer, FrunkQ ([GitHub](https://github.com/FrunkQ/star-system-generator), GPL-3: ideas only, no code) | Composition drives the other properties. Stars chosen on the HR diagram, binaries around a barycentre, frost and soot lines, four "dials" (metallicity, disc mass, dynamical history, rarity), editable rule packs, a "show your working" panel, player-safe exports. |
| Traveller ([travellermap API](https://travellermap.com/doc/api.html)) | The UWP describes a world in 8 characters. Map posters come from a URL API. |
| Stars Without Number ([swnt](https://github.com/nboughton/swnt)) | Two tags per world, each with Enemies, Friends, Complications, Things and Places*. Exports to JSON, text or a static site. |
| Starforged ([review](https://gnomestew.com/ironsworn-starforged-review/)) | Layered oracle tables, each roll feeding the next; region-weighted tables*. |
| Starfield ([traits](https://progametalk.com/starfield/planet-traits-explained/)) | 0 to 3 planet traits, discovered by scanning. |
| Starfinder Galaxy Exploration Manual ([review](https://www.enworld.org/threads/starfinder-galaxy-exploration-manual-review.681296/)) | World type, then gravity and atmosphere, then biomes, then culture, technology and religion. |
| Augur: Sci-Fi ([Foundry](https://foundryvtt.com/packages/augur-scifi)) | Orbital bands (inner, habitable, outer, remote) drive planet stats; a life ladder from None to Intelligent. |
| Master of Orion ([StrategyWiki](https://strategywiki.org/wiki/Master_of_Orion/Star_systems)) | Star colour changes the odds of each planet type. Artifact specials. |
| FTL, Cobalt Core, Void Bastards ([FTL forum](https://www.subsetgames.com/forum/viewtopic.php?p=31310), [Cobalt Core](https://en.wikipedia.org/wiki/Cobalt_Core)) | A jittered grid of 16 to 24 beacons from entry to exit. Event pools per sector with minimums and maximums. Branching zones that end in a boss. |
| Red Blob Games ([galaxy](https://www.redblobgames.com/x/1812-galaxy-generation/)), Beltoforion ([spiral](https://beltoforion.de/en/spiral_galaxy_renderer)) | Delaunay, then a spanning tree, then short edges added back. Bottleneck nodes become hubs. radius^exponent density. Spiral arms from rotated ellipses (density waves). |
| Astrosynthesis ([NBOS](https://nbos.com/products/astrosynthesis/features)) | Clumped stars, automatic routes, near and far multiples. |
| Kate Compton ([itch](https://galaxykate.itch.io), [oatmeal essay](https://galaxykate0.tumblr.com/post/139774965871/so-you-want-to-build-a-generator)) | Tracery grammars for flavour text. The "10,000 bowls of oatmeal" problem: aim for differences people notice. |
| Azgaar, watabou ([itch](https://watabou.itch.io/medieval-fantasy-city-generator)) | Seed and options in the URL. JSON and SVG export. Share links break when the generator changes, which is the case for versioned seeds. |
| donjon ([roundup](https://www.martinralya.com/tabletop-rpgs/traveller-generators-worlds-systems-sectors-subsectors-and-mongoose-2e-characters/)) | Instant, dense output; text and HTML export. |

## Libraries, algorithms and determinism

| Source | Takeaway |
|---|---|
| Martin Evans, [galaxy article](https://martindevans.me/game-development/2016/01/14/Procedural-Generation-For-Dummies-Galaxies/), [CasualGodComplex](https://github.com/martindevans/CasualGodComplex) (MIT) | Shapes built from Sphere, Cluster and Spiral blocks, plus a swirl modifier that rotates points by an angle growing with radius. Blackbody colour from temperature. Several naming strategies (Markov, catalogue, prefix/suffix). |
| [Gaia Sky procedural galaxies](https://gaia.ari.uni-heidelberg.de/gaiasky/docs/master/Procedural-galaxies.html) | A galaxy as channels (stars, gas, dust, H II regions), each with its own distribution, spiral and warp. A model for data-driven settings. |
| [Complete Galaxy Generator](https://assetstore-fallback.unity.com/packages/templates/systems/complete-galaxy-generator-277304) (Unity, $16) | Ring, wheel, spoked and spiral shapes; minimum spacing; neighbour radius for connections. |
| [Gabriel graph](https://en.wikipedia.org/wiki/Gabriel_graph), [relative neighbourhood graph](https://en.wikipedia.org/wiki/Relative_neighborhood_graph) | spanning tree ⊂ relative neighbourhood graph ⊂ Gabriel ⊂ Delaunay: lane density you can tune, always connected, never crossing. |
| [Bridson Poisson disc](https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf) | O(N) points with a minimum spacing. |
| [accrete crate](https://docs.rs/crate/accrete/latest), [accrete.js](https://github.com/kbingman/accretejs) | Dole accretion; standalone `planet()` beside `planetary_system()`; every parameter is a public setting. |
| [Stargen.Net](https://www.nuget.org/packages/Stargen.Net), [TheWand3rer/Universe](https://github.com/TheWand3rer/Universe) (MIT) | UnitsNet typed units in .NET. Calculate in double and expose float to Unity. JSON loading. |
| [lmagitem planet_generator](https://lib.rs/crates/planet_generator) | Universe, galaxy, sector, system. Golden tests in debug and release builds. An SVG density map as the galaxy shape. Input validated before generation. |
| [xenocide-world-generator](https://github.com/duchu-net/xenocide-world-generator) | Lazy iterators per level, path identifiers, `toModel()` to JSON. |
| [rhogen](https://github.com/ofasgard/rhogen) | Roche and Hill limits; JSON, Markdown and diagram output. |
| Squirrel Eiserloh, [Noise-Based RNG, GDC 2017](https://gdcvault.com/play/1024365/Math-for-Game-Programmers-Noise) | Randomness as hash(seed, position): any element, any order, regenerated alone or in parallel. |
| [Vigna PRNGs](https://prng.di.unimi.it/), [PCG](https://pcg-random.org/using-pcg-c.html) | xoshiro128** or PCG32 to generate; SplitMix64 to derive seeds; PCG has streams and a fast advance. |
| [String.GetHashCode](https://learn.microsoft.com/en-us/dotnet/api/system.string.gethashcode) | Not stable across runs; never derive a seed from it (use FNV-1a). |
| [Unity determinism thread](https://discussions.unity.com/t/state-of-determinism-in-unity/867770), [CoreMathSharp](https://www.nuget.org/packages/AndanteSoft.CoreMathSharp) | IEEE `+ - * /` are deterministic. `Sin`, `Pow` and `Exp` go through the C runtime and can differ between Windows, Linux, Mono and IL2CPP. |
| [Red Blob, RotMG seeds](https://www.redblobgames.com/blog/2025-11-07-rotmg-seeds/) | Lost seeds, and maps that could not be recovered exactly: the cautionary tale. |

## Astronomy to make it plausible

| Fact | Source |
|---|---|
| Star shares: M 72.5%, K 12.9%, G 5.9%, white dwarfs 5.9%, F 3.1%, A 0.6%, B 0.04%, O 0.00005% | [Mamajek star counts](https://www.pas.rochester.edu/~emamajek/memo_star_dens.html) |
| Temperature, mass and luminosity per spectral subtype | [Mamajek dwarf table](https://www.pas.rochester.edu/~emamajek/EEM_dwarf_UBVIJHK_colors_Teff.txt) |
| L=0.23M^2.3 (M<0.43), M^4 (0.43 to 2), 1.4M^3.5 (2 to 55); lifetime ≈ 10 Gyr·M^-2.5 | [mass-luminosity lecture](https://pages.uoregon.edu/imamura/321/122/lecture-5/ml.html) |
| Habitable zone: Seff polynomial in Teff−5780 K, d = √(L/Seff) AU | [Kopparapu 2014](https://arxiv.org/pdf/1401.3349) |
| Frost line ≈ 2.7 AU·√L | [frost line](https://en.wikipedia.org/wiki/Frost_line_(astrophysics)) |
| Multiple systems: M ~27%, G ~46%, A/B/O ≥70% | [Duchêne & Kraus 2013](https://arxiv.org/abs/1303.3028) |
| Stable planet limit in a binary a_c = (0.464 − 0.38μ − 0.631e + …)·a_bin | [Holman & Wiegert](https://arxiv.org/pdf/2108.07815) |
| ~2.5 small planets per M dwarf; M dwarfs have more small and fewer large planets than FGK | [Dressing & Charbonneau 2015](https://arxiv.org/pdf/1501.01623) |
| Giant planet chance = 0.03·10^(2[Fe/H]) | [Fischer & Valenti 2005](https://dc.g-vo.org/rr/q/lp/custom/CDS.VizieR/J/ApJ/622/1102) |
| Neighbouring period ratio rarely below 1.2; ~20 mutual Hill radii typical; planets in a system have similar sizes | [Weiss 2018](https://arxiv.org/pdf/1706.06204) |
| Regular moons of giants total ~10⁻⁴ of the planet's mass | [Canup & Ward 2006](https://www.boulder.swri.edu/~robin/canupward2006.pdf) |
| Metallicity gradient ~−0.06 dex/kpc | [arXiv 1403.6128](https://ar5iv.arxiv.org/html/1403.6128) |
| Kroupa initial mass function, sampled by inverse CDF | [pimf docs](https://pimf.readthedocs.io/en/latest/functional_forms/kroupa.html) |

## The ideas, merged and ranked

Columns:
- **Seeds:** "1.0" means adding the idea later would change what existing seeds produce, so it must be decided before the 1.0 release. "add" means it is purely additive.
- **Effort:** S, M or L.
- **Kind:** "stand-out" means rare or absent in any reusable library; "standard" means games commonly do it.

**Determinism and versioning (all before 1.0)**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 1 | Built-in PCG32 or xoshiro128** on uint arithmetic; no UnityEngine.Random or System.Random | 1.0 | S | standard |
| 2 | Seeds derived from SplitMix64(root, version, path, purpose); purpose ids constant or FNV-1a, never GetHashCode; any object regenerates alone | 1.0 | M | standard in games, rare in libraries |
| 3 | (seed, generator version) as the identity, in URLs and in serialised output; old versions frozen by golden files | 1.0 | S | stand-out |
| 4 | Integer draws against fixed thresholds for every choice; transcendental maths only for derived values (or a managed math library); double inside, invariant culture, fixed decimals | 1.0 | M | stand-out |
| 5 | A fixed number of draws per object and a separate stream per optional feature, so tuning one probability shifts nothing else | 1.0 | S | standard |

**Galaxy structure**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 6 | Poisson-disc spacing, or rejection against a density function | 1.0 | M | standard |
| 7 | Shape presets as density functions or composable blocks: elliptical, spiral N-arm (density-wave ellipses or swirl), barred, ring, cluster or starburst, cartwheel, colliding pair; an SVG or bitmap density map as a custom shape | 1.0 (the default shape) / add (more shapes) | M | standard |
| 8 | Lane graph: spanning tree, then the relative neighbourhood graph or Gabriel graph, then a tunable share of extra Delaunay edges; optional clusters joined by highways | 1.0 if lanes ship in 1.0 | M | rare in libraries |
| 9 | Graph analysis as data: chokepoints (articulation points and bridges), hop distance from the core, dead ends | add | S | stand-out |
| 10 | Constellations or sub-regions with names and shared traits (age, metallicity, theme); danger from graph distance plus regional noise | 1.0 | M | standard |
| 11 | Galaxy age, abundance and habitable-rarity settings; metallicity gradient by radius | 1.0 | S | standard |

**Stars**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 12 | Spectral class drawn from real shares (or a documented game-tuned skew); mass, luminosity, radius, temperature and lifetime derived from it | 1.0 | S-M | standard |
| 13 | Habitable zone, frost line and soot line from luminosity | 1.0 | S | standard |
| 14 | Real multiples: binary chance by primary mass; close (planets around the pair) versus wide (planets around one star); planet limit a_c; per-system mass budget | 1.0 | M-L | stand-out for a game library |
| 15 | Age decides giants and white dwarfs, rather than a flat type list | 1.0 | S | standard |

**Planetary systems and content**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 16 | Orbit spacing by period ratio (about 1.2 to 3, log-normal) or at least 10 mutual Hill radii; innermost orbit scaled by the star; "peas in a pod" size similarity; a test for monotonous patterns | 1.0 | M | standard |
| 17 | Archetypes by zone and star type (M dwarfs: many small, few giants; giants scale with metallicity); star colour shifts planet odds | 1.0 | S | standard |
| 18 | Coherent derived stats: rules so traits follow from each other, with impossible combinations blocked | 1.0 | S | standard |
| 19 | A compact profile code per system and world, UWP-style, for debugging, URLs and tabletop use | add | S | stand-out |
| 20 | Moons: giants' regular moons ~10⁻⁴ of their mass, rocky planets 0 to 2; Hill and Roche radii; rings inside Roche | 1.0 | S | standard |

**Gameplay hooks**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 21 | Tags with story seeds: 0 to 3 per world or system, each with Enemies, Friends, Complications, Things and Places | add (own stream) | M | stand-out |
| 22 | Factions grown over the lane graph from capitals; contested borders; pirate and military stations by frontier or core | add (own stream) | M | stand-out |
| 23 | Themes and points of interest with placement rules (ruins, derelicts, precursor chains spread across slices, fair starts, "not at a chokepoint") | add | M | stand-out in a library |
| 24 | Run-map extraction: an FTL or Slay-the-Spire branching path from start to boss over the lane graph, node types from content, event pools with minimums and maximums. Directly useful to SpaceDeckBuilder2. | add | M | stand-out |
| 25 | Pinned hand-made systems merged into a procedural galaxy | add | M | stand-out |

**Naming**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 26 | Hierarchical, decodable names (constellation, then star, then planet; Elite-style coordinate names); themed pools per region; several weighted strategies (catalogue, Markov, prefix/suffix); optional Tracery-style flavour text | 1.0 for the default scheme, add for others | S-M | stand-out |

**Output and tooling**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| 27 | Exports: a versioned JSON schema, an SVG map string, Traveller-style sector text, a player-safe view without spoilers (rendering stays in the web page) | add | M | standard |
| 28 | "Show your working": a per-field trace of which roll or rule produced it | add | M | stand-out |
| 29 | Data-driven tables in versioned JSON content packs, with a content hash in the version | add | M | stand-out |
| 30 | Lazy hierarchy (`Galaxy.GetSystem(i)`), immutable records without engine types, float adapters for Unity, optional typed units | 1.0 (the API shape) | M | standard |
| 31 | Tests: golden seeds in CI (CoreCLR on Windows, Linux and macOS; Unity Mono and IL2CPP); property tests (no overlapping orbits, sorted orbits, connected lanes); chi-square tests over 100k systems; an "oatmeal" contact sheet of 50 seeds for review | 1.0 | M | stand-out |

Mark's ruling on purpose (2026-10-02): "the focus is for use in gaming or writing rather than science". That changes the order the surveys suggested (1, 2, 3, 30, 12, 13, 16, 6, 7, 8, 9, 10, 21, 22, 19, 24):
- **Determinism stays first:** 1 to 5 and 30, because seeds must be stable before anything is public.
- **Then games and writing:** 21 (tags with story seeds), 26 (names and flavour text), 7 (shapes), 8 and 9 (lanes and chokepoints), 22 (factions), 23 (points of interest), 24 (run map), 10 (constellations), 19 (profile code), 27 (exports, including Markdown for writers).
- **Then only the plausibility that keeps descriptions consistent:** 12 (properties from type, with a game-tuned mix by default), 13 (zones), 17 (planet type by zone), 16 (believable spacing), 20 (moons and rings).
- **Into an optional Plausible preset:** 14 (real multiplicity, a_c), 11's metallicity gradient and 15 (age-driven types).
- **Out:** chi-square fits to astronomy in 31 (test the preset's own targets instead).
- **Additions for writers**, noted for Stage 1: history and event timelines per system or faction; short descriptive paragraphs from grammars; "what is interesting here" summaries; character and culture hooks that use the name generator.

## Planet, universe and cluster levels, and API design

Third research pass, 2026-10-02. It finished before the redirect to games and writing reached it, so its lists are still physics-heavy. They are filtered here through D18: "plausibility" marks a cheap formula that keeps descriptions consistent, and "story" marks material for games and writers. Writers' tools (World Anvil, Campfire, Worldbuilding School, the planet description generators on fantasynamegenerators.com) were not covered; Stage 1 surveys them.

**Sources**

| Source | Idea worth taking |
|---|---|
| [SpaceEngine planet classes](https://spaceengine.org/news/blog170924/) | Names built from parts: temperature class + volatiles + composition + mass prefix ("temperate marine terra") |
| [Starfield traits](https://progametalk.com/starfield/planet-traits-explained/), [moons](https://starfieldwiki.net/wiki/Starfield:Moon) | About 27 named traits (Boiled Seas, Sentient Microbial Colonies...); biomes; flora and fauna levels |
| No Man's Sky ([biomes](https://nms.miraheze.org/wiki/Biome), [galaxies](https://nomanssky.fandom.com/wiki/Galaxy_(Atlas))) | About 11 biomes; separate axes for weather, sentinels, flora and fauna; galaxy flavours (Balanced, Lush, Harsh, Empty) that bias the planet tables |
| [Elite Dangerous bodies](https://elite-journal.readthedocs.io/en/latest/Appendix/) | Body classes (metal-rich, icy, Earth-like, water world, ammonia world, gas giants with life); landable and terraformable flags; graded raw materials |
| [Stellaris traits](https://stellaris.paradoxwikis.com/Biological_traits) | Wet, dry and frozen climates give habitability per species; climate biases deposits |
| [Starbound](https://starbounder.org/Planets), [Spore](https://spore.fandom.com/wiki/Terraforming) | Biome sets a threat tier (1 to 6); a T0 to T3 terraform score |
| Traveller ([UWP](https://wiki.travellerrpg.com/UWP), [trade codes](https://wiki.travellerrpg.com/TC)) | A compact code, with trade codes worked out from it |
| [SWN-style planet tags](https://lairoftheduskwitch.bearblog.dev/planet-creation-for-skulls-without-number/), [Starfinder GEM](https://paizo.com/blog/a-galaxy-of-worlds) | Two story tags per world; gravity and atmosphere pairs plus a d12 biome roll (Weird, Airborne, Subterranean...) |
| [Chen-Kipping mass-radius](https://exoplanetarchive.ipac.caltech.edu/docs/pscp_calc.html), [Jeans 1/6 rule](https://scientific-speculation.codidact.com/posts/221505), [tidal locking](https://scientific-speculation.codidact.com/posts/219604), [Hill and Roche](https://en.wikipedia.org/wiki/Hill_sphere), [circulation cells](https://arxiv.org/pdf/1508.00419), [Whittaker biomes](https://serc.carleton.edu/eslabs/weather/4a.html), [ESI](https://scipython.com/book/chapter-2-the-core-python-language-i/questions/problems/p26/earth-similarity-index) | Cheap plausibility formulas: radius from mass, which gases stay, lock time ∝ a⁶, where moons and rings may sit, climate bands, biome from temperature and rain |
| [Worldbuilding Pasta](https://worldbuildingpasta.blogspot.com/p/blog-page.html), [Undiscovered Worlds](https://github.com/fegennari/Undiscovered_Worlds), [Azgaar templates](https://github.com/Azgaar/Fantasy-Map-Generator/wiki/Quick-Start-Tutorial) | Surface maps from a seed plus a named template of steps (Hill, Range, Trough, Strait) |
| [de Vaucouleurs morphology](https://ned.ipac.caltech.edu/level5/March01/Buta/Buta2.html), [cosmic web](https://arxiv.org/pdf/1401.7866), [Local Group](https://burro.case.edu/Academics/Astr222/Galaxy/Environ/localgroup.html) | Galaxy codes like SB(r)bc; filaments, nodes and voids; groups of up to about 50 galaxies, two big spirals plus satellites |
| [Mass Effect relays](https://www.moddb.com/features/dev-diary-1-static-galaxies-mass-relays-and-you), [Stellaris L-Gates](https://forum.paradoxplaza.com/forum/threads/stellaris-dev-diary-112-the-l-cluster.1092266/), [EVE tiers](https://docs.esi.evetech.net/docs/id_ranges.html), [Endless Space 2 sizes](https://endless-space-2.fandom.com/wiki/Galaxy), [space compression](https://tvtropes.org/pmwiki/pmwiki.php/Main/SpaceCompression), [cozy cosmology](https://kwikrick.itch.io/amazing-space-adventure/devlog/518842/a-frame-around-the-universe) | Making a small universe feel big: long primary links, hub gates to a hidden cluster, region and constellation tiers, sizes from Tiny (30 systems) to 140, compressed distances |
| [Bogus](https://github.com/bchavez/Bogus), [FastNoise2](https://github.com/Auburn/FastNoise2), [DunGen](https://dungen-docs.aegongames.com/2.18/core-concepts/), [Edgar](https://github.com/OndrejNepozitek/Edgar-Unity), [DeBroglie](https://github.com/BorisTheBrave/DeBroglie) | Simple-start, deep-config API patterns (rules below). Bogus warns that its random sequence changes between major versions. |

**Planet level, ranked for games and writing**

The "Seeds" column follows the earlier tables. Most planet fields can be additive if each field takes its own sub-seed from the address, `Hash(address, "atmosphere")`, which D17 already plans.

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| P1 | Traits and anomalies, 0 to 2 per planet, each with gameplay and story hooks (Starfield traits plus SWN tags) | add | S | story, stand-out |
| P2 | A readable descriptor worked out from the data ("cold oceanic super-aquaria"), plus a short generated description paragraph | add | S | story, stand-out |
| P3 | Life ladder (none, microbial, flora, fauna, sentient) with separate flora and fauna density | 1.0 | S | story |
| P4 | Hazard and threat tier from biome and weather | add | S | story |
| P5 | Optional society layer: population, government, law, tech, starport, trade codes; inhabitants and culture hooks using the name generator | add | M | story, stand-out |
| P6 | Resources graded 1 to 5 by composition, with a climate bias | add | M | story |
| P7 | Flags (landable, terraformable T0 to T3) and a UWP-style profile code | add | S | story |
| P8 | Composition classes beyond terra (iron, carbon, lava, eyeball, water, ammonia worlds) | 1.0 | S | plausibility, stand-out |
| P9 | Radius, density, gravity and escape velocity from mass, with scatter | 1.0 | S | plausibility |
| P10 | Temperature from orbit, albedo and greenhouse; day and night swing | 1.0 | S | plausibility |
| P11 | Atmosphere gases kept or lost by the Jeans rule, with a "why" note writers can quote | 1.0 | M | plausibility |
| P12 | Rotation, tilt and tidal lock, giving day length, seasons and climate bands | 1.0 | M | plausibility |
| P13 | Hydrosphere: coverage, ice caps, the ocean's liquid | 1.0 | S | plausibility |
| P14 | Gas giant classes; rings inside Roche, moons between Roche and about half the Hill radius | 1.0 | M | plausibility |
| P15 | Climate bands into a Whittaker table, giving biome shares per band | add | M | plausibility |
| P16 | Surface parameter pack for engines (noise seed, octaves, sea level, plate count, continent template, crater and volcano density); no meshes | add | M | games, stand-out |
| P17 | Habitability for each species' preferences (Stellaris), an Earth Similarity score | add | S | games |

**Universe and cluster level, at game scale**

| # | Idea | Seeds | Effort | Kind |
|---|---|---|---|---|
| U1 | Landmark slots: each small universe gets guaranteed archetypes (home spiral, dying elliptical, quasar, ring galaxy) | 1.0 | S | story, stand-out |
| U2 | Travel network as data: long primary links, hub gates, region and constellation tiers | add | M | games, stand-out |
| U3 | Two distance frames, scene units for the engine and lore light-years for text, plus travel-time tiers | add | S | games and writing, stand-out |
| U4 | Galaxy flavour (Balanced, Lush, Harsh, Empty) biasing the planet tables | 1.0 | S | games |
| U5 | Environment: a group (two big spirals plus satellites) or a cluster (a giant elliptical at the centre) | 1.0 | M | story |
| U6 | Galaxy type code (SB(r)bc) driving the galaxy's shape preset | 1.0 | S | plausibility |
| U7 | Galaxy age and richness biasing star and planet tables; a universe "epoch" setting | 1.0 | M | plausibility |
| U8 | Active cores (quiet, Seyfert, quasar) with a radiation-hazard radius | 1.0 | S | story, stand-out |
| U9 | Satellite dwarf galaxies bound to a host | 1.0 | S | story |
| U10 | Merging pairs with tidal tails and starbursts | 1.0 | M | story, stand-out |
| U11 | A small cosmic-web skeleton: nodes and filaments as a graph, named voids with lone "void systems" | 1.0 | M | story, stand-out |

**API rules for a simple start and deep options** (from Bogus, FastNoise2, DunGen, Edgar and DeBroglie)

1. **One call with good defaults.** `Universe.Create(42)` already gives a good result.
2. **Presets as immutable records, adjusted with `with`.** For example, `Presets.SpaceOpera with { Galaxies = 5..8 }`. One word changes the whole flavour.
3. **Levels of control:** preset, then options, then data tables, then hooks and post-processors, then constraints ("at least one Earth-like world per galaxy"). Beginners never meet the deeper levels.
4. **Data tables are swappable by ID.** Biomes, traits, resources and classes ship as JSON (or ScriptableObject in Unity), and users override or extend them by ID.
5. **A shareable config code.** Seed, preset and options fit in one string that both the web page and C# accept (`Universe.FromCode("...")`).
6. **Determinism is part of the contract.** Per-field sub-seeds, a `GeneratorVersion`, golden tests per version, and a pinned PRNG.
7. **Global sliders** (`Density`, `Weirdness`, `Realism`) that each scale many table weights. `Realism` is where D18's plausibility dial lives.
8. **Validate before generating.** `options.Validate()` returns readable issues; typed ranges.
9. **Lazy, addressable access.** `u.Galaxy(2).System(17).Planet(3).Moon(1)` costs almost nothing, gives the same result in any order, and exposes `.Address` and `.Seed`.
10. **Explainable output.** Derived fields can carry a reason, which helps debugging, teaches beginners and gives writers a ready sentence.


## Addendum: the planet and universe pass, redone for games and writing (2026-10-02)

The third research pass was re-run after Mark's ruling that the focus is games and writing. These are the ideas it added beyond the P and U tables above. "For": G = games, W = writing and game masters, P = plausibility.

**Writers' tools surveyed**
- [World Anvil article templates](https://www.worldanvil.com/learn/article-guides/article-templates)
- [Campfire](https://selfpublishing.com/campfire-writing-review/)
- [Worldbuilding School](https://worldbuilding.itch.io) (its itch.io page; the official site was not confirmed)
- [a random planet description generator](https://agat.itch.io/random-planet-description-generator)
- [Obsidian player-facing notes](https://community.obsidian.md/plugins/player-facing-notes)

What they share: writers want structured fields, a summary, GM-only secrets, nested maps and linked entries, inside the tools they already use.

**Other new sources**
- [Tracery grammars](https://publications.graphics.tudelft.nl/papers/272) and [Markov name presets](https://github.com/Tw1ddle/markov-namegen-lib).
- [Starfield POIs](https://www.sportskeeda.com/esports/are-starfield-planets-procedurally-generated): ruins are placed only where past life fits.
- [Stellaris precursors](https://admin-forum.paradoxplaza.com/forum/developer-diary/stellaris-dev-diary-283-the-vision-of-first-contact.1565311).
- [FTL sectors](https://ftl.fandom.com/wiki/Sectors): civilian, hostile and nebula sectors, with pursuit pressure.
- [Outer Wilds](https://gamingtrend.com/feature/reviews/little-big-universe-outer-wilds-review): small and dense beats huge and empty.

**New ideas**

| # | Idea | Seeds | Effort | For |
|---|---|---|---|---|
| A1 | History layers: 1 to 4 events per planet (colonised, war, extinction, precursor ruins), tied to the galaxy's precursor choice | add | M | W |
| A2 | A text grammar (Tracery-style JSON, overridable and translatable) producing three texts per object: a one-liner, a paragraph and a GM secret | add | M | W |
| A3 | Markdown/Obsidian export with YAML properties, relative links between objects and a GM-secrets section; fields that map to World Anvil and Campfire; CSV for import | add | M | W |
| A4 | A Green, Amber or Red travel zone on the civilisation profile (Traveller) | add | S | G W |
| A5 | Signature creature archetypes and a sentient yes/no beside flora and fauna density | add | S | G W |
| A6 | Points of interest generated lazily by address, with rough latitude and longitude and a link to a trait or tag; ruins only where history allows | add | M | G |
| A7 | Day side, terminator and night side for tidally locked worlds | 1.0 | S | P W |
| A8 | Universe lore: eras, 1 to 3 precursor factions spanning galaxies, and a mystery thread whose clues are planted on planets (Outer Wilds) | add | M | W G |
| A9 | Gate requirements on travel edges (a tech, a key, a quest) with stylised cost in jumps or days | 1.0 | M | G |
| A10 | "Cozy" and "epic" scale presets that set the three distances (lore light-years, float-safe game units, travel cost) | 1.0 | S | G W |
| A11 | Sectors with danger and politics tags (EVE security bands, FTL sector types) and a pressure hook | add | M | G |
| A12 | A tidal-tail frontier between interacting galaxies as a lawless zone with a shared conflict hook | 1.0 | M | W |
| A13 | One universe landmark (a merger, a ring galaxy, a quasar) visible in every galaxy's skybox | 1.0 | S | G W |
| A14 | API: `planet.Explain()` lists why a value is what it is; an `OnPlanetGenerated` hook for post-processing; writers add tables (`opts.Tables.Add("tags/my-campaign.json")`) without code; `opts.ToCode()` / `Options.FromCode()` to share a configuration | 1.0 (API shape) | M | G W |

**What makes a small universe feel big:** landmarks seen from anywhere (A13), gated travel (A9), density over size, and a thread that pulls the reader or player across galaxies (A8).

The Stage 1 survey of writers' tools in the plan is now partly done. What remains: check these tools' import formats against a real export, and survey thestoryshack.com and fantasynamegenerators.com's planet pages.

Related: [the plan](../plans/2026-10-02-galaxy-generator-extraction.md), [library or web tool](2026-10-02-galaxy-generator-library-or-web-tool.md).
