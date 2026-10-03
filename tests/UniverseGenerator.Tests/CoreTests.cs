using System;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    public class Pcg32Tests
    {
        [Test]
        public void Matches_the_reference_pcg32_demo_for_seed_42_stream_54()
        {
            // pcg32-demo.c from pcg-random.org, pcg32_srandom_r(&rng, 42u, 54u); checked against an independent Python port.
            var rng = new Pcg32(42, 54);
            var expected = new uint[] { 0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e };
            Assert.That(expected.Select(_ => rng.NextUInt()), Is.EqualTo(expected));
        }

        [Test]
        public void NextInt_stays_in_bounds_and_reaches_every_value()
        {
            var rng = new Pcg32(7, 1);
            var seen = new int[7];
            for (var i = 0; i < 7000; i++)
            {
                seen[rng.NextInt(7)]++;
            }

            Assert.That(seen, Has.All.InRange(850, 1150));
        }

        [Test]
        public void NextInt_refuses_a_bound_that_is_not_positive()
        {
            Assert.That(() => new Pcg32(1, 1).NextInt(0), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Range_includes_both_ends()
        {
            var rng = new Pcg32(3, 3);
            var values = Enumerable.Range(0, 500).Select(_ => rng.Range(-2, 2)).Distinct().OrderBy(v => v);
            Assert.That(values, Is.EqualTo(new[] { -2, -1, 0, 1, 2 }));
        }

        [Test]
        public void NextDouble_is_in_the_unit_interval()
        {
            var rng = new Pcg32(5, 9);
            Assert.That(Enumerable.Range(0, 10000).Select(_ => rng.NextDouble()), Has.All.GreaterThanOrEqualTo(0.0).And.LessThan(1.0));
        }

        [Test]
        public void Weighted_never_picks_a_zero_weight()
        {
            var rng = new Pcg32(11, 2);
            var picks = Enumerable.Range(0, 2000).Select(_ => rng.Weighted(new[] { 3, 0, 1 })).ToList();
            Assert.That(picks, Has.None.EqualTo(1));
            Assert.That(picks.Count(p => p == 0), Is.InRange(1350, 1650));
        }
    }

    public class SeedsTests
    {
        [Test]
        public void SplitMix64_matches_the_reference_outputs()
        {
            // The first two outputs of Vigna's splitmix64.c from state 0.
            Assert.That(Seeds.SplitMix64(0), Is.EqualTo(0xe220a8397b1dcdafUL));
            Assert.That(Seeds.SplitMix64(0x9E3779B97F4A7C15UL), Is.EqualTo(0x6e789e6aa1b965f4UL));
        }

        [TestCase("", 0xcbf29ce484222325UL)]
        [TestCase("a", 0xaf63dc4c8601ec8cUL)]
        [TestCase("foobar", 0x85944171f73967e8UL)]
        [TestCase("my-seed", 0xc06b1c9800757c1bUL)]
        public void Fnv1a_matches_the_reference_for_ascii(string text, ulong expected)
        {
            Assert.That(Seeds.Fnv1a(text), Is.EqualTo(expected));
        }

        [Test]
        public void Fnv1a_hashes_utf8_bytes()
        {
            // Values from Python: FNV-1a 64 over text.encode("utf-8").
            Assert.That(Seeds.Fnv1a(new string((char)0xE9, 1)), Is.EqualTo(0x0ac21707b7181e01UL));
            Assert.That(Seeds.Fnv1a(new string((char)0x661F, 1)), Is.EqualTo(0x3347e51b7469c084UL));
            Assert.That(Seeds.Fnv1a(new string(new[] { (char)0xD83D, (char)0xDE80 })), Is.EqualTo(0xff06d33875097bdaUL));
        }

        [Test]
        public void Fnv1a_reads_a_lone_surrogate_as_the_replacement_character()
        {
            Assert.That(Seeds.Fnv1a(new string((char)0xD83D, 1)), Is.EqualTo(0x6f6d661b9658624aUL));
            Assert.That(Seeds.Fnv1a(new string((char)0xFFFD, 1)), Is.EqualTo(0x6f6d661b9658624aUL));
        }

        [Test]
        public void FromText_mixes_in_the_generator_version()
        {
            Assert.That(Seeds.FromText("my-seed", 1), Is.EqualTo(0xd7402b45d1c166c0UL));
            Assert.That(Seeds.FromText("my-seed", 2), Is.Not.EqualTo(Seeds.FromText("my-seed", 1)));
        }

        [Test]
        public void Children_and_purposes_are_independent()
        {
            var parent = Seeds.FromText("x", 1);
            Assert.That(Seeds.Child(parent, "system", 0), Is.Not.EqualTo(Seeds.Child(parent, "system", 1)));
            Assert.That(Seeds.Child(parent, "system", 0), Is.Not.EqualTo(Seeds.Child(parent, "planet", 0)));
            Assert.That(Seeds.Stream(parent, "star").NextUInt(), Is.Not.EqualTo(Seeds.Stream(parent, "names").NextUInt()));
        }

        [Test]
        public void Negative_and_positive_texts_differ()
        {
            // The game's seeded System.Random folded the sign: s and -s gave the same galaxy (plan, what the game gets wrong, 8).
            Assert.That(Seeds.FromText("42", 1), Is.Not.EqualTo(Seeds.FromText("-42", 1)));
        }
    }

    public class AddressTests
    {
        [Test]
        public void Formats_as_the_plan_shows()
        {
            var a = new Address(1, "my-seed", "galaxy").Child("system", 31).Child("planet", 2);
            Assert.That(a.ToString(), Is.EqualTo("v1-my-seed/galaxy/system/31/planet/2"));
        }

        [Test]
        public void The_root_seed_is_the_galaxy_level_child_of_the_text_seed()
        {
            Assert.That(new Address(1, "my-seed", "galaxy").ObjectSeed, Is.EqualTo(0xb33df41276b22fbdUL));
        }

        [TestCase("my-seed")]
        [TestCase("Hello World/2")]
        [TestCase("100%")]
        [TestCase("")]
        [TestCase("-42")]
        [TestCase("a.b_c~d")]
        public void Round_trips_any_seed_text(string seed)
        {
            var a = new Address(1, seed, "system").Child("planet", 0);
            Assert.That(Address.TryParse(a.ToString(), out var back, out var error), Is.True, error);
            Assert.That(back!.Seed, Is.EqualTo(seed));
            Assert.That(back.ToString(), Is.EqualTo(a.ToString()));
            Assert.That(back.ObjectSeed, Is.EqualTo(a.ObjectSeed));
        }

        [Test]
        public void Escapes_reserved_and_non_ascii_characters_as_utf8()
        {
            var seed = "a b/" + new string((char)0xE9, 1);
            Assert.That(new Address(1, seed, "galaxy").ToString(), Is.EqualTo("v1-a%20b%2F%C3%A9/galaxy"));
        }

        [Test]
        public void Accepts_lowercase_percent_escapes()
        {
            Assert.That(Address.TryParse("v1-a%2fb/galaxy", out var a, out _), Is.True);
            Assert.That(a!.Seed, Is.EqualTo("a/b"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("my-seed/galaxy")]
        [TestCase("v0-x/galaxy")]
        [TestCase("v1-x")]
        [TestCase("v1-x/Galaxy")]
        [TestCase("v1-x/galaxy/system")]
        [TestCase("v1-x/galaxy/system/07")]
        [TestCase("v1-x/galaxy/system/-1")]
        [TestCase("v1-x/galaxy/system/99999999999")]
        [TestCase("v1-a b/galaxy")]
        [TestCase("v1-%G0/galaxy")]
        [TestCase("v1-%2/galaxy")]
        [TestCase("v1-%C3/galaxy")]
        public void Refuses_a_malformed_address_with_a_reason(string? text)
        {
            Assert.That(Address.TryParse(text, out var a, out var error), Is.False);
            Assert.That(a, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void Refuses_a_label_that_is_not_lowercase_letters()
        {
            Assert.That(() => new Address(1, "x", "galaxy").Child("system2", 0), Throws.ArgumentException);
            Assert.That(() => new Address(1, "x", "galaxy").Child("system", -1), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }
    }

    public class DMathTests
    {
        private static readonly double[] s_points = Enumerable.Range(-400, 801).Select(i => i / 37.0).ToArray();

        [Test]
        public void Exp_agrees_with_Math_Exp()
        {
            foreach (var x in s_points)
            {
                Assert.That(DMath.Exp(x), Is.EqualTo(Math.Exp(x)).Within(2e-15 * Math.Exp(x)), $"x = {x:R}");
            }
        }

        [Test]
        public void Log_agrees_with_Math_Log()
        {
            foreach (var x in s_points.Select(Math.Abs).Where(v => v > 0).Concat(new[] { 1e-300, 4.9e-324, 1e300, 0.5, 2 }))
            {
                Assert.That(DMath.Log(x), Is.EqualTo(Math.Log(x)).Within(2e-15 * Math.Max(1, Math.Abs(Math.Log(x)))), $"x = {x:R}");
            }
        }

        [Test]
        public void Pow_agrees_with_Math_Pow_for_positive_bases()
        {
            foreach (var x in new[] { 0.08, 0.45, 1, 1.4, 2.1, 16, 40 })
            {
                foreach (var y in new[] { -2, 0.25, 0.8, 1.0 / 3, 2.3, 3.5, 4 })
                {
                    Assert.That(DMath.Pow(x, y), Is.EqualTo(Math.Pow(x, y)).Within(1e-13 * Math.Pow(x, y)), $"{x}^{y}");
                }
            }
        }

        [Test]
        public void Sin_Cos_and_Atan2_agree_with_Math()
        {
            foreach (var x in s_points)
            {
                Assert.That(DMath.Sin(x), Is.EqualTo(Math.Sin(x)).Within(1e-14), $"sin {x:R}");
                Assert.That(DMath.Cos(x), Is.EqualTo(Math.Cos(x)).Within(1e-14), $"cos {x:R}");
                Assert.That(DMath.Atan2(x, 1.5), Is.EqualTo(Math.Atan2(x, 1.5)).Within(1e-14), $"atan2 {x:R}, 1.5");
                Assert.That(DMath.Atan2(x, -0.7), Is.EqualTo(Math.Atan2(x, -0.7)).Within(1e-14), $"atan2 {x:R}, -0.7");
            }

            Assert.That(DMath.Atan2(0, 0), Is.EqualTo(0));
            Assert.That(DMath.Atan2(1, 0), Is.EqualTo(Math.PI / 2));
        }

        [Test]
        public void Edge_values_never_reach_an_undefined_conversion()
        {
            Assert.That(DMath.Exp(double.NaN), Is.NaN);
            Assert.That(DMath.Exp(-1000), Is.EqualTo(0));
            Assert.That(DMath.Exp(1000), Is.EqualTo(DMath.Exp(700)));
            Assert.That(DMath.Log(0), Is.EqualTo(double.NegativeInfinity));
            Assert.That(DMath.Log(double.PositiveInfinity), Is.EqualTo(double.PositiveInfinity));
            Assert.That(DMath.Sin(double.PositiveInfinity), Is.NaN);
            Assert.That(DMath.Atan(double.NegativeInfinity), Is.EqualTo(-DMath.PI / 2));
            Assert.That(DMath.Pow(0, 2), Is.EqualTo(0));
        }

        [TestCase(2.5, 0, 3.0)]
        [TestCase(-2.5, 0, -3.0)]
        [TestCase(0.125, 2, 0.13)]
        [TestCase(1.005, 2, 1.0)]
        [TestCase(123.456789, 4, 123.4568)]
        [TestCase(-0.0001, 2, 0.0)]
        public void Round_goes_half_away_from_zero(double x, int decimals, double expected)
        {
            // 1.005 is 1.00499999999999989... in binary, so it rounds down, as Math.Round does.
            Assert.That(DMath.Round(x, decimals), Is.EqualTo(expected));
        }

        [Test]
        public void Round_never_returns_negative_zero()
        {
            Assert.That(BitConverter.DoubleToInt64Bits(DMath.Round(-0.0001, 2)), Is.EqualTo(0L));
            Assert.That(BitConverter.DoubleToInt64Bits(DMath.Round(-0.0, 3)), Is.EqualTo(0L));
        }
    }

    public class JsonWriterTests
    {
        [Test]
        public void Writes_indented_json_with_lf_and_a_final_newline()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("a").Int(1).Name("b").BeginArray().Bool(true).Null().EndArray().Name("c").BeginObject().EndObject().EndObject();
            Assert.That(w.ToString(), Is.EqualTo("{\n  \"a\": 1,\n  \"b\": [\n    true,\n    null\n  ],\n  \"c\": {}\n}\n"));
        }

        [Test]
        public void Writes_compact_json()
        {
            var w = new JsonWriter(indented: false);
            w.BeginArray().Int(-3).String("x").BeginArray().EndArray().EndArray();
            Assert.That(w.ToString(), Is.EqualTo("[-3,\"x\",[]]"));
        }

        [TestCase(0.1234, 4, "0.1234")]
        [TestCase(0.1, 4, "0.1")]
        [TestCase(2.0, 3, "2")]
        [TestCase(-0.00001, 3, "0")]
        [TestCase(-1.5, 0, "-2")]
        [TestCase(-0.25, 2, "-0.25")]
        [TestCase(0.05, 2, "0.05")]
        [TestCase(359083.1234, 4, "359083.1234")]
        [TestCase(200000.0, 5, "200000")]
        [TestCase(1e-7, 15, "0.0000001")]
        public void Numbers_print_at_fixed_decimals_without_trailing_zeros(double value, int decimals, string expected)
        {
            Assert.That(new JsonWriter(false).Number(value, decimals).ToString(), Is.EqualTo(expected));
        }

        [Test]
        public void Not_a_number_is_written_as_null()
        {
            Assert.That(new JsonWriter(false).Number(double.NaN, 2).ToString(), Is.EqualTo("null"));
        }

        [Test]
        public void Escapes_quotes_backslashes_controls_and_surrogates()
        {
            const char Backslash = '\\';
            var input = "q\"b" + Backslash + "n\nt\tc" + (char)1 + "s" + (char)0xD83D;
            var expected = "\"q" + Backslash + "\"b" + Backslash + Backslash + "n" + Backslash + "nt" + Backslash + "tc"
                + Backslash + "u0001s" + Backslash + "ud83d\"";
            Assert.That(new JsonWriter(false).String(input).ToString(), Is.EqualTo(expected));
        }
    }
}
