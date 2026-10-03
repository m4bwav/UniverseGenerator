# Handoff

## Current state
2026-10-02 (evening). Stages 0 to 2 done, Stop 1 ruled. Stage 3: a C# core was built and is green (branch `stage3-core`, draft PR #1: PCG32, seeds, DMath, addresses, JSON writer; `tests/Golden/v1/core.json` identical on .NET 10 and .NET Framework 4.8; CI on the self-hosted runner `universe`, registered 2026-10-02 at `D:\actions-runner-universe`). **Then Mark switched the package to F# compiled by Fable, so it also ships to npm (plan D26).** The 1.0 plan now carries D26 to D31 and the changed D8, D11, D15, D19; Stage 3 opens with a Fable spike as the gate.

- Plan: [plans/2026-10-02-universegenerator-1.0-plan.md](plans/2026-10-02-universegenerator-1.0-plan.md) (Status, D26 to D31, Stage 3 list).
- Star system design, worked out before the switch and not yet coded: [notes/2026-10-02-star-system-level-design.md](notes/2026-10-02-star-system-level-design.md).
- Fable facts used (2026-10-02): Fable 5.18.0, Fable.Core 5.3.0, FSharp.Core 10.1.401; int64/uint64 are BigInt in JS and wrap; int32/uint32 do not wrap; FSharp.Core netstandard2.0 DLL 2.39 MB (10.0.100) or 3.07 MB (6.0.7); `@fable-org/fable-library-js` 2.8.0 is 736 KB unpacked; npm name `universe-generator` free. Mark's research: `Ai/typescript-csharp-dual-packages-2026.md`.

## In progress
Waiting for Mark's Stop 1b rulings (D28 Unity delivery as DLLs with FSharp.Core, yellow on the UPM size; D30 FSharp.Core and fable-library as the one runtime dependency per registry; D11 the lab page running the npm package in the browser). The spike can start before them.

## Dead ends hit
- Unity's Mono evaluates float expressions in double precision; a .NET replay of Unity code must model that (kb/rules/determinism.md).
- Mono's `float.ToString("R")` is not round-trip; write `G9` or round.
- The prototype's sine and cosine series stopped at r^13 and r^12: errors up to 1.4e-14 against Math.Sin. The C# DMath runs them to r^21 and r^20; port that version, not the prototype's.
- Bash heredocs with apostrophes fail in this environment; write Python scripts with the Write tool and run them.

## Left / follow-ups
- The C# files under `Packages/com.m4bwav.universe-generator/Runtime/` (Core, Options, Stars, Names/StarTables.cs) are the port's reference; delete them once the F# core passes the golden file, and turn the Unity package into the DLL layout (D8).
- `scripts/embed-star-tables.py` writes C#; make it write the F# literal module.
- CI golden check now protects released golden files only (from the first `v*` tag) and the legacy recordings always; AGENTS.md says when a pre-release golden may be rewritten. Mark should confirm at Stop 2.
- Before going public: scan the whole history for secrets, private names and local paths.

## Next single action
Fable spike: create `src/UniverseGenerator/UniverseGenerator.fsproj` from the package-modernize F# rules, port `Runtime/Core/*.cs` (Pcg32, Seeds, DMath without BitConverter, Address, JsonWriter) to F#, and make `tests/Golden/v1/core.json` pass on net10.0, net48 and in Node through Fable; then time 10,000 PCG32 draws in Node and measure the core's minified, gzipped bundle.
