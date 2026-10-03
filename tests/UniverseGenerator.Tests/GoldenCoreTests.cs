using System;
using System.Globalization;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The seed promise at its root: the raw bits of seeds, draws and deterministic maths for fixed inputs, compared with
    /// tests/Golden/v1/core.json on every runtime. Bits, not printed decimals, so a last-bit difference cannot hide.
    /// </summary>
    public class GoldenCoreTests
    {
        private static readonly string[] s_seedTexts = { "my-seed", "42", "-42", "", "Andromeda 7", "a/b c" };
        private static readonly string[] s_purposes = { "layout", "star", "planets", "names", "lanes" };

        [Test]
        public void Core_streams_and_maths_match_the_golden_file()
        {
            TestContext.Out.WriteLine($"{RuntimeInformation.FrameworkDescription}, {RuntimeInformation.ProcessArchitecture}, 64-bit process: {Environment.Is64BitProcess}");
            Repo.AssertGolden("tests/Golden/v1/core.json", Build());
        }

        private static string Build()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject();
            w.Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("seeds").BeginArray();
            foreach (var text in s_seedTexts)
            {
                var root = Seeds.FromText(text, GeneratorVersion.Current);
                var galaxy = new Address(GeneratorVersion.Current, text, "galaxy");
                var system = galaxy.Child("system", 31);
                w.BeginObject();
                w.Name("text").String(text);
                w.Name("fromText").String(Hex(root));
                w.Name("galaxy").String(galaxy.ToString()).Name("galaxySeed").String(Hex(galaxy.ObjectSeed));
                w.Name("system").String(system.ToString()).Name("systemSeed").String(Hex(system.ObjectSeed));
                w.Name("streams").BeginObject();
                foreach (var purpose in s_purposes)
                {
                    var rng = Seeds.Stream(system.ObjectSeed, purpose);
                    w.Name(purpose).BeginArray();
                    w.String(Hex(rng.NextUInt())).String(Hex(rng.NextUInt()));
                    w.Int(rng.NextInt(1000)).Int(rng.Range(-5, 5)).Int(rng.Weighted(new[] { 30, 21, 14, 10, 7, 6, 5, 3 }));
                    w.String(Bits(rng.NextDouble())).String(Bits(rng.Range(0.08, 0.45)));
                    w.Bool(rng.Chance(1, 3));
                    w.EndArray();
                }

                w.EndObject();
                w.EndObject();
            }

            w.EndArray();

            w.Name("math").BeginArray();
            for (var i = -12; i <= 12; i++)
            {
                var x = i * 0.73 + 0.011;
                var positive = Math.Abs(x) + 0.05;
                w.BeginArray();
                w.String(Bits(x));
                w.String(Bits(DMath.Exp(x))).String(Bits(DMath.Log(positive))).String(Bits(DMath.Pow(positive, 2.3)));
                w.String(Bits(DMath.Sin(x))).String(Bits(DMath.Cos(x))).String(Bits(DMath.Atan2(x, 0.37)));
                w.String(Bits(Math.Sqrt(positive))).String(Bits(DMath.Round(x * 1234.5678, 3)));
                w.EndArray();
            }

            w.EndArray();
            w.EndObject();
            return w.ToString();
        }

        private static string Hex(ulong v) => v.ToString("x16", CultureInfo.InvariantCulture);

        private static string Hex(uint v) => v.ToString("x8", CultureInfo.InvariantCulture);

        private static string Bits(double v) => BitConverter.DoubleToInt64Bits(v).ToString("x16", CultureInfo.InvariantCulture);
    }
}
