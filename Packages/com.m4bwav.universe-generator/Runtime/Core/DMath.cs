#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// Deterministic maths (plan D5): built from IEEE 754 + - * /, square root and floor, which are exact or correctly
    /// rounded on every runtime. <c>Math.Exp</c>, <c>Sin</c>, <c>Pow</c>, <c>Log</c> and <c>Atan2</c> call each
    /// platform's C library and may differ in the last bit between Windows, Linux, macOS, Mono and IL2CPP.
    /// </summary>
    internal static class DMath
    {
        public const double PI = 3.141592653589793;
        private const double Ln2 = 0.6931471805599453;
        private const double Sqrt2 = 1.4142135623730951;

        private static readonly double[] s_powersOfTen =
        {
            1, 10, 100, 1e3, 1e4, 1e5, 1e6, 1e7, 1e8, 1e9, 1e10, 1e11, 1e12, 1e13, 1e14, 1e15,
        };

        /// <summary>e to the power x; 0 below -700 and capped at e^700.</summary>
        public static double Exp(double x)
        {
            if (double.IsNaN(x))
            {
                return x;
            }

            if (x < -700)
            {
                return 0;
            }

            if (x > 700)
            {
                x = 700;
            }

            var kd = Math.Floor(x / Ln2 + 0.5);
            var r = x - kd * Ln2;
            double term = 1, sum = 1;
            for (var i = 1; i <= 16; i++)
            {
                term = term * r / i;
                sum += term;
            }

            return sum * BitConverter.Int64BitsToDouble((long)((int)kd + 1023) << 52);
        }

        /// <summary>The natural logarithm; negative infinity for x at or below 0.</summary>
        public static double Log(double x)
        {
            if (double.IsNaN(x))
            {
                return x;
            }

            if (x <= 0)
            {
                return double.NegativeInfinity;
            }

            if (double.IsPositiveInfinity(x))
            {
                return x;
            }

            var bits = BitConverter.DoubleToInt64Bits(x);
            var exponent = (int)((bits >> 52) & 0x7FF) - 1023;
            if (exponent == -1023)
            {
                // Subnormal: scale into the normal range first.
                return Log(x * 4503599627370496.0) - 52 * Ln2;
            }

            var m = BitConverter.Int64BitsToDouble((bits & 0xFFFFFFFFFFFFFL) | 0x3FF0000000000000L);
            if (m > Sqrt2)
            {
                m /= 2;
                exponent++;
            }

            double s = (m - 1) / (m + 1), s2 = s * s, term = s, sum = 0;
            for (var i = 1; i <= 41; i += 2)
            {
                sum += term / i;
                term *= s2;
            }

            return 2 * sum + exponent * Ln2;
        }

        /// <summary>x to the power y for x above 0; 0 for x at or below 0 (generation never needs a negative base).</summary>
        public static double Pow(double x, double y) => x <= 0 ? 0 : Exp(y * Log(x));

        public static double Sin(double x)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                return double.NaN;
            }

            const double TwoPi = 2 * PI;
            const double HalfPi = PI / 2;
            x -= Math.Floor(x / TwoPi + 0.5) * TwoPi;
            var q = (int)Math.Floor(x / HalfPi + 0.5);
            var r = x - q * HalfPi;
            var r2 = r * r;
            // Taylor series to r^21 and r^20 on |r| <= pi/4: the first omitted term is under 1e-21.
            var s = r * (1 - r2 / 6 * (1 - r2 / 20 * (1 - r2 / 42 * (1 - r2 / 72 * (1 - r2 / 110 * (1 - r2 / 156
                * (1 - r2 / 210 * (1 - r2 / 272 * (1 - r2 / 342 * (1 - r2 / 420))))))))));
            var c = 1 - r2 / 2 * (1 - r2 / 12 * (1 - r2 / 30 * (1 - r2 / 56 * (1 - r2 / 90 * (1 - r2 / 132
                * (1 - r2 / 182 * (1 - r2 / 240 * (1 - r2 / 306 * (1 - r2 / 380)))))))));
            switch (((q % 4) + 4) % 4)
            {
                case 0: return s;
                case 1: return c;
                case 2: return -s;
                default: return -c;
            }
        }

        public static double Cos(double x) => Sin(x + PI / 2);

        /// <summary>The arc tangent: two half-angle reductions, then the Taylor series.</summary>
        public static double Atan(double z)
        {
            if (double.IsNaN(z))
            {
                return z;
            }

            if (double.IsInfinity(z))
            {
                return z > 0 ? PI / 2 : -PI / 2;
            }

            var invert = Math.Abs(z) > 1;
            var t = invert ? 1 / z : z;
            t /= 1 + Math.Sqrt(1 + t * t);
            t /= 1 + Math.Sqrt(1 + t * t);
            double t2 = t * t, term = t, sum = 0;
            for (var i = 1; i <= 41; i += 2)
            {
                sum += term / i;
                term *= -t2;
            }

            var a = 4 * sum;
            if (!invert)
            {
                return a;
            }

            return (z > 0 ? PI / 2 : -PI / 2) - a;
        }

        public static double Atan2(double y, double x)
        {
            if (x > 0)
            {
                return Atan(y / x);
            }

            if (x < 0)
            {
                return y >= 0 ? Atan(y / x) + PI : Atan(y / x) - PI;
            }

            return y > 0 ? PI / 2 : y < 0 ? -PI / 2 : 0;
        }

        /// <summary>
        /// Rounds half away from zero to <paramref name="decimals"/> places (0 to 15), by scaling and <c>Math.Floor</c>
        /// rather than <c>Math.Round(x, n, mode)</c>, whose implementation differs between runtimes. Never returns
        /// negative zero. Values are stored rounded, so a last-bit difference never reaches output.
        /// </summary>
        public static double Round(double x, int decimals)
        {
            if (double.IsNaN(x) || double.IsInfinity(x))
            {
                return x;
            }

            var p = s_powersOfTen[decimals];
            var scaled = Math.Floor(Math.Abs(x) * p + 0.5);
            if (scaled == 0)
            {
                return 0;
            }

            var result = scaled / p;
            return x < 0 ? -result : result;
        }

        /// <summary>The power of ten for <paramref name="decimals"/> places, as <see cref="Round"/> uses it.</summary>
        internal static double PowerOfTen(int decimals) => s_powersOfTen[decimals];

        public static double Clamp(double x, double min, double max) => x < min ? min : x > max ? max : x;

        public static int Clamp(int x, int min, int max) => x < min ? min : x > max ? max : x;
    }
}
