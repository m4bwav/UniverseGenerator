---
title: "Writers' apps, web generators and VTT modules for space settings"
kind: note
status: active
date: 2026-10-02
verified: 2026-10-02
stale_after: 2027-04-02
tags: [galaxy-kb, sources, writers, game-masters, obsidian, world-anvil, foundry, exports]
summary: "read when designing text, exports or the lab page for writers and game masters: about 45 tools, what they output and import, popularity, the expectations list and a proposed Markdown export shape with field mapping"
---

# Writers and game masters: what they use, and what they expect from a space generator

Researched 2026-10-02. Every page was fetched that day unless marked otherwise. Labels used below:

- **(primary)**: read on the tool's own page, repo, API docs or source code.
- **(snippet)**: seen only in a search-result summary. Not confirmed on the page itself.
- **(blocked)**: the page refused automated fetches (World Anvil returns 403 to both WebFetch and curl). What we say about it comes from snippets and from third-party tools that read its data.

Feature keys come from `feature-keys.md`. The new keys are defined at the end of this report.

## 1. Tool table

Price is as seen on 2026-10-02. "n/v" means not verified.

### Writer and worldbuilding apps, and templates

| Tool | Kind | URL | Price | Feature keys | Output / import format |
|---|---|---|---|---|---|
| World Anvil | writer app (wiki) | worldanvil.com | Freeman free; Master $7, Grandmaster $12, Sage $34 a month (snippet, kindlepreneur) | gm-secrets, map-render, history-events, export-json, web-tool, custom-fields | There are 28 article templates (snippet). Planets and star systems use the **Geography** template, which is for "mountains, rivers, continents… entire planets, galaxies, and universes" (snippet). Its sidebar has Type, Location under, Owner/Ruler and Owning Organization (snippet). Body prompts seen on real articles: Ecosystem, Ecosystem Cycles, Localized Phenomena, Natural Resources, Tourism (snippet). Users have asked for more astrophysical types and a "Planetary Ring" field (community suggestions; blocked). Article text is BBCode: the WorldAnvil-to-MD converter has an `attempt_bbcode` option (primary, GitHub). **No bulk article import.** A suggestion to bulk-upload locations from CSV was declined (snippet). CSV import exists only for interactive tables (snippet). The Boromir API (docs dated 2023-08-18, primary) is JSON-only and supports PUT, POST and PATCH, but "development of new api consumer applications is reserved to members of the guild above the rank of Grandmaster". Paid tiers can export the whole world as a ZIP of JSON, which LegendKeeper and WorldAnvil-to-MD both read. |
| Campfire (Write) | writer app | campfirewriting.com | Free signup with limited elements. Modules cost $0.25–1.50 a month each, or $7.50–45 lifetime (selfpublishing.com review, 2024-08-30) | custom-fields, map-render, cultures-species | The Locations module is for "classify[ing] different planets or dimensions and keep[ing] an index of their most important details". The Maps module offers "interactive star maps with custom pins and zones". There are Species and Cultures modules, and Encyclopedia articles with stats tables (primary, sci-fi page). Elements are built from panels: Text, Image, Statistics, Lists, References and Links (snippet). Export is to PDF, DOCX, HTML and RTF (snippet). LegendKeeper's guide says Campfire elements export as HTML (primary). **We found no structured import** (FAQ, primary): the generator's output has to be pasted in. Claims "100,000+ writers" (snippet, campfirewriting.com/about). |
| Notebook.ai | writer app (open source) | notebook.ai, github.com/indentlabs/notebook | Freemium (tiers n/v). Code is MIT; 410 stars; last push 2026-09-28 (primary) | custom-fields, gm-secrets, export-json, export-csv, export-markdown, export-text-html | **Has a dedicated Planet page type** (primary, `config/attributes/planet.yml`). Overview: name, description, universe, tags. Geography: size, surface, climate, weather, water content, natural resources, continents, countries, landmarks, locations. Climate: seasons, temperature, atmosphere, natural disasters. Time: length of day, length of night, calendar system, night sky, day sky. Inhabitants: population, races, flora, creatures, religions, deities, groups, languages, towns. **Astral**: suns, moons, orbit, visible constellations, nearby planets. History: first inhabitants story, world history. Notes: notes, **private notes**. Export (primary, export_controller.rb): CSV per content type, JSON, XML, YAML, Markdown, outline TXT, PDF, HTML and Scrivener. No import route found. |
| LegendKeeper | writer/GM wiki app | legendkeeper.com | Pro $9 a month, or $7.50 a month billed yearly; 14-day trial. Free Basic tier can view and export (primary) | custom-fields, map-render, web-tool | "Properties" blocks (labelled fields, @-mentions allowed) and an infobox (snippet, Foundations Vol. 2). **Imports HTML, plain text, Markdown, JSON, Obsidian vaults, some ZIPs, World Anvil exports and Campfire HTML** (primary, guide dated 2026-01-09). The guide does not say how front matter is handled. A Foundry module, "legendkeeper-integration", exists (snippet). |
| Kanka | GM campaign wiki | kanka.io | Free (Kobold); paid from about $4.16 a month billed yearly (snippet) | gm-secrets, custom-fields, export-json, web-tool | Location entity (API, primary): `name`, `type`, `entry` (HTML), `parent_id` (the location this one sits inside), `is_private` (admin-only), `tags`, `image`, plus Properties (attributes). Created with POST /locations. Campaign export (once a day) is a ZIP of JSON plus images. **Campaign import of that ZIP** arrived in v2.3 (2024-03-28) and is limited to Wyvern and Elemental subscribers (snippet, docs.kanka.io). A Foundry bridge module exists: owlchester/kanka-foundry (snippet). |
| Obsidian + TTRPG plugins | template / notes app | obsidian.md | Free | export-markdown (as a target) | **Properties** (primary, obsidian.md/help/properties) can be text, list, number, checkbox, date or date-time; `tags`, `aliases` and `cssclasses` are built in. **Nested properties are not supported** (you have to use source mode to see them). Internal links in properties must be quoted. **Leaflet** (primary README) turns note front matter into map markers: `location: [lat, long]`, `mapmarker`, `mapmarkers`, `mapzoom`, `mapoverlay`; image maps; GeoJSON; markerFolder and markerTag. Downloads as of 2026-10-02 (primary, obsidian-releases community-plugin-stats.json): Templater 5.79M, Dataview 5.07M, Fantasy Statblocks 340k, Leaflet 315k, Dice Roller 284k, Calendarium 195k, Initiative Tracker 162k, Fantasy Content Generator 52k, RPG Manager 48k, Solo RPG Toolkit 38k. Leaflet's repo was last pushed 2025-07-09. |
| hopeoverture "worldbuilding-system" | Obsidian template pack | github.com/hopeoverture/worldbuilding-system | Free | — | 97 templates for D&D fantasy, 18 of them geography templates. **None covers planets or star systems** (primary README). This shows that Obsidian template packs leave space out. |
| Scrivener | writer app | literatureandlatte.com | Paid (n/v) | — | No space templates ship with it. The forum advice (2017-07-30, primary) is to make your own planet document template. Imports RTF, RTFD, DOC/DOCX, ODT, TXT, FDX, Fountain and OPML (snippet), plus MultiMarkdown on Mac (on Windows "no Markdown option yet", snippet). |
| Worldbuilding School | course site | — | — | — | **We found no planet or star-system template.** Searches returned unrelated school worksheets. |
| Galaxy Builder Decks: System Data Sheets | printable template | journey-mountain-studios.itch.io | n/v | regions-sectors, points-of-interest | Fill-in sheets with these fields: star system name, star type, system traits, planetary orbits, habitability, notes, and space to draw (snippet). |

