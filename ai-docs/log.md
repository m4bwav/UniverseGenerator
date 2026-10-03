# Log

Append-only. One line per operation: `## [YYYY-MM-DD] op | title` where op is one of add, update, supersede, verify, verify-failed, prune, handoff, index. Newest at the bottom. Never edited, only appended; this is the history the entries themselves do not carry.

## [2026-10-02] init | scaffolded
## [2026-10-02] index | rebuilt (6 entries)
## [2026-10-02] add | Repository created (private, Stage 3 of the galaxy generator extraction): plans, Stage 0 and research notes, kb/ knowledge base, legacy recordings in tests/Golden/legacy/, AGENTS.md with the seed promise and determinism, size and kb rules, CLAUDE.md import, Copilot pointer; private references scrubbed before the first commit
## [2026-10-02] decision | Stop 1 ruled 2026-10-02 by Mark: every recommendation stands (UniverseGenerator, embedded star tables, game-tuned mix, 60 systems with automatic shapes, core plus add-ons, all six levels with cluster and universe as the valve, the game saves its seed, descriptor and tags in core)
## [2026-10-02] index | rebuilt (6 entries)
## [2026-10-02] add | C# core on branch stage3-core (draft PR #1): Pcg32, Seeds (FNV-1a over UTF-8), DMath (series to r^21, own Round), Address, JsonWriter; core.json golden identical on net10.0 and net48; canary red; size gate green (nupkg 43.6 KB); self-hosted runner `universe` registered, first CI run green
## [2026-10-02] decision | CI golden check protects golden files from the first release tag on, and the legacy recordings always; before the first release a golden may be rewritten on purpose (AGENTS.md)
## [2026-10-02] decision | Mark: write the package in F# and compile it with Fable so it also ships to npm (plan D26); plan updated with D26 to D31, Stop 1b questions (D28, D30, D11) and a Fable spike as the gate; C# core kept as the port's reference
## [2026-10-02] add | notes/2026-10-02-star-system-level-design.md: the star system level design worked out before the switch
## [2026-10-02] handoff | rewritten for the F# switch
## [2026-10-02] index | rebuilt (7 entries)
## [2026-10-02] decision | Mark: forget F# and Fable for now, keep C#; maybe port later. Plan's D26 to D31 removed; the assessment kept as "Deferred: F# and Fable" in the 1.0 plan
## [2026-10-02] add | Star system level: StarSystem.Generate, stars and companions, two-chain orbits with the radius valley and hot Neptune desert, moons, belts, stations, names from the embedded tables, landmarks, tags, descriptors; system.json golden (18 systems) identical on net10.0 and net48; tuning recorded in the design note; size gate green (DLL 308.5 KB)
## [2026-10-02] handoff | rewritten: galaxy level next
## [2026-10-02] index | rebuilt (7 entries)
## [2026-10-02] add | Galaxy level (9cc6ad0): prototype ported to Runtime/Galaxies on a spatial grid (lanes equal the brute-force prototype), skeleton pass giving each system its SystemContext (unique name, region age, danger), Galaxy.Generate, Map, lazily generated Systems and System(i), options Systems and Shape; galaxy.json golden identical on net10.0 and net48; default galaxy in full 1.7 ms
## [2026-10-02] add | Universe.At(address) and the hierarchy test (4e63c06): 4,929 objects of seven galaxies and 300 lone systems equal their regeneration from the address; CI green on the self-hosted runner, 101 tests on both targets, DLL 334.0 KB, nupkg 209.0 KB
## [2026-10-02] add | notes/2026-10-02-galaxy-level-design.md: what was ported and changed, the public shape, the skeleton pass, Universe.At, statistics and timing, what the level still lacks
## [2026-10-02] update | PR #1 title and description rewritten for the C# work (the F# plan was dropped)
## [2026-10-02] handoff | rewritten: planet level next
## [2026-10-02] index | rebuilt (8 entries)
## [2026-10-02] index | rebuilt (8 entries)
## [2026-10-02] add | notes/2026-10-02-planet-level-design.md: the planet level designed before coding (physics, Jeans atmosphere, temperature, rotation and tidal lock, climate bands, biomes, life, traits, resources, hazards, habitability, the lone planet)
## [2026-10-02] add | Planet level (d673430): detail on the planet's own streams, Planet.Generate, Universe.At to v1-seed/planet; planet.json golden identical on net10.0 and net48, system.json and galaxy.json unchanged; 115 tests; full default galaxy 3.28 ms; size gate green (DLL 378.0 KB, nupkg 252.3 KB)
## [2026-10-02] update | Planet note: tuning while coding (locked under 0.6 of the lock radius, temperate rocky and desert worlds target-solved, carbon dioxide for cold gardens, leaking air always a trace) and statistics; plan Stage 3 checklist and next action (moon and belt detail)
## [2026-10-02] handoff | rewritten: moon and belt detail next
## [2026-10-02] index | rebuilt (9 entries)
## [2026-10-02] index | rebuilt (9 entries)
## [2026-10-03] add | notes/2026-10-03-moon-and-belt-level-design.md: moon and belt detail designed before coding (moon physics by kind, orbit, Peale tidal heat bounded by the kind, Jeans air with renewal for hazy and garden moons, hidden oceans, life, hazards; belt addresses, composition, resources)
## [2026-10-03] update | Planet level refactor (12d0d87): greenhouse, pressure solver, climate, resource grading, similarity, habitability, trait picker and hazard list exposed for reuse; PlanetResources renamed ResourceGrades; no output change
## [2026-10-03] add | Moon and belt detail (49c5fa5): moons and belts on their own seeds, Universe.At reaches belts (.../belt/k); moon-belt.json golden identical on net10.0 and net48, the other four unchanged; 130 tests; CI green on the self-hosted runner; size gate green (DLL 400.0 KB, nupkg 275.8 KB)
## [2026-10-03] decision | Moon kinds win over physics where the system level promised more (volcanic and ocean heat floors, hazy and garden air renewed, the planet level's Jeans rule kept as Mark asked); belts follow physics (warm outer belts lose their ice); thick air shields a moon from its giant's radiation
## [2026-10-03] update | Moon and belt note: tuning while coding and statistics; plan Stage 3 checklist and next action (galaxy cluster level); PR #1 description
## [2026-10-03] handoff | rewritten: galaxy cluster level next
## [2026-10-03] index | rebuilt (10 entries)
## [2026-10-03] index | rebuilt (11 entries)
## [2026-10-03] add | notes/2026-10-03-galaxy-cluster-level-design.md: the galaxy cluster level designed before coding (groups and clusters, type codes driving the shape, age and richness, active cores, satellites, gates, a wormhole ring and tethers; a galaxy alone keeps its output)
## [2026-10-03] add | Galaxy cluster level (64e90de): GalaxyCluster.Generate, groups and clusters, type codes driving the shape, age and richness, active cores, satellites, gates, wormhole ring and tethers, galaxy.Gates; Universe.At reaches clusters; cluster.json golden identical on net10.0 and net48, the other five unchanged; 144 tests; size gate green (DLL 435.0 KB, nupkg 308.5 KB)
## [2026-10-03] decision | Cluster spirals under 80 systems grow to 80 when Systems is at least 40 (else S0), placed with room for it, because the S0 rule alone erased the morphology and density relation at the default 60; a lone galaxy is Mature, Normal and Quiet and reads its type from its shape, so its map never moves
## [2026-10-03] update | Cluster note: tuning while coding, statistics, open for tuning; plan Stage 3 checklist and next action (universe level)
## [2026-10-03] handoff | rewritten: universe level next
## [2026-10-03] index | rebuilt (11 entries)
## [2026-10-03] decision | Every session ends by rewriting ai-docs/next-session-prompt.md, a prompt for the next session that ends with the same instruction (Mark, 2026-10-03: chained fresh sessions cost less than one long one); rule in AGENTS.md
## [2026-10-03] index | rebuilt (12 entries)
## [2026-10-03] index | rebuilt (12 entries)
