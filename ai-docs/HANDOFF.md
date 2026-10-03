# Handoff

## Current state
2026-10-02. Stages 0 to 2 of the galaxy generator extraction are done and Stop 1 is ruled (every recommendation stands; the name is UniverseGenerator). Stage 3 has begun: this private repository holds the 1.0 plan, the extraction plan, the Stage 0 note and research notes, the knowledge base (`kb/`), the legacy recordings (`tests/Golden/legacy/`), AGENTS.md, CLAUDE.md and the Copilot pointer. No code yet.

- Plan to follow: [plans/2026-10-02-universegenerator-1.0-plan.md](plans/2026-10-02-universegenerator-1.0-plan.md) (D1 to D25, the idea table, effort per level, the README's first example). Stages and the maintainer's tasks: [plans/2026-10-02-galaxy-generator-extraction.md](plans/2026-10-02-galaxy-generator-extraction.md).
- Starting code: the Stage 1 prototype in the game's private repository, capture branch, folder `Tests/GalaxyPrototype` (Random.cs with PCG32, SplitMix64, FNV-1a and DMath; GalaxyProto.cs; SystemProto.cs; Svg.cs). It was written for this package and holds no game code, so it may be copied in. The game's own generator files (`Assets/Scripts/Game/Generators`, `WorldModels`) are the reference for names and fields (CelestialNaming especially).
- Star tables (D20): copy the three star resources from RandomNameGeneratorLibrary 2.2.0 (`stars.designations`, `stars.hipgaps`, `stars.proper`, 61 KB raw, 17 KB gzipped; the maintainer's own MIT package), keep its licence notice, and draw from them with PCG32.

## In progress
Stage 3, step 2: the projects from the package-modernize NuGet templates (Library.csproj, Directory.Build.props, global.json, workflows from the templates, never from memory), the Runtime folder per D8 with its asmdef and `.meta` files, then the core (seeds, DMath, options, address, budgets, JSON writer, golden harness, size gate) and the star system level.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.

## Left / follow-ups
- CI while private: the self-hosted runner rule (package-modernize references/private-repo-ci.md) needs a runner registered for this repository (`scripts/add-self-hosted-runner.ps1`).
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
Copy the NuGet templates and lay out `Packages/com.m4bwav.universe-generator/Runtime/` and `src/UniverseGenerator/`, then port the prototype's Random.cs as the core's first file with its golden and property tests.