### Web generators and table-based (oracle) generators

| Tool | Kind | URL | Price | Feature keys | Output / import format |
|---|---|---|---|---|---|
| Fantasy Name Generators (FNG) | web generator | fantasynamegenerators.com | Free (ads) | names-fantasy, description-text, cultures-species, web-tool | **Space pages** (primary, home page): Planet Names, Planet Descriptions, Star Names, Galaxy Names, Nebula Names, Constellation Names, Constellation Descriptions, Cosmic Names, Space Colony/Station Names, Spaceship Names, Space Fleet Names, Alien (species) Names, Alien (Race) Descriptions, Sci-Fi Gun Names, plus franchise pages (Star Wars planet and spaceship). Its sister site rollforfantasy.com has the Solar System Creator, Planet Interior Creator, Space Base Creator and Space Encounter Creator. **Planet Descriptions** (primary, planetDescription.js) writes several paragraphs: size and gravity relative to Earth, day length in hours, year length in days, number of continents and % of landmass, number of moons, orbit shape (circular, slightly elliptic, wide elliptic), then long paragraphs on land and sea flora and fauna, how far life has evolved, sentience and technology. The author says the physics "won't necessarily be correct". There are no seeds and no export; name pages have a save list. **Traffic: 5.66M visits a month (Dec 2025) and 5.34M (Aug 2025)** (snippet, Semrush). |
| Roll For Fantasy Solar System Creator | web generator / editor | rollforfantasy.com/tools/solar-system-creator.php | Free | export-image, edit-after-generate, map-render, web-tool | Random or custom system with up to 10 planets; star types; five planet categories; drag and resize planets; an editable info box per planet. "Turn to image" gives a PNG; print is supported. No seed (primary). |
| The Story Shack | web generators (3,179 tools listed) | thestoryshack.com | Free; links to its paid "Quarry" app | names-fantasy, biomes, web-tool | Planet Name, Exoplanet Name, Exoplanet Survey Name, Galaxy Name, Space Station Name and alien/AI name generators. The **Random Biome Generator** (updated 2026-04-24) gives a climate band, terrain, a flora note, a fauna note and a **hazard**. Results can be favourited. `/tools/planet-generator/` returned 404 on 2026-10-02, so the old planet generator seems to be gone (primary). |
| donjon Star System Generator | web generator | donjon.bin.sh/scifi/system/ | Free (Patreon) | seed-input, seed-url, spectral-classes, star-physics, multiple-stars, planet-types, planet-physics, atmosphere, hydrosphere, life-ladder, population-society, planet-traits, rings, names-hierarchical, constraints, map-render, web-tool | Inputs: star name, **seed**, companion star (none, close or distant), planets (none, few, several or many), and "force a terrestrial world" (primary form). Output (primary test run, seed 12345): star type (e.g. "M7 V Red Dwarf"), radius, mass, temperature and luminosity. For each planet: type, orbital radius (km and AU), period, physics, radius, gravity, hydrosphere (% water and ice), atmosphere, biosphere, civilization, and "Special" (e.g. "Wreckage of a crashed spaceship"). Planets are named "Test I, II…". Each world links to the **SciFi World Generator through a URL that carries the seed and parameters**; that page has 12 projections, an animated globe, palettes, and water and ice percentages. Output is HTML only. Also on the site: Traveller System Generator (UWP plus text descriptions such as "Standard tainted atmosphere, requires the use of filter masks"), SWd6 System, Alien RPG System. |
| Sectors Without Number | web generator (SWN) | sectorswithoutnumber.com, github.com/mpigsley/sectors-without-number | Free; MIT; 126 stars | regions-sectors, lanes-graph, factions-territory, story-tags, points-of-interest, stations, population-society, gm-secrets, player-view-live, edit-after-generate, export-json, export-image, export-text-html, map-render, open-source-licence (MIT), web-tool | Entity tree (primary, `constants/entities.js`): sector → system or black hole → planet, moon, asteroid belt, research base, refuelling station, moon base, orbital ruin, gas giant mine, space station, asteroid base, deep space station, each with an Occupation and a Situation, plus notes. Planet attributes: atmosphere, temperature, biosphere, population and tech level, plus world tags. Navigation routes, layers, factions. Export modal: condensed print, expanded print, image, JSON. Changelog (primary): 7.21.10 (2026-04-15) "Show inline planet tags… for players when those tags are marked visible"; 7.21.11 (2026-05-11) "Include routes, layers, and factions in the JSON export"; 7.21.13 (2026-09-22). |
| swnt | CLI generator (SWN) | github.com/nboughton/swnt | Free; MIT; 40 stars; last push 2023-02-25 | regions-sectors, story-tags, export-json, export-wiki-site | Sectors up to 99x99 hexes. Exports plain text as a directory tree, a **Hugo site** with search, or JSON (snippet). |
| SWN Sector Generator (Emichron) | web generator (SWN) | swn.emichron.com | Free; CC BY-NC-SA 4.0; source on GitHub | story-tags, cultures-species, npcs, gm-secrets, export-wiki-site | Tabs: Sector Overview, Worlds, NPCs, Corps, Politics, Religions, Aliens. Download is a ZIP holding a **GM TiddlyWiki and a separate PC (player) TiddlyWiki** (primary). |
| Iron Arachne | web generator suite | ironarachne.com, github.com/ironarachne/ironarachne | Free; GPL-3.0; repo pushed 2026-10-02 | seed-input, export-markdown, export-text-html, export-image, export-json, factions-territory, web-tool, open-source-licence (GPL-3.0) | Star system, planet, star nation and SWN starship generators. Star system tests (primary, `e2e/star_system.spec.ts`): **seed field plus "Lock Seed"**, "reproduces the same system from the same seed", **Download Markdown / PDF / SVG**. Project export is JSON in a "vault envelope" with **payload versions** (primary, docs/region-exports.md). |
| Star System Explorer (SSE) | web app (open source) | star-system-generator.vercel.app, github.com/FrunkQ/star-system-generator | Free; GPL-3.0; 28 stars; created 2025-09-21; pushed 2026-09-23 | star-physics, multiple-stars, planet-types, orbital-elements, habitable-zone, gm-secrets, player-view-live, llm-text, real-star-catalogue, import-foreign, edit-after-generate, export-json, export-text-html, share-gallery, map-3d, web-tool | "Scientifically-plausible" systems, 50–60 planet types, binaries and trinaries, transit planner (delta-v), Traveller mode on hexes. **Imports**: whole Traveller subsectors from travellermap.com, Universe Sandbox `.ubox`, SpaceEngine `.sc`, and the real sky (SIMBAD). Per-object visibility, secret tags, autosaved GM notes, **Player-Safe exports** that strip spoilers, printable GM and Player reports, a **live peer-to-peer player view**, an optional LLM for descriptions, saves as JSON or a `.sse.zip` with a `bundleFormat` integer. Shares maps at explorers.starsystemx.com (primary README). Reddit r/proceduralgeneration post 2026-04-15: 44 upvotes, 15 comments (snippet via mirror). **This is the nearest open-source competitor for GMs.** |
| RanGen Planet / Solar System | web generator | rangen.co.uk/world/planetgen.php | Free | planet-types, names-catalogue, rings, moons, description-text, web-tool | Choose a planet type: rocky earth-like, water, desert, humid, frozen, furnace, extreme temperature, ice giant, gas giant, toxic, no atmosphere. Optional "scientific-style names" (e.g. "HK 9827"). Rings, gravity, moons. **Copy, download as text, screenshot** (primary). A separate Solar System generator names and describes a star and its bodies (snippet). |
| Role Generator: Planets and Star System | web generator | rolegenerator.com/en/module/planets | Free; premium €2.49 a month | spectral-classes, star-remnants, planet-physics, orbital-elements, atmosphere, resources, life-ladder, moons, constraints, map-render, export-text-html | System name, type and age; sector. Per planet: class, distance, mass, density, diameter, gravity, temperature, rotation and translation periods, atmosphere, **seismicity**, natural resources, life type, moons. Image of the system; "force habitable world"; print (primary). |
| Springhole Earthlike Planet/World Generator | web generator | springhole.net/…/planets-and-worlds.htm | Free | description-text | One idea per click for "more or less inhabitable" worlds. The output fields were not visible to the fetcher. |
| Perchance | user-made web generators | perchance.org/planet-generator, /solar-system-generator, /sci-fi-planet-generator | Free | description-text, data-tables-editable, llm-text | All user-made, so quality varies. `planet-generator` is a nested fantasy-terrain list; `solar-system-generator` is a near-empty template ("There are {0-10} planets…"); `sci-fi-planet-generator` imports `ai-text-plugin`, so **it writes with an LLM** (primary, generator source). |
| ORBITAL | web generator (itch) | blastoffarcades.itch.io/planetgenerator | Free | planet-types, biomes, atmosphere, description-text, history-log, web-tool | Released 2026-05-08. Gives a planet type, primary biome, atmosphere, visual style, sector/node location, a "ready-to-use environmental summary paragraph" and a Discovery Log (primary). |
| Seventh Sanctum | web generators | seventhsanctum.com | Free | names-fantasy, description-text | Old sci-fi index URL returned 404. Known only from snippets as a long-running idea-generator site. |
| Mythic Star System Creator | oracle procedure | Mythic Magazine Vol. 46 (Word Mill) | Paid book (price n/v) | regions-sectors, story-tags, history-events, names-fantasy | By Tana. Steps: star name tables; layers (Deep Space, Outer Rim, Middle Sphere, Inner Region); 1d4+2 features; terrain, civilization and life descriptors; a **Star System Events** table; fate checks. Write-up dated 2026-01-07 (primary, Alone in the Realm). |
| Ironsworn: Starforged oracles / Datasworn | oracle tables plus open JSON data | github.com/rsek/datasworn | Data free. Code MIT; content CC-BY-4.0 or CC-BY-NC-4.0 (snippet) | planet-types, story-tags, points-of-interest, data-tables-editable, engine-neutral-data | **Planet oracles** (primary, markdown/oracles/planets.md): Planetary Class; Planetside Peril; Planetside Opportunity. Then 11 classes (Desert, Furnace, Grave, Ice, Jovian, Jungle, Ocean, Rocky, Shattered, Tainted, Vital), each with **Atmosphere, Settlements, Observed From Space, Planetside Feature, Life**. Includes webp planet images (snippet). |
| The Way of the GUID | printable generator | tonydowler.itch.io/the-way-of-the-guid | Pay what you want (PDF) | seed-input, population-society, story-tags, npcs | A planet's 32-hex-digit **GUID is its seed**; 22 characteristics from system type to religion, economy and political stability; NPCs, locations, passenger, mercenary and cargo job seeds; rated 4.9/5 by 8 (primary). |
| Objects in Space | printable generator | logen-nein.itch.io/objects-in-space | Pay what you want (PDF) | planet-physics, atmosphere, life-ladder, moons | One page, d6 rolls: main star and its bodies with gravity, atmosphere, mean temperature, biotics and satellites. 2022 jam entry, v0.2 on 2026-01-23 (primary). |
| Traveller Map | web map plus API | travellermap.com | Free (asks API users to email the author) | regions-sectors, lanes-graph, profile-code, trade-codes, export-image, export-json, map-render | Sector file formats (primary): legacy SEC, **T5 tab-delimited** (Hex, Name, UWP, Remarks, {Ix}, (Ex), [Cx], Nobility, Bases, Zone, PBG, W, Allegiance, Stars), and T5 column. Subsector names, borders and routes go in **XML or MSEC metadata**. `/api/poster` and `/api/jumpmap` render posted custom data to PNG, JPEG, PDF or SVG; `/api/sec` converts between formats (primary). |
| As Above So Below | web generator (Traveller) | bartlebythecoder.github.io/traveller_magnus/hex_map.html | Free; source on GitHub | regions-sectors, profile-code, edit-after-generate, export-traveller-map | Rules: CT/Book 6, MgT2e, T5, RTT. Exports to Travellermap; **Obsidian, SQLite and PDF exports are planned** (primary forum thread, 2026-03-03 to 2026-03-25, v0.5.4). |
| Other Traveller web tools | web generators | campaignwiki.org/traveller (Alex Schroeder), neuzd.org, pbegames.com/tworld, travellertools.azurewebsites.net, zhodani.space | Free | profile-code, seed-url | Per Martin Ralya, Feb 2022 (primary article, 2022 content): Schroeder gives unique URLs and takes a pasted UWP list; neuzd has rift, sparse and spiral sector types; PBE Games gives **seed strings**; Traveller Tools gives permalinks. We did not re-check them in 2026: campaignwiki returned 402 and pbegames did not resolve. |
| Astrosynthesis 3.0 | desktop app | nbos.com/products/astrosynthesis | Paid (n/v) | map-3d, lanes-graph, regions-sectors, multiple-stars, nebulae-hazards, atmosphere, surface-map | 3D star mapping, routes, subsectors, a system generator that "applies scientific principles", surface maps through Fractal Mapper (primary). An XML exporter plugin is mentioned (snippet, NBOS forum). |
| Rhogen, StarGen (jazhikho) | CLI / web | github.com/ofasgard/rhogen, jazhikho.itch.io/stargen | Free | star-physics, orbit-spacing | Semi-realistic systems drawing on Atomic Rockets: mass, luminosity, Roche and Hill limits (snippet only). |

