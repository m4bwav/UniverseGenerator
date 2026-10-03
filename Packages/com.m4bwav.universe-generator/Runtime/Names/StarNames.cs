#nullable enable
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// Catalogue-style star names (plan D20): IAU proper names and Bayer or Flamsteed designations from the embedded
    /// tables, survey numbers (HD, HIP, GJ, Kepler, TOI, Wolf, Ross, LHS), and the forms remnants carry (PSR, WD, Gaia BH).
    /// </summary>
    internal static class StarNames
    {
        private static string[]? s_proper;
        private static string[]? s_designations;
        private static int[]? s_hipparcosGaps;

        private const int HenryDraperCount = 225300;

        // Split on first use, never in a static constructor (size budget). A race only splits twice; the arrays are equal.
        public static string[] Proper => s_proper ??= StarTables.Proper.Split('|');

        public static string[] Designations => s_designations ??= StarTables.Designations.Split('|');

        private static int[] HipparcosGaps => s_hipparcosGaps ??= ParseGaps();

        /// <summary>A name for a star of class <paramref name="c"/>, drawn from <paramref name="rng"/>.</summary>
        public static string Draw(Pcg32 rng, StarClass c)
        {
            switch (c)
            {
                case StarClass.NeutronStar:
                    return "PSR J" + Coordinates(rng, 2);
                case StarClass.BlackHole:
                    return rng.Chance(1, 2) ? "Gaia BH" + Number(rng.Range(4, 99)) : "XTE J" + Coordinates(rng, 1);
                case StarClass.WhiteDwarf when rng.Chance(1, 2):
                    return "WD " + Number(rng.NextInt(24) * 100 + rng.NextInt(60), 4) + (rng.Chance(1, 2) ? "+" : "-") + Number(rng.NextInt(900), 3);
            }

            var bright = c == StarClass.O || c == StarClass.B || c == StarClass.A || c == StarClass.Giant || c == StarClass.Supergiant;
            var roll = rng.NextInt(100);
            if (roll < (bright ? 35 : 8))
            {
                return Proper[rng.NextInt(Proper.Length)];
            }

            if (roll < (bright ? 70 : 20))
            {
                return Designations[rng.NextInt(Designations.Length)];
            }

            if (bright)
            {
                return Catalogue(rng);
            }

            var survey = rng.NextInt(100);
            if (survey < 25)
            {
                return "HD " + Number(rng.Range(1, HenryDraperCount));
            }

            if (survey < 40)
            {
                return Hipparcos(rng);
            }

            if (survey < 55)
            {
                return "GJ " + Number(rng.Range(1, 915));
            }

            if (survey < 68)
            {
                return "Kepler-" + Number(rng.Range(1, 2000));
            }

            if (survey < 80)
            {
                return "TOI-" + Number(rng.Range(100, 7500));
            }

            if (survey < 88)
            {
                return "Wolf " + Number(rng.Range(1, 1566));
            }

            return survey < 94 ? "Ross " + Number(rng.Range(1, 1100)) : "LHS " + Number(rng.Range(1, 4500));
        }

        /// <summary>A Henry Draper or Hipparcos number, each real catalogue entry equally likely.</summary>
        private static string Catalogue(Pcg32 rng)
        {
            var hipparcosCount = StarTables.HipparcosHighest - HipparcosGaps.Length;
            var index = rng.NextInt(HenryDraperCount + hipparcosCount);
            return index < HenryDraperCount ? "HD " + Number(index + 1) : "HIP " + Number(HipparcosNumber(index - HenryDraperCount));
        }

        private static string Hipparcos(Pcg32 rng) =>
            "HIP " + Number(HipparcosNumber(rng.NextInt(StarTables.HipparcosHighest - HipparcosGaps.Length)));

        /// <summary>The <paramref name="k"/>-th (from 0) number the Hipparcos catalogue uses, skipping its gaps.</summary>
        internal static int HipparcosNumber(int k)
        {
            var n = k + 1;
            foreach (var gap in HipparcosGaps)
            {
                if (gap > n)
                {
                    break;
                }

                n++;
            }

            return n;
        }

        /// <summary>A sky position as hhmm then +ddmm (or +ddd with one digit of minutes).</summary>
        private static string Coordinates(Pcg32 rng, int declinationMinuteDigits)
        {
            var ra = Number(rng.NextInt(24), 2) + Number(rng.NextInt(60), 2);
            var sign = rng.Chance(1, 2) ? "+" : "-";
            var dec = Number(rng.NextInt(90), 2) + (declinationMinuteDigits == 2 ? Number(rng.NextInt(60), 2) : Number(rng.NextInt(10)));
            return ra + sign + dec;
        }

        private static string Number(int n) => n.ToString(CultureInfo.InvariantCulture);

        private static string Number(int n, int digits) => n.ToString(CultureInfo.InvariantCulture).PadLeft(digits, '0');

        private static int[] ParseGaps()
        {
            var parts = StarTables.HipparcosGaps.Split(',');
            var gaps = new int[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                gaps[i] = int.Parse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture);
            }

            return gaps;
        }
    }
}
