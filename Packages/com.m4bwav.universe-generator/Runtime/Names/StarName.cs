#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// A star's name on its own, for a name tool or a story: <c>StarName.Generate("my-seed")</c> gives one, and
    /// <c>StarName.Generate("my-seed", 20)</c> twenty different ones. The names are catalogue-style, as the systems carry
    /// (IAU proper names, Bayer and Flamsteed designations, HD, HIP, GJ, Kepler, TOI and the remnants' PSR, WD and Gaia BH),
    /// and fit the star's class. They come from their own seeds, so they are not the names of the systems of the same seed.
    /// </summary>
    public sealed record StarName
    {
        /// <summary>The most names one call makes.</summary>
        public const int MaxCount = 1000;

        /// <summary>The name, such as "HD 31487", "Lambda Pegasi" or "PSR J2204+3957".</summary>
        public string Name { get; init; } = "";

        /// <summary>The class of the star the name fits.</summary>
        public StarClass Class { get; init; }

        /// <summary>
        /// One star name from any seed text. With <paramref name="starClass"/>, a name for that class; without it, the class is
        /// drawn with the options' star mix. The same as the first of <see cref="Generate(string, int, StarClass?, GeneratorOptions?)"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static StarName Generate(string seed, StarClass? starClass = null, GeneratorOptions? options = null) =>
            Generate(seed, 1, starClass, options)[0];

        /// <summary><paramref name="count"/> different star names from any seed text, 1 to <see cref="MaxCount"/>.</summary>
        /// <exception cref="ArgumentException">The seed is too long, the count is out of range or an option is; the message says which.</exception>
        public static IReadOnlyList<StarName> Generate(string seed, int count, StarClass? starClass = null, GeneratorOptions? options = null)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            if (count < 1 || count > MaxCount)
            {
                throw new ArgumentException($"Count must be 1 to {MaxCount.ToString(CultureInfo.InvariantCulture)}; you asked for {count.ToString(CultureInfo.InvariantCulture)}.", nameof(count));
            }

            var root = Address.RootSeed(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "starname");
            var used = new HashSet<string>(StringComparer.Ordinal);
            var names = new StarName[count];
            for (var i = 0; i < count; i++)
            {
                var own = Seeds.Child(root, "name", i);
                var c = starClass ?? StarGenerator.Roll(Seeds.Stream(own, "star"), StellarAge.Mature, o.StarMix).Star.Class;
                var rng = Seeds.Stream(own, "names");
                var name = StarNames.Draw(rng, c);
                for (var tries = 0; !used.Add(name); tries++)
                {
                    // Out of reach in practice (hundreds of thousands of names); kept so a call never loops forever.
                    name = tries < 100 ? StarNames.Draw(rng, c) : StarNames.Draw(rng, c) + " " + (i + 1).ToString(CultureInfo.InvariantCulture);
                }

                names[i] = new StarName { Name = name, Class = c };
            }

            return names;
        }

        /// <summary>Star names from a number; the same as passing the number's digits as text.</summary>
        public static IReadOnlyList<StarName> Generate(long seed, int count, StarClass? starClass = null, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), count, starClass, options);
    }
}