### VTT modules and VTT import routes

| Tool | Kind | URL | Price | Feature keys | Output / import format |
|---|---|---|---|---|---|
| Augur: Sci-Fi | Foundry module | foundryvtt.com/packages/augur-scifi | Paid through the publisher (Augur Studios; price n/v) | regions-sectors, spectral-classes, multiple-stars, planet-types, moons, asteroid-belts, temperature-climate, atmosphere, resources, life-ladder, danger-levels, points-of-interest, npcs, factions-territory, entity-links, custom-fields, gm-secrets, edit-after-generate, export-vtt | Foundry 13–14; updated about 2026-09-29. Hex sectors with linked systems ("Augur: Nexus"); 1–3 suns of class O–M; animated orbits you can edit; dedicated scenes for planets, moons and sites. Worlds get type, size, gravity, temperature, atmosphere, water, resources, hazards and life "based on their orbital band" (primary). POIs open linked journals and site maps. Ships, NPCs (roles and personality prompts), factions, **two-way labelled relationships**. Profile fields can be edited or **hidden without deleting the data**, and custom fields can be added. A **journal entry for every body** (snippet). |
| Galaxy Map | Foundry module | foundryvtt.com/packages/galaxy-map | Free (needs HoloSuite Core) | map-render, lanes-graph, points-of-interest, regions-sectors | v1.0.5, about mid-August 2026. Shows systems, sectors, POIs and routes; players can start travel (primary). It is a display framework, not a generator, so **it needs content from somewhere**. |
| Traveller Map Tools | Foundry module | foundryvtt.com/packages/traveller-map | Free | profile-code, lanes-graph, export-vtt | v1.5.1, about late June 2026. `/uwp <world>` in chat, jump maps, route calculation, **auto-created journal entries with full world data**, world import for the MgT2e system (primary). |
| Systems Without Number (swnr) | Foundry game system | foundryvtt.com/packages/swnr | Free | — | v2.3.3, about 2026-09-05 (primary). We did not confirm a sector-import feature. |
| Starforged Custom Compendiums; Starsmith Expanded Oracles; Ancient Wonders | Foundry modules | foundryvtt.com/packages/starforged-custom-oracles (and the others) | Free | regions-sectors, lanes-graph, story-tags, export-vtt | A "Build Starting Sector" macro generates settlements, planets, stars, passages and a connection on a scene (snippet). Ancient Wonders ships roll tables for solar systems, planets and megastructures (primary). |
| MD to Journal | Foundry importer | foundryvtt.com/packages/md-to-journal | Free | export-markdown (as a target) | Foundry 13+, about January 2026. Syncs **a whole folder of .md files** (built "with Obsidian.md users in mind") into linked journals, **turning [[wikilinks]] and Markdown links into @UUID links** (primary plus snippet). |
| Lava Flow | Foundry importer | foundryvtt.com/packages/lava-flow | Free | export-markdown (as a target) | Obsidian vault to journals, with backlinks and images. v4.1.0, about January 2025, verified on Foundry 12 (primary). |
| Foundry core | VTT | foundryvtt.com/api (v14.368) | — | — | Any document's context menu offers "Import Data" from a JSON file (snippet). `JournalEntryPage` has `type`, `text.content`, `text.format` and `text.markdown`. **`JOURNAL_ENTRY_PAGE_FORMATS = {HTML: 1, MARKDOWN: 2}`** (primary). Pages carry their own `ownership`. |
| World Anvil, Kanka and LegendKeeper bridges | Foundry modules | foundryvtt/world-anvil, owlchester/kanka-foundry, legendkeeper-integration | Free | — | Bring wiki articles into Foundry journals and keep them synced (snippet). So a generator that **feeds the wiki apps also reaches Foundry**. |
| Roll20 | VTT | help.roll20.net (UVTT article) | — | — | Native structured import is **UVTT** (`.uvtt`, `.dd2vtt`, `.df2vtt`): a map image plus grid, walls, doors and lights (snippet). We found **no Roll20 star-system or sector generator** and no handout import format. |

