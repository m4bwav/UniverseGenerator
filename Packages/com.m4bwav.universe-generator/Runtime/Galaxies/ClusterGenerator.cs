#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// What a cluster decides for one of its galaxies (plan ideas U5 to U9, U2, N10); <see cref="Alone"/> leaves a lone
    /// galaxy's map exactly as it was before clusters existed.
    /// </summary>
    internal sealed record GalaxyContext
    {
        public static readonly GalaxyContext Alone = new GalaxyContext();

        public string? Name { get; init; }

        public string? Type { get; init; }

        public GalaxyShape? Shape { get; init; }

        public int? Systems { get; init; }

        public StellarAge Age { get; init; } = StellarAge.Mature;

        public GalaxyRichness Richness { get; init; } = GalaxyRichness.Normal;

        public CoreActivity CoreActivity { get; init; } = CoreActivity.Quiet;

        /// <summary>The host's name, for a satellite's descriptor.</summary>
        public string? Host { get; init; }

        public GalaxyLayout.LayoutTuning Tuning { get; init; } = GalaxyLayout.LayoutTuning.None;

        /// <summary>The cluster links from this galaxy, with the direction to the other galaxy on the cluster map.</summary>
        public IReadOnlyList<(int Other, string Name, LinkTier Tier, double Dx, double Dy)> Links { get; init; } =
            Array.Empty<(int, string, LinkTier, double, double)>();
    }

    /// <summary>Galaxy type codes (plan idea U6): what each means for the map and in words.</summary>
    internal static class GalaxyTypes
    {
        /// <summary>The map shape of a type code.</summary>
        public static GalaxyShape ShapeOf(string type)
        {
            if (type.StartsWith("SB", StringComparison.Ordinal))
            {
                return GalaxyShape.Barred;
            }

            if (type == "Sa" || type == "Sb" || type == "Sc")
            {
                return GalaxyShape.Spiral;
            }

            if (type == "Ring")
            {
                return GalaxyShape.Ring;
            }

            return type == "Irr" || type == "dIrr" ? GalaxyShape.Irregular : GalaxyShape.Elliptical;
        }

        /// <summary>The pitch band of a spiral's letter (a tight, c open), as tangents of the pitch angle.</summary>
        public static (double Low, double High) PitchBand(char letter) =>
            letter == 'a' ? (0.25, 0.31) : letter == 'b' ? (0.31, 0.38) : (0.38, 0.45);

        /// <summary>The letter of a drawn pitch: the band it falls in.</summary>
        public static char LetterOf(double pitch) => pitch < 0.31 ? 'a' : pitch < 0.38 ? 'b' : 'c';

        /// <summary>The type code a lone galaxy's map already implies; no draws, so lone maps do not move.</summary>
        public static string Derive(GalaxyShape shape, double pitch, double ellipse)
        {
            switch (shape)
            {
                case GalaxyShape.Spiral:
                    return "S" + LetterOf(pitch);
                case GalaxyShape.Barred:
                    return "SB" + LetterOf(pitch);
                case GalaxyShape.Ring:
                    return "Ring";
                case GalaxyShape.Irregular:
                    return "Irr";
                default:
                    // En has an axis ratio of 1 - n / 10; worked in whole thousandths so no last bit decides it.
                    var thousandths = (int)(DMath.Round(ellipse, 3) * 1000 + 0.5);
                    return "E" + ((1000 - thousandths + 50) / 100).ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>The type in words, such as "barred spiral galaxy".</summary>
        public static string Noun(string type)
        {
            switch (type)
            {
                case "cD": return "giant elliptical galaxy";
                case "S0": return "lenticular galaxy";
                case "Ring": return "ring galaxy";
                case "Irr": return "irregular galaxy";
                case "dE": return "dwarf elliptical galaxy";
                case "dSph": return "dwarf spheroidal galaxy";
                case "dIrr": return "dwarf irregular galaxy";
            }

            if (type.StartsWith("SB", StringComparison.Ordinal))
            {
                return "barred spiral galaxy";
            }

            return type[0] == 'S' ? "spiral galaxy" : "elliptical galaxy";
        }

        /// <summary>A galaxy in words: "an old giant elliptical galaxy, rich in metals, with a quasar at its core"; "a young spiral galaxy with a bright active core".</summary>
        public static string Describe(string type, StellarAge age, GalaxyRichness richness, CoreActivity core, string? host)
        {
            var words = (age == StellarAge.Young ? "young " : age == StellarAge.Old ? "old " : "") + Noun(type);
            var text = (StartsWithVowel(words) ? "an " : "a ") + words;
            if (richness != GalaxyRichness.Normal)
            {
                text += richness == GalaxyRichness.Rich ? ", rich in metals" : ", poor in metals";
            }

            if (core != CoreActivity.Quiet)
            {
                text += (richness != GalaxyRichness.Normal ? "," : "") + (core == CoreActivity.Quasar ? " with a quasar at its core" : " with a bright active core");
            }

            if (host != null)
            {
                text += ", a satellite of " + host;
            }

            return text;
        }

        private static bool StartsWithVowel(string s) => s[0] == 'a' || s[0] == 'e' || s[0] == 'i' || s[0] == 'o' || s[0] == 'u';
    }

    /// <summary>Builds a group or cluster of galaxies: members, sizes, positions, types, traits and the travel network.</summary>
    internal static class ClusterGenerator
    {
        /// <summary>The radius of the cluster map, in cluster units.</summary>
        public const double Radius = 1000;

        /// <summary>The hazard radius of a Seyfert and of a quasar core, in a galaxy's game units (its radius is 1,000).</summary>
        public const double SeyfertRadius = 150;

        public const double QuasarRadius = 300;

        private const int FirstSpiralCount = 80;

        private static readonly string[] s_roots =
        {
            "Aster", "Brontes", "Calyx", "Cassia", "Corona", "Delphis", "Elara", "Eridan", "Fenris", "Halcyon", "Helix", "Ilios",
            "Ishtar", "Kestrel", "Lumen", "Maelstrom", "Meridian", "Nyx", "Oberon", "Pallas", "Perseid", "Quill", "Rhea", "Seraph",
            "Solace", "Tethys", "Thule", "Umbra", "Vesper", "Wyvern", "Zephyr", "Zenith",
        };

        private static readonly string[] s_numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };

        private static readonly StellarAge[] s_ages = { StellarAge.Young, StellarAge.Mature, StellarAge.Old };
        private static readonly int[] s_ageWeights = { 30, 45, 25 };
        private static readonly int[] s_kindWeights = { 60, 40 };

        // Region age weights (Young, Mature, Old) by galaxy age; Mature is the lone galaxy's.
        private static readonly int[] s_youngRegions = { 55, 35, 10 };
        private static readonly int[] s_matureRegions = { 30, 45, 25 };
        private static readonly int[] s_oldRegions = { 5, 35, 60 };

        // Type classes drawn by weight; spirals then take a letter, ellipticals a digit.
        private static readonly string[] s_groupMajor = { "Sb", "Sc", "SBb", "SBc" };
        private static readonly int[] s_groupMajorWeights = { 35, 35, 15, 15 };
        private static readonly string[] s_clusterMajor = { "E", "Sb", "SBb" };
        private static readonly int[] s_clusterMajorWeights = { 50, 25, 25 };
        private static readonly string[] s_member = { "E", "S0", "S", "SB", "Irr", "Ring" };
        private static readonly int[] s_memberInner = { 45, 40, 7, 3, 5, 0 };
        private static readonly int[] s_memberMiddle = { 25, 35, 21, 9, 10, 0 };
        private static readonly int[] s_memberOuter = { 10, 20, 35, 15, 15, 5 };
        private static readonly int[] s_memberGroup = { 15, 25, 21, 9, 25, 5 };
        private static readonly string[] s_dwarf = { "dSph", "dE", "dIrr" };
        private static readonly int[] s_satelliteWeights = { 40, 25, 35 };
        private static readonly int[] s_freeDwarfWeights = { 25, 15, 60 };

        private static readonly GalaxyRichness[] s_richness = { GalaxyRichness.Poor, GalaxyRichness.Normal, GalaxyRichness.Rich };
        private static readonly int[] s_majorRichness = { 0, 50, 50 };
        private static readonly int[] s_memberRichness = { 15, 70, 15 };
        private static readonly int[] s_dwarfRichness = { 70, 30, 0 };

        private static readonly CoreActivity[] s_cores = { CoreActivity.Quiet, CoreActivity.Seyfert, CoreActivity.Quasar };
        private static readonly int[] s_majorCores = { 60, 30, 10 };
        private static readonly int[] s_giantCores = { 50, 30, 20 };
        private static readonly int[] s_memberCores = { 85, 13, 2 };

        private sealed class Draft
        {
            public GalaxyRole Role;
            public bool Giant;
            public int Host = -1;
            public int Systems;
            public double Size;
            public double Room;
            public double X;
            public double Y;
            public string Type = "";
            public GalaxyShape Shape;
            public double PitchLow;
            public double PitchHigh;
            public double Ellipse;
            public string Name = "";
            public StellarAge Age;
            public GalaxyRichness Richness;
            public CoreActivity Core;
        }

        public static GalaxyCluster Generate(Address address, GeneratorOptions options)
        {
            var seed = address.ObjectSeed;
            var kindStream = Seeds.Stream(seed, "kind");
            var rolledKind = kindStream.Weighted(s_kindWeights) == 0 ? ClusterKind.Group : ClusterKind.Cluster;
            var kind = options.ClusterKind == ClusterKind.Auto ? rolledKind : options.ClusterKind;
            var age = s_ages[kindStream.Weighted(s_ageWeights)];

            var g = Members(Seeds.Stream(seed, "members"), kind, options.Systems);
            Place(Seeds.Stream(seed, "layout"), g, kind);
            Types(Seeds.Stream(seed, "types"), g, kind, options.Systems);
            Traits(Seeds.Stream(seed, "traits"), g, age);
            var clusterName = Names(Seeds.Stream(seed, "names"), g, kind);
            var links = Network(g, kind);

            var map = new ClusterEntry[g.Count];
            var contexts = new GalaxyContext[g.Count];
            for (var i = 0; i < g.Count; i++)
            {
                var d = g[i];
                map[i] = new ClusterEntry
                {
                    Index = i,
                    Address = address.Child("galaxy", i).ToString(),
                    Name = d.Name,
                    Type = d.Type,
                    Shape = d.Shape,
                    Role = d.Role,
                    Host = d.Host,
                    X = d.X,
                    Y = d.Y,
                    Size = d.Size,
                    Systems = d.Systems,
                    Age = d.Age,
                    Richness = d.Richness,
                    CoreActivity = d.Core,
                };

                var ends = new List<(int, string, LinkTier, double, double)>();
                foreach (var l in links)
                {
                    if (l.A == i || l.B == i)
                    {
                        var other = l.A == i ? l.B : l.A;
                        ends.Add((other, g[other].Name, l.Tier, g[other].X - d.X, g[other].Y - d.Y));
                    }
                }

                contexts[i] = new GalaxyContext
                {
                    Name = d.Name,
                    Type = d.Type,
                    Shape = d.Shape,
                    Systems = d.Systems,
                    Age = d.Age,
                    Richness = d.Richness,
                    CoreActivity = d.Core,
                    Host = d.Host >= 0 ? g[d.Host].Name : null,
                    Links = ends,
                    Tuning = new GalaxyLayout.LayoutTuning
                    {
                        PitchLow = d.PitchLow,
                        PitchHigh = d.PitchHigh,
                        Ellipse = d.Ellipse,
                        AgeWeights = d.Age == StellarAge.Young ? s_youngRegions : d.Age == StellarAge.Old ? s_oldRegions : s_matureRegions,
                        CoreHazardRadius = HazardRadius(d.Core),
                        CoreDanger = d.Core == CoreActivity.Quasar ? 3 : d.Core == CoreActivity.Seyfert ? 2 : 0,
                    },
                };
            }

            return new GalaxyCluster
            {
                Address = address.ToString(),
                Name = clusterName,
                Kind = kind,
                Age = age,
                Radius = Radius,
                Map = map,
                Links = links,
                Galaxies = new LazyGalaxies(address, contexts, options),
            };
        }

        public static double HazardRadius(CoreActivity core) =>
            core == CoreActivity.Quasar ? QuasarRadius : core == CoreActivity.Seyfert ? SeyfertRadius : 0;

        /// <summary>Who is in the cluster and how big: majors first, then members, free dwarfs and satellites by host.</summary>
        private static List<Draft> Members(Pcg32 rng, ClusterKind kind, int baseSystems)
        {
            int majors, members, dwarfs;
            var satellites = new int[3];
            if (kind == ClusterKind.Group)
            {
                majors = 2;
                members = rng.Range(0, 2);
                dwarfs = rng.Range(0, 3);
                satellites[0] = rng.Range(1, 3);
                satellites[1] = rng.Range(1, 3);
            }
            else
            {
                majors = 1 + rng.Range(1, 2);
                members = rng.Range(8, 16);
                dwarfs = rng.Range(2, 5);
                // The central giant holds no satellites of its own; the other majors 0 to 2 each (drawn for both).
                satellites[1] = rng.Range(0, 2);
                satellites[2] = rng.Range(0, 2);
            }

            var g = new List<Draft>();
            for (var i = 0; i < majors; i++)
            {
                g.Add(new Draft { Role = GalaxyRole.Major, Giant = kind == ClusterKind.Cluster && i == 0 });
            }

            for (var i = 0; i < members; i++)
            {
                g.Add(new Draft { Role = GalaxyRole.Member });
            }

            for (var i = 0; i < dwarfs; i++)
            {
                g.Add(new Draft { Role = GalaxyRole.Dwarf });
            }

            for (var host = 0; host < majors; host++)
            {
                for (var s = 0; s < satellites[host]; s++)
                {
                    g.Add(new Draft { Role = GalaxyRole.Satellite, Host = host });
                }
            }

            // Sizes: one draw per galaxy, rounded to thousandths before it decides anything.
            foreach (var d in g)
            {
                double factor = d.Giant ? rng.Range(2.0, 3.0)
                    : d.Role == GalaxyRole.Major ? rng.Range(1.4, 2.0)
                    : d.Role == GalaxyRole.Member ? rng.Range(0.6, 1.1)
                    : rng.Range(0.15, 0.4);
                var systems = (int)(baseSystems * DMath.Round(factor, 3) + 0.5);
                if (d.Role == GalaxyRole.Satellite || d.Role == GalaxyRole.Dwarf)
                {
                    systems = Math.Max(systems, Math.Min(5, baseSystems));
                }

                d.Systems = DMath.Clamp(systems, 1, GeneratorOptions.MaxSystems);
                d.Size = SizeOf(d.Systems);

                // Majors and members may turn out spirals and grow to 80 systems (see Types), so they are placed with room for it.
                var mayGrow = (d.Role == GalaxyRole.Major || d.Role == GalaxyRole.Member) && GrowsSpirals(baseSystems);
                d.Room = mayGrow ? SizeOf(Math.Max(d.Systems, FirstSpiralCount)) : d.Size;
            }

            return g;
        }

        private static double SizeOf(int systems) => DMath.Round(40 * Math.Sqrt(systems / 60.0), 3);

        /// <summary>A spiral under 80 systems grows to 80 when the Systems option is at least 40; below that it becomes S0.</summary>
        private static bool GrowsSpirals(int baseSystems) => 2 * baseSystems >= FirstSpiralCount;

        /// <summary>Positions on the cluster map, without overlaps; spacing relaxes by a tenth after 200 refusals.</summary>
        private static void Place(Pcg32 rng, List<Draft> g, ClusterKind kind)
        {
            var placed = new List<int>();
            if (kind == ClusterKind.Group)
            {
                // The two big spirals on a random axis, either side of the centre.
                var axis = rng.Range(0, 2 * DMath.PI);
                var half = rng.Range(250.0, 400.0);
                double c = DMath.Cos(axis), s = DMath.Sin(axis);
                Set(g[0], half * c, half * s);
                Set(g[1], -half * c, -half * s);
                placed.Add(0);
                placed.Add(1);
            }
            else
            {
                Set(g[0], 0, 0);
                placed.Add(0);
            }

            for (var i = placed.Count; i < g.Count; i++)
            {
                var d = g[i];
                var spread = 1.0;
                for (var refusals = 1; ; refusals++)
                {
                    var angle = rng.Range(0, 2 * DMath.PI);
                    double x, y;
                    if (d.Role == GalaxyRole.Satellite)
                    {
                        var host = g[d.Host];
                        var r = host.Room + d.Size + rng.Range(0.2, 1.2) * host.Room;
                        x = host.X + r * DMath.Cos(angle);
                        y = host.Y + r * DMath.Sin(angle);
                    }
                    else
                    {
                        // Crowded towards the giant in a cluster (u^0.75), spread over the disc in a group (sqrt u).
                        var u = rng.NextDouble();
                        var root = Math.Sqrt(u);
                        var r = 0.9 * Radius * (kind == ClusterKind.Cluster ? root * Math.Sqrt(root) : root);
                        x = r * DMath.Cos(angle);
                        y = r * DMath.Sin(angle);
                    }

                    x = DMath.Round(x, 3);
                    y = DMath.Round(y, 3);
                    if (!Overlaps(g, placed, d, x, y, spread))
                    {
                        Set(d, x, y);
                        placed.Add(i);
                        break;
                    }

                    if (refusals % 200 == 0)
                    {
                        spread *= 0.9;
                    }
                }
            }
        }

        private static void Set(Draft d, double x, double y)
        {
            d.X = DMath.Round(x, 3);
            d.Y = DMath.Round(y, 3);
        }

        private static bool Overlaps(List<Draft> g, List<int> placed, Draft d, double x, double y, double spread)
        {
            foreach (var j in placed)
            {
                var o = g[j];
                double dx = x - o.X, dy = y - o.Y;
                var min = 1.2 * (d.Room + o.Room) * spread;
                if (dx * dx + dy * dy < min * min)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Type codes (plan U6): every galaxy draws a class, a letter and a digit, whatever it uses.</summary>
        private static void Types(Pcg32 rng, List<Draft> g, ClusterKind kind, int baseSystems)
        {
            var grow = GrowsSpirals(baseSystems);
            foreach (var d in g)
            {
                string type;
                var letter = "abc"[rng.NextInt(3)];
                var digit = rng.Range(0, 7);
                if (d.Giant)
                {
                    type = "cD";
                    digit %= 3;
                }
                else if (d.Role == GalaxyRole.Major)
                {
                    type = kind == ClusterKind.Group
                        ? s_groupMajor[rng.Weighted(s_groupMajorWeights)]
                        : s_clusterMajor[rng.Weighted(s_clusterMajorWeights)];
                    if (type == "E")
                    {
                        type = "E" + digit.ToString(CultureInfo.InvariantCulture);
                    }
                }
                else if (d.Role == GalaxyRole.Member)
                {
                    int[] weights;
                    if (kind == ClusterKind.Group)
                    {
                        weights = s_memberGroup;
                    }
                    else
                    {
                        // The morphology and density relation: ellipticals crowd the centre, spirals keep to the edge.
                        var r2 = d.X * d.X + d.Y * d.Y;
                        var third = 0.9 * Radius / 3;
                        weights = r2 < third * third ? s_memberInner : r2 < 4 * third * third ? s_memberMiddle : s_memberOuter;
                    }

                    type = s_member[rng.Weighted(weights)];
                    if (type == "E")
                    {
                        type = "E" + digit.ToString(CultureInfo.InvariantCulture);
                    }
                    else if (type == "S" || type == "SB")
                    {
                        type += letter;
                    }
                }
                else
                {
                    type = s_dwarf[rng.Weighted(d.Role == GalaxyRole.Satellite ? s_satelliteWeights : s_freeDwarfWeights)];
                }

                // Arms do not read under 80 systems (plan D16): a smaller spiral grows to 80, or, when the Systems option is
                // under 40, becomes a lenticular, which is what a spiral that lost its arms is.
                var shape = GalaxyTypes.ShapeOf(type);
                if ((shape == GalaxyShape.Spiral || shape == GalaxyShape.Barred) && d.Systems < FirstSpiralCount)
                {
                    if (grow)
                    {
                        d.Systems = FirstSpiralCount;
                        d.Size = d.Room;
                    }
                    else
                    {
                        type = "S0";
                        shape = GalaxyShape.Elliptical;
                    }
                }

                d.Type = type;
                d.Shape = shape;
                if (shape == GalaxyShape.Spiral || shape == GalaxyShape.Barred)
                {
                    (d.PitchLow, d.PitchHigh) = GalaxyTypes.PitchBand(type[type.Length - 1]);
                }
                else if (shape == GalaxyShape.Elliptical)
                {
                    // Axis ratio 1 - n / 10: lenticulars are flat discs (E3 to E6), dwarfs and giants nearly round.
                    var n = type == "S0" ? 3 + digit % 4
                        : type == "dE" ? digit % 5
                        : type == "dSph" || type == "cD" ? digit % 3
                        : type[1] - '0';
                    d.Ellipse = (10 - n) / 10.0;
                }
            }
        }

        /// <summary>Age, richness and core (plan U7, U8): two draws per galaxy, whatever it gets.</summary>
        private static void Traits(Pcg32 rng, List<Draft> g, StellarAge clusterAge)
        {
            foreach (var d in g)
            {
                var small = d.Role == GalaxyRole.Satellite || d.Role == GalaxyRole.Dwarf;
                var richness = s_richness[rng.Weighted(d.Role == GalaxyRole.Major ? s_majorRichness : small ? s_dwarfRichness : s_memberRichness)];
                var weights = d.Giant ? s_giantCores : d.Role == GalaxyRole.Major ? s_majorCores : s_memberCores;
                if (clusterAge == StellarAge.Young)
                {
                    // Quasars belong to the young universe.
                    weights = new[] { weights[0], weights[1], 2 * weights[2] };
                }

                var core = s_cores[rng.Weighted(weights)];
                d.Richness = richness;
                d.Core = small ? CoreActivity.Quiet : core;

                if (d.Shape == GalaxyShape.Spiral || d.Shape == GalaxyShape.Barred)
                {
                    d.Age = clusterAge;
                }
                else if (d.Shape == GalaxyShape.Elliptical)
                {
                    d.Age = clusterAge == StellarAge.Young ? StellarAge.Mature : StellarAge.Old;
                }
                else
                {
                    d.Age = StellarAge.Young;
                }
            }
        }

        /// <summary>The cluster's name and every galaxy's, from roots unique in the cluster; satellites take their host's.</summary>
        private static string Names(Pcg32 rng, List<Draft> g, ClusterKind kind)
        {
            var used = new HashSet<int>();
            var cluster = s_roots[Root(rng, used)] + (kind == ClusterKind.Group ? " Group" : " Cluster");
            var roots = new int[g.Count];
            var satellitesSoFar = new int[g.Count];
            for (var i = 0; i < g.Count; i++)
            {
                var d = g[i];
                if (d.Role == GalaxyRole.Satellite)
                {
                    var k = satellitesSoFar[d.Host]++;
                    d.Name = s_roots[roots[d.Host]] + " " + s_numerals[k];
                    continue;
                }

                roots[i] = Root(rng, used);
                d.Name = s_roots[roots[i]] + (d.Role == GalaxyRole.Dwarf ? " Dwarf" : " Galaxy");
            }

            return cluster;
        }

        /// <summary>A lone galaxy's name, from its own <c>name</c> stream.</summary>
        public static string LoneName(Pcg32 rng) => s_roots[rng.NextInt(s_roots.Length)] + " Galaxy";

        private static int Root(Pcg32 rng, HashSet<int> used)
        {
            // At most 25 roots are taken (a cluster name and 24 galaxies) of 32, so this ends quickly.
            int r;
            do
            {
                r = rng.NextInt(s_roots.Length);
            }
            while (!used.Add(r));

            return r;
        }

        /// <summary>
        /// The travel network (plan U2, N10), with no draws: gates between majors and from every member and dwarf to its
        /// nearest major, a ring of wormholes through every galaxy that is not a satellite or the central giant, ordered by
        /// angle, and a tether from each satellite to its host. One link per pair; gates win over wormholes over tethers.
        /// </summary>
        private static ClusterLink[] Network(List<Draft> g, ClusterKind kind)
        {
            var best = new Dictionary<long, LinkTier>();
            void Add(int a, int b, LinkTier tier)
            {
                if (a == b)
                {
                    return;
                }

                var key = ((long)Math.Min(a, b) << 32) | (uint)Math.Max(a, b);
                if (!best.TryGetValue(key, out var had) || tier < had)
                {
                    best[key] = tier;
                }
            }

            var majors = new List<int>();
            for (var i = 0; i < g.Count; i++)
            {
                if (g[i].Role == GalaxyRole.Major)
                {
                    majors.Add(i);
                }
            }

            foreach (var a in majors)
            {
                foreach (var b in majors)
                {
                    if (a < b)
                    {
                        Add(a, b, LinkTier.Gate);
                    }
                }
            }

            for (var i = 0; i < g.Count; i++)
            {
                if (g[i].Role == GalaxyRole.Member || g[i].Role == GalaxyRole.Dwarf)
                {
                    var nearest = majors[0];
                    var nearestD2 = double.MaxValue;
                    foreach (var m in majors)
                    {
                        var d2 = D2(g[i], g[m]);
                        if (d2 < nearestD2)
                        {
                            nearestD2 = d2;
                            nearest = m;
                        }
                    }

                    Add(i, nearest, LinkTier.Gate);
                }
                else if (g[i].Role == GalaxyRole.Satellite)
                {
                    Add(i, g[i].Host, LinkTier.Tether);
                }
            }

            var ring = new List<(double Angle, int Index)>();
            for (var i = 0; i < g.Count; i++)
            {
                if (g[i].Role != GalaxyRole.Satellite && !g[i].Giant)
                {
                    ring.Add((DMath.Atan2(g[i].Y, g[i].X), i));
                }
            }

            ring.Sort((p, q) => p.Angle != q.Angle ? p.Angle.CompareTo(q.Angle) : p.Index.CompareTo(q.Index));
            if (ring.Count == 2)
            {
                Add(ring[0].Index, ring[1].Index, LinkTier.Wormhole);
            }
            else if (ring.Count > 2)
            {
                for (var k = 0; k < ring.Count; k++)
                {
                    Add(ring[k].Index, ring[(k + 1) % ring.Count].Index, LinkTier.Wormhole);
                }
            }

            var keys = new List<long>(best.Keys);
            keys.Sort();
            var links = new ClusterLink[keys.Count];
            for (var k = 0; k < keys.Count; k++)
            {
                int a = (int)(keys[k] >> 32), b = (int)(keys[k] & 0xFFFFFFFF);
                links[k] = new ClusterLink { A = a, B = b, Tier = best[keys[k]], Length = DMath.Round(Math.Sqrt(D2(g[a], g[b])), 3) };
            }

            return links;
        }

        private static double D2(Draft a, Draft b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }
    }
}
