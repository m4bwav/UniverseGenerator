---
title: The SpaceDeckBuilder galaxy generator as a library or as a web generator site
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-generator, procedural-generation, spacedeckbuilder, web-tool, seo, game-marketing, firecat-games, celestial-naming]
summary: "read before releasing the SpaceDeckBuilder2 galaxy/star-system generator as a package or building a browser generator site to send players to the game: competition per ecosystem, generator-site traffic, the gap, and the plan"
---

# The SpaceDeckBuilder galaxy generator as a library or as a web generator site

This is a follow-up to the ports note (in the maintainer's private run record). On 2026-10-02 Mark asked whether there are many systems that generate game objects like his galaxy generator. Numbers below were read on 2026-10-02.

## What the generator is

The generator lives in the private repo the game's private repository, under `Assets/Scripts/Game/Generators/`. It is about 1,400 lines.

What it generates from one seed:
- **Galaxy:** 40 star systems on a disc. Danger runs from 1 to 10 by radius, and the regions are Core, MidRim and OuterRim.
- **Each system:** a star type from a weighted template, planets from archetypes (atmosphere, resources), moons, asteroid belts, stations and hazards.
- **Names:** `CelestialNaming.cs` (23 KB) gives catalog designations, planet discovery letters and Roman-numeral moons. Naming draws from separate RNG streams, so it never shifts what a seed generates.

Coupling to Unity and to the game:
- `CelestialNaming.cs` uses no Unity code.
- The other files use `Vector2`, `Mathf` and Unity's global `Random`. `GalaxyGenerator` calls `Random.InitState`, which a library must not do, because it resets the host game's global RNG.
- The only game-specific fields are `BaseEnemySpawnRate` and station `IsHostileToPlayer`.

## As a library: an open niche, but a small pond

Data generators (systems, planets, names, stats) and visual generators (shaders, meshes, skyboxes) need to be kept apart. The visual side is crowded; Space Graphics Toolkit alone has 4,812 favourites. The data side:

- **Unity Asset Store:** Complete Galaxy Generator ($16, 26 favourites, updated 2025-11) is tied to prefabs and does not mention naming. The other galaxy assets are visual.
- **OpenUPM:** only `com.vindemiatrixcollective.universe` (91 stars), which is a data model plus orbital mechanics and does not generate anything. There is no generator.
- **Godot Asset Library:** no data generator at all.
- **GitHub:**
  - martindevans/CasualGodComplex: 102 stars, last push 2019.
  - Accrete/StarGen physics ports: 20 to 46 stars each. They are realistic but have no galaxy layer, no game content and weak naming.
  - FrunkQ/star-system-generator: 28 stars. It is a GPL-3.0 tabletop app, not a library.
- **Registries:**
  - crates.io `accrete`: 102 downloads in 90 days.
  - npm `@cedric-pouilleux/galexjs`: 230 a month.
  - NuGet `Stargen.Net`: 510 in total.
  - PyPI: nothing relevant.

Verdict:
- **Open:** no maintained, engine-neutral, deterministic, data-only galaxy-to-moon generator with astronomy naming exists on any registry. The naming module is the most distinctive part.
- **Small:** the most popular data library has about 100 stars. A release would be a niche contribution, not a source of traffic.
- **Best fit if released:** a plain C# core on NuGet (also reachable from Godot C#), with a thin OpenUPM wrapper for Unity.

## As a web generator site: the better route to players

Generator sites draw large audiences, mostly from organic search, and that audience leans toward video games. All figures are Similarweb, August 2026, and rough.

| Site | Visits a month | Notes |
|---|---|---|
| fantasynamegenerators.com | about 3.4M | 67% organic search; men 18-24 into video games |
| thestoryshack.com | about 502K | name generators |
| azgaar.github.io | about 615K | a one-person map generator |
| donjon.bin.sh | about 428K | has a star-system generator; text output |
| travellermap.com | about 42K | |
| sectorswithoutnumber.com | probably under 30K | visits last 22 minutes |

- **Where the volume is:** name generators draw about ten times what star-system tools do.
- **Search volumes:** the keyword volumes are not verified. Check them with Google Keyword Planner or the free Ahrefs keyword generator before building pages.
- **The gap:** no web tool combines shareable seed URLs, a good-looking map, and JSON or Foundry VTT export. donjon is mostly text, though it has a seed field. The paid Foundry module Augur: Sci-Fi is the closest rival.
- **Precedents:**
  - Spore Creature Creator: 1M creatures in its first week, made by players who then bought the game.
  - Folk Emerging: started as a map generator; 725 wishlists in two weeks.
  - Stars Without Number: the free edition sells the Deluxe edition.
- **No conversion rate:** none was found from generator users to buyers.
- **Communities:**
  - r/worldbuilding (1.9M members; Resource flair)
  - r/rpg (1.6M; 90% non-promotional activity first, then one flagged self-promotion post a week)
  - r/proceduralgeneration (121K)
  - r/starfinder_rpg (50K)
  - r/traveller (20K)
  - r/SWN (16K)

## Recommendation given to Mark

1. **Do not build the library first.** Build a browser generator on the game's or the studio's domain, where links are followed. Give it one page per keyword, combining:
   - the existing name generator: person, place and star names
   - CelestialNaming: catalog designations
   - the galaxy and star-system generator

   Put a "Wishlist on Steam" link with UTM tags on every result.
2. **Make every result a permanent seed URL** with an SVG map and an og:image, and add JSON, Foundry VTT and Markdown export.
3. **Tie seeds to the game** ("fly this sector in the game"). This needs the web generator and Unity to produce the same output from the same seed. That means replacing `UnityEngine.Random` with a portable seeded PRNG in both, which is the one real use found for seeded-random-utilities' cross-language idea.
4. **Release the C# core** on NuGet and OpenUPM afterwards, as a cheap extra. It credits the game.
5. **Expect 6 to 12 months** before search traffic builds. Launch posts in this order: r/proceduralgeneration, r/worldbuilding, the system subreddits, then r/rpg.


Related: ports note (in the maintainer's private run record), inventory.md (in the maintainer's private run record).