## 2. Popularity signals

- **fantasynamegenerators.com: about 5.66M visits a month** (Semrush, Dec 2025, snippet). It is the most-visited tool in this report by far, and it offers names plus prose, not data.
- **World Anvil: "3,500,000+ worldbuilders and Dungeon Masters"** (its own claim, snippet). **Campfire: "100,000+ writers"** (snippet).
- **Obsidian plugin downloads** (primary, 2026-10-02): Dataview 5.07M, Fantasy Statblocks 340k, Leaflet 315k. Obsidian is the largest Markdown target for both GMs and writers.
- **Subreddits** (snippet, GummySearch, 2026): r/worldbuilding 1.5M members, r/scifiwriting 88k. We did not obtain counts for r/rpg, r/traveller or r/SWN.
- **Recommendations**: Sectors Without Number is the repeat recommendation in r/traveller and SWN threads (snippet). Randroll's "30 days of sci-fi tools" (2025-05-01, primary) lists 20 world and system tools, and Sectors Without Number is the only one it describes as "editable, saveable, exportable".
- **GitHub stars**: Notebook.ai 410, Sectors Without Number 126, Star System Explorer 28, swnt 40.
- **Foundry install counts are not public**: package pages show only the version and its age.

## 3. What writers and GMs expect from output

Each item lists the tools that show the expectation.

