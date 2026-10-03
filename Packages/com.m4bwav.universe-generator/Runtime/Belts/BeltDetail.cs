#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// The belt level, following ai-docs/notes/2026-10-03-moon-and-belt-level-design.md: each belt's address, name,
    /// composition, mass, largest body, temperature and resources, from new streams of its own seed
    /// <c>Child(system, "belt", k)</c>, so no system's existing output changes.
    /// </summary>
    internal static class BeltDetail
    {
        private static readonly string[] s_resourceNames = { "metals", "rare elements", "ices", "gases", "organics" };

        /// <summary>Belt <paramref name="index"/> of <paramref name="system"/>, lit by <paramref name="light"/> Suns (both stars of a close pair).</summary>
        public static Belt Apply(Belt b, int index, Address system, ulong systemSeed, string systemName, double light, double frost)
        {
            var seed = Seeds.Child(systemSeed, "belt", index);
            var rng = Seeds.Stream(seed, "belt");
            var metalDraw = rng.NextInt(9);
            var carbonDraw = rng.NextInt(21);
            var iceDraw = rng.NextInt(6);
            var icyIceDraw = rng.NextInt(26);
            var icyMetalDraw = rng.NextInt(3);
            var icyCarbonDraw = rng.NextInt(16);
            var massDraw = rng.NextDouble();
            var sizeDraw = rng.NextDouble();

            // Where the belt lies against the frost line decides how much carbon and ice it holds; an outer belt too
            // warm for ice (bare rock over 170 K, all of it gone by 250 K) is rock.
            var middle = Math.Sqrt(b.Inner * b.Outer);
            var x = DMath.Round(middle / frost, 2);
            var rockTemperature = DMath.Round(278.6 * Math.Sqrt(Math.Sqrt(light / (middle * middle) * 0.9)), 0);
            int metal, carbon, ice;
            double mass, largest;
            if (b.Kind == BeltKind.Asteroid)
            {
                metal = 4 + metalDraw;
                ice = x > 1.2 ? Math.Min(30, (int)Math.Floor(25 * (x - 1.2)) + iceDraw) : 0;
                carbon = DMath.Clamp((int)Math.Floor(70 * x - 5 + 0.5) + carbonDraw - 10, 10, 80);
                mass = 0.0001 * DMath.Exp(massDraw * DMath.Log(30));
                largest = 100 * DMath.Exp(sizeDraw * DMath.Log(5));
            }
            else
            {
                metal = icyMetalDraw;
                ice = (int)Math.Floor((55 + icyIceDraw) * DMath.Clamp((250 - rockTemperature) / 80, 0, 1));
                carbon = 15 + icyCarbonDraw;
                mass = 0.005 * DMath.Exp(massDraw * DMath.Log(40));
                largest = 300 * DMath.Exp(sizeDraw * DMath.Log(1300.0 / 300));
            }

            carbon = Math.Min(carbon, 95 - metal - ice);
            var silicate = 100 - metal - carbon - ice;
            var composition = new BeltComposition { Silicate = silicate, Carbonaceous = carbon, Metal = metal, Ice = ice };

            var albedo = (silicate * 0.20 + carbon * 0.06 + metal * 0.15 + ice * 0.6) / 100;
            var temperature = DMath.Round(278.6 * Math.Sqrt(Math.Sqrt(light / (middle * middle) * (1 - albedo))), 0);
            var resources = PlanetDetail.Grades(
                Seeds.Stream(seed, "resources"),
                DMath.Clamp(1 + metal / 4 + silicate / 40, 1, 5),
                DMath.Clamp(1 + metal / 5, 1, 5),
                ice > 0 ? DMath.Clamp(1 + ice / 20, 1, 5) : carbon >= 50 ? 1 : 0,
                ice / 30,
                DMath.Clamp(carbon / 20, 0, 5));
            largest = DMath.Round(largest, 0);

            return b with
            {
                Address = system.Child("belt", index).ToString(),
                Index = index,
                Name = systemName + (b.Kind == BeltKind.Asteroid ? " Belt" : " Outer Belt"),
                Composition = composition,
                Mass = DMath.Round(mass, 6),
                LargestBody = largest,
                Temperature = temperature,
                Resources = resources,
                Summary = Summary(composition, largest, temperature, resources),
            };
        }

        /// <summary>"65% carbonaceous rock, 25% silicate rock, 10% metal; largest body 470 km in radius; -103 °C; richest in organics".</summary>
        private static string Summary(BeltComposition c, double largest, double temperature, ResourceGrades r)
        {
            var parts = new List<(string Name, int Share)>
            {
                ("silicate rock", c.Silicate), ("carbonaceous rock", c.Carbonaceous), ("metal", c.Metal), ("ice", c.Ice),
            };
            var made = new List<string>();
            while (parts.Count > 0)
            {
                // The largest share first; on a tie, the order above.
                var best = 0;
                for (var i = 1; i < parts.Count; i++)
                {
                    if (parts[i].Share > parts[best].Share)
                    {
                        best = i;
                    }
                }

                if (parts[best].Share > 0)
                {
                    made.Add(parts[best].Share.ToString(CultureInfo.InvariantCulture) + "% " + parts[best].Name);
                }

                parts.RemoveAt(best);
            }

            var grades = new[] { r.Metals, r.RareElements, r.Ices, r.Gases, r.Organics };
            var top = 0;
            foreach (var g in grades)
            {
                top = Math.Max(top, g);
            }

            var richest = new List<string>();
            for (var i = 0; i < grades.Length; i++)
            {
                if (grades[i] == top)
                {
                    richest.Add(s_resourceNames[i]);
                }
            }

            var last = richest[richest.Count - 1];
            richest.RemoveAt(richest.Count - 1);
            var rich = richest.Count == 0 ? last : string.Join(", ", richest) + " and " + last;
            return string.Join(", ", made) + "; largest body " + ((int)largest).ToString(CultureInfo.InvariantCulture) + " km in radius; "
                + ((int)temperature - 273).ToString(CultureInfo.InvariantCulture) + " °C; richest in " + rich;
        }
    }
}
