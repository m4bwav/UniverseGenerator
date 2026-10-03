---
title: "Determinism rules for a seeded generator that runs on .NET, Mono and IL2CPP"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, determinism, prng, pcg32, splitmix64, mono, il2cpp, floating-point, seeds]
summary: "read before writing or reviewing any code that turns a seed into output: which PRNG and hashes, which maths, what was measured on Unity's Mono, and the traps in System.Random, UnityEngine.Random and float formatting"
---

# Determinism rules

What makes "the same seed gives the same output everywhere" (plan decisions D3, D4, D5, D17) true, with the evidence for each rule.

## Rules

1. **Own PRNG, specified exactly.** PCG32 (XSH RR, 64-bit state; O'Neill 2014, pcg-random.org) for draws, SplitMix64 (Vigna, prng.di.unimi.it) to derive seeds. Never `System.Random` or `UnityEngine.Random`. Prototype: `Tests/GalaxyPrototype/Random.cs` on the game repository's capture branch.
2. **Hierarchical seeds.** Child seed = SplitMix64(SplitMix64(parent XOR FNV-1a(label)) + index). Each purpose (layout, star, planets, names, text, lanes) gets its own PCG stream from `Stream(objectSeed, purpose)`, so a new field, a new level or a tuned probability shifts nothing else. Text labels are hashed with FNV-1a, never `string.GetHashCode` (randomised per process on .NET Core).
3. **Decisions on integers.** Every choice is an integer draw against integer thresholds or weights (`NextInt`, `Chance(numerator, denominator)`, `Weighted(int[])`). Where a continuous value must decide something (a density test), quantise it first (`(int)(density * 4096)` against `NextInt(4096)`).
4. **Maths from + - * / and sqrt only.** IEEE 754 makes those correctly rounded everywhere; `Math.Exp`, `Sin`, `Pow`, `Log`, `Atan2` go to each platform's C library and may differ in the last bit between Windows, Linux, macOS, Mono and IL2CPP (Unity forum "State of determinism in Unity"). The prototype's `DMath` builds Exp, Log, Pow, Sin, Cos, Atan and Atan2 from series. Open risk: a C++ compiler may contract `a * b + c` into a fused multiply-add on ARM64 (IL2CPP); test on an ARM device, and consider `-ffp-contract=off` guidance or explicit rounding of products if a golden test fails there.
5. **Double inside, rounded outside.** Compute in `double`; store and compare values rounded to fixed decimals (`Math.Round(x, n, MidpointRounding.AwayFromZero)`), with invariant culture when formatting.
6. **A fixed number of draws per object, per stream.** An optional feature draws from its own stream, so switching it on or off moves nothing else.
7. **Seed identity = (seed text, generator version).** Seed strings carry the version (`v1-...`); a change to what a seed produces adds a generator version and keeps the old one selectable (D4). Golden files per version, never regenerated.

## Measured traps (2026-10-02, SpaceDeckBuilder2 capture)

- **Unity's Editor Mono evaluates float expressions in double precision.** `Mathf.Lerp(a, b, t)` written as `a + (b - a) * Clamp01(t)` in single precision differed from Unity 6000.6.0f1 (Mono 6.13) in the last bit in 1,188 values; computing in double and rounding once matched all 200 systems. IL2CPP is expected to round per operation, so an Editor and a player build can disagree. Never let a float expression's result decide anything or reach output unrounded.
- **Mono's `float.ToString("R")` is not round-trip** (printed 7 significant digits where 9 were needed). Use `"G9"` for float and `"G17"` for double, or better, round to fixed decimals.
- **Seeded `System.Random` folds the sign:** `new Random(s)` and `new Random(-s)` give the same sequence, and so do `int.MaxValue` and `int.MinValue`. Microsoft also does not promise the seeded sequence across .NET versions (learn.microsoft.com, System.Random remarks).
- **`UnityEngine.Random.InitState(seed)`** resets the host game's global RNG; a library must never call it.
- **`Guid.NewGuid()` in generated objects** (the game's stations) breaks reproducibility; derive identifiers from the address.

## Tests that prove it

Golden seeds per generator version on Windows, Linux and macOS (.NET 10 and .NET Framework 4.8) and in Unity (Mono and IL2CPP, one ARM64 player); a property test that two runs and two threads agree; a test that generating `galaxy/7/system/31` alone equals the same system inside its galaxy; a canary that a planted change turns the golden test red.

Related: [plausibility rules](plausibility-rules.md), [design rules](design-rules.md), [the Stage 0 note](../../ai-docs/notes/2026-10-02-galaxy-stage0-survey-and-capture.md), [INDEX](../INDEX.md).
