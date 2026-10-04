---
title: "Stage 5 Unity checks: 1.0.0 in Unity Mono and IL2CPP, WebGL size"
kind: note
status: active
date: 2026-10-03
stale_after: 2027-01-03
tags: [universegenerator, stage-5, unity, il2cpp, webgl, golden, size-budget, openupm]
summary: "read before any Unity work on the package: what was checked in Unity (git-URL install, sample, all tests in Mono, golden checks in IL2CPP/WebGL, WebGL size delta), the numbers, what is still open (Windows IL2CPP, ARM64), and how the harness works"
---

# Stage 5 Unity checks

Run on 2026-10-03 (late evening, US Central) on Mark's go-ahead to work through Stage 5 overnight. Every check installed the package **by git URL at tag `v1.0.0`**, the way users will get it, not from a local path. Harness and commands: [tests/Unity/README.md](../../tests/Unity/README.md).

## Results

| Check | Unity 6000.6.4f1 | Unity 6000.5.8f1 |
|---|---|---|
| Package compiles (both assemblies), no error or warning from the package or tests | yes | yes |
| Galaxy printer sample: found, imported, report run for my-seed | 64 lines, starting "Wyvern Galaxy: a ring galaxy, 60 systems", then "HD 147927: ... 6 planets ... danger 9", as in the README | not run (same package) |
| The repository's NUnit tests in Unity Mono (PlayMode in the Editor, batchmode) | 266 of 266 passed, the 18 golden checks among them | 266 of 266 passed |
| Golden checks in IL2CPP: WebGL player (wasm32), Chrome | not installed | `UG-GOLDEN DONE pass=18 fail=0`; a warm default galaxy took 0.4 ms |
| Golden checks in IL2CPP: Windows x64 player | not run: no Windows IL2CPP module and no C++ toolchain | not run |
| ARM64 fused multiply-add check (plan, Risks) | not run: no ARM64 device | not run |

266 tests run, against 272 on net10.0: `RepoRulesTests` (repository rules, with C# 11 raw strings) is left out, since .NET CI covers it.

## WebGL size delta (budget row: added Brotli size, green under 300 KB)

Unity 6000.5.8f1, Web build, IL2CPP "optimize for size", managed stripping High, Brotli, no decompression fallback, an empty scene. Baseline: the same project without the package. With: the package plus one `Galaxy.Generate("my-seed")` at start.

| File | Without | With | Added |
|---|---|---|---|
| wasm.br | 2,877,014 | 2,948,885 | +71,871 |
| data.br | 627,103 | 674,005 | +46,902 |
| framework.js.br | 64,290 | 64,290 | 0 |
| loader.js | 27,222 | 27,222 | 0 |
| **Total** | 3,595,629 | 3,714,402 | **+118,773 bytes (116.0 KB): green** |

The stripped `UniverseGenerator.dll` in the build was 323,072 bytes before compression (563 KB unstripped). The plan's estimate was 120 to 250 KB, so the measurement sits at the low end.

## The machine on 2026-10-03

- Editors 6000.5.8f1, 6000.6.0f1, 6000.6.4f1, under Program Files. Installing a module needs elevation (a UAC prompt), so none was installed overnight.
- Modules: Windows Mono players on all three. Web Build Support on 6000.5.8f1 only. Windows IL2CPP on none.
- No Visual Studio or C++ build tools, which Windows IL2CPP needs.
- For a Windows x64 IL2CPP run: install `windows-il2cpp` for 6000.6.4f1 (258 MB download, 1.04 GB on disk; `unity install-modules -e 6000.6.4f1 -m windows-il2cpp`) and Visual Studio 2022 Build Tools with the C++ desktop workload. Then build `UGBuild.WindowsIL2CPP` and run the exe with `-logFile`. Web support for 6000.6.4f1 is `webgl` (1.04 GB download, 5.61 GB on disk).

## How the harness works, and what it taught

- The tests are the repository's own files, copied into an asmdef named `UniverseGenerator.Tests`, so the runtime's `InternalsVisibleTo("UniverseGenerator.Tests")` gives them `JsonWriter`. `autoReferenced` is false, so the copied `GalaxyReport` does not clash with the imported sample's.
- Unity ships a custom NUnit 3.5, not NUnit 4. Rewrites in `make_project.py`: `TestContext.Out.WriteLine` goes to `Debug.Log` (players have no test context); `Using<Lane>(Func<,,bool>)` becomes a `Comparison`; `Does.Not.Contain(item)` becomes `Has.No.Member`; `Has.Count` on the galaxy map becomes `.Map.Count`, because 3.5's property lookup fails on that type. The tests also need `#nullable enable`.
- A test asmdef for every platform runs as PlayMode tests. `-testPlatform EditMode` found 0 tests, and `-testPlatform PlayMode` runs them in batchmode in about 50 s.
- A player that holds NUnit code needs `BuildOptions.IncludeTestAssemblies`. Without it, the IL2CPP linker fails with "Failed to resolve assembly: 'nunit.framework'".
- WebGL is IL2CPP, so a Web build is an IL2CPP golden check without the Windows module. A golden build uses uncompressed output, so a plain `python -m http.server` can serve it.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (Stage 5, Risks); see also [package size budget](2026-10-02-package-size-budget.md), [Stage 4 checklist](2026-10-03-stage-4-checklist.md).