1. **A readable paragraph first, numbers second.** The most-used tools give prose: FNG Planet Descriptions, RanGen, ORBITAL's "environmental summary paragraph", Springhole, and Perchance and SSE through an LLM. Players and readers meet a world as prose, then look up stats. Output should start with a 2–4 sentence summary that can be pasted as is.
2. **Earth-relative physical stats, as a sidebar block.** Writers think "1.2× Earth gravity, 31-hour day, 431-day year". FNG uses × Earth for size and gravity and hours/days for time. donjon gives both SI and × Earth or × Sol. Role Generator gives mass, density, diameter, gravity, rotation and translation. WA's sidebar, Campfire's Statistics panel, Kanka Properties and LegendKeeper Properties all hold key/value stats, so this block should be a flat key/value list.
3. **The view from the surface.** Notebook.ai has fields for night sky, day sky, visible constellations, suns, moons, length of day, length of night and calendar system. Starforged has "Observed From Space". Writers need to know what the sky looks like and how time passes; this is the gap in almost every generator (new key `sky-view`).
4. **Life, ecology and hazards in words.** FNG's long flora/fauna/sentience paragraphs; WA Geography's Ecosystem, Ecosystem Cycles and Localized Phenomena; the Story Shack biome hazard; Starforged Life and Planetside Peril; Augur hazards; donjon "Special".
5. **People and how they live, when a world is inhabited.** Population, government, tech level, starport: SWN, Traveller UWP, Way of the GUID, Notebook.ai Inhabitants, donjon Civilization.
6. **Story hooks, not just facts.** SWN world tags, Starforged Planetside Opportunity and Peril, Way of the GUID job seeds, Mythic Star System Events, SWN Occupation and Situation on every station, ruin and base, the WA Tourism prompt.
7. **A GM-only layer, and a player-safe version.** WA Secrets; Notebook.ai "private notes"; Kanka `is_private`; Sectors Without Number hidden entities and player view (tags revealed one by one since 2026-04-15); Emichron ships separate GM and PC wikis; SSE has Player-Safe exports, redacted reports and a live player view; Augur hides fields without deleting the data. Output needs secrets marked per section, plus a mode that **removes** them rather than just hiding them.
8. **Links up and down the hierarchy, and across it.** Kanka `parent_id`; WA "Location under"; Notebook.ai universe, nearby planets, suns and moons; Campfire References and Links panels; Augur two-way labelled relationships (faction ↔ world, NPC ↔ ship); md-to-journal and Lava Flow turn links into Foundry @UUID links. Every object should link to its parent, its children and its neighbours by stable names.
9. **Names that fit the setting, with catalogue aliases.** FNG has a whole page per object type (star, galaxy, nebula, constellation, station, fleet); RanGen has a "scientific names" toggle; donjon numbers planets I, II, III after the star; the Story Shack has exoplanet survey names. Each object should carry an evocative name plus catalogue aliases.
10. **Reproducibility.** Seeds in donjon (also carried in the world-map URL), Iron Arachne (seed plus lock), PBE Games seed strings, and Way of the GUID (GUID = seed). Writers come back to the same system months later.
11. **A picture.** donjon world maps (12 projections), Roll For Fantasy PNG, SWN image export, Iron Arachne SVG, Traveller Map posters, ORBITAL art, Datasworn planet images. Writers want something to pin above the desk; GMs want a scene background.
12. **Edit, then keep.** Roll For Fantasy custom mode, SWN, SSE, Augur ("generated content remains fully editable"), As Above So Below. A generator that cannot lock and edit becomes a one-time inspiration tool.
13. **Formats they already use.**
    - Markdown for Obsidian, LegendKeeper, Foundry (through md-to-journal) and Iron Arachne.
    - JSON for SWN, Kanka, Notebook.ai, SSE and Iron Arachne.
    - PDF or print for SWN, Iron Arachne, SSE and Role Generator.
    - A static wiki: Emichron (TiddlyWiki) and swnt (Hugo).
    - CSV: Notebook.ai per type, and WA interactive tables.
    - Traveller T5 tab-delimited/SEC plus XML metadata: Traveller Map, As Above So Below, SSE import.
    - HTML pasted into Campfire.
    - BBCode for World Anvil article bodies.
    - Plain text download: RanGen.
