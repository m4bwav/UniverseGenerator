#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// Rolls stars (plan ideas 12, 14, 15; D18). Properties derive from the class, and main-sequence mass bands are chosen
    /// so the derived temperature lands in the class's own band (ai-docs/notes/2026-10-02-star-system-level-design.md).
    /// </summary>
    internal static class StarGenerator
    {
        // Order of every weight table below.
        internal static readonly StarClass[] Classes =
        {
            StarClass.M, StarClass.K, StarClass.G, StarClass.F, StarClass.A, StarClass.B, StarClass.O,
            StarClass.WhiteDwarf, StarClass.Giant, StarClass.Supergiant, StarClass.NeutronStar, StarClass.BlackHole,
        };

        internal static readonly int[] GameYoung = { 260, 170, 140, 110, 90, 40, 10, 20, 25, 15, 6, 3 };
        internal static readonly int[] GameMature = { 300, 200, 150, 90, 50, 15, 3, 60, 50, 8, 10, 5 };
        internal static readonly int[] GameOld = { 320, 210, 140, 70, 25, 4, 1, 110, 80, 4, 14, 8 };
        internal static readonly int[] Plausible = { 7250, 1290, 590, 310, 60, 4, 0, 590, 40, 1, 10, 2 };

        // Percent of stars of each class with a companion (Duchene and Kraus 2013, rounded for play).
        private static readonly int[] s_binaryPercent = { 25, 35, 45, 50, 65, 70, 75, 20, 30, 50, 15, 20 };

        private static readonly StarClass[] s_companionClasses = { StarClass.M, StarClass.K, StarClass.G, StarClass.WhiteDwarf };
        private static readonly int[] s_companionWeights = { 60, 25, 10, 5 };

        public static (Star Star, Companion? Companion) Roll(Pcg32 rng, StellarAge age, GeneratorOptions options)
        {
            var tables = GeneratorTables.For(options);
            var weights = tables.Weights(options.StarMix == StarMix.Plausible ? GeneratorTables.StarPlausible
                : age == StellarAge.Young ? GeneratorTables.StarYoung
                : age == StellarAge.Old ? GeneratorTables.StarOld
                : GeneratorTables.StarMature);
            var index = rng.Weighted(weights);
            var star = Make(Classes[index], rng);

            Companion? companion = null;
            if (rng.NextInt(100) < s_binaryPercent[index])
            {
                var companionClass = s_companionClasses[rng.Weighted(s_companionWeights)];
                var second = Make(companionClass, rng);
                if (second.Mass > star.Mass)
                {
                    // A companion is never the heavier star: a red dwarf under the primary's mass instead.
                    second = Make(StarClass.M, rng, Math.Min(0.47, Math.Max(0.08, star.Mass * 0.9)));
                }

                var close = rng.Chance(45, 100);
                var separation = close ? rng.Range(0.02, 0.3) : LogUniform(rng, 50, 2000);
                companion = new Companion
                {
                    Star = second,
                    Orbit = close ? CompanionOrbit.Close : CompanionOrbit.Wide,
                    Separation = DMath.Round(separation, 4),
                };
            }

            return (star, companion);
        }

        /// <summary>A star of class <paramref name="c"/>; <paramref name="maxMass"/> caps a main-sequence mass band.</summary>
        public static Star Make(StarClass c, Pcg32 rng, double maxMass = double.MaxValue)
        {
            double mass, luminosity, radius;
            switch (c)
            {
                case StarClass.Giant:
                    mass = rng.Range(0.9, 4.0);
                    luminosity = LogUniform(rng, 40, 600);
                    radius = rng.Range(10.0, 60.0);
                    break;
                case StarClass.Supergiant:
                    mass = rng.Range(10.0, 30.0);
                    luminosity = LogUniform(rng, 2e4, 2e5);
                    radius = LogUniform(rng, 30, 800);
                    break;
                case StarClass.WhiteDwarf:
                    mass = rng.Range(0.5, 1.2);
                    luminosity = LogUniform(rng, 0.0005, 0.01);
                    radius = 0.012;
                    break;
                case StarClass.NeutronStar:
                    mass = rng.Range(1.3, 2.1);
                    luminosity = 0.00001;
                    radius = 0.000015;
                    break;
                case StarClass.BlackHole:
                    mass = rng.Range(5.0, 20.0);
                    luminosity = 0;
                    radius = 0.0000042 * mass; // the event horizon, 2.95 km per solar mass
                    break;
                default:
                    var (low, high) = MassBand(c);
                    mass = LogUniform(rng, low, Math.Min(high, Math.Max(low, maxMass)));
                    luminosity = mass < 0.43 ? 0.23 * DMath.Pow(mass, 2.3) : mass < 2 ? DMath.Pow(mass, 4) : 1.4 * DMath.Pow(mass, 3.5);
                    radius = DMath.Pow(mass, 0.8);
                    break;
            }

            var temperature = luminosity <= 0 ? 0 : 5778 * DMath.Pow(luminosity / (radius * radius), 0.25);
            mass = DMath.Round(mass, 3);
            luminosity = DMath.Round(luminosity, 5);
            radius = DMath.Round(radius, 6);
            temperature = DMath.Round(temperature, 0);
            return new Star
            {
                Class = c,
                SpectralType = SpectralType(c, temperature),
                Mass = mass,
                Luminosity = luminosity,
                Radius = radius,
                Temperature = temperature,
                Colour = Colour(c, temperature),
                Description = Describe(c, temperature),
            };
        }

        /// <summary>Main-sequence mass bands (Suns) in which T = 5778 (L / R²)^0.25 stays inside the class's temperature band.</summary>
        private static (double Low, double High) MassBand(StarClass c)
        {
            switch (c)
            {
                case StarClass.O: return (27, 60);
                case StarClass.B: return (2.7, 26);
                case StarClass.A: return (1.55, 2.65);
                case StarClass.F: return (1.07, 1.54);
                case StarClass.G: return (0.84, 1.06);
                case StarClass.K: return (0.48, 0.83);
                default: return (0.08, 0.47);
            }
        }

        private static double LogUniform(Pcg32 rng, double low, double high) =>
            low * DMath.Exp(rng.NextDouble() * DMath.Log(high / low));

        private static (char Letter, double Low, double High) TemperatureBand(double t)
        {
            if (t >= 30000)
            {
                return ('O', 30000, 50000);
            }

            if (t >= 10000)
            {
                return ('B', 10000, 30000);
            }

            if (t >= 7500)
            {
                return ('A', 7500, 10000);
            }

            if (t >= 6000)
            {
                return ('F', 6000, 7500);
            }

            if (t >= 5200)
            {
                return ('G', 5200, 6000);
            }

            return t >= 3700 ? ('K', 3700, 5200) : ('M', 2400, 3700);
        }

        private static string SpectralType(StarClass c, double t)
        {
            switch (c)
            {
                case StarClass.NeutronStar:
                case StarClass.BlackHole:
                    return "";
                case StarClass.WhiteDwarf:
                    return "DA" + Digit((int)Math.Floor(50400 / t + 0.5), 1, 9);
            }

            var (letter, low, high) = TemperatureBand(t);
            var digit = Digit((int)Math.Floor(10 * (high - t) / (high - low)), 0, 9);
            var luminosityClass = c == StarClass.Giant ? "III" : c == StarClass.Supergiant ? "I" : "V";
            return letter + digit + luminosityClass;
        }

        private static string Digit(int value, int min, int max) => ((char)('0' + DMath.Clamp(value, min, max))).ToString();

        private static string Describe(StarClass c, double t)
        {
            switch (c)
            {
                case StarClass.O: return "blue star";
                case StarClass.B: return "blue-white star";
                case StarClass.A: return "white star";
                case StarClass.F: return "yellow-white star";
                case StarClass.G: return "yellow star";
                case StarClass.K: return "orange dwarf";
                case StarClass.M: return "red dwarf";
                case StarClass.WhiteDwarf: return "white dwarf";
                case StarClass.Giant: return t < 5200 ? "red giant" : "yellow giant";
                case StarClass.Supergiant: return t >= 10000 ? "blue supergiant" : t >= 5200 ? "yellow supergiant" : "red supergiant";
                case StarClass.NeutronStar: return "neutron star";
                default: return "black hole";
            }
        }

        private static string Colour(StarClass c, double t)
        {
            if (c == StarClass.BlackHole)
            {
                return "#7a4dff";
            }

            if (c == StarClass.NeutronStar)
            {
                return "#e0f0ff";
            }

            if (t > 30000)
            {
                return "#9bb0ff";
            }

            if (t > 10000)
            {
                return "#aabfff";
            }

            if (t > 7500)
            {
                return "#cad7ff";
            }

            if (t > 6000)
            {
                return "#f8f7ff";
            }

            if (t > 5200)
            {
                return "#fff4ea";
            }

            return t > 3700 ? "#ffd2a1" : "#ffad51";
        }
    }
}
