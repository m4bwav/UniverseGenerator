#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// Builds a universe (plan U1, U3, U7, U10, U11, A12, A13): the epoch, the cosmic web of clusters and filaments, the
    /// voids and their lone systems, the landmark slots and the merging pair. Each cluster takes a <see cref="ClusterContext"/>
    /// that changes values only, so every stream of a cluster draws what it draws alone.
    /// </summary>
    internal static class UniverseGenerator
    {
        /// <summary>The radius of the universe map, in universe units.</summary>
        public const double Radius = 1000;

        /// <summary>How close two clusters may be, in universe units.</summary>
        private const double NodeSpacing = 250;

        /// <summary>A cluster's radius on the universe map: 1,000 cluster units at 0.02.</summary>
        private const double ClusterRadius = 20;

        /// <summary>The grid step of void candidates and the smallest void, in universe units.</summary>
        private const double VoidStep = 50;

        private static readonly StellarAge[] s_ages = { StellarAge.Young, StellarAge.Mature, StellarAge.Old };
        private static readonly int[] s_ageWeights = { 30, 45, 25 };

        // Group and cluster weights for the field nodes: structure grows with time.
        private static readonly int[] s_youngKinds = { 75, 25 };
        private static readonly int[] s_matureKinds = { 60, 40 };
        private static readonly int[] s_oldKinds = { 45, 55 };

        private static readonly string[] s_universeRoots =
        {
            "Lantern", "Silent", "Ember", "Hollow", "Gilded", "Shattered", "Endless", "Pale", "Burning", "Drowned", "Crowned", "Veiled",
            "Iron", "Last", "Wandering", "Hidden",
        };

        private static readonly string[] s_voidRoots =
        {
            "Abyssal", "Barren", "Ashen", "Cinder", "Dark", "Empty", "Forsaken", "Great", "Lonely", "Mourning", "Quiet", "Starless",
            "Still", "Sundered", "Widow", "Yawning",
        };

        private static readonly string[] s_hooks =
        {
            "Both galaxies claim the stars the collision threw loose.",
            "Refugees from the starburst flood the frontier, and neither galaxy will take them.",
            "A precursor relay has surfaced in the tidal stream, and two navies race to reach it.",
            "Pirates rule the frontier, paid by each galaxy to raid the other.",
            "The collision is dragging a holy world from one galaxy into the other.",
            "Smugglers fly routes through the tidal stream that no navy can follow.",
            "A truce holds the frontier, and someone is working to break it.",
            "New stars are born on the frontier every year, and every claim to them is disputed.",
        };

        public static Universe Generate(Address address, GeneratorOptions options, GeneratorHooks? hooks = null)
        {
            var seed = address.ObjectSeed;
            var drawnAge = s_ages[Seeds.Stream(seed, "epoch").Weighted(s_ageWeights)];
            var age = options.Epoch == Epoch.Auto ? drawnAge : ClusterGenerator.AgeOf(options.Epoch);

            // The web: node count, void slots and their system counts, positions, then kinds; all drawn whatever is used.
            var web = Seeds.Stream(seed, "web");
            var count = web.Range(4, 7);
            var voidSlots = web.Range(1, 3);
            var voidSystems = new[] { web.Range(1, 3), web.Range(1, 3), web.Range(1, 3) };
            var x = new double[count];
            var y = new double[count];
            Place(web, x, y);
            var kindWeights = age == StellarAge.Young ? s_youngKinds : age == StellarAge.Old ? s_oldKinds : s_matureKinds;
            var kinds = new ClusterKind[count];
            for (var i = 0; i < count; i++)
            {
                var drawn = web.Weighted(kindWeights) == 0 ? ClusterKind.Group : ClusterKind.Cluster;
                kinds[i] = i == 0 || i == 2 ? ClusterKind.Group : i == 1 ? ClusterKind.Cluster : drawn;
            }

            var names = Seeds.Stream(seed, "names");
            var name = "the " + s_universeRoots[names.NextInt(s_universeRoots.Length)] + " Reach";
            var used = new HashSet<int>();
            var clusterNames = new string[count];
            for (var i = 0; i < count; i++)
            {
                clusterNames[i] = ClusterGenerator.Roots[Unique(names, used, ClusterGenerator.Roots.Count)] + (kinds[i] == ClusterKind.Group ? " Group" : " Cluster");
            }

            var hook = s_hooks[Seeds.Stream(seed, "story").NextInt(s_hooks.Length)];

            // The distant quasar: the field node farthest from home, ties to the lowest index.
            var quasar = 3;
            for (var i = 4; i < count; i++)
            {
                if (D2(x, y, i, 0) > D2(x, y, quasar, 0))
                {
                    quasar = i;
                }
            }

            // First pass: every cluster's map with its kind, age, name and slots; filaments and the landmark need the maps.
            var bases = new ClusterContext[count];
            var maps = new GalaxyCluster[count];
            for (var i = 0; i < count; i++)
            {
                bases[i] = new ClusterContext
                {
                    Kind = kinds[i],
                    Age = age,
                    Name = clusterNames[i],
                    Home = i == 0,
                    Dying = i == 1,
                    Ring = i == 1,
                    Merging = i == 2,
                    Quasar = i == quasar,
                    X = x[i],
                    Y = y[i],
                };
                maps[i] = ClusterGenerator.Generate(address.Child("cluster", i), options, bases[i]);
            }

            var filaments = Filaments(x, y, maps);
            var landmark = maps[quasar].Map[1];
            var landmarkX = x[quasar] + landmark.X * ClusterGenerator.UniverseScale;
            var landmarkY = y[quasar] + landmark.Y * ClusterGenerator.UniverseScale;

            var clusters = new GalaxyCluster[count];
            var nodes = new UniverseNode[count];
            for (var i = 0; i < count; i++)
            {
                var ends = new List<(int, int, int, string, double, double)>();
                foreach (var f in filaments)
                {
                    if (f.A == i || f.B == i)
                    {
                        var (other, mine, theirs) = f.A == i ? (f.B, f.GalaxyA, f.GalaxyB) : (f.A, f.GalaxyB, f.GalaxyA);
                        ends.Add((mine, other, theirs, maps[other].Map[theirs].Name, x[other] - x[i], y[other] - y[i]));
                    }
                }

                clusters[i] = ClusterGenerator.Generate(address.Child("cluster", i), options, hooks: hooks, context: bases[i] with
                {
                    Filaments = ends,
                    LandmarkName = landmark.Name,
                    LandmarkAddress = landmark.Address,
                    LandmarkX = landmarkX,
                    LandmarkY = landmarkY,
                });
                clusters[i] = Hook.Cluster(hooks, clusters[i]);
                nodes[i] = new UniverseNode
                {
                    Index = i,
                    Address = clusters[i].Address,
                    Name = clusters[i].Name,
                    Kind = clusters[i].Kind,
                    Role = i == 0 ? NodeRole.Home : i == 1 ? NodeRole.GreatCluster : i == 2 ? NodeRole.Merger : NodeRole.Field,
                    X = x[i],
                    Y = y[i],
                    Galaxies = clusters[i].Map.Count,
                };
            }

            var voids = Voids(address, options, hooks, names, voidSlots, voidSystems, x, y, filaments);

            var pairA = clusters[2].Map[0];
            var pairB = clusters[2].Map[1];
            var merger = new UniverseMerger
            {
                Name = "the " + Root(pairA.Name) + "-" + Root(pairB.Name) + " Collision",
                Cluster = 2,
                GalaxyA = pairA.Address,
                GalaxyB = pairB.Address,
                Hook = hook,
            };

            var ring = clusters[1].Map[0];
            foreach (var e in clusters[1].Map)
            {
                if (e.Role == GalaxyRole.Member)
                {
                    ring = e;
                    break;
                }
            }

            var landmarks = new[]
            {
                new UniverseLandmark { Kind = UniverseLandmarkKind.Home, Name = clusters[0].Map[0].Name, Address = clusters[0].Map[0].Address },
                new UniverseLandmark { Kind = UniverseLandmarkKind.DyingGiant, Name = clusters[1].Map[0].Name, Address = clusters[1].Map[0].Address },
                new UniverseLandmark { Kind = UniverseLandmarkKind.RingGalaxy, Name = ring.Name, Address = ring.Address },
                new UniverseLandmark { Kind = UniverseLandmarkKind.Quasar, Name = landmark.Name, Address = landmark.Address },
                new UniverseLandmark { Kind = UniverseLandmarkKind.Merger, Name = merger.Name, Address = clusters[2].Address },
            };

            return Hook.Universe(hooks, new Universe
            {
                Address = address.ToString(),
                Name = name,
                Age = age,
                Radius = Radius,
                Nodes = nodes,
                Filaments = filaments,
                Voids = voids,
                Landmarks = landmarks,
                Merger = merger,
                Clusters = clusters,
            });
        }

        /// <summary>Node 0 near the centre, the rest over the disc, at least the node spacing apart (relaxed by a tenth after 200 refusals).</summary>
        private static void Place(Pcg32 rng, double[] x, double[] y)
        {
            for (var i = 0; i < x.Length; i++)
            {
                var spread = 1.0;
                for (var refusals = 1; ; refusals++)
                {
                    var angle = rng.Range(0, 2 * DMath.PI);
                    var r = (i == 0 ? 0.3 : 0.9) * Radius * Math.Sqrt(rng.NextDouble());
                    var px = DMath.Round(r * DMath.Cos(angle), 3);
                    var py = DMath.Round(r * DMath.Sin(angle), 3);
                    var clear = true;
                    for (var j = 0; j < i && clear; j++)
                    {
                        double dx = px - x[j], dy = py - y[j];
                        var min = NodeSpacing * spread;
                        clear = dx * dx + dy * dy >= min * min;
                    }

                    if (clear)
                    {
                        x[i] = px;
                        y[i] = py;
                        break;
                    }

                    if (refusals % 200 == 0)
                    {
                        spread *= 0.9;
                    }
                }
            }
        }

        /// <summary>
        /// The filaments (plan U11): the Gabriel graph of the nodes, which holds the minimum spanning tree and so reaches
        /// every cluster. Each opens in the galaxy of each cluster lying farthest towards the other, satellites aside.
        /// </summary>
        private static Filament[] Filaments(double[] x, double[] y, GalaxyCluster[] maps)
        {
            var list = new List<Filament>();
            var n = x.Length;
            for (var a = 0; a < n; a++)
            {
                for (var b = a + 1; b < n; b++)
                {
                    double mx = (x[a] + x[b]) / 2, my = (y[a] + y[b]) / 2;
                    var r2 = D2(x, y, a, b) / 4;
                    var gabriel = true;
                    for (var c = 0; c < n && gabriel; c++)
                    {
                        if (c != a && c != b)
                        {
                            double dx = x[c] - mx, dy = y[c] - my;
                            gabriel = dx * dx + dy * dy >= r2;
                        }
                    }

                    if (gabriel)
                    {
                        list.Add(new Filament
                        {
                            A = a,
                            B = b,
                            Length = DMath.Round(Math.Sqrt(D2(x, y, a, b)), 3),
                            GalaxyA = Facing(maps[a], x[b] - x[a], y[b] - y[a]),
                            GalaxyB = Facing(maps[b], x[a] - x[b], y[a] - y[b]),
                        });
                    }
                }
            }

            return list.ToArray();
        }

        private static int Facing(GalaxyCluster cluster, double dx, double dy)
        {
            var best = 0;
            var reach = double.MinValue;
            foreach (var e in cluster.Map)
            {
                var r = e.X * dx + e.Y * dy;
                if (e.Role != GalaxyRole.Satellite && r > reach)
                {
                    reach = r;
                    best = e.Index;
                }
            }

            return best;
        }

        /// <summary>
        /// The voids (plan U11): the grid point with the most room from every cluster, filament and earlier void, that room
        /// its radius; a slot with under one grid step of room makes no void. Their systems are lone, old, quiet and poor.
        /// </summary>
        private static CosmicVoid[] Voids(Address address, GeneratorOptions options, GeneratorHooks? hooks, Pcg32 names, int slots, int[] systems, double[] x, double[] y, Filament[] filaments)
        {
            var voids = new List<CosmicVoid>();
            var usedRoots = new HashSet<int>();
            var steps = (int)(Radius / VoidStep);
            for (var k = 0; k < slots; k++)
            {
                // The name is drawn for every slot, so a slot without room shifts nothing.
                var root = s_voidRoots[Unique(names, usedRoots, s_voidRoots.Length)];
                double bestX = 0, bestY = 0, best = double.MinValue;
                for (var j = -steps; j <= steps; j++)
                {
                    for (var i = -steps; i <= steps; i++)
                    {
                        double px = i * VoidStep, py = j * VoidStep;
                        if (px * px + py * py > 0.81 * Radius * Radius)
                        {
                            continue;
                        }

                        var room = Room(px, py, x, y, filaments, voids);
                        if (room > best)
                        {
                            best = room;
                            bestX = px;
                            bestY = py;
                        }
                    }
                }

                if (best < VoidStep)
                {
                    continue;
                }

                var where = address.Child("void", voids.Count);
                var contexts = new SystemContext[systems[k]];
                for (var s = 0; s < contexts.Length; s++)
                {
                    contexts[s] = VoidSystem(where.Child("system", s));
                }

                voids.Add(new CosmicVoid
                {
                    Index = voids.Count,
                    Address = where.ToString(),
                    Name = "the " + root + " Void",
                    X = bestX,
                    Y = bestY,
                    Radius = DMath.Round(best, 3),
                    Systems = new LazySystems(where, contexts, options, null, hooks),
                });
            }

            return voids.ToArray();
        }

        /// <summary>A void system: old, poor in metals, danger 1 to 3 from its own danger stream.</summary>
        private static SystemContext VoidSystem(Address system)
        {
            var danger = 1 + (Seeds.Stream(system.ObjectSeed, "danger").Range(1, 10) - 1) / 4;
            return new SystemContext(null, StellarAge.Old, danger, 50);
        }

        private static double Room(double px, double py, double[] x, double[] y, Filament[] filaments, List<CosmicVoid> voids)
        {
            // The map edge bounds a void too, so every void lies inside the map.
            var room = Radius - Math.Sqrt(px * px + py * py);
            for (var i = 0; i < x.Length; i++)
            {
                double dx = px - x[i], dy = py - y[i];
                room = Math.Min(room, Math.Sqrt(dx * dx + dy * dy) - ClusterRadius);
            }

            foreach (var f in filaments)
            {
                room = Math.Min(room, Segment(px, py, x[f.A], y[f.A], x[f.B], y[f.B]));
            }

            foreach (var v in voids)
            {
                double dx = px - v.X, dy = py - v.Y;
                room = Math.Min(room, Math.Sqrt(dx * dx + dy * dy) - v.Radius);
            }

            return room;
        }

        /// <summary>The distance from a point to a segment.</summary>
        private static double Segment(double px, double py, double ax, double ay, double bx, double by)
        {
            double vx = bx - ax, vy = by - ay;
            var t = ((px - ax) * vx + (py - ay) * vy) / (vx * vx + vy * vy);
            t = DMath.Clamp(t, 0, 1);
            double dx = px - (ax + t * vx), dy = py - (ay + t * vy);
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static int Unique(Pcg32 rng, HashSet<int> used, int count)
        {
            // At most 7 of 32 cluster roots and 3 of 16 void roots are taken, so this ends quickly.
            int r;
            do
            {
                r = rng.NextInt(count);
            }
            while (!used.Add(r));

            return r;
        }

        /// <summary>A galaxy name's root: "Kestrel Galaxy" gives "Kestrel".</summary>
        private static string Root(string galaxy)
        {
            var space = galaxy.IndexOf(' ');
            return space < 0 ? galaxy : galaxy.Substring(0, space);
        }

        private static double D2(double[] x, double[] y, int a, int b)
        {
            double dx = x[a] - x[b], dy = y[a] - y[b];
            return dx * dx + dy * dy;
        }
    }
}
