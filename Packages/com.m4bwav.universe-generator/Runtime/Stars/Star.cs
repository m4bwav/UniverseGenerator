#nullable enable
namespace UniverseGeneration
{
    /// <summary>The kind of star. Main-sequence classes run from hot blue O to cool red M.</summary>
    public enum StarClass
    {
        /// <summary>A blue star, over 30,000 K.</summary>
        O,

        /// <summary>A blue-white star, 10,000 to 30,000 K.</summary>
        B,

        /// <summary>A white star, 7,500 to 10,000 K.</summary>
        A,

        /// <summary>A yellow-white star, 6,000 to 7,500 K.</summary>
        F,

        /// <summary>A yellow star like the Sun, 5,200 to 6,000 K.</summary>
        G,

        /// <summary>An orange dwarf, 3,700 to 5,200 K.</summary>
        K,

        /// <summary>A red dwarf, under 3,700 K; the commonest star.</summary>
        M,

        /// <summary>The cooling core of a dead Sun-like star.</summary>
        WhiteDwarf,

        /// <summary>A swollen, ageing star.</summary>
        Giant,

        /// <summary>A huge, very bright star near the end of its life.</summary>
        Supergiant,

        /// <summary>The collapsed core of a massive star, often a pulsar.</summary>
        NeutronStar,

        /// <summary>A stellar black hole.</summary>
        BlackHole,
    }

    /// <summary>How old a region's stars are; old regions hold more giants and remnants, young ones more blue stars.</summary>
    public enum StellarAge
    {
        /// <summary>Young: more blue and white stars.</summary>
        Young,

        /// <summary>Middle-aged, like the Sun's neighbourhood.</summary>
        Mature,

        /// <summary>Old: more giants, white dwarfs and remnants.</summary>
        Old,
    }

    /// <summary>A star. Mass, luminosity and radius are in Suns, temperature in kelvin.</summary>
    public sealed record Star
    {
        /// <summary>The kind of star.</summary>
        public StarClass Class { get; init; }

        /// <summary>The spectral type, such as G2V, K3III or DA5; empty for neutron stars and black holes.</summary>
        public string SpectralType { get; init; } = "";

        /// <summary>Mass in Suns.</summary>
        public double Mass { get; init; }

        /// <summary>Luminosity in Suns.</summary>
        public double Luminosity { get; init; }

        /// <summary>Radius in Suns.</summary>
        public double Radius { get; init; }

        /// <summary>Surface temperature in kelvin (0 for a black hole).</summary>
        public double Temperature { get; init; }

        /// <summary>The colour of its light as #rrggbb.</summary>
        public string Colour { get; init; } = "";

        /// <summary>Plain words for the star, such as "red dwarf" or "blue supergiant".</summary>
        public string Description { get; init; } = "";
    }

    /// <summary>How far a companion star is from the primary.</summary>
    public enum CompanionOrbit
    {
        /// <summary>A tight pair under an au apart; planets circle both stars.</summary>
        Close,

        /// <summary>A distant companion, tens to thousands of au out; planets circle the primary.</summary>
        Wide,
    }

    /// <summary>A second star in the system.</summary>
    public sealed record Companion
    {
        /// <summary>The companion star.</summary>
        public Star Star { get; init; } = new Star();

        /// <summary>Close (planets circle both) or wide (planets circle the primary).</summary>
        public CompanionOrbit Orbit { get; init; }

        /// <summary>Distance from the primary in au.</summary>
        public double Separation { get; init; }
    }
}
