---
title: Package size budget for NuGet and Unity (OpenUPM) packages
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [package-size, nuget, openupm, unity, webgl, il2cpp, budget, size-gate, satellite-packages]
summary: "read when planning or growing any of Mark's packages, and whenever a change adds data or features: hard registry limits, measured sizes of comparable packages, the green/yellow/red budget, the practices that keep a package small, and the rule to warn Mark at yellow"
---

# Package size budget for NuGet and Unity packages

On 2026-10-02 Mark said: "I love adding tools, but it should remain within the size range that wouldn't make it difficult to use. Like if the lib is 2 gig people might think twice before using it." He asked for research on the best upper size for NuGet and Unity packages, and for a warning before a package gets there.

**The rule:**
- Every package plan carries the budget below, and CI measures it.
- An agent warns Mark in its reply as soon as a change would cross into yellow, and proposes the split before adding more.
- Red fails CI.

The overlay's standing decisions point here.

Sizes were measured on 2026-10-02:
- nupkg Content-Length from `api.nuget.org/v3-flatcontainer`;
- OpenUPM tarballs downloaded from `package.openupm.com` and unpacked;
- embedded resources read by reflection.

## Hard limits (none of them is the real constraint)

| Venue | Limit | Source |
|---|---|---|
| nuget.org | about 250 MB per package (HTTP 413 above it) | learn.microsoft.com/nuget/nuget-org/publish-a-package (updated 2026-06-11) |
| OpenUPM | 512 MB | openupm.com/docs/adding-upm-package |
| Unity Asset Store | 6 GB for regular packages; 700 MB for UPM packages in the guidelines, 550 MB in the UPM validator (plan for 550) | assetstore.unity.com/publishing/submission-guidelines; docs.unity.com/en-us/asset-store/publishing/upm-packages/validate |
| GitHub | warns at 50 MiB, blocks files over 100 MiB; keep repositories under 1 GB | docs.github.com, about large files |
| Git LFS | 2 GB per file on Free and Pro | docs.github.com, about Git LFS |
| Unity package manager and scoped registries | no documented limit | |

The constraint that binds is whether people hesitate to install the package. For a game library, that also means how much it adds to a WebGL or mobile build.

## Measured comparable packages

