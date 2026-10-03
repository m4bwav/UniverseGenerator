#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// The star system level (plan ideas 12 to 21, N1, D23, D24), following
    /// ai-docs/notes/2026-10-02-star-system-level-design.md. Each purpose draws from its own stream of the system's seed.
    /// </summary>
    internal static class StarSystemGenerator
    {
        internal const double AuPerSolarRadius = 0.00465;
        private const double KmPerAu = 1.496e8;
        private const double KmPerEarthRadius = 6371;
        private const double SunsPerEarthMass = 3.003e-6;

        private static readonly StellarAge[] s_ages = { StellarAge.Young, StellarAge.Mature, StellarAge.Old };
        private static readonly int[] s_ageWeights = { 30, 45, 25 };
        private static readonly PlanetKind[] s_hotKinds = { PlanetKind.Lava, PlanetKind.Iron, PlanetKind.Barren, PlanetKind.Greenhouse };
        private static readonly int[] s_hotWeights = { 30, 20, 40, 10 };
        private static readonly PlanetKind[] s_warmKinds = { PlanetKind.Barren, PlanetKind.Desert, PlanetKind.Greenhouse, PlanetKind.Rocky };
        private static readonly int[] s_warmWeights = { 25, 30, 20, 25 };
        private static readonly PlanetKind[] s_temperateKinds = { PlanetKind.Rocky, PlanetKind.Ocean, PlanetKind.Garden, PlanetKind.Desert, PlanetKind.Barren };
        private static readonly int[] s_temperateWeights = { 22, 25, 22, 15, 10 };
        private static readonly PlanetKind[] s_coldKinds = { PlanetKind.Barren, PlanetKind.Rocky, PlanetKind.Ice, PlanetKind.Desert };
        private static readonly int[] s_coldWeights = { 30, 15, 40, 10 };
        private static readonly MoonKind[] s_giantMoonKinds = { MoonKind.Ice, MoonKind.Barren, MoonKind.Volcanic, MoonKind.Ocean, MoonKind.Hazy };
        private static readonly int[] s_giantMoonWeights = { 45, 20, 10, 15, 10 };
        private static readonly int[] s_stationCountWeights = { 60, 30, 10 };
        private static readonly string[] s_romanNumerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        private static readonly int[] s_romanValues = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };

        private enum SizeClass
        {
            Dwarf,
            Terrestrial,
            SubNeptune,
            IceGiant,
            GasGiant,
        }

        internal sealed class Draft
        {
            public double Orbit;
            public double Period;
            public double Mass;
            public double Radius;
            public PlanetKind Kind;
            public OrbitZone Zone;
        }

        public static StarSystem Generate(Address address, SystemContext context, GeneratorOptions options)
        {
            var seed = address.ObjectSeed;
            var age = context.Age ?? DrawAge(seed);
            var danger = context.Danger ?? Seeds.Stream(seed, "danger").Range(1, 10);
            var (star, companion) = StarGenerator.Roll(Seeds.Stream(seed, "star"), age, options.StarMix);
            var name = context.Name ?? StarNames.Draw(Seeds.Stream(seed, "names"), star.Class);

            var close = companion != null && companion.Orbit == CompanionOrbit.Close;
            var totalMass = star.Mass + (close ? companion!.Star.Mass : 0);
            var light = Math.Max(star.Luminosity + (close ? companion!.Star.Luminosity : 0), 0.0001);
            var (hzInner, hzOuter, frost) = Zones(light);

            var drafts = Planets(Seeds.Stream(seed, "planets"), star, companion, totalMass, hzInner, hzOuter, frost, context.Richness);
            if (drafts.Count > options.MaxPlanetsPerSystem)
            {
                drafts.RemoveRange(options.MaxPlanetsPerSystem, drafts.Count - options.MaxPlanetsPerSystem);
            }

            var letters = DiscoveryLetters(drafts.Count, Seeds.Stream(seed, "letters"));
            var host = new PlanetHost(star, totalMass, light, age);
            var planets = new Planet[drafts.Count];
            for (var i = 0; i < drafts.Count; i++)
            {
                planets[i] = BuildPlanet(drafts[i], i, Seeds.Child(seed, "planet", i), address.Child("planet", i), name + " " + letters[i], host, options.Weirdness);
            }

            var belts = Belts(Seeds.Stream(seed, "belts"), planets, frost);
            for (var k = 0; k < belts.Length; k++)
            {
                belts[k] = BeltDetail.Apply(belts[k], k, address, seed, name, light, frost);
            }

            var stations = Stations(Seeds.Stream(seed, "stations"), address, name, planets.Length);
            var landmarks = Story.Landmarks(seed, star, companion, planets, belts, options.Weirdness);
            return new StarSystem
            {
                Address = address.ToString(),
                Name = name,
                Star = star,
                Companion = companion,
                Age = age,
                Danger = danger,
                HabitableZoneInner = hzInner,
                HabitableZoneOuter = hzOuter,
                FrostLine = frost,
                Planets = planets,
                Belts = belts,
                Stations = stations,
                Landmarks = landmarks,
                Tags = Story.SystemTags(Seeds.Stream(seed, "tags"), star, planets, belts, stations, landmarks, danger),
                Descriptor = Story.SystemDescriptor(star, companion, planets, belts),
            };
        }

        /// <summary>
        /// A planet from its draft: rings and moons from its <c>moons</c> stream, then the planet level's detail from its
        /// other streams (<see cref="PlanetDetail"/>), then each moon's detail from the moon's own seed
        /// (<see cref="MoonDetail"/>). <paramref name="planetSeed"/> is the seed of <paramref name="planetAddress"/>.
        /// </summary>
        internal static Planet BuildPlanet(Draft d, int index, ulong planetSeed, Address planetAddress, string planetName, PlanetHost host, int weirdness)
        {
            var moons = Seeds.Stream(planetSeed, "moons");
            var rings = moons.Chance(RingPercent(d.Kind), 100);
            var planet = new Planet
            {
                Address = planetAddress.ToString(),
                Index = index,
                Name = planetName,
                Kind = d.Kind,
                Zone = d.Zone,
                Orbit = DMath.Round(d.Orbit, 4),
                Period = DMath.Round(d.Period, 2),
                Mass = DMath.Round(d.Mass, 3),
                Radius = DMath.Round(d.Radius, 3),
                Rings = rings,
                Moons = Moons(moons, d, host.TotalMass, planetAddress, planetName),
                Descriptor = Story.PlanetDescriptor(d.Kind, d.Zone, rings, host.Star),
            };
            planet = PlanetDetail.Apply(planet, host, planetSeed, weirdness);
            return planet with { Moons = MoonDetail.ApplyAll(planet, host, planetSeed, weirdness) };
        }

        /// <summary>The system's age when no galaxy gives one: Young 30, Mature 45, Old 25 from the <c>age</c> stream.</summary>
        internal static StellarAge DrawAge(ulong seed) => s_ages[Seeds.Stream(seed, "age").Weighted(s_ageWeights)];

        /// <summary>
        /// The optimistic habitable zone (Kopparapu et al. 2013: recent Venus to early Mars), wider for play, and the
        /// frost line, in au for a luminosity in Suns.
        /// </summary>
        internal static (double Inner, double Outer, double Frost) Zones(double light) =>
            (DMath.Round(Math.Sqrt(light / 1.776), 4), DMath.Round(Math.Sqrt(light / 0.32), 4), DMath.Round(2.7 * Math.Sqrt(light), 4));

        /// <summary>No garden worlds around blazing, swollen or dead stars.</summary>
        internal static bool NoGarden(StarClass c) =>
            c == StarClass.O || c == StarClass.B || c == StarClass.Giant || c == StarClass.Supergiant || c == StarClass.WhiteDwarf
            || c == StarClass.NeutronStar || c == StarClass.BlackHole;

        private static List<Draft> Planets(Pcg32 rng, Star star, Companion? companion, double totalMass, double hzInner, double hzOuter, double frost, int richness)
        {
            var (innerMin, innerMax, outerMin, outerMax) = Counts(star.Class);
            var innerCount = rng.Range(innerMin, innerMax);
            var outerCount = rng.Range(outerMin, outerMax);
            var giantWeight = (int)(30 * DMath.Clamp(star.Mass, 0.15, 1.6));
            // Giant planets follow the metallicity of their galaxy (Fischer and Valenti 2005); 100% leaves the weight as it was.
            if (richness != 100)
            {
                giantWeight = giantWeight * richness / 100;
            }

            // Peas in a pod (Weiss 2018): one typical mass per system for small planets, one for sub-Neptunes.
            var pod = (Rocky: LogUniform(rng, 0.25, 2.2), Gassy: LogUniform(rng, 4.7, 12));
            var noGarden = NoGarden(star.Class);

            var minOrbit = Math.Max(0.01, star.Radius * AuPerSolarRadius * 3);
            if (companion != null && companion.Orbit == CompanionOrbit.Close)
            {
                minOrbit = Math.Max(minOrbit, 3 * companion.Separation);
            }

            var planets = new List<Draft>();
            var innerLink = false;
            var orbit = Math.Max(rng.Range(0.04, 0.3) * DMath.Pow(totalMass, 1.0 / 3), minOrbit);
            for (var i = 0; i < innerCount && orbit <= frost; i++)
            {
                var p = Draw(rng, orbit, totalMass, Zone(orbit, hzInner, hzOuter, frost), giantWeight, false, pod, noGarden);
                planets.Add(p);
                if (p.Mass >= 2 && p.Mass <= 20)
                {
                    innerLink = true;
                }

                // Neighbour period ratio log-normal around 1.8 (Weiss 2018: rarely under 1.2); giants keep at least 1.6.
                var z = rng.Range(-1.0, 1.0) + rng.Range(-1.0, 1.0);
                var ratio = DMath.Clamp(1.8 * DMath.Exp(0.28 * z), 1.25, 4.0);
                if (p.Mass >= 10)
                {
                    ratio = Math.Max(ratio, 1.6);
                }

                orbit *= DMath.Pow(ratio, 2.0 / 3);
            }

            var last = planets.Count > 0 ? planets[planets.Count - 1].Orbit : minOrbit;
            orbit = Math.Max(Math.Max(last * 1.4, minOrbit), frost * rng.Range(0.8, 1.5));
            for (var j = 0; j < outerCount; j++)
            {
                planets.Add(Draw(rng, orbit, totalMass, Zone(orbit, hzInner, hzOuter, frost), giantWeight, innerLink, pod, noGarden));
                orbit *= rng.Range(1.5, 2.3);
            }

            // A wide companion under 200 au truncates the disc at about 0.3 of its distance (Holman and Wiegert 1999, roughly).
            if (companion != null && companion.Orbit == CompanionOrbit.Wide && companion.Separation < 200)
            {
                planets.RemoveAll(p => p.Orbit > 0.3 * companion.Separation);
            }

            return planets;
        }

        private static (int InnerMin, int InnerMax, int OuterMin, int OuterMax) Counts(StarClass c)
        {
            switch (c)
            {
                case StarClass.O: return (0, 1, 0, 3);
                case StarClass.B: return (0, 2, 0, 3);
                case StarClass.A: return (0, 3, 1, 4);
                case StarClass.F:
                case StarClass.G: return (1, 5, 0, 4);
                case StarClass.K: return (1, 5, 0, 3);
                case StarClass.M: return (1, 5, 0, 2);
                case StarClass.WhiteDwarf: return (0, 1, 0, 3);
                case StarClass.Giant: return (0, 1, 1, 4);
                case StarClass.Supergiant: return (0, 0, 0, 2);
                case StarClass.NeutronStar: return (0, 2, 0, 0);
                default: return (0, 1, 0, 2);
            }
        }

        internal static OrbitZone Zone(double orbit, double hzInner, double hzOuter, double frost) =>
            orbit < hzInner * 0.5 ? OrbitZone.Hot
            : orbit < hzInner ? OrbitZone.Warm
            : orbit <= hzOuter ? OrbitZone.Temperate
            : orbit <= frost ? OrbitZone.Cold
            : OrbitZone.Outer;

        internal static Draft Draw(Pcg32 rng, double orbit, double totalMass, OrbitZone zone, int g, bool innerLink, (double Rocky, double Gassy) pod, bool noGarden)
        {
            // Weights [dwarf, terrestrial, sub-Neptune, ice giant, gas giant] per zone; giants follow the star's mass.
            int[] weights;
            switch (zone)
            {
                case OrbitZone.Hot: weights = new[] { 0, 70, 12, 0, g / 6 }; break;
                case OrbitZone.Warm: weights = new[] { 0, 65, 28, 0, g / 8 }; break;
                case OrbitZone.Temperate: weights = new[] { 0, 70, 22, 0, g / 10 }; break;
                case OrbitZone.Cold: weights = new[] { 8, 45, 15, 10, g }; break;
                default:
                    var link = innerLink ? 2 : 1;
                    weights = new[] { 20, 12, 6, 30 * link, g * 16 / 10 * link };
                    break;
            }

            var size = (SizeClass)rng.Weighted(weights);
            double mass;
            var z = rng.Range(-1.0, 1.0) + rng.Range(-1.0, 1.0);
            switch (size)
            {
                case SizeClass.Dwarf: mass = LogUniform(rng, 0.001, 0.05); break;
                case SizeClass.Terrestrial: mass = DMath.Clamp(pod.Rocky * DMath.Exp(0.35 * z), 0.05, 2.8); break;
                case SizeClass.SubNeptune: mass = DMath.Clamp(pod.Gassy * DMath.Exp(0.35 * z), 4.7, 20); break;
                case SizeClass.IceGiant: mass = LogUniform(rng, 10, 50); break;
                default:
                    // Skewed towards Saturn and Jupiter masses; super-Jupiters up to 4,000 Earths stay rare.
                    var u = rng.NextDouble();
                    mass = 50 * DMath.Exp(u * u * DMath.Log(80));
                    break;
            }

            var period = 365.25 * Math.Sqrt(orbit * orbit * orbit / totalMass);
            if (size == SizeClass.SubNeptune && period < 10)
            {
                // The hot Neptune desert: its gas boiled away, leaving the rocky core.
                size = SizeClass.Terrestrial;
                mass = DMath.Clamp(mass * 0.3, 0.05, 2.8);
            }

            PlanetKind kind;
            switch (size)
            {
                case SizeClass.Dwarf: kind = PlanetKind.Dwarf; break;
                case SizeClass.SubNeptune: kind = PlanetKind.SubNeptune; break;
                case SizeClass.IceGiant: kind = PlanetKind.IceGiant; break;
                case SizeClass.GasGiant: kind = zone == OrbitZone.Hot ? PlanetKind.HotJupiter : PlanetKind.GasGiant; break;
                default: kind = TerrestrialKind(rng, zone, mass, noGarden); break;
            }

            return new Draft { Orbit = orbit, Period = period, Mass = mass, Radius = Radius(mass, kind), Kind = kind, Zone = zone };
        }

        private static PlanetKind TerrestrialKind(Pcg32 rng, OrbitZone zone, double mass, bool noGarden)
        {
            PlanetKind kind;
            switch (zone)
            {
                case OrbitZone.Hot:
                    kind = s_hotKinds[rng.Weighted(s_hotWeights)];
                    break;
                case OrbitZone.Warm:
                    kind = s_warmKinds[rng.Weighted(s_warmWeights)];
                    break;
                case OrbitZone.Temperate:
                    kind = s_temperateKinds[rng.Weighted(s_temperateWeights)];
                    break;
                case OrbitZone.Cold:
                    kind = s_coldKinds[rng.Weighted(s_coldWeights)];
                    break;
                default:
                    kind = rng.Chance(70, 100) ? PlanetKind.Ice : PlanetKind.Barren;
                    break;
            }

            // Small worlds cannot hold an atmosphere (the Jeans rule, coarsely); dead or blazing stars host no gardens.
            if (kind == PlanetKind.Greenhouse && mass < 0.5)
            {
                kind = PlanetKind.Barren;
            }

            if (mass < 0.2 && (kind == PlanetKind.Ocean || kind == PlanetKind.Garden))
            {
                kind = PlanetKind.Barren;
            }

            if (mass < 0.1 && (kind == PlanetKind.Desert || kind == PlanetKind.Rocky))
            {
                kind = zone == OrbitZone.Cold ? PlanetKind.Ice : PlanetKind.Barren;
            }

            if (kind == PlanetKind.Garden && noGarden)
            {
                kind = PlanetKind.Rocky;
            }

            return kind;
        }

        /// <summary>Radius in Earths from mass (Chen and Kipping 2017, adjusted so Jupiter's mass gives Jupiter's radius).</summary>
        private static double Radius(double mass, PlanetKind kind)
        {
            if (mass <= 2.8 && kind != PlanetKind.SubNeptune)
            {
                return DMath.Pow(mass, 0.279);
            }

            var r = mass <= 95 ? 0.808 * DMath.Pow(mass, 0.589) : 11.2 * DMath.Pow(mass / 318, -0.044);
            return kind == PlanetKind.HotJupiter ? r * 1.2 : r;
        }

        private static int RingPercent(PlanetKind kind)
        {
            switch (kind)
            {
                case PlanetKind.GasGiant: return 40;
                case PlanetKind.IceGiant: return 30;
                case PlanetKind.SubNeptune: return 5;
                case PlanetKind.HotJupiter: return 0;
                default: return 2;
            }
        }

        private static Moon[] Moons(Pcg32 rng, Draft planet, double totalMass, Address planetAddress, string planetName)
        {
            int count;
            var giant = planet.Kind == PlanetKind.GasGiant || planet.Kind == PlanetKind.IceGiant;
            switch (planet.Kind)
            {
                case PlanetKind.GasGiant: count = rng.Range(1, 8); break;
                case PlanetKind.IceGiant: count = rng.Range(1, 5); break;
                case PlanetKind.SubNeptune: count = rng.Range(0, 2); break;
                case PlanetKind.HotJupiter: count = 0; break;
                case PlanetKind.Dwarf: count = rng.Range(0, 1); break;
                default: count = planet.Mass < 0.3 ? rng.Range(0, 1) : rng.Range(0, 2); break;
            }

            var planetKm = planet.Radius * KmPerEarthRadius;
            // Half the Hill radius, in planet radii: moons beyond it would not stay.
            var hillHalf = 0.5 * planet.Orbit * KmPerAu * DMath.Pow(planet.Mass * SunsPerEarthMass / (3 * totalMass), 1.0 / 3) / planetKm;
            var moons = new List<Moon>();
            var orbit = rng.Range(3.0, 6.0);
            var garden = false;
            for (var j = 0; j < count && orbit < hillHalf; j++)
            {
                MoonKind kind;
                if (giant)
                {
                    kind = s_giantMoonKinds[rng.Weighted(s_giantMoonWeights)];
                    if (!garden && planet.Zone == OrbitZone.Temperate && rng.Chance(25, 100))
                    {
                        kind = MoonKind.Garden;
                        garden = true;
                    }
                }
                else
                {
                    kind = rng.Chance(85, 100) ? MoonKind.Barren : MoonKind.Ice;
                }

                var radius = giant ? LogUniform(rng, 200, 2700) : rng.Range(100.0, Math.Max(101.0, 0.3 * planetKm));
                moons.Add(new Moon
                {
                    Address = planetAddress.Child("moon", j).ToString(),
                    Index = j,
                    Name = planetName + " " + Roman(j + 1),
                    Kind = kind,
                    Orbit = DMath.Round(orbit, 2),
                    Radius = DMath.Round(radius, 0),
                });
                orbit *= rng.Range(1.3, 2.0);
            }

            return moons.ToArray();
        }

        private static Belt[] Belts(Pcg32 rng, Planet[] planets, double frost)
        {
            var belts = new List<Belt>();
            if (planets.Length == 0)
            {
                if (rng.Chance(60, 100))
                {
                    belts.Add(new Belt { Kind = BeltKind.Asteroid, Inner = DMath.Round(frost * 0.8, 4), Outer = DMath.Round(frost * 1.2, 4) });
                }

                return belts.ToArray();
            }

            // An asteroid belt goes in the widest gap, never over a planet.
            if (planets.Length >= 2 && rng.Chance(55, 100))
            {
                var best = -1;
                var bestRatio = 0.0;
                for (var i = 0; i + 1 < planets.Length; i++)
                {
                    var r = planets[i + 1].Orbit / planets[i].Orbit;
                    if (r > bestRatio)
                    {
                        bestRatio = r;
                        best = i;
                    }
                }

                if (bestRatio > 1.6)
                {
                    double a = planets[best].Orbit, b = planets[best + 1].Orbit;
                    // Edges proportional to the middle, so the belt fits any gap over 1.6 (0.87 and 1.15 of a middle over 1.26 a).
                    var mid = Math.Sqrt(a * b);
                    belts.Add(new Belt { Kind = BeltKind.Asteroid, Inner = DMath.Round(mid * 0.87, 4), Outer = DMath.Round(mid * 1.15, 4) });
                }
            }

            if (rng.Chance(35, 100))
            {
                var edge = planets[planets.Length - 1].Orbit;
                belts.Add(new Belt { Kind = BeltKind.Ice, Inner = DMath.Round(edge * 1.6, 4), Outer = DMath.Round(edge * 2.1, 4) });
            }

            return belts.ToArray();
        }

        private static readonly string[] s_stationWords =
        {
            "Nova", "Orion", "Helios", "Drift", "Kepler", "Atlas", "Meridian", "Halcyon", "Tethys", "Corvid", "Ember", "Lantern",
            "Bastion", "Solace", "Vigil", "Cinder", "Aurora", "Beacon", "Cairn", "Dawn", "Echo", "Farpoint", "Gantry", "Harbor",
            "Icarus", "Juno", "Keystone", "Lodestar", "Magellan", "Nadir", "Oasis", "Paragon", "Quarry", "Rampart", "Sable",
            "Talon", "Umbra", "Vanguard", "Waypoint", "Zenith", "Anchor", "Bulwark", "Crucible", "Dominion", "Esprit", "Fathom",
            "Garrison", "Haven", "Ironside", "Jubilee", "Kestrel", "Liberty", "Mariner", "Nimbus", "Outlook", "Pinnacle",
            "Respite", "Sentinel", "Threshold", "Unity", "Valor", "Wayfarer", "Yardarm", "Zephyr",
        };

        private static readonly string[] s_stationKindNames =
        {
            "Trade Hub", "Shipyard", "Mining Platform", "Research Station", "Naval Base", "Pirate Den", "Refinery", "Relay",
        };

        private static Station[] Stations(Pcg32 rng, Address address, string systemName, int planetCount)
        {
            var count = rng.Weighted(s_stationCountWeights);
            var kinds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
            var stations = new Station[count];
            for (var k = 0; k < count; k++)
            {
                var pick = rng.NextInt(kinds.Count);
                var kind = kinds[pick];
                kinds.RemoveAt(pick);
                var prefix = rng.Chance(20, 100) ? systemName : s_stationWords[rng.NextInt(s_stationWords.Length)];
                int? planet = planetCount == 0 || rng.Chance(20, 100) ? null : rng.NextInt(planetCount);
                stations[k] = new Station
                {
                    Address = address.Child("station", k).ToString(),
                    Name = prefix + " " + s_stationKindNames[kind],
                    Kind = (StationKind)kind,
                    Planet = planet,
                };
            }

            return stations;
        }

        /// <summary>b, c, d ... in a seeded discovery order (IAU letters follow discovery, not distance).</summary>
        private static char[] DiscoveryLetters(int count, Pcg32 rng)
        {
            var letters = new char[count];
            for (var i = 0; i < count; i++)
            {
                letters[i] = (char)('b' + Math.Min(i, 24));
            }

            for (var i = count - 1; i > 0; i--)
            {
                var j = rng.NextInt(i + 1);
                (letters[i], letters[j]) = (letters[j], letters[i]);
            }

            return letters;
        }

        private static string Roman(int n)
        {
            var text = "";
            for (var i = 0; i < s_romanValues.Length; i++)
            {
                while (n >= s_romanValues[i])
                {
                    text += s_romanNumerals[i];
                    n -= s_romanValues[i];
                }
            }

            return text;
        }

        internal static double LogUniform(Pcg32 rng, double low, double high) =>
            low * DMath.Exp(rng.NextDouble() * DMath.Log(high / low));
    }
}
