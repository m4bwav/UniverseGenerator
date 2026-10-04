#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// A planet on its own (<see cref="Planet.Generate(string, GeneratorOptions?)"/>, address <c>v1-my-seed/planet</c>).
    /// Its context comes from its own streams, as a lone system's does: age, a single star and the star's name; then a
    /// zone, an orbit in it and the body by the system level's own <see cref="StarSystemGenerator.Draw"/>. Rings, moons
    /// and detail come from the same streams as a planet in a system.
    /// </summary>
    internal static class PlanetGenerator
    {
        // Hot, warm, temperate, cold, outer: games want temperate worlds more often than a real system holds them.
        private static readonly int[] s_zoneWeights = { 10, 15, 35, 15, 25 };

        public static Planet Generate(Address address, GeneratorOptions options)
        {
            var seed = address.ObjectSeed;
            var age = StarSystemGenerator.DrawAge(seed);
            var (star, _) = StarGenerator.Roll(Seeds.Stream(seed, "star"), age, options.StarMix);
            var starName = InventedNames.SystemName(Seeds.Stream(seed, "names"), star.Class, options.Names);
            var light = Math.Max(star.Luminosity, 0.0001);
            var (hzInner, hzOuter, frost) = StarSystemGenerator.Zones(light);

            var body = Seeds.Stream(seed, "body");
            var target = (OrbitZone)body.Weighted(s_zoneWeights);
            double low, high;
            switch (target)
            {
                case OrbitZone.Hot: low = hzInner * 0.1; high = hzInner * 0.5; break;
                case OrbitZone.Warm: low = hzInner * 0.5; high = hzInner; break;
                case OrbitZone.Temperate: low = hzInner; high = hzOuter; break;
                case OrbitZone.Cold: low = hzOuter; high = frost; break;
                default: low = frost; high = frost * 4; break;
            }

            // Never inside three star radii; the zone is then read back from the orbit, so it always agrees with it.
            low = Math.Max(low, Math.Max(0.01, star.Radius * StarSystemGenerator.AuPerSolarRadius * 3));
            high = Math.Max(high, low * 1.2);
            var orbit = StarSystemGenerator.LogUniform(body, low, high);
            var zone = StarSystemGenerator.Zone(orbit, hzInner, hzOuter, frost);
            var giantWeight = (int)(30 * DMath.Clamp(star.Mass, 0.15, 1.6));
            var pod = (Rocky: StarSystemGenerator.LogUniform(body, 0.25, 2.2), Gassy: StarSystemGenerator.LogUniform(body, 4.7, 12));
            var draft = StarSystemGenerator.Draw(body, orbit, star.Mass, zone, giantWeight, false, pod, StarSystemGenerator.NoGarden(star.Class));

            var host = new PlanetHost(star, star.Mass, light, age);
            return StarSystemGenerator.BuildPlanet(draft, 0, seed, address, starName + " b", host, options.Weirdness, new StarSystemGenerator.OrbitShape(false, 0.6));
        }
    }
}
