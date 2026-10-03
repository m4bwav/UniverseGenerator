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

    /// <summary>A moon. Orbit in planet radii, radius in kilometres.</summary>
    public sealed record Moon
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
    }

    /// <summary>A belt of asteroids or icy bodies, from <see cref="Inner"/> to <see cref="Outer"/> au.</summary>
    public sealed record Belt
    {
        /// <summary>Rocky or icy.</summary>
        public BeltKind Kind { get; init; }

        /// <summary>Inner edge in au.</summary>
        public double Inner { get; init; }

        /// <summary>Outer edge in au.</summary>
        public double Outer { get; init; }
    }

    /// <summary>A station.</summary>
    public sealed record Station
    {
        /// <summary>Where it is, such as <c>v1-my-seed/system/station/0</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "Halcyon Shipyard".</summary>
        public string Name { get; init; } = "";

        /// <summary>What it is for.</summary>
        public StationKind Kind { get; init; }

        /// <summary>The index of the planet it orbits, or null when it orbits the star.</summary>
        public int? Planet { get; init; }
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
