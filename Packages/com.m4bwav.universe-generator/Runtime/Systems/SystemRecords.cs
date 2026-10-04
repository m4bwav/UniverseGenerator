#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>Where an orbit sits relative to the star's habitable zone and frost line.</summary>
    public enum OrbitZone
    {
        /// <summary>Inside half the habitable zone's inner edge.</summary>
        Hot,

        /// <summary>Between that and the habitable zone.</summary>
        Warm,

        /// <summary>In the habitable zone, where liquid water can last.</summary>
        Temperate,

        /// <summary>Between the habitable zone and the frost line.</summary>
        Cold,

        /// <summary>Past the frost line, where ices condense and giants form.</summary>
        Outer,
    }

    /// <summary>What kind of world a planet is.</summary>
    public enum PlanetKind
    {
        /// <summary>A molten surface close to its star.</summary>
        Lava,

        /// <summary>A dense, metal-rich world like Mercury.</summary>
        Iron,

        /// <summary>Airless rock.</summary>
        Barren,

        /// <summary>Dry, with a thin atmosphere.</summary>
        Desert,

        /// <summary>A rocky world with an atmosphere but no open water.</summary>
        Rocky,

        /// <summary>A runaway greenhouse like Venus.</summary>
        Greenhouse,

        /// <summary>Covered by water.</summary>
        Ocean,

        /// <summary>Temperate, with water, air and life.</summary>
        Garden,

        /// <summary>Frozen water and rock.</summary>
        Ice,

        /// <summary>A dwarf planet like Ceres or Pluto.</summary>
        Dwarf,

        /// <summary>A mini-Neptune wrapped in thick gas, the commonest planet in the galaxy.</summary>
        SubNeptune,

        /// <summary>A giant of hydrogen and helium like Jupiter.</summary>
        GasGiant,

        /// <summary>A giant of ices like Neptune.</summary>
        IceGiant,

        /// <summary>A gas giant orbiting very close to its star.</summary>
        HotJupiter,
    }

    /// <summary>What kind of moon.</summary>
    public enum MoonKind
    {
        /// <summary>Cratered, airless rock.</summary>
        Barren,

        /// <summary>An ice-covered moon.</summary>
        Ice,

        /// <summary>Tidally heated, with active volcanoes like Io.</summary>
        Volcanic,

        /// <summary>An ocean under an ice shell, like Europa.</summary>
        Ocean,

        /// <summary>A thick, hazy atmosphere, like Titan.</summary>
        Hazy,

        /// <summary>A habitable moon of a giant in the habitable zone.</summary>
        Garden,
    }

    /// <summary>What kind of belt.</summary>
    public enum BeltKind
    {
        /// <summary>Rocky asteroids in a gap between planets.</summary>
        Asteroid,

        /// <summary>Icy bodies beyond the last planet, like the Kuiper belt.</summary>
        Ice,
    }

    /// <summary>What a station is for.</summary>
    public enum StationKind
    {
        /// <summary>A market and docking port.</summary>
        TradeHub,

        /// <summary>Builds and repairs ships.</summary>
        Shipyard,

        /// <summary>Mines a world, moon or belt.</summary>
        MiningPlatform,

        /// <summary>Studies something in the system.</summary>
        ResearchStation,

        /// <summary>A military base.</summary>
        NavalBase,

        /// <summary>A pirates' hideout.</summary>
        PirateDen,

        /// <summary>Processes ore or gas.</summary>
        Refinery,

        /// <summary>A communications relay.</summary>
        Relay,
    }

    /// <summary>What makes a system memorable (plan D24).</summary>
    public enum LandmarkKind
    {
        /// <summary>A blue star, giant, supergiant or stellar remnant.</summary>
        UnusualStar,

        /// <summary>Two stars.</summary>
        BinaryStar,

        /// <summary>A world or moon with life.</summary>
        LivingWorld,

        /// <summary>A giant planet with rings.</summary>
        RingedGiant,

        /// <summary>A gas giant skimming its star.</summary>
        HotJupiter,

        /// <summary>An asteroid belt.</summary>
        AsteroidBelt,

        /// <summary>The ruins of someone who came before.</summary>
        Ruins,

        /// <summary>An abandoned ship or station.</summary>
        Derelict,

        /// <summary>The system lies inside a glowing nebula.</summary>
        Nebula,

        /// <summary>A great comet visible across the system.</summary>
        Comet,

        /// <summary>Something sensors cannot explain.</summary>
        Anomaly,

        /// <summary>A vast artificial structure (an outlier).</summary>
        Megastructure,

        /// <summary>A starless planet passing through (an outlier).</summary>
        RoguePlanet,

        /// <summary>An ancient signal still transmitting (an outlier).</summary>
        AncientBeacon,

        /// <summary>The remains of a world torn apart (an outlier).</summary>
        ShatteredWorld,

        /// <summary>Clocks drift here (an outlier).</summary>
        TemporalAnomaly,

        /// <summary>A fleet of dead ships (an outlier).</summary>
        DerelictFleet,
    }

    /// <summary>
    /// A moon. Orbit in planet radii, radius and distance in kilometres, temperatures in kelvin, gravity in g. Every moon
    /// keeps one face to its planet. The fields after <see cref="Radius"/> are the moon level
    /// (ai-docs/notes/2026-10-03-moon-and-belt-level-design.md).
    /// </summary>
    public sealed partial record Moon
    {
        /// <summary>Where it is, such as <c>v1-my-seed/system/planet/3/moon/0</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its position counting outwards from 0.</summary>
        public int Index { get; init; }

        /// <summary>Its name, such as "Kepler-22 c II".</summary>
        public string Name { get; init; } = "";

        /// <summary>What kind of moon.</summary>
        public MoonKind Kind { get; init; }

        /// <summary>Distance from the planet's centre in planet radii.</summary>
        public double Orbit { get; init; }

        /// <summary>Radius in kilometres.</summary>
        public double Radius { get; init; }

        /// <summary>One line in plain words, such as "a hazy moon of a ringed gas giant" (plan D23).</summary>
        public string Descriptor { get; init; } = "";

        /// <summary>Distance from the planet's centre in kilometres.</summary>
        public double Distance { get; init; }

        /// <summary>The time to circle its planet, in days.</summary>
        public double Period { get; init; }

        /// <summary>What it is mostly made of.</summary>
        public Composition Composition { get; init; }

        /// <summary>Mean density in g/cm³ (the Moon 3.34).</summary>
        public double Density { get; init; }

        /// <summary>Mass in Earths (the Moon 0.0123).</summary>
        public double Mass { get; init; }

        /// <summary>Surface gravity in g (the Moon 0.165).</summary>
        public double Gravity { get; init; }

        /// <summary>Escape velocity in km/s.</summary>
        public double EscapeVelocity { get; init; }

        /// <summary>Starlight received, Earth = 1 (its planet's).</summary>
        public double Insolation { get; init; }

        /// <summary>The share of starlight reflected, 0 to 1.</summary>
        public double Albedo { get; init; }

        /// <summary>Heat raised inside it by its planet's tides, in watts per square metre (Io about 2.4, Earth's own heat 0.09).</summary>
        public double TidalHeating { get; init; }

        /// <summary>Average surface temperature in kelvin.</summary>
        public double Temperature { get; init; }

        /// <summary>Average day-side temperature in kelvin.</summary>
        public double DayTemperature { get; init; }

        /// <summary>Average night-side temperature in kelvin.</summary>
        public double NightTemperature { get; init; }

        /// <summary>Its air.</summary>
        public Atmosphere Atmosphere { get; init; } = new Atmosphere();

        /// <summary>The share of the surface under liquid water, 0 to 1.</summary>
        public double Water { get; init; }

        /// <summary>The share of the surface under ice, 0 to 1.</summary>
        public double Ice { get; init; }

        /// <summary>True when an ocean lies under its ice, like Europa's.</summary>
        public bool SubsurfaceOcean { get; init; }

        /// <summary>The time to turn once, in hours: its period, since it keeps one face to its planet.</summary>
        public double Rotation { get; init; }

        /// <summary>Axial tilt to its star in degrees: its planet's, since it circles in the planet's equator.</summary>
        public double Tilt { get; init; }

        /// <summary>Six bands from equator to pole.</summary>
        public IReadOnlyList<ClimateBand> Bands { get; init; } = System.Array.Empty<ClimateBand>();

        /// <summary>The surface by biome, largest first.</summary>
        public IReadOnlyList<BiomeShare> Biomes { get; init; } = System.Array.Empty<BiomeShare>();

        /// <summary>How far life has come.</summary>
        public LifeLevel Life { get; init; }

        /// <summary>How much plant life, 0 to 5.</summary>
        public int Flora { get; init; }

        /// <summary>How much animal life, 0 to 5.</summary>
        public int Fauna { get; init; }

        /// <summary>One or two things that make it memorable, such as "ice geysers", true to its data.</summary>
        public IReadOnlyList<string> Traits { get; init; } = System.Array.Empty<string>();

        /// <summary>Something no one can explain, or null; the Weirdness option sets how often.</summary>
        public string? Anomaly { get; init; }

        /// <summary>What can be mined or harvested.</summary>
        public ResourceGrades Resources { get; init; } = new ResourceGrades();

        /// <summary>How dangerous the surface is: 1 shirt-sleeves, 2 a mask or coat, 3 a pressure suit, 4 heavy protection, 5 lethal.</summary>
        public int Hazard { get; init; }

        /// <summary>The dangers in words, worst first, such as "the giant's radiation belts".</summary>
        public IReadOnlyList<string> Hazards { get; init; } = System.Array.Empty<string>();

        /// <summary>The Earth Similarity Index, 0 to 1 (Earth 1).</summary>
        public double Similarity { get; init; }

        /// <summary>How well people could live there; <see cref="HabitabilityFor"/> asks for another species.</summary>
        public Habitability Habitability { get; init; }

        /// <summary>A second line of plain words, such as "0.13 g, dense nitrogen air at 1.5 bar, -176 °C, an ocean under the ice, prebiotic chemistry; hazard 3".</summary>
        public string Summary { get; init; } = "";

        /// <summary>Your own fields, by name, set by a <see cref="GeneratorHooks"/> hook or your code; empty from the generator.</summary>
        public IReadOnlyDictionary<string, string> Custom { get; init; } = CustomFields.Empty;

        /// <summary>How well <paramref name="species"/> could live here, from its temperature, gravity, pressure and air.</summary>
        public Habitability HabitabilityFor(Species species)
        {
            if (species is null)
            {
                throw new System.ArgumentNullException(nameof(species));
            }

            return PlanetDetail.HabitabilityOf(species, Atmosphere, Temperature, Gravity);
        }
    }

    /// <summary>
    /// A belt of asteroids or icy bodies, from <see cref="Inner"/> to <see cref="Outer"/> au. The fields after
    /// <see cref="Outer"/> are the belt level (ai-docs/notes/2026-10-03-moon-and-belt-level-design.md).
    /// </summary>
    public sealed partial record Belt
    {
        /// <summary>Rocky or icy.</summary>
        public BeltKind Kind { get; init; }

        /// <summary>Inner edge in au.</summary>
        public double Inner { get; init; }

        /// <summary>Outer edge in au.</summary>
        public double Outer { get; init; }

        /// <summary>Where it is, such as <c>v1-my-seed/system/belt/0</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its position in the system's list of belts, from 0.</summary>
        public int Index { get; init; }

        /// <summary>Its name, such as "Tau Ceti Belt" or "Tau Ceti Outer Belt".</summary>
        public string Name { get; init; } = "";

        /// <summary>What its bodies are made of, in whole percent.</summary>
        public BeltComposition Composition { get; init; } = new BeltComposition();

        /// <summary>Total mass in Earths (the Sun's asteroid belt about 0.0004).</summary>
        public double Mass { get; init; }

        /// <summary>The radius of its largest body in kilometres (Ceres 470).</summary>
        public double LargestBody { get; init; }

        /// <summary>The temperature of its bodies at its middle, in kelvin.</summary>
        public double Temperature { get; init; }

        /// <summary>What can be mined.</summary>
        public ResourceGrades Resources { get; init; } = new ResourceGrades();

        /// <summary>One line in plain words, such as "65% carbonaceous rock, 25% silicate rock, 10% metal; largest body 470 km in radius; -103 °C; richest in organics".</summary>
        public string Summary { get; init; } = "";

        /// <summary>Your own fields, by name, set by a <see cref="GeneratorHooks"/> hook or your code; empty from the generator.</summary>
        public IReadOnlyDictionary<string, string> Custom { get; init; } = CustomFields.Empty;
    }

    /// <summary>What a belt's bodies are made of, in whole percent summing to 100.</summary>
    public sealed record BeltComposition
    {
        /// <summary>Stony silicate rock (S-type asteroids).</summary>
        public int Silicate { get; init; }

        /// <summary>Dark carbon-rich rock (C-type asteroids).</summary>
        public int Carbonaceous { get; init; }

        /// <summary>Iron and nickel (M-type asteroids).</summary>
        public int Metal { get; init; }

        /// <summary>Water and other ices.</summary>
        public int Ice { get; init; }
    }

    /// <summary>A station.</summary>
    public sealed partial record Station
    {
        /// <summary>Where it is, such as <c>v1-my-seed/system/station/0</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "Halcyon Shipyard".</summary>
        public string Name { get; init; } = "";

        /// <summary>What it is for.</summary>
        public StationKind Kind { get; init; }

        /// <summary>The index of the planet it orbits, or null when it orbits the star.</summary>
        public int? Planet { get; init; }

        /// <summary>Your own fields, by name, set by a <see cref="GeneratorHooks"/> hook or your code; empty from the generator.</summary>
        public IReadOnlyDictionary<string, string> Custom { get; init; } = CustomFields.Empty;
    }

    /// <summary>Something memorable in a system (plan D24).</summary>
    public sealed record Landmark
    {
        /// <summary>What it is.</summary>
        public LandmarkKind Kind { get; init; }

        /// <summary>The planet it belongs to, when it belongs to one.</summary>
        public int? Planet { get; init; }

        /// <summary>True for an outlier that breaks the usual rules on purpose (the Weirdness option).</summary>
        public bool Outlier { get; init; }

        /// <summary>One line in plain words.</summary>
        public string Text { get; init; } = "";
    }
}
