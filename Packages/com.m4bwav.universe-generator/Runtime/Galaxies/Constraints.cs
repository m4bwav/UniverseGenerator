#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// The galaxy-level repair pass for <see cref="GeneratorOptions.Require"/>. Every choice draws from the galaxy's own
    /// <c>constraints</c> stream, in a fixed order (stars, then planets, then the precursor site), and reaches a system
    /// only through its <see cref="SystemContext"/>, so <c>Universe.At</c> regenerates the same system. With no guarantee
    /// nothing here draws or changes anything.
    /// </summary>
    internal static class Constraints
    {
        private static readonly (Guarantee Need, StarClass Make)[] s_stars =
        {
            (Guarantee.BlueStar, StarClass.B),
            (Guarantee.Giant, StarClass.Giant),
            (Guarantee.WhiteDwarf, StarClass.WhiteDwarf),
            (Guarantee.NeutronStar, StarClass.NeutronStar),
            (Guarantee.BlackHole, StarClass.BlackHole),
            (Guarantee.SunLikeStar, StarClass.G),
        };

        public static Pcg32 Stream(ulong galaxySeed) => Seeds.Stream(galaxySeed, "constraints");

        /// <summary>
        /// Gives a missing star class to a system the stream picks (not the core while there are others), as a lone star
        /// made from that system's own <c>constraint-star</c> stream. Returns which systems were changed.
        /// </summary>
        public static bool[] Stars(Pcg32 rng, Guarantee require, ulong galaxySeed, int core, (Star Star, Companion? Companion)[] stars, List<GeneratorWarning> warnings)
        {
            var n = stars.Length;
            var forced = new bool[n];
            foreach (var (need, make) in s_stars)
            {
                if ((require & need) == 0 || Has(stars, need))
                {
                    continue;
                }

                var candidates = new List<int>();
                for (var i = 0; i < n; i++)
                {
                    // Never the core while there are others, nor a star some guarantee already counts on.
                    if (!forced[i] && (i != core || n == 1) && !Wanted(stars[i].Star.Class, require))
                    {
                        candidates.Add(i);
                    }
                }

                if (candidates.Count == 0)
                {
                    Unmet(warnings, need, "every other system already holds a guaranteed star");
                    continue;
                }

                var pick = candidates[rng.NextInt(candidates.Count)];
                var star = StarGenerator.Make(make, Seeds.Stream(GalaxyLayout.SystemSeed(galaxySeed, pick), "constraint-star"));
                stars[pick] = (star, null);
                forced[pick] = true;
            }

            return forced;
        }

        /// <summary>
        /// Generates systems in order until each required planet kind is found. When one is missing, the stream picks a
        /// system with a temperate planet that could be that kind, and its context turns that planet into it. Systems
        /// generated here and left unchanged are returned so the galaxy keeps them.
        /// </summary>
        public static StarSystem?[] Planets(Pcg32 rng, Guarantee require, Address galaxy, SystemContext[] contexts, GeneratorOptions options, List<GeneratorWarning> warnings)
        {
            var n = contexts.Length;
            var made = new StarSystem?[n];
            var needGarden = (require & Guarantee.GardenWorld) != 0;
            var needOcean = (require & Guarantee.OceanWorld) != 0;
            if (!needGarden && !needOcean)
            {
                return made;
            }

            bool foundGarden = false, foundOcean = false;
            for (var i = 0; i < n && ((needGarden && !foundGarden) || (needOcean && !foundOcean)); i++)
            {
                made[i] = StarSystemGenerator.Generate(galaxy.Child("system", i), contexts[i], options);
                foreach (var p in made[i]!.Planets)
                {
                    foundGarden |= p.Kind == PlanetKind.Garden;
                    foundOcean |= p.Kind == PlanetKind.Ocean;
                }
            }

            var changed = new bool[n];
            if (needGarden && !foundGarden)
            {
                Repair(rng, PlanetKind.Garden, Guarantee.GardenWorld, galaxy, contexts, options, made, changed, warnings);
            }

            if (needOcean && !foundOcean)
            {
                Repair(rng, PlanetKind.Ocean, Guarantee.OceanWorld, galaxy, contexts, options, made, changed, warnings);
            }

            for (var i = 0; i < n; i++)
            {
                if (changed[i])
                {
                    made[i] = null;
                }
            }

            return made;
        }

        /// <summary>Adds a lone precursor site in a system the stream picks among those with no point of interest.</summary>
        public static PointOfInterest[] Precursor(Pcg32 rng, Guarantee require, PointOfInterest[] points, MapEntry[] map, List<GeneratorWarning> warnings)
        {
            if ((require & Guarantee.PrecursorSite) == 0)
            {
                return points;
            }

            var taken = new bool[map.Length];
            foreach (var p in points)
            {
                if (p.Kind == PointOfInterestKind.PrecursorSite)
                {
                    return points;
                }

                taken[p.System] = true;
            }

            var free = new List<int>();
            for (var i = 0; i < map.Length; i++)
            {
                if (!taken[i])
                {
                    free.Add(i);
                }
            }

            if (free.Count == 0)
            {
                Unmet(warnings, Guarantee.PrecursorSite, "every system already holds a point of interest");
                return points;
            }

            var at = free[rng.NextInt(free.Count)];
            var more = new PointOfInterest[points.Length + 1];
            points.CopyTo(more, 0);
            more[points.Length] = new PointOfInterest
            {
                Kind = PointOfInterestKind.PrecursorSite,
                System = at,
                Name = "the precursor vault at " + map[at].Name,
                Text = "a lone precursor site, its trail long erased; whatever it guards is here",
                ChainStep = 1,
                ChainLength = 1,
            };
            return more;
        }

        private static void Repair(Pcg32 rng, PlanetKind kind, Guarantee need, Address galaxy, SystemContext[] contexts, GeneratorOptions options, StarSystem?[] made, bool[] changed, List<GeneratorWarning> warnings)
        {
            var systems = new List<int>();
            var planets = new List<int>();
            for (var i = 0; i < made.Length; i++)
            {
                if (changed[i])
                {
                    continue;
                }

                var system = made[i] ??= StarSystemGenerator.Generate(galaxy.Child("system", i), contexts[i], options);
                if (kind == PlanetKind.Garden && StarSystemGenerator.NoGarden(system.Star.Class))
                {
                    continue;
                }

                foreach (var p in system.Planets)
                {
                    if (Fits(p, kind))
                    {
                        systems.Add(i);
                        planets.Add(p.Index);
                        break;
                    }
                }
            }

            if (systems.Count == 0)
            {
                Unmet(warnings, need, "no system has a temperate rocky planet to hold it");
                return;
            }

            var k = rng.NextInt(systems.Count);
            var at = systems[k];
            contexts[at] = contexts[at].Forcing(planets[k], kind);
            changed[at] = true;
        }

        // A temperate terrestrial world big enough to keep an atmosphere and seas (the Jeans rule in TerrestrialKind).
        private static bool Fits(Planet p, PlanetKind kind) =>
            p.Zone == OrbitZone.Temperate && p.Mass >= 0.2 && p.Kind != PlanetKind.Garden && p.Kind != PlanetKind.Ocean
            && (p.Kind == PlanetKind.Rocky || p.Kind == PlanetKind.Desert || p.Kind == PlanetKind.Barren);

        private static bool Has((Star Star, Companion? Companion)[] stars, Guarantee need)
        {
            foreach (var (star, _) in stars)
            {
                if (Meets(star.Class, need))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Wanted(StarClass c, Guarantee require)
        {
            foreach (var (need, _) in s_stars)
            {
                if ((require & need) != 0 && Meets(c, need))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Meets(StarClass c, Guarantee need) => need switch
        {
            Guarantee.BlueStar => c == StarClass.O || c == StarClass.B,
            Guarantee.Giant => c == StarClass.Giant || c == StarClass.Supergiant,
            Guarantee.WhiteDwarf => c == StarClass.WhiteDwarf,
            Guarantee.NeutronStar => c == StarClass.NeutronStar,
            Guarantee.BlackHole => c == StarClass.BlackHole,
            _ => c == StarClass.G,
        };

        private static void Unmet(List<GeneratorWarning> warnings, Guarantee need, string why) =>
            warnings.Add(new GeneratorWarning { Code = WarningCode.GuaranteeUnmet, Message = $"Require {need} is not met: {why}." });
    }
}
