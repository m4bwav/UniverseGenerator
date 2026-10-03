#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>The three maps, each with its own units so game coordinates stay small enough for floats.</summary>
    public enum MapLevel
    {
        /// <summary>A galaxy map: radius 1,000 game units, 50,000 light-years.</summary>
        Galaxy,

        /// <summary>A cluster map: radius 1,000 cluster units, 2 million light-years.</summary>
        Cluster,

        /// <summary>The universe map: radius 1,000 universe units, 100 million light-years.</summary>
        Universe,
    }

    /// <summary>
    /// Two distance frames (plan U3): the map units an engine draws with, and light-years for text, plus a stylised travel
    /// time for every kind of route. Nothing here draws from a seed; the numbers are for games and stories, not physics.
    /// </summary>
    public static class Distances
    {
        /// <summary>Light-years in one unit of a galaxy map.</summary>
        public const double GalaxyUnitLightYears = 50;

        /// <summary>Light-years in one unit of a cluster map.</summary>
        public const double ClusterUnitLightYears = 2000;

        /// <summary>Light-years in one unit of the universe map.</summary>
        public const double UniverseUnitLightYears = 100000;

        /// <summary>The light-years that <paramref name="units"/> of the map at <paramref name="level"/> stand for.</summary>
        public static double LightYears(MapLevel level, double units) =>
            units * (level == MapLevel.Galaxy ? GalaxyUnitLightYears : level == MapLevel.Cluster ? ClusterUnitLightYears : UniverseUnitLightYears);

        /// <summary>Days along a lane of a galaxy map: one day per 100 game units or part of one, at least one.</summary>
        public static int LaneDays(double length) => Math.Max(1, (int)Math.Ceiling(length / 100));

        /// <summary>
        /// Days through a link between galaxies or clusters: a gate 7, a wormhole 3, a tether 2, a tidal link 1, a filament
        /// 30 plus a day per 50 universe units of <paramref name="length"/> or part of them.
        /// </summary>
        public static int TravelDays(LinkTier tier, double length)
        {
            switch (tier)
            {
                case LinkTier.Gate: return 7;
                case LinkTier.Wormhole: return 3;
                case LinkTier.Tether: return 2;
                case LinkTier.Tidal: return 1;
                default: return 30 + (int)Math.Ceiling(length / 50);
            }
        }
    }
}
