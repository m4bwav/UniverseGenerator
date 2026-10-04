#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// A planet. Orbit in au, period in days, mass and radius in Earths, temperatures in kelvin, gravity in g.
    /// <c>Planet.Generate("my-seed")</c> makes one on its own; a star system makes them in place. The fields after
    /// <see cref="Descriptor"/> are the planet level (ai-docs/notes/2026-10-02-planet-level-design.md).
    /// </summary>
    public sealed partial record Planet
    {
        /// <summary>Where it is, such as <c>v1-my-seed/galaxy/system/31/planet/2</c> or <c>v1-my-seed/planet</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its position counting outwards from 0.</summary>
        public int Index { get; init; }

        /// <summary>Its name: the system's name and a letter, such as "Tau Ceti e".</summary>
        public string Name { get; init; } = "";

        /// <summary>What kind of world.</summary>
        public PlanetKind Kind { get; init; }

        /// <summary>Its zone: hot, warm, temperate, cold or outer.</summary>
        public OrbitZone Zone { get; init; }

        /// <summary>Distance from the star (or both stars of a close pair) in au.</summary>
        public double Orbit { get; init; }

        /// <summary>Orbital period in days.</summary>
        public double Period { get; init; }

        /// <summary>
        /// Orbital eccentricity, 0 (a circle) to 0.6: small in systems of several planets (about 0.05, as Kepler's
        /// multi-planet systems show), larger for a planet alone, near 0 for hot planets, and never enough to cross a
        /// neighbour's orbit. <see cref="Orbit"/> is the semi-major axis.
        /// </summary>
        public double Eccentricity { get; init; }

        /// <summary>Inclination in degrees to the system's mean plane (to the star's equator for a planet alone).</summary>
        public double Inclination { get; init; }

        /// <summary>Where the orbit comes closest to the star, as an angle in degrees (0 to 360) on the map, for drawing the ellipse.</summary>
        public double PeriapsisAngle { get; init; }

        /// <summary>Mass in Earths.</summary>
        public double Mass { get; init; }

        /// <summary>Radius in Earths.</summary>
        public double Radius { get; init; }

        /// <summary>True when it has rings.</summary>
        public bool Rings { get; init; }

        /// <summary>Its moons, innermost first.</summary>
        public IReadOnlyList<Moon> Moons { get; init; } = System.Array.Empty<Moon>();

        /// <summary>One line in plain words, such as "a cold ocean world under a red dwarf" (plan D23).</summary>
        public string Descriptor { get; init; } = "";

        /// <summary>What it is mostly made of.</summary>
        public Composition Composition { get; init; }

        /// <summary>Surface gravity in g (at the cloud tops for giants).</summary>
        public double Gravity { get; init; }

        /// <summary>Escape velocity in km/s.</summary>
        public double EscapeVelocity { get; init; }

        /// <summary>Mean density in g/cm³ (Earth 5.51).</summary>
        public double Density { get; init; }

        /// <summary>Starlight received, Earth = 1.</summary>
        public double Insolation { get; init; }

        /// <summary>The share of starlight reflected, 0 to 1.</summary>
        public double Albedo { get; init; }

        /// <summary>Average surface temperature in kelvin (cloud tops for giants).</summary>
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

        /// <summary>The time to turn once, in hours (its orbital period when locked).</summary>
        public double Rotation { get; init; }

        /// <summary>Free, in a 3:2 resonance, or tidally locked.</summary>
        public Spin Spin { get; init; }

        /// <summary>Axial tilt in degrees; over 90 it spins backwards.</summary>
        public double Tilt { get; init; }

        /// <summary>Six bands from equator to pole (or from the point under the star when locked); empty for giants.</summary>
        public IReadOnlyList<ClimateBand> Bands { get; init; } = System.Array.Empty<ClimateBand>();

        /// <summary>The surface by biome, largest first; empty for giants.</summary>
        public IReadOnlyList<BiomeShare> Biomes { get; init; } = System.Array.Empty<BiomeShare>();

        /// <summary>How far life has come.</summary>
        public LifeLevel Life { get; init; }

        /// <summary>How much plant life, 0 to 5.</summary>
        public int Flora { get; init; }

        /// <summary>How much animal life, 0 to 5.</summary>
        public int Fauna { get; init; }

        /// <summary>One or two things that make it memorable, such as "global dust storms", true to its data (plan P1).</summary>
        public IReadOnlyList<string> Traits { get; init; } = System.Array.Empty<string>();

        /// <summary>Something no one can explain, or null; the Weirdness option sets how often (plan P1, D24).</summary>
        public string? Anomaly { get; init; }

        /// <summary>What can be mined or harvested.</summary>
        public ResourceGrades Resources { get; init; } = new ResourceGrades();

        /// <summary>How dangerous the surface is: 1 shirt-sleeves, 2 a mask or coat, 3 a pressure suit, 4 heavy protection, 5 lethal (plan P4).</summary>
        public int Hazard { get; init; }

        /// <summary>The dangers in words, worst first, such as "vacuum" or "dangerous wildlife".</summary>
        public IReadOnlyList<string> Hazards { get; init; } = System.Array.Empty<string>();

        /// <summary>The Earth Similarity Index, 0 to 1 (Earth 1).</summary>
        public double Similarity { get; init; }

        /// <summary>How well people could live there; <see cref="HabitabilityFor"/> asks for another species.</summary>
        public Habitability Habitability { get; init; }

        /// <summary>A second line of plain words, such as "1 g, breathable air at 1.2 bar, 17 °C, 64% ocean, mostly temperate forest, complex life; hazard 1".</summary>
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

        /// <summary>Generates a planet on its own, with its star drawn from the same seed, from any seed text such as "my-seed".</summary>
        /// <exception cref="System.ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static Planet Generate(string seed, GeneratorOptions? options = null) => Generate(seed, options, null);

        /// <summary>Generates a planet on its own from any seed text, running <paramref name="hooks"/> on it.</summary>
        /// <exception cref="System.ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static Planet Generate(string seed, GeneratorOptions? options, GeneratorHooks? hooks)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            var address = new Address(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "planet");
            return Hook.Planet(hooks, PlanetGenerator.Generate(address, o));
        }

        /// <summary>Generates a planet from a number; the same as passing the number's digits as text.</summary>
        public static Planet Generate(long seed, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), options);
    }
}
