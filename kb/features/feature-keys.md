# Feature keys for the generator feature matrix

Use these keys when you report which features a generator has. If you find a feature that fits no key, add a new key at the end of your report (lowercase-with-dashes, one line saying what it means). One generator can have many keys.

## Seeds and API
- seed-input: user can type a seed and get the same result again
- seed-url: seed and options live in a shareable URL or code
- seed-versioned: seeds/URLs carry a generator version so old ones keep working
- hierarchical-address: any object (galaxy/7/system/31/planet/3) regenerates alone from its address
- lazy-generation: objects generated on demand, not all at once
- presets: named one-click presets (e.g. "space opera", "realistic")
- options-sliders: tunable parameters (counts, density, rarity, age...)
- data-tables-editable: users can edit/replace the tables (JSON, rule packs, ScriptableObjects)
- hooks-plugins: code hooks, post-processors, mod support
- constraints: "at least one X" style guarantees
- explain-trace: "show your working" (why a value is what it is)
- engine-neutral-data: output is plain data usable outside one engine
- deterministic-cross-platform: documented identical output across OS/runtimes

## Universe / cluster / galaxy
- universe-level: generates several galaxies / a universe
- cluster-level: galaxy groups or clusters, filaments, voids
- galaxy-shapes: more than one galaxy shape (spiral, elliptical, ring, barred, irregular, cluster...)
- custom-shape-map: user-supplied image/density map as galaxy shape
- spiral-arm-count: configurable number of arms
- min-spacing: systems never overlap (Poisson disc or similar)
- regions-sectors: named regions, sectors, constellations or subsectors
- region-traits: regions share traits (age, danger, theme, metallicity)
- lanes-graph: hyperlanes / jump routes / connections between systems
- lane-density: tunable lane density
- chokepoints: chokepoints, bridges, hubs identified as data
- wormholes-gates: special long links, gates, relays
- danger-levels: danger/threat/security levels per system or sector
- factions-territory: factions, empires or territories placed on the map
- nebulae-hazards: nebulae, storms, black holes and other map hazards
- galaxy-age-metallicity: galaxy age/metallicity settings affecting content
- handmade-systems: hand-made systems mixed into the procedural map
- run-map: roguelike branching run map (FTL / Slay the Spire style)

## Stars
- spectral-classes: real spectral classes (O B A F G K M)
- star-physics: mass, luminosity, radius, temperature derived consistently
- star-remnants: white dwarfs, neutron stars, black holes, giants
- multiple-stars: binary/trinary systems with orbit configuration
- habitable-zone: habitable zone computed
- frost-line: frost/snow line computed
- star-age: star age / lifetime affects output
- star-colour: star colour (blackbody) given for rendering

## Planets and moons
- planet-types: varied planet types/classes (more than four)
- orbit-spacing: believable orbit spacing (resonances, Hill stability, Titius-Bode)
- orbital-elements: eccentricity, inclination, period
- planet-physics: radius, mass, density, gravity, escape velocity
- temperature-climate: surface temperature, climate bands
- atmosphere: atmosphere composition and pressure
- hydrosphere: oceans/ice coverage
- rotation-tilt: day length, axial tilt, tidal locking
- biomes: biomes or surface types
- life-ladder: levels of life (none to sentient), flora/fauna
- habitability-score: habitability score or Earth similarity
- resources: minerals/resources by planet
- moons: moons generated
- rings: planetary rings
- asteroid-belts: asteroid belts or debris fields
- surface-map: surface/heightmap/texture output or parameters
- planet-traits: special traits or anomalies per world
- terraforming: terraforming potential

## Content and story
- stations: space stations / starports
- population-society: population, government, law, tech level
- trade-codes: trade codes / economy class
- profile-code: compact profile code (UWP-like)
- story-tags: story tags/hooks (enemies, friends, complications, things, places)
- points-of-interest: POIs, ruins, derelicts, anomalies to explore
- history-events: history timeline or events
- description-text: generated descriptive paragraph(s)
- gm-secrets: GM-only / spoiler section, player-safe view
- names-catalogue: catalogue-style astronomical names (HD 1234, Kepler-16 b)
- names-fantasy: invented/fantasy names (Markov, syllables, themed pools)
- names-hierarchical: names derived from parent (system -> planet -> moon)
- cultures-species: species, cultures, aliens
- creatures: creatures/fauna generated

## Output and tools
- map-render: draws a map (2D)
- map-3d: 3D view
- export-json: JSON export
- export-markdown: Markdown / Obsidian export
- export-image: PNG/SVG export
- export-vtt: Foundry VTT / Roll20 / other VTT export
- export-text-html: text / HTML / PDF export
- export-csv: CSV/spreadsheet export
- edit-after-generate: edit or lock objects after generation, then regenerate the rest
- open-source-licence: source available (name the licence)
- unity-integration: Unity package/asset
- godot-integration: Godot addon
- web-tool: runs in a browser
- price: free or the price
