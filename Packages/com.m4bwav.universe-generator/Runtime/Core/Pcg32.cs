#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// PCG32 (XSH RR; O'Neill 2014, pcg-random.org): 64-bit state, 32-bit output. Every object and purpose draws from
    /// its own instance (<see cref="Seeds.Stream"/>), so the same seed gives the same draws on every runtime.
    /// </summary>
    internal sealed class Pcg32
    {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private readonly ulong _increment;

        /// <summary>The reference <c>pcg32_srandom_r(initstate, initseq)</c>.</summary>
        public Pcg32(ulong seed, ulong stream)
        {
            _increment = (stream << 1) | 1UL;
            _state = 0;
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        public uint NextUInt()
        {
            var old = _state;
            _state = unchecked(old * Multiplier + _increment);
            var xorShifted = (uint)(((old >> 18) ^ old) >> 27);
            var rotation = (int)(old >> 59);
            return (xorShifted >> rotation) | (xorShifted << (-rotation & 31));
        }

        /// <summary>An unbiased integer in [0, <paramref name="bound"/>) by Lemire's method.</summary>
        public int NextInt(int bound)
        {
            if (bound <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bound), bound, "The bound must be positive.");
            }

            var b = (uint)bound;
            var m = (ulong)NextUInt() * b;
            var low = (uint)m;
            if (low < b)
            {
                var threshold = unchecked(0u - b) % b;
                while (low < threshold)
                {
                    m = (ulong)NextUInt() * b;
                    low = (uint)m;
                }
            }

            return (int)(m >> 32);
        }

        /// <summary>An integer from <paramref name="min"/> to <paramref name="max"/>, both included.</summary>
        public int Range(int min, int max) => min + NextInt(max - min + 1);

        /// <summary>A double in [0, 1) from 53 random bits; always two draws.</summary>
        public double NextDouble()
        {
            ulong high = NextUInt() >> 5, low = NextUInt() >> 6;
            return (high * 67108864.0 + low) / 9007199254740992.0;
        }

        /// <summary>A double in [<paramref name="min"/>, <paramref name="max"/>).</summary>
        public double Range(double min, double max) => min + (max - min) * NextDouble();

        /// <summary>True with probability <paramref name="numerator"/> / <paramref name="denominator"/>, decided on integers.</summary>
        public bool Chance(int numerator, int denominator) => NextInt(denominator) < numerator;

        /// <summary>An index into integer weights, each chosen in proportion to its weight.</summary>
        public int Weighted(int[] weights)
        {
            var total = 0;
            foreach (var w in weights)
            {
                total += w;
            }

            var roll = NextInt(total);
            for (var i = 0; i < weights.Length; i++)
            {
                if (roll < weights[i])
                {
                    return i;
                }

                roll -= weights[i];
            }

            return weights.Length - 1;
        }
    }
}
