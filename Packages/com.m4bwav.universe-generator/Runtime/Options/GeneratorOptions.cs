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

        /// <summary>Throws an <see cref="ArgumentException"/> naming the first invalid setting and what it must be.</summary>
        public void Validate()
        {
            CheckRange(nameof(Weirdness), Weirdness, 0, 100);
            CheckRange(nameof(MaxPlanetsPerSystem), MaxPlanetsPerSystem, 0, 20);
            CheckRange(nameof(Systems), Systems, 1, MaxSystems);
            if (Shape < GalaxyShape.Auto || Shape > GalaxyShape.Irregular)
            {
                throw new ArgumentException($"{nameof(Shape)} must be a GalaxyShape such as Auto or Spiral; you asked for {(int)Shape}.", nameof(Shape));
            }
            if (ClusterKind < ClusterKind.Auto || ClusterKind > ClusterKind.Cluster)
            {
                throw new ArgumentException($"{nameof(ClusterKind)} must be Auto, Group or Cluster; you asked for {(int)ClusterKind}.", nameof(ClusterKind));
            }

            if (StarMix != StarMix.Game && StarMix != StarMix.Plausible)
            {
                throw new ArgumentException($"{nameof(StarMix)} must be Game or Plausible; you asked for {(int)StarMix}.", nameof(StarMix));
            }
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
    }
}