14. **Bulk import into wiki apps is mostly closed**, so per-object files that are easy to paste win. World Anvil declined bulk CSV import and gates its API. Campfire has no import. Kanka import needs a top-tier sub and its own export ZIP. LegendKeeper (Markdown and Obsidian vault import) and Foundry (md-to-journal) are the open doors. **One Markdown file per object in a folder tree is the shape that reaches the most tools.**
15. **Optional AI prose is now common.** Perchance's sci-fi planet generator and SSE both write with an LLM. A deterministic text generator that needs no API key is a point of difference, and so is a clean JSON or prompt hand-off for those who want an LLM rewrite.

## 4. Proposed Markdown export: one star system, one planet

Design rules come from the findings above. **Front matter is flat** (Obsidian does not support nested properties). Links in front matter are **quoted** (Obsidian requires it). Use one file per object, in a folder tree that mirrors the hierarchy. Keep a player-safe export mode that drops every `GM only` block and every `gm_*` key. A link-style option should choose between `[[wikilinks]]` (Obsidian, md-to-journal, Lava Flow) and relative Markdown links (GitHub and other renderers). md-to-journal converts both. The front matter values are stable, so Dataview queries keep working across regenerations.

Folder tree:

```
Veil Reach/                      (sector or region note: Veil Reach.md)
  Kessra/
    Kessra.md                    (star system)
    Kessra II.md                 (planet)
    Kessra II a.md               (moon)
    Kessra Belt.md
    Kessra.json                  (full data, same seed and address)
    Kessra.svg                   (system diagram)
```

