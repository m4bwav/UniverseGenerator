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
## [2026-10-03] add | notes/2026-10-03-universe-level-design.md: the universe level designed before coding (epoch, cosmic web of clusters and filaments, voids with lone systems, landmark slots, merging pair with a tidal frontier and hook, sky landmark, distance frames); plan Stage 6 gains a star name tool for the site (asked by Mark; site plan PR m4bwav/markdavidrogers-site#2, merged)
## [2026-10-03] add | Universe level (8a42448): Universe record with Generate and At, Epoch option, filaments as the Gabriel graph, voids bounded by clusters, filaments and the map edge, landmark slots through ClusterContext (values only, no draws), merging pair with Tidal link and frontier, galaxy.Landmark, GalaxyGate.Cluster, Distances; universe.json golden identical on net10.0 and net48, the other six unchanged; 158 tests; size gate green (DLL 467.0 KB, nupkg 340.3 KB)
## [2026-10-03] decision | Universe roles are fixed by node index (0 home group, 1 great cluster, 2 merging group, the farthest field node holds the quasar) so every slot is guaranteed without retries; inside a universe the ClusterKind option is ignored as Shape is inside a cluster; a lone cluster obeys Epoch only when it names an epoch
## [2026-10-03] update | Universe note: tuning while coding, statistics, open for tuning; plan Stage 3 checklist (universe done; galaxy extras and the star-name entry point owed) and next action (galaxy extras); PR #1 description
## [2026-10-03] handoff | rewritten: galaxy extras next
## [2026-10-03] index | rebuilt (13 entries)
## [2026-10-03] add | notes/2026-10-03-galaxy-extras-design.md: the galaxy extras designed before coding (factions over lanes, points of interest with placement rules and a precursor trail, hazard areas, a monument per region, beacons per galaxy; each on its own stream of the galaxy seed, never changing the map)
## [2026-10-03] add | Galaxy extras (b8dc686): galaxy.Factions with MapEntry.Faction and Contested, PointsOfInterest, Hazards, Monuments, Beacons; galaxy-extras.json golden identical on net10.0 and net48, the seven others unchanged; hierarchy tests compare the extras; 163 tests; size gate green (DLL 497.0 KB, nupkg 368.5 KB)
## [2026-10-03] decision | Faction reach scales with the map's depth over 10 hops (a 5-hop reach left bubbles on 2,000-system maps); the first beacon sits at a bright star when the map has one (92% of beacons were artificial); extras never change danger, which feeds every system's context
## [2026-10-03] add | StarName (958960b): public StarName.Generate(seed) and Generate(seed, count, starClass, options) for the website's star name tool, on their own seeds (root starname); star-name.json golden; 166 tests; size gate green (DLL 499.0 KB, nupkg 370.7 KB)
## [2026-10-03] update | Extras note: built, statistics, open for tuning, StarName; galaxy level note (extras done); plan Stage 3 checklist (extras and StarName done) and next action (README, CHANGELOG, samples, Stop 2); PR #1 description
## [2026-10-03] handoff | rewritten: README, CHANGELOG, samples and Stop 2 next; next-session-prompt.md rewritten
## [2026-10-03] index | rebuilt (14 entries)
## [2026-10-03] add | README and samples (7a3fe40): the plan's first example as it runs, presets and addresses, validation, the feature matrix's short form, clusters and universes, extras, StarName, Universe.At, the seed promise; samples/ConsoleSample holds every README example (SampleTests checks the README's blocks against it and runs them on net10.0 and net48; CI runs it); Unity sample Samples~/GalaxyPrinter with engine-free GalaxyReport.cs compiled by the tests; the size gate counts Samples~; 170 tests; no Runtime change, golden files unchanged
## [2026-10-03] add | CHANGELOG (c14c7ba): [1.0.0-beta.1] - Unreleased with the size numbers (nupkg 373.5 KB, DLL 499.0 KB, UPM 456.9 KB unpacked and 116.0 KB compressed); CI green on the self-hosted runner
## [2026-10-03] decision | The README follows the real API, not the plan's second example: galaxy.System(31).Planets[1] (no StarSystem.Planet(int)) and Universe.At(address, options) (an address carries no options; without them the plan's example finds a different planet); recorded for Stop 2 as N2 instead of changing the Runtime
## [2026-10-03] add | notes/2026-10-03-stop-2-questions.md: S1 to S16 seed-changing (golden files each moves), E1 to E4 extras-only, N1 to N8 non-seed (13 matrix rows marked 1.0 not built, awkward APIs, CI and test items); same list in PR #1's description; PR #1 ready for review, assigned to m4bwav with needs-review
## [2026-10-03] handoff | rewritten: waiting for Mark at Stop 2; next-session-prompt.md rewritten (apply the rulings)
## [2026-10-03] index | rebuilt (15 entries)
## [2026-10-03] decision | Stop 2 ruled: Mark merged PR #1 into master (32df421) with no rulings left; by the session prompt's rule every S, E and N item keeps its current value, no rule or golden file changed; N1 (matrix rows marked 1.0 not built) and N2 (API additions) stay open for 1.0.0
## [2026-10-03] update | Stop 2 note (Ruled section), plan (Stop 2 checked, Stage 4 gains the history scan and release.yml, next action Stage 4 preparation); release.yml found missing although AGENTS.md describes it
## [2026-10-03] handoff | rewritten: Stage 3 merged, Stage 4 preparation next; next-session-prompt.md rewritten (history scan, release.yml, Mark's checklist)
## [2026-10-03] index | rebuilt (15 entries)
## [2026-10-03] add | History scan (9586a58): gitleaks 8.30.1 (official release, sha256 checked, run from scratch) over every ref and the working tree, no leaks; scripts/history-scan.py regex pass: no secret, LAN address, internal host or private name; old commits keep local paths and Mark's second commit address, already public in other m4bwav repos, so no rewrite; current tree fixed (next-session prompt, runner folder to the private sidecar); notes/2026-10-03-history-scan.md
## [2026-10-03] add | release.yml (23ecbaf): from the package-modernize template; tag v* checks, Linux and Windows tests, package check (now scripts/check-package.sh, shared with ci.yml), size gate, attestation, publish job gated by the nuget environment through Trusted Publishing, GitHub Release; actionlint, shellcheck, zizmor clean; not run
## [2026-10-03] add | notes/2026-10-03-stage-4-checklist.md: Mark's Stage 4 steps; nuget.org Trusted Publishing doc (updated 2026-09-01) matches the plan; package ID UniverseGenerator free; CI must leave the self-hosted runner before the repository goes public
## [2026-10-03] decision | No history rewrite before going public: every history finding (local paths, second author address) is already public in other m4bwav repositories and none is a secret; the private name list for the scan lives in the private sidecar, never in the repository
## [2026-10-03] handoff | rewritten: Stage 4 prepared, waiting for Mark's review of stage4-prep; next-session-prompt.md rewritten (checklist agent steps)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] update | Mark merged stage4-prep (PR #3); repository still private, no nuget environment, no RUNS_ON, no v* tag
## [2026-10-03] verify | History scan re-run on master 272b0e0 (37 commits): gitleaks 8.30.1 no leaks; history-scan.py only the five known kinds plus removal commits, current 0; the script now skips its own file in history (its patterns matched themselves); PR #4 for Mark
## [2026-10-03] update | RUNS_ON set to "ubuntu-latest" with Mark's go-ahead; ci on master green on hosted Ubuntu 24.04 (170 tests, size gate green: nupkg 373.2 KB, DLL 499.0 KB)
## [2026-10-03] add | PR #5: ci.yml build job as the package-modernize NuGet template's hosted matrix plus macOS (plan D9), every check kept, package check and size gate on Linux; 170 tests green on Ubuntu, Windows (net10.0 and net48) and macOS; actionlint 1.7.12, zizmor 1.30.1 clean; the RUNS_ON switch and self-hosted fallback removed
## [2026-10-03] handoff | rewritten: waiting for Mark (PR #4, PR #5, remove the runner, go public); next-session-prompt.md rewritten (public-repository settings and rulesets, then the release pull request)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] update | Mark merged PR #4 (scan re-run), PR #5 (hosted matrix) and PR #6; repository still private, runner universe still registered and online, no nuget environment, no v* tag
## [2026-10-03] verify | ci on master 558c544 (run 37145366186) green on ubuntu-24.04, windows-latest and macos-latest: 170 tests each on net10.0, net48 on Windows, package check and size gate on Linux (nupkg 373.2 KB, DLL 499.0 KB, green)
## [2026-10-03] update | Actions variable RUNS_ON deleted (no workflow reads it since PR #5); variables read back empty; checklist step 2 split: CI off the PC done, runner removal left for Mark
## [2026-10-03] handoff | rewritten: waiting for Mark (remove the runner, go public); next-session-prompt.md rewritten (public-repository settings and rulesets, then the release pull request)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] verify | Status check for the next session: repository still private, runner universe still registered and online, no environments (so no nuget or NUGET_USER), no v* tag, no release run; every agent step waits for Mark, nothing applied; next-session-prompt.md kept as it was
## [2026-10-03] update | Mark removed the runner universe (runners list empty, no workflow names self-hosted); at his explicit request the agent made the repository public (gh repo edit --visibility public); reads back PUBLIC
## [2026-10-03] add | Public-repository settings, all accepted on the free plan and read back: secret scanning and push protection enabled, private vulnerability reporting enabled, workflow token read with no pull request approval, ruleset master 24430276 (deletion, non_fast_forward, required check ci, admin bypass), ruleset Tags only by admins 24430278 (all tags, package-modernize template unchanged); recorded in the checklist
## [2026-10-03] handoff | rewritten: waiting for Mark (Trusted Publishing policy, nuget environment); next-session-prompt.md rewritten (release pull request, then nuget.org verification)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] update | nuget environment: Mark created it; the agent added required reviewer m4bwav and the v* tag policy (read back); the agent's NUGET_USER write was refused by its permission settings, Mark set it (profile name = nuget.org owner of his six packages) and turned admin bypass off; Mark reports the Trusted Publishing policy is in place
## [2026-10-03] add | Release PR #10 (release/1.0.0-beta.1): CHANGELOG heading dated 2026-10-03; size numbers checked against the size gate on master 04f3a16 (ci run 37151962628): nupkg 373.5 to 373.2 KB and UPM compressed 116.0 to 114.5 KB corrected, rest unchanged, all green; for Mark
## [2026-10-03] handoff | rewritten: waiting for Mark (merge PR #10, tag, approve); next-session-prompt.md rewritten (watch the release run, verify from nuget.org)
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] update | Mark merged release PR #10 (1776834); ci on that master commit green on all three systems (run 37154206419), size gate matches the CHANGELOG (nupkg 373.2 KB, UPM 114.5 KB compressed)
## [2026-10-03] learn | The size gate's UPM compressed number wobbles by about 0.1 KB between builds of the same tree (114.6 on PR #10's run, 114.5 on master); a 114.6 correction pushed after the merge never reached master and its branch was deleted; quote master's run, ignore 0.1 KB moves there
## [2026-10-03] handoff | rewritten: ready to tag, waiting for Mark (tag v1.0.0-beta.1, approve nuget); next-session-prompt.md updated
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] update | On Mark's explicit instruction the agent tagged v1.0.0-beta.1 on master c4f4e9e (ci run 37154681055 green, Version 1.0.0-beta.1, golden files clean); release run 37158022980: build and test, attest, Windows net48 and net10.0 passed, waiting at push to nuget.org
## [2026-10-03] update | Approval of the nuget deployment: the first API call was refused by the agent's permission classifier (Production Deploy); after Mark repeated the permission a second call returned no error, but watching the run was then refused, so the push is unconfirmed
## [2026-10-03] handoff | rewritten: confirm release run 37158022980, then verify from nuget.org; next-session-prompt.md rewritten
## [2026-10-03] verify | Release run 37158022980 finished green: approval took, push to nuget.org (after approval) passed in 10 s (Trusted Publishing accepted; nupkg and snupkg Created, 'Your package was pushed.'), GitHub Release passed
## [2026-10-03] verify | nuget.org, 1.0.0-beta.1 (checklist step 7, all passed): flat container lists it; registration listed true; dotnet nuget verify: NuGet.org repository signature; snupkg served via the symbol-packages CDN; GitHub Release prerelease with nupkg and snupkg; gh attestation verify on the run's nupkg: release.yml@refs/tags/v1.0.0-beta.1, run 37158022980 (snupkg not attested, by design); fresh net10.0 and net48 consoles (empty package cache, nuget.org only) print the README's first example, outputs byte-identical
## [2026-10-03] learn | dotnet add package refuses --version together with --prerelease (SDK 10.0.401); use --prerelease alone or --version alone. Attestation must be verified on the run's artifact: the nupkg from nuget.org carries the repository signature, so its digest differs
## [2026-10-03] index | rebuilt (17 entries)
## [2026-10-03] handoff | rewritten: 1.0.0-beta.1 published and verified; waiting for Mark to choose N1 (towards 1.0.0) or Stage 5; next-session-prompt.md rewritten
## [2026-10-03] decision | Mark chose N1 first (towards 1.0.0), Stage 5 after 1.0.0; next-session-prompt.md rewritten for N1 (ruling table first, then build or move per row, then the 1.0.0 release pull request)
## [2026-10-03] index | rebuilt (17 entries)