| Package | Download | Inside |
|---|---|---|
| RandomNameGeneratorLibrary 2.3.0 (Mark's) | 956 KB | DLL 996 KB, 98% of it raw text resources. Gzip would bring that to 454 KB, Brotli to 374 KB. |
| Bogus 35.6.5 | 3.69 MB | DLL 2.43 MB, 92% of it 50 BSON locale resources; 4 target frameworks |
| Newtonsoft.Json 13.0.4 | 2.48 MB | 0.52 to 0.72 MB DLL × 8 target frameworks |
| Humanizer.Core 2.14.1 / 3.0.10 | 538 KB / 1.93 MB | In 3.x, XML docs (0.5 to 0.64 MB per target) are most of the growth. The 51 locales are satellite packages, 4.4 MB in total, pulled in by a 36 KB metapackage. |
| NodaTime 3.3.5 | 812 KB | DLL 529 KB, with a 133 KB time zone database |
| MathNet.Numerics 5.0.0 | 4.17 MB | |
| DotRecast Core / Detour / Recast | 141 / 166 / 218 KB | split into separate packages |
| Faker.Net 2.0.163 | 607 KB | |
| SharpNoise | 67 KB | |
| JsonPrettyPrinter 3.0.2, TrailerClipper 2.0.0 (Mark's) | 37 KB, 51 KB | |

OpenUPM packages (compressed / unpacked / .cs files / lines):

| Package | Compressed | Unpacked | .cs files | Lines |
|---|---|---|---|---|
| UniTask 2.5.11 | 175 KB | 2.6 MB | 156 | 70.6k |
| NaughtyAttributes 2.1.6 | 734 KB | 1.75 MB | 123 | 6.7k |
| UniRx 7.1.0 | 159 KB | 1.35 MB | 240 | 36.5k |
| ZString 2.6.0 | 98 KB | 1.12 MB | 47 | 28.5k |
| PrimeTween 1.3.3 | 332 KB | 1.35 MB | 50 | 16.4k |
| Extenject 9.2.0 | 381 KB | 1.78 MB | 294 | 35.8k |
| Newtonsoft-for-Unity 13.0.102 (ships DLLs) | 825 KB | 3.24 MB | | |

## The budget (judgment, anchored to the measurements)

| Metric | Green | Yellow: warn Mark | Red: split or stop (CI fails) |
|---|---|---|---|
| nupkg | under 1 MB | 1 to 3 MB | over 3 MB (Bogus territory) |
| Each DLL | under 750 KB | 0.75 to 1.5 MB | over 1.5 MB |
| Embedded data, compressed | under 500 KB | 0.5 to 1.5 MB | over 1.5 MB, or any one table over 300 KB, moves to a satellite package |
| UPM package, unpacked / compressed | under 2 MB / under 500 KB | 2 to 5 MB / 0.5 to 1.5 MB | over 5 MB / over 1.5 MB |
| Runtime .cs files / lines | under 150 / under 25k | 150 to 300 / 25k to 50k | over 300 / over 50k |
| Added WebGL or IL2CPP build size (Brotli, stripping High, default galaxy) | under 300 KB | 0.3 to 1 MB | over 1 MB |
| Default galaxy cold start (desktop .NET) | under 50 ms and under 10 MB allocated | 50 to 200 ms / 10 to 30 MB | over 200 ms or 30 MB, or any data parsed in a static constructor |

Notes on the budget:
- **WebGL:** a minimal Unity 6 URP web build is about 3.3 MiB (forum report by CodeSmile, 2024-12-18), so 1 MB added would be about 30% more. Unity recommends stripping High, "optimize for code size" and Brotli. **Measured for UniverseGenerator 1.0.0 on 2026-10-03:** +116.0 KB Brotli (wasm +70.2 KB, data +45.8 KB) over a 3.43 MiB empty-scene Unity 6000.5.8f1 Web build, which is green ([Stage 5 Unity checks](2026-10-03-stage-5-unity-checks.md)).
- **Mobile:** cold start there is 3 to 5 times slower than on desktop.
- **Compile and reload cost:** no published per-line measurement was found. Since Unity 2020.2, assemblies that depend on a package are not recompiled when its public metadata is unchanged, so a package in its own asmdef costs mostly at import and upgrade. Domain reload grows with type count, static constructors and `[InitializeOnLoad]`. The line limits are judgment; the real number comes from the Editor's compilation and reload profiler markers.

## Staying small while adding features

1. **Core plus satellites,** as Humanizer and DotRecast do. A small core with default tables; data packs such as `X.Names` and `X.Grammars.Extra` as their own packages; optionally an `X.All` metapackage. In Unity, separate packages, or asmdefs gated with `versionDefines` / `defineConstraints`.
2. **Gzip embedded tables:** 54 to 63% smaller in the measurements. `GZipStream` exists on netstandard2.0 and in Unity. Brotli saves about 18% more but is not in netstandard2.0.
3. **Lazy loading per table** (`Lazy<T>`, decoded on first use). Never parse data in a static constructor; let callers release tables.
4. **Unity data:**
   - Never use giant C# array initializers: IL2CPP turns them into huge methods (a known IL2CPP hang with large string arrays), and string literals go into global-metadata.dat.
   - Use `.bytes` TextAssets referenced from an asset, or ship the data inside a prebuilt DLL as embedded resources.
   - Avoid `Resources/`: Unity always includes it in builds and discourages it.
5. **Layout:**
   - One Runtime asmdef, with `noEngineReferences` where possible.
   - Editor and Tests asmdefs separate.
   - Demos in `Samples~`, which is not imported until the user clicks Import.
6. **Trimming and AOT:**
   - Set `IsTrimmable` and `IsAotCompatible` on net10.0.
   - No reflection, including reflection-based JSON. Use a hand-written parser or source generation, so no `link.xml` or `[Preserve]` is needed.
   - Trimmers keep a surviving assembly's embedded resources whole, so data is paid in full. That is another reason to split it out.
7. **Two target frameworks only** (netstandard2.0 and net10.0), with symbols in a .snupkg. Every extra target framework duplicates the DLL and the XML docs; that is what nearly quadrupled Humanizer.Core.
8. **CI size gate:**
   - Report nupkg, DLL, resource bytes, tarball, unpacked size, .cs count, lines and a default-galaxy benchmark.
   - Warn on yellow and fail on red.
   - Write each release's numbers in the CHANGELOG, so growth is visible over time.

Related: [the galaxy generator plan](../plans/2026-10-02-galaxy-generator-extraction.md) (D19), ../../overlay/OVERLAY.md (in the maintainer's private run record).