### 4.1 Star system file: `Kessra.md`

```markdown
---
kind: star-system
aliases: ["HD 40312", "Kessra System"]
tags: [space/star-system, sector/veil-reach, danger/moderate]
wa_template: geography
wa_type: "Star System"
kanka_type: "Star System"
parent: "[[Veil Reach]]"
children: ["[[Kessra I]]", "[[Kessra II]]", "[[Kessra Belt]]", "[[Kessra III]]"]
neighbours: ["[[Orrin]]", "[[Tal Varo]]"]
address: "u0/g7/s31"
seed: "7f3a9c21"
generator: "spacegen 1.0.0"
permalink: "https://example.org/#v1/u0/g7/s31"
primary_star: "G2 V"
star_count: 1
star_colour: "#fff4ea"
age_gyr: 4.1
habitable_zone_au: "0.95-1.67"
frost_line_au: 2.7
planet_count: 4
danger: moderate
controlled_by: "[[Ashen Compact]]"
population: 1200000000
profile: "B6875A8-9"
location: [412, 1290]
mapmarker: star-g
has_gm_section: true
---

# Kessra

> A yellow sun a little older than Sol, with one ocean world worth the trip and a debris belt nobody charts twice.

## At a glance
| | |
|---|---|
| Star | G2 V, 1.02 Sol masses, 5,790 K |
| Worlds | 3 planets, 1 belt, 5 moons |
| Habitable zone | 0.95–1.67 AU |
| Controlled by | [[Ashen Compact]] |
| Danger | Moderate |

## Stars
## Orbits
| # | Name | Type | a (AU) | Period | Radius (⊕) | Gravity (g) | Temp (°C) | Atmosphere | Water | Life | Moons |
|---|---|---|---|---|---|---|---|---|---|---|---|

## Worlds
- [[Kessra II]]: ocean world, complex life. (one line each)

## Belts, stations and points of interest
## Routes and neighbours
## Factions and presence
## History
## Story hooks
## GM only
> [!gm]- Secrets
> The belt hides a derelict...

## Generator data
Seed `7f3a9c21`, address `u0/g7/s31`, generator 1.0.0. Full data: Kessra.json
```

### 4.2 Planet file: `Kessra II.md`

```markdown
---
kind: planet
aliases: ["HD 40312 c"]
tags: [space/planet, planet/ocean, life/complex, habitable]
wa_template: geography
wa_type: "Planet"
kanka_type: "Planet"
parent: "[[Kessra]]"
moons: ["[[Kessra II a]]"]
address: "u0/g7/s31/p2"
seed: "7f3a9c21"
generator: "spacegen 1.0.0"
planet_class: ocean
orbit_au: 1.12
period_days: 431
eccentricity: 0.03
radius_earth: 1.08
mass_earth: 1.21
gravity_g: 1.04
day_hours: 31.2
axial_tilt_deg: 18
tidally_locked: false
temperature_c: 14
atmosphere: "N2-O2, 1.3 bar"
breathable: true
water_pct: 82
ice_pct: 6
life: complex
sentient_life: false
habitability: 0.81
resources: ["deuterium", "biologics"]
population: 940000000
government: "Corporate charter"
tech_level: 4
starport: B
profile: "B6875A8-9"
danger: low
location: [233, 518]
has_gm_section: true
---

# Kessra II

> A blue world under a slightly paler sun, where thirty-one-hour days drift through long, wet seasons.

## At a glance
(key/value table: class, size, gravity, day, year, temperature, atmosphere, water, life, population)

## Sky and time
The sun at noon, the moons and their phases, which neighbours are visible, day and night length, year length, seasons, a suggested local calendar.

## Surface and climate
## Life
## Resources
## Peoples and settlements
## History
## Points of interest
## Story hooks
## GM only
> [!gm]- Secrets

## Generator data
```

### 4.3 Field mapping

