#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// A star system: its star (and companion), planets with their moons, belts, stations, landmarks and story tags.
    /// Distances are in au. <c>StarSystem.Generate("my-seed")</c> makes one on its own; a galaxy makes them in place.
    /// </summary>
    public sealed partial record StarSystem
    {
        /// <summary>Where it is, such as <c>v1-my-seed/system</c> or <c>v1-my-seed/galaxy/system/31</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>The star's name, which is also the system's.</summary>
        public string Name { get; init; } = "";

        /// <summary>The primary star.</summary>
        public Star Star { get; init; } = new Star();

        /// <summary>A second star, or null.</summary>
        public Companion? Companion { get; init; }

        /// <summary>How old the system's region is.</summary>
        public StellarAge Age { get; init; }

        /// <summary>How dangerous it is, 1 (safe) to 10 (deadly).</summary>
        public int Danger { get; init; }

        /// <summary>The inner edge of the habitable zone in au.</summary>
        public double HabitableZoneInner { get; init; }

        /// <summary>The outer edge of the habitable zone in au.</summary>
        public double HabitableZoneOuter { get; init; }

        /// <summary>The frost line in au, past which ices condense.</summary>
        public double FrostLine { get; init; }

        /// <summary>The planets, innermost first.</summary>
        public IReadOnlyList<Planet> Planets { get; init; } = System.Array.Empty<Planet>();

        /// <summary>Asteroid and icy belts, innermost first.</summary>
        public IReadOnlyList<Belt> Belts { get; init; } = System.Array.Empty<Belt>();

        /// <summary>The stations.</summary>
        public IReadOnlyList<Station> Stations { get; init; } = System.Array.Empty<Station>();

        /// <summary>What makes the system memorable; always at least one (plan D24).</summary>
        public IReadOnlyList<Landmark> Landmarks { get; init; } = System.Array.Empty<Landmark>();

        /// <summary>One or two story tags, such as "mining boom", that agree with the system's data (plan D23).</summary>
        public IReadOnlyList<string> Tags { get; init; } = System.Array.Empty<string>();

        /// <summary>One line in plain words, such as "a yellow star with 6 planets and an asteroid belt".</summary>
        public string Descriptor { get; init; } = "";

        /// <summary>Your own fields, by name, set by a <see cref="GeneratorHooks"/> hook or your code; empty from the generator.</summary>
        public IReadOnlyDictionary<string, string> Custom { get; init; } = CustomFields.Empty;

        /// <summary>Generates a star system on its own from any seed text, such as "my-seed".</summary>
        /// <exception cref="System.ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static StarSystem Generate(string seed, GeneratorOptions? options = null) => Generate(seed, options, null);

        /// <summary>Generates a star system on its own from any seed text, running <paramref name="hooks"/> on its planets and on it.</summary>
        /// <exception cref="System.ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static StarSystem Generate(string seed, GeneratorOptions? options, GeneratorHooks? hooks)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            var address = new Address(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "system");
            return Hook.System(hooks, StarSystemGenerator.Generate(address, SystemContext.Alone, o));
        }

        /// <summary>Generates a star system from a number; the same as passing the number's digits as text.</summary>
        public static StarSystem Generate(long seed, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), options);
    }

    /// <summary>What a galaxy decides for a system from its neighbours; null fields are drawn from the system's own streams.</summary>
    internal sealed class SystemContext
    {
        public static readonly SystemContext Alone = new SystemContext(null, null, null);

        public SystemContext(string? name, StellarAge? age, int? danger, int richness = 100, Star? star = null, int forcedPlanet = -1, PlanetKind forcedKind = PlanetKind.Rocky)
        {
            Name = name;
            Age = age;
            Danger = danger;
            Richness = richness;
            Star = star;
            ForcedPlanet = forcedPlanet;
            ForcedKind = forcedKind;
        }

        public string? Name { get; }

        public StellarAge? Age { get; }

        public int? Danger { get; }

        /// <summary>The galaxy's metallicity as a percentage of the gas giant weight: 50 poor, 100 normal, 160 rich.</summary>
        public int Richness { get; }

        /// <summary>A lone star a galaxy's guarantee gave this system (<see cref="Constraints"/>), or null to roll one.</summary>
        public Star? Star { get; }

        /// <summary>The index of the planet a guarantee turns into <see cref="ForcedKind"/>, or -1.</summary>
        public int ForcedPlanet { get; }

        public PlanetKind ForcedKind { get; }

        public SystemContext Forcing(int planet, PlanetKind kind) => new SystemContext(Name, Age, Danger, Richness, Star, planet, kind);
    }
}
