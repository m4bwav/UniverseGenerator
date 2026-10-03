#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// The moon level, following ai-docs/notes/2026-10-03-moon-and-belt-level-design.md. Everything here is derived from
    /// a moon's system-level fields, its planet's detail and new streams of the moon's own seed, so no planet's or
    /// system's output changes. Where a moon behaves like a small planet, the planet level's code does the work.
    /// </summary>
    internal static class MoonDetail
    {
        private const double KmPerEarthRadius = 6371;
        private const double EarthGm = 398600.4;
        private const double Gravitation = 6.674e-11;
        private const double KgPerEarthMass = 5.972e24;
        private const double StefanBoltzmann = 5.670e-8;

        private static readonly Gas[] s_sulphurDioxide = { Gas.SulphurDioxide };
        private static readonly Gas[] s_nitrogenMethane = { Gas.Nitrogen, Gas.Methane };
        private static readonly Gas[] s_nitrogenOxygen = { Gas.Nitrogen, Gas.Oxygen };

        /// <summary>The detail of every moon of <paramref name="planet"/>, each from its own seed <c>Child(planet, "moon", j)</c>.</summary>
        public static Moon[] ApplyAll(Planet planet, PlanetHost host, ulong planetSeed, int weirdness)
        {
            var moons = new Moon[planet.Moons.Count];
            for (var j = 0; j < moons.Length; j++)
            {
                moons[j] = Apply(planet.Moons[j], planet, host, Seeds.Child(planetSeed, "moon", j), weirdness);
            }

            return moons;
        }

        public static Moon Apply(Moon m, Planet planet, PlanetHost host, ulong seed, int weirdness)
        {
            var radius = m.Radius / KmPerEarthRadius;

            // 1. Physics, from the kind's density and the stored radius.
            var (low, high) = DensityRange(m.Kind);
            var density = DMath.Round(Seeds.Stream(seed, "body").Range(low, high), 2);
            var gravity = DMath.Round(density / 5.514 * radius, 3);
            var escape = DMath.Round(11.186 * radius * Math.Sqrt(density / 5.514), 2);
            var mass = DMath.Round(density / 5.514 * radius * radius * radius, 6);

            // 2. The orbit around its planet; it keeps one face to the planet, so it turns once per orbit.
            var distance = DMath.Round(m.Orbit * planet.Radius * KmPerEarthRadius, 0);
            var period = DMath.Round(2 * DMath.PI * Math.Sqrt(distance * distance * distance / (EarthGm * planet.Mass)) / 86400, 3);
            var rotation = DMath.Round(period * 24, 1);

            // 3. Tidal heat, bounded by what the kind promises.
            var tides = Tides(seed, m, planet, distance);
            var heating = DMath.Round(Math.Max(tides.Floor, Math.Min(tides.Cap, tides.Flow)), 4);

            // 4. Light, albedo, water; tidal heat adds to the starlight.
            var surface = Seeds.Stream(seed, "surface");
            var albedo = DMath.Round(BaseAlbedo(m.Kind) + surface.Range(-0.05, 0.05), 2);
            var waterDraw = surface.NextDouble();
            var targetDraw = surface.NextDouble();
            var flux = host.Light / (planet.Orbit * planet.Orbit);
            var teq = 278.6 * Math.Sqrt(Math.Sqrt(flux * (1 - albedo)));
            var warmed = Math.Sqrt(Math.Sqrt(teq * teq * teq * teq + heating / StefanBoltzmann));
            double inventory;
            switch (m.Kind)
            {
                case MoonKind.Ice: inventory = 0.6 + 0.4 * waterDraw; break;
                case MoonKind.Ocean: inventory = 1; break;
                case MoonKind.Garden: inventory = 0.3 + 0.55 * waterDraw; break;
                default: inventory = 0; break;
            }

            inventory = DMath.Round(inventory, 2);

            // 5. Air by the planet level's Jeans rule, then the surface temperature.
            var (atmosphere, temperature) = Air(Seeds.Stream(seed, "atmosphere"), m.Kind, teq, warmed, escape, inventory, targetDraw);
            var pressure = atmosphere.Pressure ?? 0;
            var redistribution = pressure / (pressure + 0.1);
            var swing = (1 - redistribution) * Math.Min(1, rotation / 100);
            var day = DMath.Round(temperature * (1 + 0.3 * swing), 0);
            var night = DMath.Round(temperature * (1 - 0.7 * swing), 0);

            // 6. A hidden ocean.
            var ocean = m.Kind == MoonKind.Ocean
                || ((m.Kind == MoonKind.Ice || m.Kind == MoonKind.Hazy) && temperature < 260 && (heating >= 0.01 || m.Radius >= 1200));

            // 7 and 8. Life, then climate bands and biomes as a planet turning once per orbit with its planet's tilt.
            var (life, flora, fauna) = Life(Seeds.Stream(seed, "life"), m.Kind, ocean, (int)temperature, host.Age);
            var like = Like(m.Kind);
            var (bands, biomes, water, ice) = PlanetDetail.Climate(like, Spin.Free, temperature, day, night, planet.Tilt, redistribution, pressure, inventory, flora);

            var moon = m with
            {
                Distance = distance,
                Period = period,
                Composition = m.Kind == MoonKind.Barren || m.Kind == MoonKind.Volcanic || m.Kind == MoonKind.Garden ? Composition.Rock : Composition.IceAndRock,
                Density = density,
                Mass = mass,
                Gravity = gravity,
                EscapeVelocity = escape,
                Insolation = DMath.Round(flux, 4),
                Albedo = albedo,
                TidalHeating = heating,
                Temperature = temperature,
                DayTemperature = day,
                NightTemperature = night,
                Atmosphere = atmosphere,
                Water = water,
                Ice = ice,
                SubsurfaceOcean = ocean,
                Rotation = rotation,
                Tilt = planet.Tilt,
                Bands = bands,
                Biomes = biomes,
                Life = life,
                Flora = flora,
                Fauna = fauna,
            };

            // 9 and 10. Traits and anomaly, resources.
            moon = moon with
            {
                Traits = MoonStory.Traits(Seeds.Stream(seed, "traits"), moon, planet),
                Anomaly = PlanetStory.Anomaly(Seeds.Stream(seed, "anomaly"), false, weirdness),
                Resources = Resources(Seeds.Stream(seed, "resources"), moon, planet),
            };

            // 11 and 12. Hazards, similarity, habitability, descriptor and summary, through the planet level's code.
            var standIn = new Planet
            {
                Kind = like,
                Zone = planet.Zone,
                Gravity = gravity,
                Temperature = temperature,
                DayTemperature = day,
                NightTemperature = night,
                Atmosphere = atmosphere,
                Water = water,
                Ice = ice,
                Spin = Spin.Free,
                Biomes = biomes,
                Life = life,
                Fauna = fauna,
                Traits = moon.Traits,
            };
            var (hazard, hazards) = PlanetStory.Hazards(standIn, host.Star, MoonStory.Hazards(moon, planet));
            standIn = standIn with { Hazard = hazard };
            moon = moon with
            {
                Hazard = hazard,
                Hazards = hazards,
                Similarity = PlanetDetail.Similarity(radius, density, escape, temperature),
                Habitability = PlanetDetail.HabitabilityOf(Species.Human, atmosphere, temperature, gravity),
                Descriptor = MoonStory.Descriptor(m.Kind, planet),
            };
            return moon with { Summary = PlanetStory.Summary(standIn, ocean ? "an ocean under the ice" : null, gravity < 0.01 ? 3 : 2) };
        }

        /// <summary>The planet kind whose climate, biomes and hazards a moon kind shares.</summary>
        private static PlanetKind Like(MoonKind kind)
        {
            switch (kind)
            {
                case MoonKind.Ice:
                case MoonKind.Ocean: return PlanetKind.Ice;
                case MoonKind.Volcanic: return PlanetKind.Lava;
                case MoonKind.Garden: return PlanetKind.Garden;
                default: return PlanetKind.Barren;
            }
        }

        private static (double Low, double High) DensityRange(MoonKind kind)
        {
            switch (kind)
            {
                case MoonKind.Barren: return (2.8, 3.6);
                case MoonKind.Ice: return (1.2, 2.0);
                case MoonKind.Volcanic: return (3.3, 3.7);
                case MoonKind.Ocean: return (2.6, 3.1);
                case MoonKind.Hazy: return (1.7, 2.0);
                default: return (4.0, 5.5);
            }
        }

        private static double BaseAlbedo(MoonKind kind)
        {
            switch (kind)
            {
                case MoonKind.Barren: return 0.12;
                case MoonKind.Ice: return 0.5;
                case MoonKind.Volcanic: return 0.6;
                case MoonKind.Ocean: return 0.65;
                case MoonKind.Hazy: return 0.22;
                default: return 0.30;
            }
        }

        /// <summary>
        /// Peale's tidal heat flow in W/m², (21/2)(k₂/Q) G M² R³ n e² / (4π a⁶), with a small drawn eccentricity; a
        /// volcanic or ocean moon has at least its kind's drawn floor (resonances pump it), and no moon more than its
        /// kind's cap. Stream <c>tides</c>; draws: always two.
        /// </summary>
        internal static (double Flow, double Floor, double Cap) Tides(ulong seed, Moon m, Planet planet, double distanceKm)
        {
            var rng = Seeds.Stream(seed, "tides");
            var e = StarSystemGenerator.LogUniform(rng, 0.0001, 0.003);
            var floorDraw = rng.NextDouble();
            double k2q, floor = 0, cap = 0.5;
            switch (m.Kind)
            {
                case MoonKind.Volcanic: k2q = 0.015; floor = DMath.Exp(floorDraw * DMath.Log(4)); cap = 10; break;
                case MoonKind.Ocean: k2q = 0.01; floor = 0.02 * DMath.Exp(floorDraw * DMath.Log(15)); break;
                case MoonKind.Ice:
                case MoonKind.Hazy: k2q = 0.003; break;
                default: k2q = 0.001; break;
            }

            var a = distanceKm * 1000;
            var r = m.Radius * 1000;
            var gm = Gravitation * planet.Mass * KgPerEarthMass;
            var n = Math.Sqrt(gm / (a * a * a));
            var a3 = a * a * a;
            var flow = 10.5 * k2q * gm * planet.Mass * KgPerEarthMass * r * r * r * n * e * e / (4 * DMath.PI * a3 * a3);
            return (flow, floor, cap);
        }

        /// <summary>The air and the surface temperature it gives. Draws: always two.</summary>
        private static (Atmosphere Air, double Temperature) Air(Pcg32 rng, MoonKind kind, double teq, double warmed, double escape, double inventory, double targetDraw)
        {
            var pressureDraw = rng.NextDouble();
            var icyAir = rng.Chance(25, 100);

            // The planet level's Jeans rule, on the starlight's temperature.
            var lightest = DMath.Round(0.8976 * 4 * teq / (escape * escape), 1);
            Gas[] candidates;
            double low = 0, high = 0;
            switch (kind)
            {
                case MoonKind.Ice: candidates = icyAir ? s_nitrogenMethane : Array.Empty<Gas>(); low = 0.0001; high = 0.009; break;
                case MoonKind.Volcanic: candidates = s_sulphurDioxide; low = 0.0001; high = 0.001; break;
                case MoonKind.Hazy: candidates = s_nitrogenMethane; low = 0.5; high = 5; break;
                case MoonKind.Garden: candidates = s_nitrogenOxygen; low = 0.5; high = 3; break;
                default: candidates = Array.Empty<Gas>(); break;
            }

            var kept = new List<Gas>();
            var lost = new List<Gas>();
            foreach (var g in candidates)
            {
                (PlanetDetail.GasMass(g) >= lightest ? kept : lost).Add(g);
            }

            // A hazy or garden moon's kind promises its air: it keeps it, and the why says what renews it.
            var renewed = (kind == MoonKind.Hazy || kind == MoonKind.Garden) && lost.Count > 0;
            if (renewed)
            {
                kept.Clear();
                kept.AddRange(candidates);
                lost.Clear();
            }

            var water = kind == MoonKind.Garden && inventory > 0;
            double pressure;
            if (candidates.Length == 0)
            {
                pressure = 0;
            }
            else if (kind == MoonKind.Garden)
            {
                var k = 0.6 + (water ? 0.25 : 0);
                var target = 275 + 25 * targetDraw;
                pressure = PlanetDetail.Solve(target, warmed, k, low, high);
                if (pressure >= high)
                {
                    // As on garden worlds near the outer edge of the habitable zone, carbon dioxide keeps it warm.
                    kept.Add(Gas.CarbonDioxide);
                    pressure = PlanetDetail.Solve(target, warmed, k + 0.6, low, high);
                }
            }
            else
            {
                pressure = low * DMath.Exp(pressureDraw * DMath.Log(high / low));
            }

            string why;
            if (pressure > 0 && kept.Count == 0)
            {
                // Nothing it was given can stay: a trace of the heaviest, leaking away.
                kept.Add(lost[lost.Count - 1]);
                lost.Clear();
                why = kind == MoonKind.Volcanic
                    ? "its volcanoes breathe out sulphur dioxide faster than it leaks away to space"
                    : "a breath of " + PlanetStory.GasName(kept[0]) + " leaking away to space";
            }
            else if (pressure == 0)
            {
                why = kind == MoonKind.Ice || kind == MoonKind.Ocean ? "its gases lie frozen on the surface"
                    : lightest > 44 ? "too little gravity for its heat: even carbon dioxide escapes"
                    : "lost its air long ago";
            }
            else if (renewed)
            {
                why = kind == MoonKind.Hazy
                    ? "too light to hold its air for ever: cryovolcanoes renew the nitrogen and methane as fast as they leak"
                    : "too light to hold its air for ever: life and volcanoes renew it as fast as it leaks";
            }
            else if (kind == MoonKind.Garden)
            {
                why = "life keeps oxygen in the air";
            }
            else if (kind == MoonKind.Hazy)
            {
                why = "its gravity holds nitrogen and methane under an orange haze";
            }
            else if (kind == MoonKind.Volcanic)
            {
                why = "its volcanoes keep a thin haze of sulphur dioxide";
            }
            else
            {
                why = "its gravity holds a thin breath of " + PlanetStory.GasList(kept);
            }

            pressure = DMath.Round(pressure, 4);
            var greenhouse = kept.Count == 0 ? 0 : (kept[0] == Gas.CarbonDioxide ? 1.2 : 0.6) + (water ? 0.25 : 0) + (PlanetDetail.Has(kept, Gas.CarbonDioxide) && kept[0] != Gas.CarbonDioxide ? 0.6 : 0);
            var atmosphere = new Atmosphere
            {
                Class = PlanetDetail.Classify(pressure),
                Pressure = pressure,
                Gases = pressure == 0 ? Array.Empty<Gas>() : kept.ToArray(),
                LightestGasKept = lightest,
                Breathable = pressure >= 0.5 && pressure <= 3 && PlanetDetail.Has(kept, Gas.Oxygen),
                Why = why,
            };
            return (atmosphere, DMath.Round(PlanetDetail.Greenhouse(warmed, greenhouse, pressure), 0));
        }

        /// <summary>Life level, flora and fauna. Draws: always three.</summary>
        private static (LifeLevel Life, int Flora, int Fauna) Life(Pcg32 rng, MoonKind kind, bool ocean, int temperature, StellarAge age)
        {
            if (kind == MoonKind.Garden)
            {
                return PlanetDetail.Life(rng, PlanetKind.Garden, temperature, age);
            }

            var roll = rng.NextInt(100);
            rng.NextInt(4);
            rng.NextInt(5);
            switch (kind)
            {
                case MoonKind.Ocean:
                    // Under the ice: no plants, and nothing yet that is complex.
                    return roll < 45 ? (LifeLevel.None, 0, 0) : roll < 65 ? (LifeLevel.Prebiotic, 0, 0) : roll < 90 ? (LifeLevel.Microbial, 0, 0) : (LifeLevel.Simple, 0, 0);
                case MoonKind.Ice:
                case MoonKind.Hazy:
                    if (ocean)
                    {
                        return roll < 5 ? (LifeLevel.Microbial, 0, 0) : roll < 15 ? (LifeLevel.Prebiotic, 0, 0) : (LifeLevel.None, 0, 0);
                    }

                    // Titan's tholins: the chemistry, in the cold, without the life.
                    return kind == MoonKind.Hazy && roll < 30 ? (LifeLevel.Prebiotic, 0, 0) : (LifeLevel.None, 0, 0);
                default:
                    return (LifeLevel.None, 0, 0);
            }
        }

        /// <summary>The planet level's grades with a base by moon kind. Draws: always five.</summary>
        private static ResourceGrades Resources(Pcg32 rng, Moon m, Planet planet)
        {
            var far = planet.Zone == OrbitZone.Cold || planet.Zone == OrbitZone.Outer;
            int metals, rare, ices, gases;
            switch (m.Kind)
            {
                case MoonKind.Barren: (metals, rare, ices, gases) = (3, 2, far ? 1 : 0, 0); break;
                case MoonKind.Volcanic: (metals, rare, ices, gases) = (3, 4, 0, 1); break;
                case MoonKind.Hazy: (metals, rare, ices, gases) = (1, 1, 4, 4); break;
                case MoonKind.Garden: (metals, rare, ices, gases) = (3, 2, 4, 1); break;
                default: (metals, rare, ices, gases) = (1, 1, 5, 1); break;
            }

            var organics = PlanetDetail.Organics(m.Life, m.Flora, m.Atmosphere);
            if (m.Kind == MoonKind.Hazy)
            {
                // Hydrocarbon dunes and lakes.
                organics = Math.Max(organics, 3);
            }

            return PlanetDetail.Grades(rng, metals, rare, ices, gases, organics);
        }
    }
}