| Export field or section | Obsidian / Dataview / Leaflet | World Anvil | Campfire | Kanka | Notebook.ai | Foundry |
|---|---|---|---|---|---|---|
| file name / H1 | note title | article title | element name | `name` | name | JournalEntry name |
| `aliases` | built-in aliases property | aliases (n/v), or first line of body | Text panel | `title` field or a property | — | — |
| `tags` | built-in tags | article tags (n/v) | — | `tags` (tag IDs; create tags first) | tags | — |
| `wa_type`, `kanka_type`, `kind` | Dataview filter | Geography sidebar **Type** (snippet) | Locations module category | `type` | page type Planet (for planets) | folder |
| `parent` | Dataview, graph | sidebar **Location under** (snippet) | References or Links panel | `parent_id` (resolve after creating the parent) | universe (top only) | @UUID link (md-to-journal) |
| `children`, `moons`, `neighbours` | graph links | links in body | Links panel | children follow automatically from `parent_id` | Astral: moons, nearby planets | @UUID links |
| `controlled_by` | link | sidebar **Owning Organization** (snippet) | References panel | entity relation (n/v) | Inhabitants: groups | link |
| summary blockquote | body | article excerpt or opening text | first Text panel | top of `entry` | description | first page |
| At a glance table and physical keys | properties plus table | sidebar (statblock or table) | **Statistics panel** | **Properties** (attributes) | Geography: size, surface, water content; Climate: temperature, atmosphere | page text |
| Sky and time | section | body | Text panel | `entry` section | **Time** (day/night length, calendar, day/night sky) plus **Astral** (suns, moons, orbit, visible constellations) | page |
| Surface and climate | section | Geography body prompts, Localized Phenomena (snippet) | Text panel | `entry` | Geography / Climate: climate, weather, seasons, natural disasters | page |
| Life | section | Ecosystem, Ecosystem Cycles (snippet) | Species module links | `entry` | Inhabitants: flora, creatures, races | page |
| Resources | `resources` list | Natural Resources (snippet) | Text panel | property | natural resources | page |
| Peoples and settlements | section | body; settlement articles | Cultures module | child locations or organisations | population, towns, groups, languages | page |
| History | section | body; timeline | Timeline module | `entry`; timeline (n/v) | first inhabitants story, world history | page |
| Story hooks | section | body | Text panel | `entry` | notes | page |
| GM only (callout) | collapsible callout; removed in player-safe export | **Secrets** (snippet) | separate private element (n/v) | `is_private` on a separate child entity or post | **private notes** | separate page with player ownership NONE |
| `location`, `mapmarker` | **Leaflet marker from front matter** (primary) | map pin (manual) | Maps module pin (manual) | map marker (manual) | — | Scene Note at x,y |
| `seed`, `address`, `generator`, `permalink` | properties | footer text | Text panel | properties | notes | flags |
| `Kessra.json` | attachment | — | — | — | — | input for a generator-side importer |

**Practical notes**

- **World Anvil:** its bodies are BBCode, so offer a "copy as BBCode" button for each object (sections become `[h2]` headings, the GM block becomes a secret). Bulk import is closed to most users.
- **Kanka:** import needs two passes: create the parents, record their IDs, then create the children with `parent_id`. The Kanka API makes that possible as an optional exporter.
- **Foundry:** the cheapest route is the Markdown folder plus md-to-journal. A direct route is a JSON JournalEntry with one `text` page per section (`format: 2`, Markdown) and the GM page set to player ownership NONE. A scene export would be the system image with Notes pinned to each body's journal; that is the same idea as UVTT, a background image plus structured data.
- **Traveller:** when a Traveller profile is on, also write a T5 tab-delimited sector file plus MSEC/XML metadata for routes and borders. Traveller Map, As Above So Below and SSE already read these.

## 5. New feature keys

- `sky-view`: what the sky looks like from a world's surface: the apparent size and colour of each sun, the moons and their phases, which planets and neighbours are visible, constellations, and day and night colour.
- `local-calendar`: day, night, year and season lengths turned into a usable local calendar (months from moon periods, festivals optional).
- `player-view-live`: a live, redacted view of the map or system served to players during play (SSE peer-to-peer view, SWN player view, Galaxy Map travel).
- `player-safe-export`: an export mode that strips GM-only content instead of only marking it (SSE Player-Safe export, Emichron PC wiki).
- `llm-text`: optional prose written by a language model, user-supplied or built-in (SSE, Perchance ai-text-plugin).
- `real-star-catalogue`: build maps from real catalogue stars (SIMBAD, HYG) as well as procedural ones (SSE).
- `import-foreign`: imports from other tools' formats (Traveller Map sectors, Universe Sandbox `.ubox`, SpaceEngine `.sc`, World Anvil and Campfire exports).
- `export-traveller-map`: writes Traveller SEC or T5 tab-delimited sector data plus XML/MSEC metadata that travellermap.com can render.
- `export-wiki-site`: exports a browsable static wiki or site (TiddlyWiki, Hugo), often as separate GM and player copies.
- `export-wiki-tool`: export shaped for a specific wiki app: World Anvil BBCode, Kanka API or JSON, Campfire paste, LegendKeeper or Notebook.ai import, or a Foundry JournalEntry JSON.
- `entity-links`: typed, labelled two-way relationships between objects (faction ↔ world, NPC ↔ ship, world ↔ neighbour).
- `custom-fields`: users can add, rename or hide fields on generated objects without losing the underlying data.
- `npcs`: generates named characters tied to places (Emichron, Augur, Way of the GUID, Iron Arachne).
- `share-gallery`: a public gallery or link where users share and open each other's generated maps (SSE Explorers site).
- `history-log`: a session log or favourites list of earlier results (ORBITAL Discovery Log, Story Shack favourites, FNG saved names).
- `dice-tables-printable`: the generator also exists as printed dice tables a GM can roll at the table (Starforged oracles, Mythic, Objects in Space, Way of the GUID).

## 6. Gaps and caveats

- **World Anvil** blocked automated fetches (HTTP 403), so its Geography field list comes from snippets and from public articles that use the template. The Boromir Swagger field names were not read.
- **Kanka and World Anvil prices, Campfire's export list, Seventh Sanctum, Rhogen, StarGen and Astrosynthesis's XML exporter** are snippet-level only.
- **Foundry install counts** are not published; there are no popularity numbers for Foundry modules.
- **Roll20:** no space generator or handout import found. UVTT is the only structured import confirmed, and only from snippets.
- **Mongoose's official Traveller tools:** we found none online. The community tools above fill that gap.

Related: [../INDEX.md](../INDEX.md), [../features/feature-matrix.md](../features/feature-matrix.md).
