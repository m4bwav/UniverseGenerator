#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>What a planet's detail needs from its system: the star, the mass and light it orbits, the region's age.</summary>
    internal sealed class PlanetHost
    {
        public PlanetHost(Star star, double totalMass, double light, StellarAge age)
        {
            Star = star;
            TotalMass = totalMass;
            Light = light;
            Age = age;
        }

        public Star Star { get; }

        /// <summary>The mass the planet orbits in Suns (both stars of a close pair).</summary>
        public double TotalMass { get; }

        /// <summary>The light the planet receives in Suns (both stars of a close pair; at least 0.0001).</summary>
        public double Light { get; }

        public StellarAge Age { get; }
    }

    /// <summary>
    /// The planet level (plan P1 to P17, A7), following ai-docs/notes/2026-10-02-planet-level-design.md. Everything here
    /// is derived from a planet's system-level fields and from new streams of the planet's own seed, so a system's
    /// existing output never changes. Each stream draws a fixed number of values whatever it decides.
    /// </summary>
    internal static class PlanetDetail
    {
        // Molecular masses in Gas order; rock vapour is resupplied by the molten surface and never escapes here.
        private static readonly double[] s_gasMass = { 2, 4, 16, 18, 28, 32, 44, 64, 0 };
        private static readonly Gas[] s_hydrogenHelium = { Gas.Hydrogen, Gas.Helium };
        private static readonly Gas[] s_iceGiantGases = { Gas.Hydrogen, Gas.Helium, Gas.Methane };
        private static readonly Gas[] s_subNeptuneGases = { Gas.Hydrogen, Gas.Helium, Gas.WaterVapour };
        private static readonly Gas[] s_rockVapour = { Gas.RockVapour };
        private static readonly Gas[] s_nitrogenFirst = { Gas.Nitrogen, Gas.CarbonDioxide };
        private static readonly Gas[] s_carbonFirst = { Gas.CarbonDioxide, Gas.Nitrogen };
        private static readonly Gas[] s_greenhouseGases = { Gas.CarbonDioxide, Gas.Nitrogen, Gas.SulphurDioxide };
        private static readonly Gas[] s_oceanGases = { Gas.Nitrogen, Gas.CarbonDioxide, Gas.WaterVapour };
        private static readonly Gas[] s_gardenGases = { Gas.Nitrogen, Gas.Oxygen };
        private static readonly Gas[] s_iceGases = { Gas.Nitrogen, Gas.Methane };

        // Six latitude bands, both hemispheres together: area shares, P2(sin of the middle latitude), and a Hadley rain
        // pattern (wet equator, dry subtropics, wet middle latitudes, dry poles).
        private static readonly int[] s_latitudeFrom = { 0, 15, 30, 45, 60, 75 };
        private static readonly int[] s_latitudeTo = { 15, 30, 45, 60, 75, 90 };
        private static readonly double[] s_latitudeShare = { 0.2588, 0.2412, 0.2071, 0.1589, 0.0999, 0.0341 };
        private static readonly double[] s_latitudeP2 = { -0.4744, -0.2803, 0.0559, 0.4441, 0.7803, 0.9744 };
        private static readonly double[] s_latitudeRain = { 1.0, 0.35, 0.65, 0.8, 0.45, 0.15 };

        // A locked planet's bands by angle from the point under its star; 80 to 100 is the terminator. "Light" places
        // each band between the night and day temperatures; its area-weighted mean is LockedMean.
        private static readonly int[] s_lockedFrom = { 0, 30, 60, 80, 100, 140 };
        private static readonly int[] s_lockedTo = { 30, 60, 80, 100, 140, 180 };
        private static readonly double[] s_lockedShare = { 0.0670, 0.1830, 0.1632, 0.1736, 0.2962, 0.1170 };
        private static readonly double[] s_lockedLight = { 1.0, 0.8, 0.55, 0.4, 0.15, 0.0 };
        private static readonly double[] s_lockedRain = { 1.0, 0.7, 0.5, 0.45, 0.15, 0.05 };
        private const double LockedMean = 0.417;

        public static bool IsGiant(PlanetKind k) =>
            k == PlanetKind.SubNeptune || k == PlanetKind.GasGiant || k == PlanetKind.IceGiant || k == PlanetKind.HotJupiter;

        public static bool Has(IReadOnlyList<Gas> gases, Gas gas)
        {
            foreach (var g in gases)
            {
                if (g == gas)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>A gas's molecular mass, for the Jeans rule (0 for rock vapour, which never escapes here).</summary>
        public static double GasMass(Gas gas) => s_gasMass[(int)gas];

        public static AtmosphereClass Classify(double pressure) =>
            pressure == 0 ? AtmosphereClass.None
            : pressure < 0.01 ? AtmosphereClass.Trace
            : pressure < 0.5 ? AtmosphereClass.Thin
            : pressure < 2 ? AtmosphereClass.Standard
            : pressure < 10 ? AtmosphereClass.Dense
            : AtmosphereClass.Crushing;

        /// <summary>The grey greenhouse: τ = k P^1.1 warms <paramref name="teq"/> to T_eq (1 + 0.75 τ)^¼.</summary>
        public static double Greenhouse(double teq, double k, double pressure) =>
            pressure == 0 ? teq : teq * Math.Sqrt(Math.Sqrt(1 + 0.75 * k * DMath.Pow(pressure, 1.1)));

        /// <summary>How well <paramref name="species"/> could live with this air, temperature (K) and gravity (g).</summary>
        public static Habitability HabitabilityOf(Species species, Atmosphere atmosphere, double t, double gravity)
        {
            if (atmosphere.Pressure is null)
            {
                return Habitability.Hostile;
            }

            var p = atmosphere.Pressure.Value;
            var air = !species.BreathesOxygen || Has(atmosphere.Gases, Gas.Oxygen);
            if (air && t >= species.MinTemperature && t <= species.MaxTemperature && gravity >= species.MinGravity
                && gravity <= species.MaxGravity && p >= species.MinPressure && p <= species.MaxPressure)
            {
                return Habitability.Ideal;
            }

            if (t >= species.MinTemperature - 20 && t <= species.MaxTemperature + 20 && gravity >= species.MinGravity * 0.5
                && gravity <= species.MaxGravity * 1.25 && p >= species.MinPressure / 5 && p <= species.MaxPressure * 3)
            {
                return Habitability.Habitable;
            }

            if (t >= species.MinTemperature - 80 && t <= species.MaxTemperature + 80 && gravity <= species.MaxGravity * 1.6
                && p <= species.MaxPressure * 10)
            {
                return Habitability.Marginal;
            }

            return Habitability.Hostile;
        }

        public static Planet Apply(Planet p, PlanetHost host, ulong seed, int weirdness)
        {
            var giant = IsGiant(p.Kind);

            // 1. Physics (P9), from the stored (rounded) mass and radius.
            var gravity = DMath.Round(p.Mass / (p.Radius * p.Radius), 2);
            var escape = DMath.Round(11.186 * Math.Sqrt(p.Mass / p.Radius), 2);
            var density = DMath.Round(5.514 * p.Mass / (p.Radius * p.Radius * p.Radius), 2);

            // 2. Light, albedo and water (P10, P13).
            var surface = Seeds.Stream(seed, "surface");
            var albedo = DMath.Round(BaseAlbedo(p.Kind) + surface.Range(-0.05, 0.05), 2);
            var waterDraw = surface.NextDouble();
            var targetDraw = surface.NextDouble();
            var flux = host.Light / (p.Orbit * p.Orbit);
            var teq = 278.6 * Math.Sqrt(Math.Sqrt(flux * (1 - albedo)));
            double inventory;
            switch (p.Kind)
            {
                case PlanetKind.Ocean: inventory = 0.92 + 0.08 * waterDraw; break;
                case PlanetKind.Garden: inventory = 0.3 + 0.55 * waterDraw; break;
                case PlanetKind.Ice: inventory = 0.6 + 0.4 * waterDraw; break;
                default: inventory = 0; break;
            }

            inventory = DMath.Round(inventory, 2);

            // 3 and 4. Atmosphere by the Jeans rule (P11), then the surface temperature.
            var (atmosphere, temperature) = Air(Seeds.Stream(seed, "atmosphere"), p, teq, escape, inventory, targetDraw);
            var pressure = atmosphere.Pressure ?? 0;

            // 5. Rotation, tilt and tidal lock (P12), day and night sides (A7).
            var (rotation, spin, tilt) = Turn(Seeds.Stream(seed, "rotation"), p, host, giant);
            var redistribution = giant ? 1 : pressure / (pressure + 0.1);
            var swing = 1 - redistribution;
            if (spin == Spin.Free)
            {
                swing *= Math.Min(1, rotation / 100);
            }

            var day = DMath.Round(temperature * (1 + 0.3 * swing), 0);
            var night = DMath.Round(temperature * (1 - 0.7 * swing), 0);

            // 7. Life (P3); it decides whether plants cover the land below.
            var (life, flora, fauna) = Life(Seeds.Stream(seed, "life"), p.Kind, (int)temperature, host.Age);

            // 6 and 8. Climate bands and biomes (P15).
            var bands = Array.Empty<ClimateBand>();
            var biomes = Array.Empty<BiomeShare>();
            double water = 0, ice = 0;
            if (!giant)
            {
                (bands, biomes, water, ice) = Climate(p.Kind, spin, temperature, day, night, tilt, redistribution, pressure, inventory, flora);
            }

            var planet = p with
            {
                Composition = CompositionOf(p.Kind),
                Gravity = gravity,
                EscapeVelocity = escape,
                Density = density,
                Insolation = DMath.Round(flux, 4),
                Albedo = albedo,
                Temperature = temperature,
                DayTemperature = day,
                NightTemperature = night,
                Atmosphere = atmosphere,
                Water = water,
                Ice = ice,
                Rotation = rotation,
                Spin = spin,
                Tilt = tilt,
                Bands = bands,
                Biomes = biomes,
                Life = life,
                Flora = flora,
                Fauna = fauna,
            };

            // 9 to 13. Traits and anomalies (P1), resources (P6), hazards (P4), habitability (P17), summary (P2).
            planet = planet with
            {
                Traits = PlanetStory.Traits(Seeds.Stream(seed, "traits"), planet),
                Anomaly = PlanetStory.Anomaly(Seeds.Stream(seed, "anomaly"), giant, weirdness),
                Resources = Resources(Seeds.Stream(seed, "resources"), planet),
            };
            var (hazard, hazards) = PlanetStory.Hazards(planet, host.Star);
            planet = planet with { Hazard = hazard, Hazards = hazards, Similarity = Similarity(planet) };
            planet = planet with { Habitability = planet.HabitabilityFor(Species.Human) };
            return planet with { Summary = PlanetStory.Summary(planet) };
        }

        private static double BaseAlbedo(PlanetKind kind)
        {
            switch (kind)
            {
                case PlanetKind.Lava: return 0.10;
                case PlanetKind.Iron: return 0.12;
                case PlanetKind.Barren: return 0.12;
                case PlanetKind.Desert: return 0.28;
                case PlanetKind.Rocky: return 0.22;
                case PlanetKind.Greenhouse: return 0.75;
                case PlanetKind.Ocean: return 0.30;
                case PlanetKind.Garden: return 0.30;
                case PlanetKind.Ice: return 0.60;
                case PlanetKind.Dwarf: return 0.45;
                case PlanetKind.SubNeptune: return 0.35;
                case PlanetKind.GasGiant: return 0.34;
                case PlanetKind.IceGiant: return 0.30;
                default: return 0.08;
            }
        }

        private static Composition CompositionOf(PlanetKind kind)
        {
            switch (kind)
            {
                case PlanetKind.Iron: return Composition.Metal;
                case PlanetKind.Ice:
                case PlanetKind.Dwarf: return Composition.IceAndRock;
                case PlanetKind.Ocean: return Composition.Water;
                case PlanetKind.SubNeptune: return Composition.GasEnvelope;
                case PlanetKind.GasGiant:
                case PlanetKind.HotJupiter: return Composition.HydrogenHelium;
                case PlanetKind.IceGiant: return Composition.Ices;
                default: return Composition.Rock;
            }
        }

        /// <summary>The atmosphere and the surface temperature it gives. Draws: always three values.</summary>
        private static (Atmosphere Air, double Temperature) Air(Pcg32 rng, Planet p, double teq, double escape, double inventory, double targetDraw)
        {
            var pressureDraw = rng.NextDouble();
            var nitrogenFirst = rng.Chance(50, 100);
            var icyAir = rng.Chance(40, 100);

            // The Jeans rule: the exosphere runs about 4 times the equilibrium temperature; a gas stays when the escape
            // velocity is at least 6 times its thermal speed 0.1579 √(T / μ) km/s.
            var lightest = DMath.Round(0.8976 * 4 * teq / (escape * escape), 1);
            Gas[] candidates;
            double low = 0, high = 0;
            string? envelope = null;
            switch (p.Kind)
            {
                case PlanetKind.GasGiant:
                    candidates = s_hydrogenHelium;
                    envelope = "a deep envelope of hydrogen and helium; there is no surface";
                    break;
                case PlanetKind.HotJupiter:
                    candidates = s_hydrogenHelium;
                    envelope = "hydrogen and helium heated by its star; there is no surface";
                    break;
                case PlanetKind.IceGiant:
                    candidates = s_iceGiantGases;
                    envelope = "hydrogen, helium and methane over a mantle of ices; there is no surface";
                    break;
                case PlanetKind.SubNeptune:
                    candidates = s_subNeptuneGases;
                    envelope = "a thick envelope of hydrogen, helium and steam hides any surface";
                    break;
                case PlanetKind.Lava: candidates = s_rockVapour; low = 0.001; high = 0.009; break;
                case PlanetKind.Desert: candidates = nitrogenFirst ? s_nitrogenFirst : s_carbonFirst; low = 0.01; high = 0.6; break;
                case PlanetKind.Rocky: candidates = nitrogenFirst ? s_nitrogenFirst : s_carbonFirst; low = 0.05; high = 3; break;
                case PlanetKind.Greenhouse: candidates = s_greenhouseGases; low = 20; high = 150; break;
                case PlanetKind.Ocean: candidates = s_oceanGases; low = 0.5; high = 8; break;
                case PlanetKind.Garden: candidates = s_gardenGases; low = 0.5; high = 3; break;
                case PlanetKind.Ice: candidates = icyAir ? s_iceGases : Array.Empty<Gas>(); low = 0.001; high = 1.5; break;
                default: candidates = Array.Empty<Gas>(); break;
            }

            if (envelope != null)
            {
                var air = new Atmosphere { Class = AtmosphereClass.Envelope, Gases = candidates, LightestGasKept = lightest, Why = envelope };
                return (air, DMath.Round(teq, 0));
            }

            var kept = new List<Gas>();
            var lost = new List<Gas>();
            foreach (var g in candidates)
            {
                (s_gasMass[(int)g] >= lightest ? kept : lost).Add(g);
            }

            var water = inventory > 0 && (p.Kind == PlanetKind.Ocean || p.Kind == PlanetKind.Garden);
            var k = kept.Count == 0 ? 0 : (kept[0] == Gas.CarbonDioxide ? 1.2 : 0.6) + (water ? 0.25 : 0);
            // Kinds whose words promise a climate start from a temperature and solve for the pressure that gives it, so a
            // temperate ocean world never boils; the kind's pressure range still bounds it.
            double? target = p.Kind == PlanetKind.Garden ? 275 + 25 * targetDraw
                : p.Kind == PlanetKind.Ocean ? 275 + 45 * targetDraw
                : p.Zone == OrbitZone.Temperate && (p.Kind == PlanetKind.Rocky || p.Kind == PlanetKind.Desert) ? 250 + 70 * targetDraw
                : (double?)null;
            double pressure;
            if (candidates.Length == 0)
            {
                pressure = 0;
            }
            else if (target != null && k > 0)
            {
                pressure = Solve(target.Value, teq, k, low, high);
                if (pressure >= high && water && !Has(kept, Gas.CarbonDioxide) && 44 >= lightest)
                {
                    // Near the outer edge of the habitable zone, carbon dioxide keeps a living world warm.
                    kept.Add(Gas.CarbonDioxide);
                    k += 0.6;
                    pressure = Solve(target.Value, teq, k, low, high);
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
                pressure = Math.Min(pressure * 0.01, 0.009);
                why = "too little gravity for its heat: what air it has is leaking away to space";
            }
            else if (pressure == 0)
            {
                why = p.Kind == PlanetKind.Ice ? "its gases lie frozen on the surface"
                    : lightest > 44 ? "too little gravity for its heat: even carbon dioxide escapes"
                    : p.Zone == OrbitZone.Hot || p.Zone == OrbitZone.Warm ? "stripped by its star's heat and wind"
                    : "lost its air long ago";
            }
            else if (p.Kind == PlanetKind.Lava)
            {
                why = "a thin haze of vaporised rock";
            }
            else if (p.Kind == PlanetKind.Garden && Has(kept, Gas.Oxygen))
            {
                why = "life keeps oxygen in the air";
            }
            else if (p.Kind == PlanetKind.Greenhouse && kept[0] == Gas.CarbonDioxide)
            {
                why = "a runaway greenhouse: carbon dioxide traps the heat";
            }
            else
            {
                why = lost.Count > 0
                    ? "its gravity keeps " + PlanetStory.GasList(kept) + " but lets " + PlanetStory.GasList(lost) + " escape"
                    : "its gravity holds " + PlanetStory.GasList(kept);
            }

            pressure = DMath.Round(pressure, 4);
            var temperature = Greenhouse(teq, k, pressure);
            var atmosphere = new Atmosphere
            {
                Class = Classify(pressure),
                Pressure = pressure,
                Gases = pressure == 0 ? Array.Empty<Gas>() : kept.ToArray(),
                LightestGasKept = lightest,
                Breathable = pressure >= 0.5 && pressure <= 3 && Has(kept, Gas.Oxygen),
                Why = why,
            };
            return (atmosphere, DMath.Round(temperature, 0));
        }

        /// <summary>The pressure whose greenhouse (τ = k P^1.1) warms <paramref name="teq"/> to <paramref name="target"/>, within the kind's range.</summary>
        internal static double Solve(double target, double teq, double k, double low, double high)
        {
            var ratio = target / teq;
            var need = (ratio * ratio * ratio * ratio - 1) / 0.75;
            return need <= 0 ? low : DMath.Clamp(DMath.Pow(need / k, 1 / 1.1), low, high);
        }

        /// <summary>Rotation in hours, spin and tilt in degrees. Draws: always the same nine values.</summary>
        private static (double Rotation, Spin Spin, double Tilt) Turn(Pcg32 rng, Planet p, PlanetHost host, bool giant)
        {
            var slow = rng.Chance(10, 100);
            var fast = StarSystemGenerator.LogUniform(rng, 8, 60);
            var slowHours = StarSystemGenerator.LogUniform(rng, 200, 6000);
            var giantHours = rng.Range(9.0, 18.0);
            var subNeptuneHours = StarSystemGenerator.LogUniform(rng, 10, 40);
            var steep = rng.Chance(10, 100);
            var lean = rng.Range(0.0, 15.0) + rng.Range(0.0, 20.0);
            var steepTilt = rng.Range(35.0, 180.0);

            // Tidal locking radius for 4.5 billion years (Gladman et al. 1996 form), scaled by age; giants resist.
            var age = host.Age == StellarAge.Young ? 0.85 : host.Age == StellarAge.Old ? 1.1 : 1.0;
            var lockRadius = DMath.Round(0.8 * DMath.Pow(host.TotalMass, 1.0 / 3) * age * (giant ? 0.35 : 1), 4);
            Spin spin;
            if (p.Kind == PlanetKind.HotJupiter || p.Orbit < DMath.Round(0.6 * lockRadius, 4))
            {
                spin = Spin.Locked;
            }
            else
            {
                spin = !giant && p.Orbit < lockRadius ? Spin.Resonant : Spin.Free;
            }

            double hours;
            if (spin == Spin.Locked)
            {
                hours = p.Period * 24;
            }
            else if (spin == Spin.Resonant)
            {
                hours = p.Period * 16;
            }
            else if (p.Kind == PlanetKind.GasGiant || p.Kind == PlanetKind.IceGiant)
            {
                hours = giantHours;
            }
            else if (p.Kind == PlanetKind.SubNeptune)
            {
                hours = subNeptuneHours;
            }
            else
            {
                hours = slow ? slowHours : fast;
            }

            var tilt = spin != Spin.Free ? lean * 0.1 : steep ? steepTilt : lean;
            return (DMath.Round(hours, 1), spin, DMath.Round(tilt, 1));
        }

        /// <summary>Life level, flora and fauna. Draws: always three.</summary>
        internal static (LifeLevel Life, int Flora, int Fauna) Life(Pcg32 rng, PlanetKind kind, int temperature, StellarAge age)
        {
            var roll = rng.NextInt(100);
            var a = rng.NextInt(4);
            var b = rng.NextInt(5);
            var mild = temperature >= 250 && temperature <= 380;
            var young = age == StellarAge.Young ? 1 : 0;
            switch (kind)
            {
                case PlanetKind.Garden:
                    return (LifeLevel.Complex, 2 + Math.Min(a, 3), Math.Max(1, 1 + b - young));
                case PlanetKind.Ocean:
                    if (!mild)
                    {
                        return roll < 20 ? (LifeLevel.Microbial, 0, 0) : (LifeLevel.None, 0, 0);
                    }

                    return roll < 15 ? (LifeLevel.None, 0, 0)
                        : roll < 45 ? (LifeLevel.Microbial, 0, 0)
                        : roll < 75 ? (LifeLevel.Simple, 1, 0)
                        : (LifeLevel.Complex, 1 + Math.Min(a, 2), Math.Max(1, 1 + Math.Min(b, 3) - young));
                case PlanetKind.Rocky:
                    return !mild ? (LifeLevel.None, 0, 0) : roll < 15 ? (LifeLevel.Microbial, 0, 0) : roll < 35 ? (LifeLevel.Prebiotic, 0, 0) : (LifeLevel.None, 0, 0);
                case PlanetKind.Desert:
                    return !mild ? (LifeLevel.None, 0, 0) : roll < 8 ? (LifeLevel.Microbial, 0, 0) : roll < 20 ? (LifeLevel.Prebiotic, 0, 0) : (LifeLevel.None, 0, 0);
                case PlanetKind.Ice:
                    // Under the ice, in a hidden ocean.
                    return roll < 6 ? (LifeLevel.Microbial, 0, 0) : roll < 15 ? (LifeLevel.Prebiotic, 0, 0) : (LifeLevel.None, 0, 0);
                case PlanetKind.SubNeptune:
                case PlanetKind.GasGiant:
                case PlanetKind.IceGiant:
                    // Drifting in the clouds, where the temperature is mild.
                    return temperature >= 200 && temperature <= 350 && roll < 2 ? (LifeLevel.Microbial, 0, 0) : (LifeLevel.None, 0, 0);
                case PlanetKind.Greenhouse:
                    return roll < 3 ? (LifeLevel.Microbial, 0, 0) : (LifeLevel.None, 0, 0);
                default:
                    // Barren, iron, lava, dwarf and hot Jupiter: never, so the system tag "dead system" stays true.
                    return (LifeLevel.None, 0, 0);
            }
        }

        /// <summary>Six bands with their temperatures and biomes, the biome shares, and the ocean and ice shares. No draws.</summary>
        internal static (ClimateBand[] Bands, BiomeShare[] Biomes, double Water, double Ice) Climate(PlanetKind kind, Spin spin,
            double temperature, double day, double night, double tilt, double redistribution, double pressure, double inventory, int flora)
        {
            var locked = spin == Spin.Locked;
            var from = locked ? s_lockedFrom : s_latitudeFrom;
            var to = locked ? s_lockedTo : s_latitudeTo;
            var shares = locked ? s_lockedShare : s_latitudeShare;
            var pattern = locked ? s_lockedRain : s_latitudeRain;
            var folded = tilt > 90 ? 180 - tilt : tilt;
            var contrast = 0.43 * temperature * (1 - 0.8 * redistribution) * (1 - folded / 90);
            var openWater = inventory > 0 && (kind == PlanetKind.Ocean || kind == PlanetKind.Garden);
            var humidity = openWater ? DMath.Clamp(0.25 + 1.1 * inventory, 0, 1.3) : pressure >= 0.01 ? 0.1 : 0;

            var bands = new ClimateBand[6];
            var totals = new double[11];
            for (var i = 0; i < 6; i++)
            {
                var t = locked ? temperature + (day - night) * (s_lockedLight[i] - LockedMean) : temperature - contrast * s_latitudeP2[i];
                t = DMath.Round(Math.Max(t, 3), 0);
                var cold = DMath.Clamp((t - 240) / 60, 0, 1.2);
                var rain = (int)Math.Floor(pattern[i] * humidity * cold * 100 + 0.5);

                var waterBiome = kind == PlanetKind.Ice || t < 263 ? Biome.IceSheet : t >= 373 ? Biome.Desert : Biome.Ocean;
                var land = LandBiome(kind, (int)t, rain, inventory, flora);
                totals[(int)waterBiome] += shares[i] * inventory;
                totals[(int)land] += shares[i] * (1 - inventory);
                bands[i] = new ClimateBand
                {
                    From = from[i],
                    To = to[i],
                    Share = shares[i],
                    Temperature = t,
                    Biome = inventory >= 0.99 ? waterBiome : land,
                };
            }

            var list = new List<BiomeShare>();
            for (var b = 0; b < totals.Length; b++)
            {
                var share = DMath.Round(totals[b], 2);
                if (share > 0)
                {
                    // Insertion keeps the largest first and, on a tie, the biome order.
                    var at = list.Count;
                    while (at > 0 && list[at - 1].Share < share)
                    {
                        at--;
                    }

                    list.Insert(at, new BiomeShare { Biome = (Biome)b, Share = share });
                }
            }

            return (bands, list.ToArray(), DMath.Round(totals[(int)Biome.Ocean], 2), DMath.Round(totals[(int)Biome.IceSheet], 2));
        }

        /// <summary>A Whittaker table on temperature (kelvin) and rain (percent) where plants grow; bare ground elsewhere.</summary>
        private static Biome LandBiome(PlanetKind kind, int t, int rain, double inventory, int flora)
        {
            if (kind == PlanetKind.Lava)
            {
                return Biome.Volcanic;
            }

            if (t < 263)
            {
                return inventory > 0.05 || kind == PlanetKind.Ice ? Biome.IceSheet : Biome.Barren;
            }

            if (flora == 0)
            {
                return kind == PlanetKind.Desert ? Biome.Desert : Biome.Barren;
            }

            var c = t - 273;
            if (c < 0)
            {
                return Biome.Tundra;
            }

            if (c < 5)
            {
                return rain >= 35 ? Biome.Taiga : Biome.Tundra;
            }

            if (c < 20)
            {
                return rain < 20 ? Biome.Desert : rain < 45 ? Biome.Grassland : Biome.TemperateForest;
            }

            if (c < 40)
            {
                return rain < 20 ? Biome.Desert : rain < 55 ? Biome.Savanna : Biome.Rainforest;
            }

            return c < 60 ? (rain < 50 ? Biome.Desert : Biome.Savanna) : Biome.Barren;
        }

        /// <summary>Grades by kind, each moved by -1, 0 or +1 (a zero stays zero); organics from life. Draws: always five.</summary>
        private static ResourceGrades Resources(Pcg32 rng, Planet p)
        {
            var far = p.Zone == OrbitZone.Cold || p.Zone == OrbitZone.Outer;
            int metals, rare, ices, gases;
            switch (p.Kind)
            {
                case PlanetKind.Lava: (metals, rare, ices, gases) = (3, 4, 0, 0); break;
                case PlanetKind.Iron: (metals, rare, ices, gases) = (5, 3, 0, 0); break;
                case PlanetKind.Barren: (metals, rare, ices, gases) = (3, 2, far ? 1 : 0, 0); break;
                case PlanetKind.Desert: (metals, rare, ices, gases) = (2, 2, 0, 1); break;
                case PlanetKind.Rocky: (metals, rare, ices, gases) = (3, 2, 1, 1); break;
                case PlanetKind.Greenhouse: (metals, rare, ices, gases) = (2, 2, 0, 2); break;
                case PlanetKind.Ocean: (metals, rare, ices, gases) = (1, 1, 5, 1); break;
                case PlanetKind.Garden: (metals, rare, ices, gases) = (3, 2, 4, 1); break;
                case PlanetKind.Ice: (metals, rare, ices, gases) = (1, 1, 5, 2); break;
                case PlanetKind.Dwarf: (metals, rare, ices, gases) = (2, 1, far ? 4 : 1, 1); break;
                case PlanetKind.SubNeptune: (metals, rare, ices, gases) = (0, 0, 3, 3); break;
                case PlanetKind.GasGiant: (metals, rare, ices, gases) = (0, 0, 0, 5); break;
                case PlanetKind.IceGiant: (metals, rare, ices, gases) = (0, 0, 3, 4); break;
                default: (metals, rare, ices, gases) = (0, 0, 0, 2); break;
            }

            return Grades(rng, metals, rare, ices, gases, Organics(p.Life, p.Flora, p.Atmosphere));
        }

        /// <summary>Organics from life; without life, 1 where methane lies in air over a surface.</summary>
        internal static int Organics(LifeLevel life, int flora, Atmosphere air)
        {
            switch (life)
            {
                case LifeLevel.Complex: return 2 + flora / 2;
                case LifeLevel.Simple: return 2;
                case LifeLevel.Microbial:
                case LifeLevel.Prebiotic: return 1;
                default: return Has(air.Gases, Gas.Methane) && air.Class != AtmosphereClass.Envelope ? 1 : 0;
            }
        }

        /// <summary>Each base moved by -1, 0 or +1 and kept in 1 to 5; a zero stays zero. Draws: always five.</summary>
        internal static ResourceGrades Grades(Pcg32 rng, int metals, int rare, int ices, int gases, int organics)
        {
            int Grade(int value, int step) => value == 0 ? 0 : DMath.Clamp(value + step - 1, 1, 5);
            return new ResourceGrades
            {
                Metals = Grade(metals, rng.NextInt(3)),
                RareElements = Grade(rare, rng.NextInt(3)),
                Ices = Grade(ices, rng.NextInt(3)),
                Gases = Grade(gases, rng.NextInt(3)),
                Organics = Grade(organics, rng.NextInt(3)),
            };
        }

        private static double Similarity(Planet p) => Similarity(p.Radius, p.Density, p.EscapeVelocity, p.Temperature);

        /// <summary>The Earth Similarity Index (Schulze-Makuch et al. 2011) from radius (Earths), density, escape velocity and temperature.</summary>
        internal static double Similarity(double radius, double density, double escape, double temperature)
        {
            static double Term(double x, double earth, double weight) => DMath.Pow(1 - Math.Abs(x - earth) / (x + earth), weight / 4);
            return DMath.Round(Term(radius, 1, 0.57) * Term(density, 5.51, 1.07) * Term(escape, 11.19, 0.70) * Term(temperature, 288, 5.58), 2);
        }
    }
}
