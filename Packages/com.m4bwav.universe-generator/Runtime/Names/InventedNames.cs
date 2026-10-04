#nullable enable
using System.Text;

namespace UniverseGeneration
{
    /// <summary>How systems are named.</summary>
    public enum NameStyle
    {
        /// <summary>Real catalogue names: IAU proper names, Bayer, HD, HIP, GJ, Kepler and TOI designations.</summary>
        Catalogue,

        /// <summary>Invented names of two or three syllables, such as Kessaran or Valdor, for fantasy and space opera.</summary>
        Invented,
    }

    /// <summary>
    /// Invented system names from one syllable set (the culture sets belong to the Names add-on): an onset, a vowel and an
    /// optional coda per syllable, joined and capitalised, with a few awkward letter runs smoothed. Integer draws only.
    /// </summary>
    internal static class InventedNames
    {
        private static readonly string[] s_onsets =
        {
            "b", "c", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "s", "t", "v", "z", "br", "dr", "kr", "st", "th", "tr", "sh",
        };

        private static readonly string[] s_vowels = { "a", "a", "a", "e", "e", "i", "i", "o", "o", "u", "ae", "ia" };

        private static readonly string[] s_innerCodas = { "l", "n", "r", "s" };

        private static readonly string[] s_finalCodas = { "", "", "", "n", "n", "r", "r", "s", "l", "th", "x", "nd" };

        private static readonly string[] s_endings = { "a", "on", "is", "ar", "ia", "or", "us", "ion" };

        /// <summary>Longer names are drawn again, up to five times, then cut to this many letters.</summary>
        private const int MaxLength = 10;

        public static string Draw(Pcg32 rng)
        {
            var name = "";
            for (var attempt = 0; attempt < 5; attempt++)
            {
                name = Word(rng);
                if (name.Length <= MaxLength)
                {
                    break;
                }
            }

            if (name.Length > MaxLength)
            {
                name = name.Substring(0, MaxLength);
            }

            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private static string Word(Pcg32 rng)
        {
            var text = new StringBuilder();
            var syllables = rng.Chance(1, 4) ? 3 : 2;
            for (var i = 0; i < syllables; i++)
            {
                // The first syllable may open on a vowel; later ones start with a consonant, so vowels never pile up.
                if (i > 0 || !rng.Chance(1, 6))
                {
                    text.Append(s_onsets[rng.NextInt(s_onsets.Length)]);
                }

                text.Append(s_vowels[rng.NextInt(s_vowels.Length)]);
                if (i < syllables - 1 && rng.Chance(1, 4))
                {
                    text.Append(s_innerCodas[rng.NextInt(s_innerCodas.Length)]);
                }
            }

            var coda = s_finalCodas[rng.NextInt(s_finalCodas.Length)];
            text.Append(coda);
            if (coda.Length == 0 && rng.Chance(1, 3))
            {
                var ending = s_endings[rng.NextInt(s_endings.Length)];
                text.Append(IsVowel(ending[0]) ? "r" + ending : ending);
            }

            return Smooth(text.ToString());
        }

        /// <summary>A system's name in the chosen style, drawn from the system's own names stream.</summary>
        public static string SystemName(Pcg32 rng, StarClass c, NameStyle style) =>
            style == NameStyle.Invented ? Draw(rng) : StarNames.Draw(rng, c);

        private static bool IsVowel(char c) => c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u' || c == 'y';

        // Three of a letter, or "yy", read badly: keep two, or one y.
        private static string Smooth(string s)
        {
            var text = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                var n = text.Length;
                if (n >= 2 && text[n - 1] == c && text[n - 2] == c)
                {
                    continue;
                }

                if (n >= 1 && c == 'y' && text[n - 1] == 'y')
                {
                    continue;
                }

                text.Append(c);
            }

            return text.ToString();
        }
    }
}
