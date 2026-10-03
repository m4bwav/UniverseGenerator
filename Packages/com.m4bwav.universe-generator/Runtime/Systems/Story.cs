#nullable enable
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// Landmarks (D24), story tags (idea 21, D23) and one-line descriptors (P2). Every line is built from the data, and a
    /// tag is offered only when the data supports it, so text never contradicts the generated facts.
    /// </summary>
    internal static class Story
    {
        private static readonly LandmarkKind[] s_fallbacks = { LandmarkKind.Ruins, LandmarkKind.Derelict, LandmarkKind.Nebula, LandmarkKind.Comet, LandmarkKind.Anomaly };
        private static readonly int[] s_fallbackWeights = { 30, 25, 15, 15, 15 };
        private static readonly LandmarkKind[] s_outliers =
        {
            LandmarkKind.Megastructure, LandmarkKind.RoguePlanet, LandmarkKind.AncientBeacon,
            LandmarkKind.ShatteredWorld, LandmarkKind.TemporalAnomaly, LandmarkKind.DerelictFleet,
        };

        public static string PlanetDescriptor(PlanetKind kind, OrbitZone zone, bool rings, Star star)
        {
            string noun;
            var zoned = true;
            switch (kind)
            {
                case PlanetKind.Lava: noun = "lava world"; zoned = false; break;
                case PlanetKind.Iron: noun = "iron world"; break;
                case PlanetKind.Barren: noun = "barren rock"; break;
                case PlanetKind.Desert: noun = "desert world"; break;
                case PlanetKind.Rocky: noun = "rocky world"; break;
                case PlanetKind.Greenhouse: noun = "greenhouse world"; zoned = false; break;
                case PlanetKind.Ocean: noun = "ocean world"; break;
                case PlanetKind.Garden: noun = "garden world"; zoned = false; break;
                case PlanetKind.Ice: noun = "ice world"; zoned = zone != OrbitZone.Outer; break;
                case PlanetKind.Dwarf: noun = "dwarf planet"; zoned = zone == OrbitZone.Warm || zone == OrbitZone.Temperate; break;
                case PlanetKind.SubNeptune: noun = "mini-Neptune"; break;
                case PlanetKind.GasGiant: noun = "gas giant"; zoned = zone == OrbitZone.Warm || zone == OrbitZone.Temperate; break;
                case PlanetKind.IceGiant: noun = "ice giant"; zoned = zone == OrbitZone.Warm || zone == OrbitZone.Temperate; break;
                default: noun = "hot Jupiter"; zoned = false; break;
            }

            var words = (rings ? "ringed " : "") + (zoned ? ZoneWord(zone) + " " : "") + noun;
            var preposition = zone == OrbitZone.Outer ? "far from" : zone == OrbitZone.Hot ? "skimming" : "under";
            return WithArticle(words) + " " + preposition + " " + WithArticle(star.Description);
        }

        public static string SystemDescriptor(Star star, Companion? companion, IReadOnlyList<Planet> planets, IReadOnlyList<Belt> belts)
        {
            var text = companion == null
                ? WithArticle(star.Description)
                : WithArticle(star.Description) + " and " + companion.Star.Description + (companion.Orbit == CompanionOrbit.Close ? " in a close pair" : " pair");
            text += planets.Count == 0 ? " with no planets"
                : planets.Count == 1 ? " with one planet"
                : " with " + planets.Count.ToString(CultureInfo.InvariantCulture) + " planets";
            foreach (var b in belts)
            {
                if (b.Kind == BeltKind.Asteroid)
                {
                    return text + (planets.Count == 0 ? " but an asteroid belt" : " and an asteroid belt");
                }
            }

            return text;
        }

        public static Landmark[] Landmarks(ulong seed, Star star, Companion? companion, IReadOnlyList<Planet> planets, IReadOnlyList<Belt> belts, int weirdness)
        {
            var list = new List<Landmark>();
            var c = star.Class;
            if (c == StarClass.O || c == StarClass.B || c == StarClass.Giant || c == StarClass.Supergiant
                || c == StarClass.WhiteDwarf || c == StarClass.NeutronStar || c == StarClass.BlackHole)
            {
                list.Add(new Landmark { Kind = LandmarkKind.UnusualStar, Text = WithArticle(star.Description) + " at its heart" });
            }

            if (companion != null)
            {
                list.Add(new Landmark { Kind = LandmarkKind.BinaryStar, Text = WithArticle(companion.Star.Description) + " companion" });
            }

            foreach (var p in planets)
            {
                if (p.Kind == PlanetKind.Garden)
                {
                    list.Add(new Landmark { Kind = LandmarkKind.LivingWorld, Planet = p.Index, Text = "life on " + p.Name });
                }
                else
                {
                    foreach (var m in p.Moons)
                    {
                        if (m.Kind == MoonKind.Garden)
                        {
                            list.Add(new Landmark { Kind = LandmarkKind.LivingWorld, Planet = p.Index, Text = "life on " + m.Name + ", a moon of " + p.Name });
                        }
                    }
                }

                if (p.Rings && (p.Kind == PlanetKind.GasGiant || p.Kind == PlanetKind.IceGiant))
                {
                    list.Add(new Landmark { Kind = LandmarkKind.RingedGiant, Planet = p.Index, Text = "the rings of " + p.Name });
                }

                if (p.Kind == PlanetKind.HotJupiter)
                {
                    list.Add(new Landmark { Kind = LandmarkKind.HotJupiter, Planet = p.Index, Text = p.Name + ", a hot Jupiter" });
                }
            }

            foreach (var b in belts)
            {
                if (b.Kind == BeltKind.Asteroid)
                {
                    list.Add(new Landmark { Kind = LandmarkKind.AsteroidBelt, Text = "an asteroid belt" });
                    break;
                }
            }

            if (list.Count == 0)
            {
                var rng = Seeds.Stream(seed, "landmarks");
                var kind = s_fallbacks[rng.Weighted(s_fallbackWeights)];
                int? planet = kind == LandmarkKind.Ruins && planets.Count > 0 ? rng.NextInt(planets.Count) : (int?)null;
                list.Add(new Landmark { Kind = kind, Planet = planet, Text = Fallback(kind, planet == null ? null : planets[planet.Value].Name) });
            }

            var weird = Seeds.Stream(seed, "weird");
            if (weird.Chance(weirdness, 100))
            {
                var kind = s_outliers[weird.NextInt(s_outliers.Length)];
                list.Add(new Landmark { Kind = kind, Outlier = true, Text = Outlier(kind) });
            }

            return list.ToArray();
        }

        private static string Fallback(LandmarkKind kind, string? planet)
        {
            switch (kind)
            {
                case LandmarkKind.Ruins: return planet == null ? "ruins drifting in orbit" : "ruins on " + planet;
                case LandmarkKind.Derelict: return "a derelict ship";
                case LandmarkKind.Nebula: return "a glowing nebula around the system";
                case LandmarkKind.Comet: return "a great comet";
                default: return "a signal no one can explain";
            }
        }

        private static string Outlier(LandmarkKind kind)
        {
            switch (kind)
            {
                case LandmarkKind.Megastructure: return "a vast structure around the star";
                case LandmarkKind.RoguePlanet: return "a rogue planet passing through";
                case LandmarkKind.AncientBeacon: return "an ancient beacon still transmitting";
                case LandmarkKind.ShatteredWorld: return "the remains of a shattered world";
                case LandmarkKind.TemporalAnomaly: return "a region where clocks drift";
                default: return "a fleet of dead ships";
            }
        }

        public static string[] SystemTags(Pcg32 rng, Star star, IReadOnlyList<Planet> planets, IReadOnlyList<Belt> belts,
            IReadOnlyList<Station> stations, IReadOnlyList<Landmark> landmarks, int danger)
        {
            bool habitable = false, life = false, mining = false, giant = false, dead = true;
            foreach (var p in planets)
            {
                var k = p.Kind;
                habitable |= k == PlanetKind.Garden || k == PlanetKind.Ocean || (k == PlanetKind.Rocky && p.Zone == OrbitZone.Temperate);
                life |= k == PlanetKind.Garden;
                mining |= k == PlanetKind.Barren || k == PlanetKind.Iron;
                giant |= k == PlanetKind.GasGiant || k == PlanetKind.IceGiant || k == PlanetKind.HotJupiter;
                dead &= k == PlanetKind.Barren || k == PlanetKind.Lava || k == PlanetKind.Iron || k == PlanetKind.Dwarf;
                foreach (var m in p.Moons)
                {
                    life |= m.Kind == MoonKind.Garden;
                    habitable |= m.Kind == MoonKind.Garden;
                }
            }

            mining |= belts.Count > 0;
            bool Has(StationKind kind)
            {
                foreach (var s in stations)
                {
                    if (s.Kind == kind)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool Marked(params LandmarkKind[] kinds)
            {
                foreach (var l in landmarks)
                {
                    if (System.Array.IndexOf(kinds, l.Kind) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            var unusual = Marked(LandmarkKind.UnusualStar);
            var outlier = false;
            foreach (var l in landmarks)
            {
                outlier |= l.Outlier;
            }

            // Every tag in a fixed order with its weight; a weight of 0 means the data does not support it.
            var candidates = new (string Tag, int Weight)[]
            {
                ("frontier colony", habitable ? 10 : 0),
                ("mining boom", mining ? 10 : 0),
                ("pirate haven", Has(StationKind.PirateDen) || danger >= 7 ? 10 : 0),
                ("quarantine zone", life ? 6 : 0),
                ("abandoned colony", Marked(LandmarkKind.Ruins) || habitable ? 6 : 0),
                ("research outpost", Has(StationKind.ResearchStation) || unusual ? 8 : 0),
                ("trade crossroads", Has(StationKind.TradeHub) ? 10 : 0),
                ("holy site", 3),
                ("naval stronghold", Has(StationKind.NavalBase) ? 10 : 0),
                ("smuggler route", danger >= 4 && danger <= 8 ? 6 : 0),
                ("refugee haven", habitable ? 5 : 0),
                ("corporate enclave", Has(StationKind.Refinery) || Has(StationKind.Shipyard) || Has(StationKind.MiningPlatform) ? 8 : 0),
                ("precursor ruins", Marked(LandmarkKind.Ruins, LandmarkKind.AncientBeacon, LandmarkKind.Megastructure) ? 10 : 0),
                ("untouched wilderness", life && stations.Count == 0 ? 10 : 0),
                ("dead system", dead ? 8 : 0),
                ("gas mining", giant ? 8 : 0),
                ("scientific curiosity", outlier || unusual ? 8 : 0),
            };

            var weights = new int[candidates.Length];
            for (var i = 0; i < candidates.Length; i++)
            {
                weights[i] = candidates[i].Weight;
            }

            var first = rng.Weighted(weights);
            var wantSecond = rng.Chance(35, 100);
            weights[first] = 0;
            var any = false;
            foreach (var w in weights)
            {
                any |= w > 0;
            }

            if (!wantSecond || !any)
            {
                return new[] { candidates[first].Tag };
            }

            return new[] { candidates[first].Tag, candidates[rng.Weighted(weights)].Tag };
        }

        private static string ZoneWord(OrbitZone zone)
        {
            switch (zone)
            {
                case OrbitZone.Hot: return "scorched";
                case OrbitZone.Warm: return "warm";
                case OrbitZone.Temperate: return "temperate";
                case OrbitZone.Cold: return "cold";
                default: return "frozen";
            }
        }

        private static string WithArticle(string words) =>
            (words[0] == 'a' || words[0] == 'e' || words[0] == 'i' || words[0] == 'o' || words[0] == 'u' ? "an " : "a ") + words;
    }
}
