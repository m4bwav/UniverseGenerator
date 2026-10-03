#nullable enable
namespace UniverseGeneration
{
    /// <summary>
    /// Hierarchical, addressable seeds (plan D17): a child's seed comes from its parent's seed, a label and an index, and
    /// each purpose of an object (layout, star, planets, names) has its own <see cref="Pcg32"/> stream, so a new field,
    /// level or probability never shifts the draws of anything else.
    /// </summary>
    internal static class Seeds
    {
        private const ulong Golden = 0x9E3779B97F4A7C15UL;
        private const ulong FnvOffset = 0xCBF29CE484222325UL;
        private const ulong FnvPrime = 0x100000001B3UL;

        /// <summary>SplitMix64's output function (Vigna): the first output of a SplitMix64 generator whose state is <paramref name="x"/>.</summary>
        public static ulong SplitMix64(ulong x)
        {
            x = unchecked(x + Golden);
            x = unchecked((x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL);
            x = unchecked((x ^ (x >> 27)) * 0x94D049BB133111EBUL);
            return x ^ (x >> 31);
        }

        /// <summary>
        /// 64-bit FNV-1a over the text's UTF-8 bytes. Encoded by hand so that every runtime agrees; a lone surrogate
        /// counts as U+FFFD. Never <c>string.GetHashCode</c>, which .NET randomises per process.
        /// </summary>
        public static ulong Fnv1a(string text)
        {
            var h = FnvOffset;
            for (var i = 0; i < text.Length; i++)
            {
                int c = text[i];
                if (c >= 0xD800 && c <= 0xDBFF && i + 1 < text.Length && text[i + 1] >= 0xDC00 && text[i + 1] <= 0xDFFF)
                {
                    c = 0x10000 + ((c - 0xD800) << 10) + (text[i + 1] - 0xDC00);
                    i++;
                }
                else if (c >= 0xD800 && c <= 0xDFFF)
                {
                    c = 0xFFFD;
                }

                if (c < 0x80)
                {
                    h = Mix(h, c);
                }
                else if (c < 0x800)
                {
                    h = Mix(Mix(h, 0xC0 | (c >> 6)), 0x80 | (c & 0x3F));
                }
                else if (c < 0x10000)
                {
                    h = Mix(Mix(Mix(h, 0xE0 | (c >> 12)), 0x80 | ((c >> 6) & 0x3F)), 0x80 | (c & 0x3F));
                }
                else
                {
                    h = Mix(Mix(Mix(Mix(h, 0xF0 | (c >> 18)), 0x80 | ((c >> 12) & 0x3F)), 0x80 | ((c >> 6) & 0x3F)), 0x80 | (c & 0x3F));
                }
            }

            return h;
        }

        private static ulong Mix(ulong h, int b) => unchecked((h ^ (uint)b) * FnvPrime);

        /// <summary>A child's seed, e.g. <c>Child(galaxy, "system", 31)</c>.</summary>
        public static ulong Child(ulong parent, string label, int index) =>
            SplitMix64(unchecked(SplitMix64(parent ^ Fnv1a(label)) + (uint)index));

        /// <summary>The stream of one purpose of an object, e.g. <c>Stream(system, "names")</c>.</summary>
        public static Pcg32 Stream(ulong objectSeed, string purpose)
        {
            var p = Fnv1a(purpose);
            return new Pcg32(SplitMix64(objectSeed ^ p), p);
        }

        /// <summary>The root seed of a seed text under a generator version (plan D4: the version is part of a seed's identity).</summary>
        public static ulong FromText(string text, int generatorVersion) =>
            SplitMix64(Fnv1a(text) ^ (uint)generatorVersion);
    }
}
