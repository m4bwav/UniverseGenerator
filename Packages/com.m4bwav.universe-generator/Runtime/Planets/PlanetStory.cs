#nullable enable
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// A planet's traits and anomalies (P1), hazards (P4) and summary line (P2). A trait is offered only when the data
    /// supports it and a hazard only follows from the data, so text never contradicts the generated facts.
    /// </summary>
    internal static class PlanetStory
    {
        private static readonly string[] s_surfaceAnomalies =
        {
            "a perfectly hexagonal crater", "gravity readings that do not match its mass", "a signal from beneath the surface",
            "a hollow the size of a moon under the crust", "auroras that repeat like writing", "a surface that rearranges itself overnight",
            "a region where compasses spin", "ruins no survey can date",
        };

        private static readonly string[] s_giantAnomalies =
        {
            "gravity readings that do not match its mass", "a signal from deep inside", "auroras that repeat like writing",
            "a storm that has not moved in a thousand years", "a perfectly square cloud", "something the size of a moon hiding in the clouds",
        };

        public static string[] Traits(Pcg32 rng, Planet p)
        {
            var k = p.Kind;
            var giant = PlanetDetail.IsGiant(k);
            var a = p.Atmosphere;
            var air = !giant && (a.Pressure ?? 0) >= 0.01;
            var locked = p.Spin == Spin.Locked;
            var folded = p.Tilt > 90 ? 180 - p.Tilt : p.Tilt;
            bool Is(params PlanetKind[] kinds) => System.Array.IndexOf(kinds, k) >= 0;

            // Every trait in a fixed order with its weight; a weight of 0 means the data does not support it.
            var candidates = new (string Trait, int Weight)[]
            {
                ("global dust storms", k == PlanetKind.Desert ? 10 : 0),
                ("salt flats", k == PlanetKind.Desert ? 6 : 0),
                ("glass plains", !giant && p.Zone == OrbitZone.Hot && k != PlanetKind.Lava ? 6 : 0),
                ("lava oceans", k == PlanetKind.Lava ? 10 : 0),
                ("volcanic chains", Is(PlanetKind.Lava, PlanetKind.Iron, PlanetKind.Rocky, PlanetKind.Greenhouse) ? 5 : 0),
                ("a metal-rich crust", k == PlanetKind.Iron ? 10 : 0),
                ("an ancient impact basin", !giant ? 3 : 0),
                ("deep canyons", Is(PlanetKind.Rocky, PlanetKind.Desert, PlanetKind.Barren, PlanetKind.Iron, PlanetKind.Ice) ? 5 : 0),
                ("acid clouds", k == PlanetKind.Greenhouse ? 10 : 0),
                ("superrotating clouds", k == PlanetKind.Greenhouse ? 6 : 0),
                ("ice geysers", Is(PlanetKind.Ice, PlanetKind.Dwarf) && (p.Zone == OrbitZone.Cold || p.Zone == OrbitZone.Outer) ? 8 : 0),
                ("an ocean under the ice", k == PlanetKind.Ice ? 8 : 0),
                ("methane lakes", air && PlanetDetail.Has(a.Gases, Gas.Methane) ? 10 : 0),
                ("nitrogen glaciers", Is(PlanetKind.Ice, PlanetKind.Dwarf) && PlanetDetail.Has(a.Gases, Gas.Nitrogen) ? 6 : 0),
                ("scattered archipelagos", p.Water > 0 && p.Water < 0.95 ? 6 : 0),
                ("an endless ocean", p.Water >= 0.9 ? 10 : 0),
                ("glowing seas", p.Water > 0 && p.Life >= LifeLevel.Simple ? 6 : 0),
                ("towering forests", p.Flora >= 4 ? 8 : 0),
                ("great herds", p.Fauna >= 4 ? 8 : 0),
                ("fungal jungles", k == PlanetKind.Garden ? 4 : 0),
                ("an eyeball climate", !giant && locked && (p.Water > 0 || p.Ice > 0) ? 10 : 0),
                ("a twilight band", !giant && locked && air ? 8 : 0),
                ("days longer than its year", !giant && p.Spin == Spin.Free && p.Rotation > p.Period * 24 ? 8 : 0),
                ("long days", !giant && p.Rotation >= 200 && p.Rotation <= p.Period * 24 ? 6 : 0),
                ("a fast spin", !giant && p.Rotation < 10 ? 5 : 0),
                ("extreme seasons", !giant && folded >= 40 ? 8 : 0),
                ("spins on its side", p.Tilt >= 60 && p.Tilt <= 120 ? 10 : 0),
                ("spins backwards", p.Tilt > 120 ? 6 : 0),
                ("auroras", (air || giant) && p.Mass >= 0.5 ? 3 : 0),
                ("a great storm", Is(PlanetKind.GasGiant, PlanetKind.IceGiant, PlanetKind.SubNeptune) ? 8 : 0),
                ("banded clouds", Is(PlanetKind.GasGiant, PlanetKind.HotJupiter) ? 8 : 0),
                ("diamond rain", k == PlanetKind.IceGiant ? 6 : 0),
                ("a glowing night side", k == PlanetKind.HotJupiter ? 10 : 0),
                ("an evaporating atmosphere", k == PlanetKind.HotJupiter ? 8 : 0),
                ("bright rings", p.Rings ? 8 : 0),
                ("many moons", p.Moons.Count >= 5 ? 6 : 0),
                ("a thick haze", (k == PlanetKind.Ice && (a.Pressure ?? 0) >= 0.5) || k == PlanetKind.SubNeptune ? 6 : 0),
                ("steam skies", k == PlanetKind.SubNeptune && p.Temperature >= 400 ? 8 : 0),
                ("tidal flexing", p.Spin == Spin.Resonant ? 6 : 0),
            };

            var weights = new int[candidates.Length];
            for (var i = 0; i < candidates.Length; i++)
            {
                weights[i] = candidates[i].Weight;
            }

            var first = rng.Weighted(weights);
            var wantSecond = rng.Chance(40, 100);
            weights[first] = 0;
            var any = false;
            foreach (var w in weights)
            {
                any |= w > 0;
            }

            if (!wantSecond || !any)
            {
                return new[] { candidates[first].Trait };
            }

            return new[] { candidates[first].Trait, candidates[rng.Weighted(weights)].Trait };
        }

        public static string? Anomaly(Pcg32 rng, bool giant, int weirdness)
        {
            if (!rng.Chance(weirdness, 100))
            {
                return null;
            }

            var list = giant ? s_giantAnomalies : s_surfaceAnomalies;
            return list[rng.NextInt(list.Length)];
        }

        /// <summary>The hazards in words, worst first, and the worst tier (1 when there is none). No draws.</summary>
        public static (int Hazard, string[] Hazards) Hazards(Planet p, Star star)
        {
            var found = new List<(string Text, int Tier)>();
            var c = star.Class;
            if (c == StarClass.O || c == StarClass.B || c == StarClass.NeutronStar)
            {
                found.Add(("hard radiation", 4));
            }
            else if (c == StarClass.M && p.Zone <= OrbitZone.Temperate)
            {
                found.Add(("stellar flares", 3));
            }

            var a = p.Atmosphere;
            if (a.Pressure is null)
            {
                found.Add(("no surface", 5));
            }
            else
            {
                var pressure = a.Pressure.Value;
                if (p.Kind == PlanetKind.Lava)
                {
                    found.Add(("molten rock", 5));
                }

                if (pressure >= 10)
                {
                    found.Add(("crushing pressure", 5));
                }
                else if (pressure < 0.01)
                {
                    found.Add(("vacuum", 3));
                    if (p.Zone <= OrbitZone.Warm && c != StarClass.O && c != StarClass.B && c != StarClass.NeutronStar)
                    {
                        found.Add(("radiation", 3));
                    }
                }
                else if (!a.Breathable)
                {
                    found.Add(("unbreathable air", 2));
                }

                if (pressure >= 3 && pressure < 10)
                {
                    found.Add(("heavy air", 2));
                }
                else if (pressure >= 0.01 && pressure < 0.5)
                {
                    found.Add(("thin air", 2));
                }

                if (p.DayTemperature >= 500)
                {
                    found.Add(("searing heat", 5));
                }
                else if (p.DayTemperature >= 330)
                {
                    found.Add(("extreme heat", 3));
                }

                if (p.NightTemperature < 180)
                {
                    found.Add(("deep cold", 3));
                }
                else if (p.NightTemperature < 250)
                {
                    found.Add(("cold", 2));
                }

                if (p.Gravity >= 2.5)
                {
                    found.Add(("crushing gravity", 4));
                }
                else if (p.Gravity >= 1.5)
                {
                    found.Add(("heavy gravity", 2));
                }

                if (Has(p.Traits, "global dust storms"))
                {
                    found.Add(("dust storms", 2));
                }

                if (Has(p.Traits, "volcanic chains"))
                {
                    found.Add(("eruptions", 2));
                }

                if (p.Fauna >= 4)
                {
                    found.Add(("dangerous wildlife", 2));
                }
            }

            var worst = 1;
            foreach (var (_, tier) in found)
            {
                worst = System.Math.Max(worst, tier);
            }

            var texts = new List<string>(found.Count);
            for (var tier = 5; tier >= 2; tier--)
            {
                foreach (var (text, t) in found)
                {
                    if (t == tier)
                    {
                        texts.Add(text);
                    }
                }
            }

            return (worst, texts.ToArray());
        }

        /// <summary>"1 g, breathable air at 1.2 bar, 17 °C, 64% ocean, mostly temperate forest, complex life; hazard 1".</summary>
        public static string Summary(Planet p)
        {
            var parts = new List<string> { JsonWriter.Format(p.Gravity, p.Gravity < 10 ? 1 : 0) + " g" };
            var a = p.Atmosphere;
            switch (a.Class)
            {
                case AtmosphereClass.Envelope: parts.Add("a deep envelope of " + GasList(a.Gases)); break;
                case AtmosphereClass.None: parts.Add("no air"); break;
                case AtmosphereClass.Trace: parts.Add("a trace of " + GasName(a.Gases[0])); break;
                default:
                    var pressure = a.Pressure ?? 0;
                    var decimals = pressure >= 10 ? 0 : pressure >= 1 ? 1 : pressure >= 0.1 ? 2 : 3;
                    var kind = a.Breathable ? "breathable"
                        : (a.Class == AtmosphereClass.Thin ? "thin " : a.Class == AtmosphereClass.Dense ? "dense " : a.Class == AtmosphereClass.Crushing ? "crushing " : "") + GasName(a.Gases[0]);
                    parts.Add(kind + " air at " + JsonWriter.Format(pressure, decimals) + " bar");
                    break;
            }

            if (a.Class == AtmosphereClass.Envelope)
            {
                parts.Add(Celsius(p.Temperature) + " at the cloud tops");
            }
            else if (p.Spin == Spin.Locked)
            {
                parts.Add(Celsius(p.DayTemperature) + " by day, " + Celsius(p.NightTemperature) + " by night");
            }
            else
            {
                parts.Add(Celsius(p.Temperature));
            }

            if (p.Water >= 0.01)
            {
                parts.Add(Percent(p.Water) + " ocean");
            }

            if (p.Ice >= 0.01)
            {
                parts.Add(Percent(p.Ice) + " ice");
            }

            foreach (var b in p.Biomes)
            {
                if (b.Biome >= Biome.Tundra && b.Biome <= Biome.Rainforest)
                {
                    parts.Add("mostly " + BiomeName(b.Biome));
                    break;
                }
            }

            if (p.Spin == Spin.Locked && a.Class != AtmosphereClass.Envelope)
            {
                parts.Add("tidally locked");
            }

            switch (p.Life)
            {
                case LifeLevel.Prebiotic: parts.Add("prebiotic chemistry"); break;
                case LifeLevel.Microbial: parts.Add("microbial life"); break;
                case LifeLevel.Simple: parts.Add("simple life"); break;
                case LifeLevel.Complex: parts.Add("complex life"); break;
            }

            return string.Join(", ", parts) + "; hazard " + p.Hazard.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>"nitrogen", "nitrogen and oxygen", "hydrogen, helium and methane".</summary>
        public static string GasList(IReadOnlyList<Gas> gases)
        {
            var text = "";
            for (var i = 0; i < gases.Count; i++)
            {
                text += (i == 0 ? "" : i == gases.Count - 1 ? " and " : ", ") + GasName(gases[i]);
            }

            return text;
        }

        private static string GasName(Gas gas)
        {
            switch (gas)
            {
                case Gas.Hydrogen: return "hydrogen";
                case Gas.Helium: return "helium";
                case Gas.Methane: return "methane";
                case Gas.WaterVapour: return "water vapour";
                case Gas.Nitrogen: return "nitrogen";
                case Gas.Oxygen: return "oxygen";
                case Gas.CarbonDioxide: return "carbon dioxide";
                case Gas.SulphurDioxide: return "sulphur dioxide";
                default: return "rock vapour";
            }
        }

        private static string BiomeName(Biome biome)
        {
            switch (biome)
            {
                case Biome.Tundra: return "tundra";
                case Biome.Taiga: return "taiga";
                case Biome.TemperateForest: return "temperate forest";
                case Biome.Grassland: return "grassland";
                case Biome.Savanna: return "savanna";
                default: return "rainforest";
            }
        }

        private static string Celsius(double kelvin) => ((int)kelvin - 273).ToString(CultureInfo.InvariantCulture) + " °C";

        private static string Percent(double share) => ((int)System.Math.Floor(share * 100 + 0.5)).ToString(CultureInfo.InvariantCulture) + "%";

        private static bool Has(IReadOnlyList<string> list, string item)
        {
            foreach (var s in list)
            {
                if (s == item)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
