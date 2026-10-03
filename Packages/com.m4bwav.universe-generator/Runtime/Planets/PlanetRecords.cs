#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>What a planet is mostly made of (plan P8).</summary>
    public enum Composition
    {
        /// <summary>Mostly iron and other metals, like Mercury.</summary>
        Metal,

        /// <summary>Silicate rock around an iron core, like Earth.</summary>
        Rock,

        /// <summary>Rock and water ice, like Ganymede or Pluto.</summary>
        IceAndRock,

        /// <summary>A deep layer of water over rock.</summary>
        Water,

        /// <summary>A rocky or icy core under a thick envelope of gas.</summary>
        GasEnvelope,

        /// <summary>Hydrogen and helium, like Jupiter.</summary>
        HydrogenHelium,

        /// <summary>Water, ammonia and methane ices under hydrogen, like Neptune.</summary>
        Ices,
    }

    /// <summary>How much air a planet has.</summary>
    public enum AtmosphereClass
    {
        /// <summary>No air at all.</summary>
        None,

        /// <summary>A trace, under 0.01 bar.</summary>
        Trace,

        /// <summary>Thin, under 0.5 bar.</summary>
        Thin,

        /// <summary>Like Earth's, 0.5 to 2 bar.</summary>
        Standard,

        /// <summary>Dense, 2 to 10 bar.</summary>
        Dense,

        /// <summary>Crushing, 10 bar or more, like Venus.</summary>
        Crushing,

        /// <summary>A deep envelope with no surface beneath it (giants and mini-Neptunes).</summary>
        Envelope,
    }

    /// <summary>A gas in an atmosphere.</summary>
    public enum Gas
    {
        /// <summary>Hydrogen (molecular mass 2).</summary>
        Hydrogen,

        /// <summary>Helium (4).</summary>
        Helium,

        /// <summary>Methane (16).</summary>
        Methane,

        /// <summary>Water vapour (18).</summary>
        WaterVapour,

        /// <summary>Nitrogen (28).</summary>
        Nitrogen,

        /// <summary>Oxygen (32).</summary>
        Oxygen,

        /// <summary>Carbon dioxide (44).</summary>
        CarbonDioxide,

        /// <summary>Sulphur dioxide (64).</summary>
        SulphurDioxide,

        /// <summary>Vaporised rock above a molten surface.</summary>
        RockVapour,
    }

    /// <summary>How a planet turns (plan P12).</summary>
    public enum Spin
    {
        /// <summary>Turns freely, with days and nights.</summary>
        Free,

        /// <summary>Turns three times every two orbits, like Mercury.</summary>
        Resonant,

        /// <summary>Tidally locked: one side always faces the star.</summary>
        Locked,
    }

    /// <summary>A kind of surface (plan P15). Plants grow only where there is life to grow them.</summary>
    public enum Biome
    {
        /// <summary>Open water.</summary>
        Ocean,

        /// <summary>Sea ice, glaciers and ice sheets.</summary>
        IceSheet,

        /// <summary>Cold, treeless plains.</summary>
        Tundra,

        /// <summary>Cold forest.</summary>
        Taiga,

        /// <summary>Temperate forest.</summary>
        TemperateForest,

        /// <summary>Grassland and steppe.</summary>
        Grassland,

        /// <summary>Hot grassland with scattered trees.</summary>
        Savanna,

        /// <summary>Hot, wet forest.</summary>
        Rainforest,

        /// <summary>Sand and dust.</summary>
        Desert,

        /// <summary>Bare rock and dust with nothing growing.</summary>
        Barren,

        /// <summary>Lava fields and molten rock.</summary>
        Volcanic,
    }

    /// <summary>How far life has come on a world (plan P3).</summary>
    public enum LifeLevel
    {
        /// <summary>No life.</summary>
        None,

        /// <summary>The chemistry of life, but no life yet.</summary>
        Prebiotic,

        /// <summary>Single cells.</summary>
        Microbial,

        /// <summary>Mats, algae and simple many-celled life.</summary>
        Simple,

        /// <summary>Plants and animals, or their like.</summary>
        Complex,
    }

    /// <summary>How well a species could live on a world (plan P17).</summary>
    public enum Habitability
    {
        /// <summary>Not without a sealed ship or station.</summary>
        Hostile,

        /// <summary>Only in domes, suits or underground.</summary>
        Marginal,

        /// <summary>Livable with masks, coats or care.</summary>
        Habitable,

        /// <summary>Comfortable in the open.</summary>
        Ideal,
    }

    /// <summary>A planet's air (plan P11). Pressure in bar.</summary>
    public sealed record Atmosphere
    {
        /// <summary>How much air, from none to crushing, or an envelope with no surface.</summary>
        public AtmosphereClass Class { get; init; }

        /// <summary>Surface pressure in bar; null where there is no surface (giants and mini-Neptunes).</summary>
        public double? Pressure { get; init; }

        /// <summary>The main gases, most abundant first.</summary>
        public IReadOnlyList<Gas> Gases { get; init; } = System.Array.Empty<Gas>();

        /// <summary>
        /// The molecular mass of the lightest gas the planet can keep for billions of years (the Jeans rule: escape
        /// velocity at least six times the gas's thermal speed). Earth's is about 7: it keeps water and nitrogen and
        /// loses hydrogen and helium.
        /// </summary>
        public double LightestGasKept { get; init; }

        /// <summary>True when people could breathe it: oxygen at 0.5 to 3 bar.</summary>
        public bool Breathable { get; init; }

        /// <summary>Why the air is as it is, in plain words.</summary>
        public string Why { get; init; } = "";
    }

    /// <summary>
    /// A band of a planet's surface (plan P15, A7): latitude <see cref="From"/> to <see cref="To"/> in degrees, both
    /// hemispheres together; for a tidally locked planet, degrees from the point under its star (80 to 100 is the
    /// terminator).
    /// </summary>
    public sealed record ClimateBand
    {
        /// <summary>Where the band starts, in degrees.</summary>
        public int From { get; init; }

        /// <summary>Where the band ends, in degrees.</summary>
        public int To { get; init; }

        /// <summary>Its share of the planet's surface, 0 to 1.</summary>
        public double Share { get; init; }

        /// <summary>Its average temperature in kelvin.</summary>
        public double Temperature { get; init; }

        /// <summary>The band's land biome, or its water's when it has no land.</summary>
        public Biome Biome { get; init; }
    }

    /// <summary>A biome and its share of the planet's surface.</summary>
    public sealed record BiomeShare
    {
        /// <summary>The biome.</summary>
        public Biome Biome { get; init; }

        /// <summary>Its share of the surface, 0 to 1.</summary>
        public double Share { get; init; }
    }

    /// <summary>What can be mined or harvested, each graded 0 (none) or 1 (poor) to 5 (rich) (plan P6).</summary>
    public sealed record PlanetResources
    {
        /// <summary>Iron, nickel and common metals.</summary>
        public int Metals { get; init; }

        /// <summary>Rare earths, heavy and radioactive elements.</summary>
        public int RareElements { get; init; }

        /// <summary>Water and other ices.</summary>
        public int Ices { get; init; }

        /// <summary>Hydrogen, helium-3 and other fuel gases.</summary>
        public int Gases { get; init; }

        /// <summary>Biomass and organic compounds.</summary>
        public int Organics { get; init; }
    }

    /// <summary>
    /// What a species needs, for <see cref="Planet.HabitabilityFor"/>: temperatures in kelvin, gravity in g, pressure in
    /// bar. Start from <see cref="Human"/> and change what differs, for example <c>Species.Human with { MaxTemperature = 340 }</c>.
    /// </summary>
    public sealed record Species
    {
        /// <summary>People: 263 to 313 K (-10 to 40 °C), 0.4 to 1.6 g, 0.5 to 3 bar, breathing oxygen.</summary>
        public static Species Human { get; } = new Species();

        /// <summary>The species' name.</summary>
        public string Name { get; init; } = "human";

        /// <summary>The coldest comfortable average temperature in kelvin.</summary>
        public double MinTemperature { get; init; } = 263;

        /// <summary>The warmest comfortable average temperature in kelvin.</summary>
        public double MaxTemperature { get; init; } = 313;

        /// <summary>The lowest comfortable gravity in g.</summary>
        public double MinGravity { get; init; } = 0.4;

        /// <summary>The highest comfortable gravity in g.</summary>
        public double MaxGravity { get; init; } = 1.6;

        /// <summary>The lowest comfortable pressure in bar.</summary>
        public double MinPressure { get; init; } = 0.5;

        /// <summary>The highest comfortable pressure in bar.</summary>
        public double MaxPressure { get; init; } = 3;

        /// <summary>True when it needs oxygen in the air.</summary>
        public bool BreathesOxygen { get; init; } = true;
    }
}
