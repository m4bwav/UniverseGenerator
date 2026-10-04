using System;
using UnityEngine;

namespace UniverseGeneration.Unity
{
    /// <summary>Which plane a map lies in when it becomes a <see cref="Vector3"/>.</summary>
    public enum MapPlane
    {
        /// <summary>The ground plane of a Y-up 3D scene: map X goes to X, map Y to Z.</summary>
        XZ,

        /// <summary>The screen plane of a 2D scene: map X goes to X, map Y to Y.</summary>
        XY,
    }

    /// <summary>
    /// Unity float helpers for the generator's double-precision records: map positions as <see cref="Vector2"/> and
    /// <see cref="Vector3"/>, lane end points, and where a planet is on its orbit. These are for drawing: the generated
    /// values (doubles) are what the seed promise covers, and these floats are made from them on demand.
    /// </summary>
    public static class UnityVectors
    {
        /// <summary>A system's position on its galaxy map, times <paramref name="scale"/> (game units to scene units).</summary>
        public static Vector2 ToVector2(this MapEntry entry, float scale = 1f) => V2(entry.X, entry.Y, scale);

        /// <summary>A system's position in a scene, on <paramref name="plane"/>.</summary>
        public static Vector3 ToVector3(this MapEntry entry, float scale = 1f, MapPlane plane = MapPlane.XZ) => V3(entry.X, entry.Y, scale, plane);

        /// <summary>A galaxy's position on its cluster map.</summary>
        public static Vector2 ToVector2(this ClusterEntry entry, float scale = 1f) => V2(entry.X, entry.Y, scale);

        /// <summary>A galaxy's position in a scene, on <paramref name="plane"/>.</summary>
        public static Vector3 ToVector3(this ClusterEntry entry, float scale = 1f, MapPlane plane = MapPlane.XZ) => V3(entry.X, entry.Y, scale, plane);

        /// <summary>A group's or cluster's position on the universe map.</summary>
        public static Vector2 ToVector2(this UniverseNode node, float scale = 1f) => V2(node.X, node.Y, scale);

        /// <summary>A group's or cluster's position in a scene, on <paramref name="plane"/>.</summary>
        public static Vector3 ToVector3(this UniverseNode node, float scale = 1f, MapPlane plane = MapPlane.XZ) => V3(node.X, node.Y, scale, plane);

        /// <summary>A void's centre on the universe map; its <c>Radius</c> times the same scale gives its size.</summary>
        public static Vector2 ToVector2(this CosmicVoid hole, float scale = 1f) => V2(hole.X, hole.Y, scale);

        /// <summary>A void's centre in a scene, on <paramref name="plane"/>.</summary>
        public static Vector3 ToVector3(this CosmicVoid hole, float scale = 1f, MapPlane plane = MapPlane.XZ) => V3(hole.X, hole.Y, scale, plane);

        /// <summary>A hazard area's centre on its galaxy map; its <c>Radius</c> times the same scale gives its size.</summary>
        public static Vector2 ToVector2(this GalaxyHazard hazard, float scale = 1f) => V2(hazard.X, hazard.Y, scale);

        /// <summary>A hazard area's centre in a scene, on <paramref name="plane"/>.</summary>
        public static Vector3 ToVector3(this GalaxyHazard hazard, float scale = 1f, MapPlane plane = MapPlane.XZ) => V3(hazard.X, hazard.Y, scale, plane);

        /// <summary>The two ends of <paramref name="lane"/> in <paramref name="galaxy"/>'s scene, for a line renderer.</summary>
        public static (Vector3 A, Vector3 B) Ends(this Lane lane, Galaxy galaxy, float scale = 1f, MapPlane plane = MapPlane.XZ)
        {
            if (lane == null)
            {
                throw new ArgumentNullException(nameof(lane));
            }

            if (galaxy == null)
            {
                throw new ArgumentNullException(nameof(galaxy));
            }

            return (galaxy.Map[lane.A].ToVector3(scale, plane), galaxy.Map[lane.B].ToVector3(scale, plane));
        }

        /// <summary>
        /// Where <paramref name="planet"/> is <paramref name="days"/> after it passed its periapsis, in au from its star
        /// times <paramref name="scale"/>, in a Y-up scene: its orbit lies in the XZ plane, turned by its periapsis angle
        /// and tilted by its inclination. Kepler's equation is solved in floats, which is plenty for drawing.
        /// </summary>
        public static Vector3 OrbitPosition(this Planet planet, float days, float scale = 1f)
        {
            if (planet == null)
            {
                throw new ArgumentNullException(nameof(planet));
            }

            var a = (float)planet.Orbit;
            var e = Mathf.Clamp((float)planet.Eccentricity, 0f, 0.99f);
            var period = Mathf.Max((float)planet.Period, 1e-6f);
            var mean = 2f * Mathf.PI * Mathf.Repeat(days / period, 1f);
            var anomaly = mean;
            for (var i = 0; i < 8; i++)
            {
                anomaly -= (anomaly - e * Mathf.Sin(anomaly) - mean) / (1f - e * Mathf.Cos(anomaly));
            }

            var x = a * (Mathf.Cos(anomaly) - e);
            var z = a * Mathf.Sqrt(1f - e * e) * Mathf.Sin(anomaly);
            var inPlane = Quaternion.Euler(0f, -(float)planet.PeriapsisAngle, 0f) * new Vector3(x, 0f, z);
            return Quaternion.Euler((float)planet.Inclination, 0f, 0f) * inPlane * scale;
        }

        private static Vector2 V2(double x, double y, float scale) => new Vector2((float)x * scale, (float)y * scale);

        private static Vector3 V3(double x, double y, float scale, MapPlane plane) =>
            plane == MapPlane.XY ? new Vector3((float)x * scale, (float)y * scale, 0f) : new Vector3((float)x * scale, 0f, (float)y * scale);
    }
}
