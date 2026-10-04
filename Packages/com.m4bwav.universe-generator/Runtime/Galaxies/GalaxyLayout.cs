#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// The galaxy level's map (plan ideas 6 to 11): a shape as a density function, systems placed with a minimum
    /// spacing, lanes from the relative neighbourhood graph plus a share of the Gabriel graph's other edges, chokepoints
    /// and bridges, named regions with an age and a theme, and danger from hops to the core plus regional noise. Ported
    /// from the Stage 1 prototype (GalaxyProto.cs on the game repository's capture branch); a spatial grid replaces its
    /// all-pairs scans so 2,000 systems stay fast, and gives the same lanes. Each purpose draws from its own stream.
    /// </summary>
    internal sealed class GalaxyLayout
    {
        /// <summary>The galaxy's radius in game units.</summary>
        public const double Radius = 1000;

        /// <summary>The share of Gabriel-only edges kept as extra lanes, so maps have loops as well as a tree.</summary>
        /// <summary>The default share of extra lanes, in percent (<see cref="GeneratorOptions.ExtraLanes"/>).</summary>
        internal const int DefaultExtraLanes = 25;

        private const int FirstSpiralCount = Diagnostics.FirstSpiralCount;

        private static readonly GalaxyShape[] s_shapes = { GalaxyShape.Spiral, GalaxyShape.Barred, GalaxyShape.Elliptical, GalaxyShape.Ring, GalaxyShape.Irregular };
        private static readonly int[] s_shapeWeights = { 40, 20, 15, 10, 15 };
        private static readonly GalaxyShape[] s_smallShapes = { GalaxyShape.Elliptical, GalaxyShape.Ring, GalaxyShape.Irregular };
        private static readonly int[] s_smallShapeWeights = { 15, 10, 15 };
        private static readonly StellarAge[] s_ages = { StellarAge.Young, StellarAge.Mature, StellarAge.Old };
        private static readonly int[] s_ageWeights = { 30, 45, 25 };

        private static readonly string[] s_regionRoots =
        {
            "Vela", "Draco", "Lyra", "Corvus", "Hydra", "Carina", "Orion", "Cygnus", "Pavo", "Lupus", "Ara", "Norma", "Pyxis",
            "Fornax", "Indus", "Musca", "Tucana", "Volans", "Aquila", "Serpens",
        };

        private static readonly string[] s_regionKinds = { "Reach", "Expanse", "March", "Drift", "Verge", "Cluster", "Deep", "Spur" };

        private static readonly string[] s_themes =
        {
            "frontier", "old empire", "pirate haven", "trade corridor", "precursor ruins", "nebula maze", "mining belt", "quarantine zone",
        };

        public GalaxyShape Shape;
        public int Arms;
        public double Pitch;
        public double Ellipse;
        public readonly List<double> X = new List<double>();
        public readonly List<double> Y = new List<double>();
        public readonly List<(int A, int B, bool Bridge)> Lanes = new List<(int, int, bool)>();
        public readonly List<(string Name, StellarAge Age, string Theme, int Centre)> Regions = new List<(string, StellarAge, string, int)>();
        public int Core;
        public int[] Hops = Array.Empty<int>();
        public int[] Region = Array.Empty<int>();
        public int[] Danger = Array.Empty<int>();
        public bool[] Chokepoint = Array.Empty<bool>();

        private List<int>[] _adjacency = Array.Empty<List<int>>();

        public int Count => X.Count;

        /// <summary>The seed of system <paramref name="index"/> of the galaxy whose seed is <paramref name="galaxySeed"/>.</summary>
        public static ulong SystemSeed(ulong galaxySeed, int index) => Seeds.Child(galaxySeed, "system", index);

        public static GalaxyLayout Generate(ulong seed, int systems, GalaxyShape requested) =>
            Generate(seed, systems, requested, LayoutTuning.None, Preset.Default);

        /// <summary>
        /// As above, with what a cluster decides for the galaxy. Every stream draws exactly what it draws alone; the tuning
        /// only changes which values are used, so <see cref="LayoutTuning.None"/> gives the lone galaxy's map. The options'
        /// arm count, extra lanes and danger shift also only change values after every draw, so their defaults change nothing.
        /// </summary>
        public static GalaxyLayout Generate(ulong seed, int systems, GalaxyShape requested, LayoutTuning tuning, GeneratorOptions options)
        {
            var g = new GalaxyLayout();
            var shape = Seeds.Stream(seed, "shape");
            // Both the roll and every shape parameter are always drawn, so a chosen shape keeps the seed's other values.
            var rolled = systems < FirstSpiralCount ? s_smallShapes[shape.Weighted(s_smallShapeWeights)] : s_shapes[shape.Weighted(s_shapeWeights)];
            g.Shape = requested == GalaxyShape.Auto ? rolled : requested;
            g.Arms = shape.Range(2, 4);
            if (options.Arms.HasValue)
            {
                g.Arms = options.Arms.Value;
            }
            var pitch = shape.Range(0.25, 0.45); // the tangent of the arms' pitch angle
            if (tuning.PitchHigh > 0)
            {
                // The type's band (Sa tight, Sc open): the drawn pitch mapped into it.
                pitch = tuning.PitchLow + (pitch - 0.25) / 0.2 * (tuning.PitchHigh - tuning.PitchLow);
            }

            var twist = shape.Range(0, 2 * DMath.PI);
            var blobCount = shape.Range(3, 5);
            var blobs = new (double X, double Y, double S)[blobCount];
            for (var i = 0; i < blobCount; i++)
            {
                blobs[i] = (shape.Range(-0.6, 0.6), shape.Range(-0.6, 0.6), shape.Range(0.15, 0.35));
            }

            var ellipse = shape.Range(0.55, 0.9);
            if (tuning.Ellipse > 0)
            {
                ellipse = tuning.Ellipse;
            }

            g.Pitch = pitch;
            g.Ellipse = ellipse;
            var density = new Density(g.Shape, g.Arms, pitch, twist, ellipse, blobs);

            var grid = g.Place(Seeds.Stream(seed, "layout"), systems, density);
            g.BuildLanes(Seeds.Stream(seed, "lanes"), grid, options.ExtraLanes);
            g.Analyse();
            g.BuildRegions(Seeds.Stream(seed, "regions"), tuning.AgeWeights ?? s_ageWeights);
            g.SetDanger(seed, tuning.CoreHazardRadius, tuning.CoreDanger, options.DangerShift);
            return g;
        }

        private Grid Place(Pcg32 rng, int count, Density density)
        {
            // Keeps systems apart without hiding the shape; relaxed by a tenth every 5,000 crowded attempts.
            var spacing = 0.4 * Radius / Math.Sqrt(count);
            var grid = new Grid(spacing);
            var maxAttempts = Math.Max(200000, 500 * count);
            for (var attempts = 1; X.Count < count && attempts <= maxAttempts; attempts++)
            {
                double x = rng.Range(-1.0, 1.0), y = rng.Range(-1.0, 1.0);
                // A decision on integers: the density quantised to 1/4096 against an integer draw.
                var threshold = (int)(density.At(x, y) * 4096);
                if (rng.NextInt(4096) >= threshold)
                {
                    continue;
                }

                double px = DMath.Round(x * Radius, 3), py = DMath.Round(y * Radius, 3);
                if (grid.AnyCloser(px, py, spacing * spacing, X, Y))
                {
                    if (attempts % 5000 == 0)
                    {
                        spacing *= 0.9;
                    }

                    continue;
                }

                grid.Add(X.Count, px, py);
                X.Add(px);
                Y.Add(py);
            }

            return grid;
        }

        private double D2(int a, int b)
        {
            double dx = X[a] - X[b], dy = Y[a] - Y[b];
            return dx * dx + dy * dy;
        }

        private void BuildLanes(Pcg32 rng, Grid grid, int extraLanes)
        {
            var n = Count;
            for (var a = 0; a < n; a++)
            {
                for (var b = a + 1; b < n; b++)
                {
                    var dab = D2(a, b);
                    // Every relative-neighbourhood edge is a Gabriel edge, so a pair outside the Gabriel graph is no lane.
                    if (grid.AnyInCircle(a, b, dab, X, Y))
                    {
                        continue;
                    }

                    // The extra-lane roll is drawn for every Gabriel pair, so the share shifts nothing else.
                    var extra = rng.NextInt(100) < extraLanes;
                    if (extra || !grid.AnyInLune(a, b, dab, X, Y))
                    {
                        Lanes.Add((a, b, false));
                    }
                }
            }

            _adjacency = new List<int>[n];
            for (var i = 0; i < n; i++)
            {
                _adjacency[i] = new List<int>();
            }

            foreach (var (a, b, _) in Lanes)
            {
                _adjacency[a].Add(b);
                _adjacency[b].Add(a);
            }
        }

        private void Analyse()
        {
            var n = Count;
            // The core is the system nearest the centre (the first on a tie); hops count lanes from it.
            var best = double.MaxValue;
            for (var i = 0; i < n; i++)
            {
                var r2 = X[i] * X[i] + Y[i] * Y[i];
                if (r2 < best)
                {
                    best = r2;
                    Core = i;
                }
            }

            Hops = Bfs(Core);
            Chokepoint = new bool[n];
            var bridges = Bridges();
            for (var i = 0; i < Lanes.Count; i++)
            {
                var (a, b, _) = Lanes[i];
                if (bridges.Contains(((long)a << 32) | (uint)b))
                {
                    Lanes[i] = (a, b, true);
                }
            }
        }

        /// <summary>
        /// Tarjan's bridges and articulation points, iteratively so a long chain of systems cannot overflow the stack:
        /// marks the chokepoint systems and returns the bridge lanes as (low index &lt;&lt; 32) | high index.
        /// </summary>
        private HashSet<long> Bridges()
        {
            var n = Count;
            var disc = new int[n];
            var low = new int[n];
            var parent = new int[n];
            var next = new int[n];
            for (var i = 0; i < n; i++)
            {
                disc[i] = -1;
                parent[i] = -1;
            }

            var bridges = new HashSet<long>();
            var stack = new Stack<int>();
            var time = 0;
            for (var root = 0; root < n; root++)
            {
                if (disc[root] != -1)
                {
                    continue;
                }

                disc[root] = low[root] = time++;
                stack.Push(root);
                var rootChildren = 0;
                while (stack.Count > 0)
                {
                    var u = stack.Peek();
                    if (next[u] < _adjacency[u].Count)
                    {
                        var v = _adjacency[u][next[u]++];
                        if (disc[v] == -1)
                        {
                            parent[v] = u;
                            disc[v] = low[v] = time++;
                            stack.Push(v);
                            if (u == root)
                            {
                                rootChildren++;
                            }
                        }
                        else if (v != parent[u])
                        {
                            low[u] = Math.Min(low[u], disc[v]);
                        }

                        continue;
                    }

                    stack.Pop();
                    var p = parent[u];
                    if (p == -1)
                    {
                        continue;
                    }

                    low[p] = Math.Min(low[p], low[u]);
                    if (parent[p] != -1 && low[u] >= disc[p])
                    {
                        Chokepoint[p] = true;
                    }

                    if (low[u] > disc[p])
                    {
                        bridges.Add(((long)Math.Min(p, u) << 32) | (uint)Math.Max(p, u));
                    }
                }

                if (rootChildren > 1)
                {
                    Chokepoint[root] = true;
                }
            }

            return bridges;
        }

        private int[] Bfs(int start)
        {
            var dist = new int[Count];
            for (var i = 0; i < dist.Length; i++)
            {
                dist[i] = int.MaxValue;
            }

            var queue = new Queue<int>();
            dist[start] = 0;
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                foreach (var v in _adjacency[u])
                {
                    if (dist[v] == int.MaxValue)
                    {
                        dist[v] = dist[u] + 1;
                        queue.Enqueue(v);
                    }
                }
            }

            return dist;
        }

        private void BuildRegions(Pcg32 rng, int[] ageWeights)
        {
            var n = Count;
            var k = Math.Min(n, DMath.Clamp(n / 8, 2, 8));
            // Farthest-point sampling over hops picks the region centres; each system joins the nearest (the first on a tie).
            var centres = new List<int> { Core };
            var dists = new List<int[]> { Hops };
            while (centres.Count < k)
            {
                int far = 0, farthest = -1;
                for (var i = 0; i < n; i++)
                {
                    var nearest = int.MaxValue;
                    foreach (var d in dists)
                    {
                        nearest = Math.Min(nearest, d[i]);
                    }

                    if (nearest > farthest)
                    {
                        farthest = nearest;
                        far = i;
                    }
                }

                centres.Add(far);
                dists.Add(Bfs(far));
            }

            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var centre in centres)
            {
                string name;
                do
                {
                    name = s_regionRoots[rng.NextInt(s_regionRoots.Length)] + " " + s_regionKinds[rng.NextInt(s_regionKinds.Length)];
                }
                while (!used.Add(name));

                var age = s_ages[rng.Weighted(ageWeights)];
                Regions.Add((name, age, s_themes[rng.NextInt(s_themes.Length)], centre));
            }

            Region = new int[n];
            for (var i = 0; i < n; i++)
            {
                var nearest = 0;
                for (var c = 1; c < k; c++)
                {
                    if (dists[c][i] < dists[nearest][i])
                    {
                        nearest = c;
                    }
                }

                Region[i] = nearest;
            }
        }

        private void SetDanger(ulong seed, double hazardRadius, int hazardDanger, int shift)
        {
            var maxHops = 1;
            foreach (var h in Hops)
            {
                maxHops = Math.Max(maxHops, h);
            }

            var rng = Seeds.Stream(seed, "danger");
            var regionNoise = new int[Regions.Count];
            for (var i = 0; i < regionNoise.Length; i++)
            {
                regionNoise[i] = rng.Range(-2, 2);
            }

            Danger = new int[Count];
            for (var i = 0; i < Count; i++)
            {
                // 1 at the core to 10 at the farthest system, rounded half up on integers, then regional and local noise.
                var baseDanger = 1 + (18 * Hops[i] + maxHops) / (2 * maxHops);
                var local = Seeds.Stream(SystemSeed(seed, i), "danger").Range(-1, 1);
                // An active core's radiation adds its danger to every system within its hazard radius (plan U8).
                var core = hazardDanger > 0 && X[i] * X[i] + Y[i] * Y[i] <= hazardRadius * hazardRadius ? hazardDanger : 0;
                Danger[i] = DMath.Clamp(baseDanger + regionNoise[Region[i]] + local + core + shift, 1, 10);
            }
        }

        /// <summary>Relative density in [0, 1] at (x, y), both in units of the radius.</summary>
        /// <summary>What a galaxy cluster decides for one galaxy's map; <see cref="None"/> changes nothing.</summary>
        internal sealed record LayoutTuning
        {
            public static readonly LayoutTuning None = new LayoutTuning();

            /// <summary>The pitch band of the galaxy's type code; 0 keeps the drawn pitch.</summary>
            public double PitchLow { get; init; }

            public double PitchHigh { get; init; }

            /// <summary>The axis ratio of an elliptical type code; 0 keeps the drawn one.</summary>
            public double Ellipse { get; init; }

            /// <summary>Weights of Young, Mature and Old regions; null keeps 30, 45, 25.</summary>
            public int[]? AgeWeights { get; init; }

            /// <summary>The radius of an active core's radiation, in game units, and the danger it adds inside it.</summary>
            public double CoreHazardRadius { get; init; }

            public int CoreDanger { get; init; }
        }

        private sealed class Density
        {
            private readonly GalaxyShape _shape;
            private readonly int _arms;
            private readonly double _pitch;
            private readonly double _twist;
            private readonly double _cosTwist;
            private readonly double _sinTwist;
            private readonly double _ellipse;
            private readonly (double X, double Y, double S)[] _blobs;

            public Density(GalaxyShape shape, int arms, double pitch, double twist, double ellipse, (double X, double Y, double S)[] blobs)
            {
                _shape = shape;
                _arms = arms;
                _pitch = pitch;
                _twist = twist;
                _cosTwist = DMath.Cos(twist);
                _sinTwist = DMath.Sin(twist);
                _ellipse = ellipse;
                _blobs = blobs;
            }

            public double At(double x, double y)
            {
                var r = Math.Sqrt(x * x + y * y);
                if (r > 1)
                {
                    return 0;
                }

                var bulge = DMath.Exp(-r * r / 0.02);
                switch (_shape)
                {
                    case GalaxyShape.Elliptical:
                        var e = x * x / (_ellipse * _ellipse) + y * y;
                        return DMath.Exp(-e * 2.2);
                    case GalaxyShape.Ring:
                        var d = (r - 0.68) / 0.09;
                        return Math.Min(1, DMath.Exp(-d * d) + 0.6 * bulge);
                    case GalaxyShape.Irregular:
                        var sum = 0.0;
                        foreach (var b in _blobs)
                        {
                            double dx = x - b.X, dy = y - b.Y;
                            sum += DMath.Exp(-(dx * dx + dy * dy) / (2 * b.S * b.S));
                        }

                        return Math.Min(1, sum * 0.8);
                    default:
                        return Spiral(x, y, r, bulge);
                }
            }

            private double Spiral(double x, double y, double r, double bulge)
            {
                if (r < 0.05)
                {
                    return 1;
                }

                var angle = DMath.Atan2(y, x);
                var logR = DMath.Log(r);
                var best = double.MaxValue;
                for (var k = 0; k < _arms; k++)
                {
                    // A logarithmic spiral per arm; the angular distance to the nearest arm, wrapped to [-pi, pi].
                    var armAngle = logR / _pitch + _twist + 2 * DMath.PI * k / _arms;
                    var diff = angle - armAngle;
                    diff -= Math.Floor(diff / (2 * DMath.PI) + 0.5) * 2 * DMath.PI;
                    best = Math.Min(best, Math.Abs(diff));
                }

                var width = 0.16 + 0.12 * r;
                var arm = DMath.Exp(-(best * best) / (2 * width * width)) * (1 - 0.6 * r);
                var core = bulge;
                if (_shape == GalaxyShape.Barred)
                {
                    var bx = x * _cosTwist + y * _sinTwist;
                    var by = -x * _sinTwist + y * _cosTwist;
                    if (Math.Abs(bx) < 0.32)
                    {
                        core = Math.Max(core, DMath.Exp(-(by * by) / 0.004));
                    }

                    if (r < 0.32)
                    {
                        arm *= 0.25;
                    }
                }

                return Math.Min(1, arm + core + 0.01);
            }
        }

        /// <summary>A square grid over the galaxy, so neighbour and empty-circle questions look at nearby cells only.</summary>
        private sealed class Grid
        {
            private readonly double _cell;
            private readonly int _side;
            private readonly int[] _head;
            private readonly List<int> _next = new List<int>();

            public Grid(double cell)
            {
                _cell = cell;
                _side = (int)(2 * Radius / cell) + 1;
                _head = new int[_side * _side];
                for (var i = 0; i < _head.Length; i++)
                {
                    _head[i] = -1;
                }
            }

            private int Cell(double v) => DMath.Clamp((int)((v + Radius) / _cell), 0, _side - 1);

            public void Add(int index, double x, double y)
            {
                var cell = Cell(y) * _side + Cell(x);
                _next.Add(_head[cell]);
                _head[cell] = index;
            }

            /// <summary>True when a placed system is closer than the square root of <paramref name="d2"/>, at most one cell away.</summary>
            public bool AnyCloser(double x, double y, double d2, List<double> xs, List<double> ys)
            {
                int cx = Cell(x), cy = Cell(y);
                for (var j = Math.Max(0, cy - 1); j <= Math.Min(_side - 1, cy + 1); j++)
                {
                    for (var i = Math.Max(0, cx - 1); i <= Math.Min(_side - 1, cx + 1); i++)
                    {
                        for (var p = _head[j * _side + i]; p != -1; p = _next[p])
                        {
                            double dx = xs[p] - x, dy = ys[p] - y;
                            if (dx * dx + dy * dy < d2)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            /// <summary>True when a third system lies strictly inside the circle on a-b as diameter (not a Gabriel edge).</summary>
            public bool AnyInCircle(int a, int b, double dab, List<double> xs, List<double> ys)
            {
                double mx = (xs[a] + xs[b]) / 2, my = (ys[a] + ys[b]) / 2;
                return Search(mx, my, dab / 4, a, b, xs, ys, false);
            }

            /// <summary>True when a third system is nearer to both a and b than they are to each other (not a relative-neighbourhood edge).</summary>
            public bool AnyInLune(int a, int b, double dab, List<double> xs, List<double> ys) =>
                Search(xs[a], ys[a], dab, a, b, xs, ys, true);

            /// <summary>Rings of cells outwards from the centre's cell, so a crowded circle answers at its first cells.</summary>
            private bool Search(double x, double y, double r2, int a, int b, List<double> xs, List<double> ys, bool lune)
            {
                int cx = Cell(x), cy = Cell(y);
                var reach = Math.Min(_side, (int)(Math.Sqrt(r2) / _cell) + 1);
                for (var ring = 0; ring <= reach; ring++)
                {
                    for (var j = cy - ring; j <= cy + ring; j++)
                    {
                        if (j < 0 || j >= _side)
                        {
                            continue;
                        }

                        var edgeRow = j == cy - ring || j == cy + ring;
                        var step = edgeRow || ring == 0 ? 1 : 2 * ring;
                        for (var i = cx - ring; i <= cx + ring; i += step)
                        {
                            if (i < 0 || i >= _side)
                            {
                                continue;
                            }

                            for (var c = _head[j * _side + i]; c != -1; c = _next[c])
                            {
                                if (c == a || c == b)
                                {
                                    continue;
                                }

                                bool inside;
                                if (lune)
                                {
                                    double ax = xs[c] - xs[a], ay = ys[c] - ys[a], bx = xs[c] - xs[b], by = ys[c] - ys[b];
                                    inside = Math.Max(ax * ax + ay * ay, bx * bx + by * by) < r2;
                                }
                                else
                                {
                                    double dx = xs[c] - x, dy = ys[c] - y;
                                    inside = dx * dx + dy * dy < r2;
                                }

                                if (inside)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
