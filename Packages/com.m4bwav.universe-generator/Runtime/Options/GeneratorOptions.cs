#nullable enable
using System;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>Which star types a generated region holds.</summary>
    public enum StarMix
    {
        /// <summary>Tuned for play: about a third red dwarfs, and giants, blue stars and remnants often enough to find (plan D18).</summary>
        Game,

        /// <summary>The real shares near the Sun: about 73% red dwarfs, blue stars almost never.</summary>
        Plausible,
    }

    /// <summary>The age of a universe, which every cluster in it takes as its own.</summary>
    public enum Epoch
    {
        /// <summary>Drawn from the seed: young three times in ten, mature 45 in a hundred, old a quarter.</summary>
        Auto,

        /// <summary>A young universe: more small groups, young spirals and twice the quasars.</summary>
        Young,

        /// <summary>A middle-aged universe, like ours.</summary>
        Mature,

        /// <summary>An old universe: more rich clusters, old galaxies and few quasars.</summary>
        Old,
    }

    /// <summary>
    /// Every setting of the generator, as an immutable record: start from a <see cref="Preset"/> and change what you
    /// need with <c>with</c>, for example <c>Preset.SpaceOpera with { Weirdness = 10 }</c>. Each level reads the
    /// settings it uses. Invalid values are refused before anything is generated, with a message saying what to change.
    /// </summary>
    public sealed record GeneratorOptions
    {
        /// <summary>The longest seed text accepted.</summary>
        public const int MaxSeedLength = 200;

        /// <summary>The star types to draw from.</summary>
        public StarMix StarMix { get; init; } = StarMix.Game;

        /// <summary>The percentage of systems (0 to 100) that get an outlier landmark breaking the usual rules (plan D24).</summary>
        public int Weirdness { get; init; } = 5;

        /// <summary>The most planets a system may have (0 to 20).</summary>
        public int MaxPlanetsPerSystem { get; init; } = 12;

        /// <summary>The most systems a galaxy may have; a galaxy has fewer only if its shape cannot hold them all.</summary>
        public const int MaxSystems = 2000;

        /// <summary>How many systems a galaxy has (1 to <see cref="MaxSystems"/>); 60 reads well on one screen (plan D16).</summary>
        public int Systems { get; init; } = 60;

        /// <summary>The galaxy's shape; <see cref="GalaxyShape.Auto"/> draws one that reads at the requested count.</summary>
        public GalaxyShape Shape { get; init; } = GalaxyShape.Auto;

        /// <summary>Whether a galaxy cluster is a small group or a rich cluster; <see cref="ClusterKind.Auto"/> draws one.</summary>
        public ClusterKind ClusterKind { get; init; } = ClusterKind.Auto;

        /// <summary>The age of a universe or of a cluster generated alone; <see cref="Epoch.Auto"/> draws one.</summary>
        public Epoch Epoch { get; init; } = Epoch.Auto;

        /// <summary>The number of arms of spiral and barred galaxies (2 to 4); null draws it from the seed.</summary>
        public int? Arms { get; init; }

        /// <summary>
        /// The chance, in percent (0 to 100), that two neighbouring systems get a lane beyond the network that keeps every
        /// system reachable; higher means more routes and fewer chokepoints. The default is 25.
        /// </summary>
        public int ExtraLanes { get; init; } = GalaxyLayout.DefaultExtraLanes;

        /// <summary>Added to every system's danger (-5 to 5), which stays within 1 to 10; negative for a safer galaxy.</summary>
        public int DangerShift { get; init; }

        /// <summary>Catalogue names (the default) or invented ones; planets and moons take their system's name either way.</summary>
        public NameStyle Names { get; init; } = NameStyle.Catalogue;

        /// <summary>Throws an <see cref="ArgumentException"/> naming the first invalid setting and what it must be.</summary>
        public void Validate()
        {
            CheckRange(nameof(Weirdness), Weirdness, 0, 100);
            CheckRange(nameof(MaxPlanetsPerSystem), MaxPlanetsPerSystem, 0, 20);
            CheckRange(nameof(Systems), Systems, 1, MaxSystems);
            if (Arms.HasValue)
            {
                CheckRange(nameof(Arms), Arms.Value, 2, 4);
            }

            CheckRange(nameof(ExtraLanes), ExtraLanes, 0, 100);
            CheckRange(nameof(DangerShift), DangerShift, -5, 5);
            if (Shape < GalaxyShape.Auto || Shape > GalaxyShape.Irregular)
            {
                throw new ArgumentException($"{nameof(Shape)} must be a GalaxyShape such as Auto or Spiral; you asked for {(int)Shape}.", nameof(Shape));
            }
            if (ClusterKind < ClusterKind.Auto || ClusterKind > ClusterKind.Cluster)
            {
                throw new ArgumentException($"{nameof(ClusterKind)} must be Auto, Group or Cluster; you asked for {(int)ClusterKind}.", nameof(ClusterKind));
            }

            if (Epoch < Epoch.Auto || Epoch > Epoch.Old)
            {
                throw new ArgumentException($"{nameof(Epoch)} must be Auto, Young, Mature or Old; you asked for {(int)Epoch}.", nameof(Epoch));
            }

            if (Names != NameStyle.Catalogue && Names != NameStyle.Invented)
            {
                throw new ArgumentException($"{nameof(Names)} must be Catalogue or Invented; you asked for {(int)Names}.", nameof(Names));
            }

            if (StarMix != StarMix.Game && StarMix != StarMix.Plausible)
            {
                throw new ArgumentException($"{nameof(StarMix)} must be Game or Plausible; you asked for {(int)StarMix}.", nameof(StarMix));
            }
        }

        /// <summary>
        /// The settings that differ from the defaults as short text, such as <c>systems=120&amp;shape=barred</c> (empty for
        /// the defaults). Put it after an address with <c>?</c> to share an object with its options (see
        /// <see cref="Universe.Link"/>), or read it back with <see cref="FromCode"/>. Codes stay readable for the whole major version.
        /// </summary>
        public string ToCode() => OptionsCode.Write(this);

        /// <summary>Reads a code written by <see cref="ToCode"/>; a leading <c>?</c> is allowed and an empty code gives the defaults.</summary>
        /// <exception cref="ArgumentException">The code names an unknown setting, repeats one, or gives a value out of range; the message says which.</exception>
        public static GeneratorOptions FromCode(string code)
        {
            if (code is null)
            {
                throw new ArgumentNullException(nameof(code), "A code is text such as \"systems=120\"; use \"\" for the defaults.");
            }

            return OptionsCode.Read(code);
        }

        internal static string CheckSeed(string? seed)
        {
            if (seed is null)
            {
                throw new ArgumentNullException(nameof(seed), "A seed is any text, such as \"my-seed\" or \"42\"; it cannot be null.");
            }

            if (seed.Length > MaxSeedLength)
            {
                throw new ArgumentException($"A seed must be at most {MaxSeedLength} characters; yours has {seed.Length}.", nameof(seed));
            }

            return seed;
        }

        internal static string SeedText(long seed) => seed.ToString(CultureInfo.InvariantCulture);

        private static void CheckRange(string name, int value, int min, int max)
        {
            if (value < min || value > max)
            {
                throw new ArgumentException($"{name} must be {min} to {max}; you asked for {value}.", name);
            }
        }
    }

    /// <summary>Ready-made settings. Each is a <see cref="GeneratorOptions"/> record, so <c>with</c> adjusts it.</summary>
    public static class Preset
    {
        /// <summary>The defaults: game-tuned stars, a 5% outlier share.</summary>
        public static GeneratorOptions Default { get; } = new GeneratorOptions();

        /// <summary>Busier and stranger: more outliers.</summary>
        public static GeneratorOptions SpaceOpera { get; } = new GeneratorOptions { Weirdness = 10 };

        /// <summary>Real star shares and few outliers, for hard science fiction.</summary>
        public static GeneratorOptions Plausible { get; } = new GeneratorOptions { StarMix = StarMix.Plausible, Weirdness = 1 };

        /// <summary>A small map for a short game or one session: 20 systems with at most 6 planets each.</summary>
        public static GeneratorOptions Pocket { get; } = new GeneratorOptions { Systems = 20, MaxPlanetsPerSystem = 6 };

        /// <summary>A run: 30 systems, few extra lanes (so more chokepoints), danger two levels higher, more outliers.</summary>
        public static GeneratorOptions Roguelike { get; } = new GeneratorOptions { Systems = 30, ExtraLanes = 10, DangerShift = 2, Weirdness = 15 };

        /// <summary>Gentle: 40 well-connected systems, danger three levels lower, few outliers.</summary>
        public static GeneratorOptions Cozy { get; } = new GeneratorOptions { Systems = 40, ExtraLanes = 50, DangerShift = -3, Weirdness = 3 };

        /// <summary>A long campaign: 300 systems, so spirals and bars read (Auto picks them from 80).</summary>
        public static GeneratorOptions Epic { get; } = new GeneratorOptions { Systems = 300 };
    }
}
