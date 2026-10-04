#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>Which kind of galaxy gathering a cluster is.</summary>
    public enum ClusterKind
    {
        /// <summary>Drawn from the seed: a group six times in ten, a cluster four.</summary>
        Auto,

        /// <summary>A small group: two big spirals with their satellites and a few small galaxies, like the Local Group.</summary>
        Group,

        /// <summary>A rich cluster: a giant elliptical at the centre, ellipticals crowded round it, spirals at the edge.</summary>
        Cluster,
    }

    /// <summary>A galaxy's part in its cluster.</summary>
    public enum GalaxyRole
    {
        /// <summary>One of the big galaxies; majors are the hubs of the gate network.</summary>
        Major,

        /// <summary>A middle-sized galaxy.</summary>
        Member,

        /// <summary>A small galaxy bound to a major (see <see cref="ClusterEntry.Host"/>).</summary>
        Satellite,

        /// <summary>A small galaxy on its own.</summary>
        Dwarf,
    }

    /// <summary>How rich a galaxy's stars are in metals; rich galaxies have more giant planets.</summary>
    public enum GalaxyRichness
    {
        /// <summary>Metal-poor, as dwarf galaxies are: half as many giant planets.</summary>
        Poor,

        /// <summary>Like the Sun's neighbourhood.</summary>
        Normal,

        /// <summary>Metal-rich, as big galaxies are: more giant planets.</summary>
        Rich,
    }

    /// <summary>What the black hole at a galaxy's centre is doing.</summary>
    public enum CoreActivity
    {
        /// <summary>Asleep; the core is safe.</summary>
        Quiet,

        /// <summary>A bright active nucleus; its radiation adds 2 to the danger of systems near the centre.</summary>
        Seyfert,

        /// <summary>A quasar, outshining the galaxy; its radiation adds 3 to the danger of systems near the centre.</summary>
        Quasar,
    }

    /// <summary>The kind of a link between galaxies.</summary>
    public enum LinkTier
    {
        /// <summary>A hub gate: majors join each other, and every other galaxy joins its nearest major. Ends at the core.</summary>
        Gate,

        /// <summary>A link of the wormhole ring that makes every galaxy reachable. Ends at the edge facing the other galaxy.</summary>
        Wormhole,

        /// <summary>A satellite's link to its host. Ends at the edge facing the other galaxy.</summary>
        Tether,

        /// <summary>A filament of the cosmic web, to a galaxy of another cluster in the universe. Ends at the edge facing it.</summary>
        Filament,

        /// <summary>The tidal link between the two galaxies of a merging pair. Ends at the edge facing the other galaxy.</summary>
        Tidal,
    }

    /// <summary>
    /// A group or cluster of galaxies: a map of galaxies with their type codes, ages and cores, joined by gates, a ring
    /// of wormholes and tethers so that every galaxy can be reached. Positions are in cluster units within about
    /// <see cref="Radius"/> of the centre. <c>GalaxyCluster.Generate("my-seed")</c> makes one; each galaxy is generated
    /// when first read, and equals what its address alone regenerates.
    /// </summary>
    public sealed partial record GalaxyCluster
    {
        /// <summary>Where it is, such as <c>v1-my-seed/cluster</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "Halcyon Group" or "Vesper Cluster".</summary>
        public string Name { get; init; } = "";

        /// <summary>A group or a cluster (never <see cref="ClusterKind.Auto"/>).</summary>
        public ClusterKind Kind { get; init; }

        /// <summary>The cluster's epoch: young clusters hold younger spirals and more quasars.</summary>
        public StellarAge Age { get; init; }

        /// <summary>The radius of the cluster map in cluster units; satellites may lie a little beyond it.</summary>
        public double Radius { get; init; }

        /// <summary>Every galaxy as the cluster map shows it. Cheap to read.</summary>
        public IReadOnlyList<ClusterEntry> Map { get; init; } = Array.Empty<ClusterEntry>();

        /// <summary>The links between galaxies, each pair once with the lower index first, in index order.</summary>
        public IReadOnlyList<ClusterLink> Links { get; init; } = Array.Empty<ClusterLink>();

        /// <summary>Every galaxy in full, in the order of <see cref="Map"/>; each is generated when first read.</summary>
        public IReadOnlyList<Galaxy> Galaxies { get; init; } = Array.Empty<Galaxy>();

        /// <summary>Your own fields, by name, set by a <see cref="GeneratorHooks"/> hook or your code; empty from the generator.</summary>
        public IReadOnlyDictionary<string, string> Custom { get; init; } = CustomFields.Empty;

        /// <summary>Galaxy <paramref name="index"/> in full, the same as <c>Galaxies[index]</c>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The cluster has no galaxy with that index; the message gives the range.</exception>
        public Galaxy Galaxy(int index)
        {
            if (index < 0 || index >= Galaxies.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"This cluster has {Galaxies.Count} galaxies, numbered 0 to {Galaxies.Count - 1}; you asked for {index}.");
            }

            return Galaxies[index];
        }

        /// <summary>Generates a group or cluster of galaxies from any seed text, such as "my-seed".</summary>
        /// <exception cref="ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static GalaxyCluster Generate(string seed, GeneratorOptions? options = null) => Generate(seed, options, null);

        /// <summary>Generates a group or cluster from any seed text, running <paramref name="hooks"/> on it and on each galaxy as it is first read.</summary>
        /// <exception cref="ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static GalaxyCluster Generate(string seed, GeneratorOptions? options, GeneratorHooks? hooks)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            var address = new Address(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "cluster");
            return Hook.Cluster(hooks, ClusterGenerator.Generate(address, o, ClusterContext.Alone, hooks));
        }

        /// <summary>Generates a cluster from a number; the same as passing the number's digits as text.</summary>
        public static GalaxyCluster Generate(long seed, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), options);
    }

    /// <summary>A galaxy as the cluster map shows it, without its systems.</summary>
    public sealed record ClusterEntry
    {
        /// <summary>Its index in the cluster, as in <c>cluster.Galaxy(index)</c>.</summary>
        public int Index { get; init; }

        /// <summary>Its address, such as <c>v1-my-seed/cluster/galaxy/2</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, unique in the cluster.</summary>
        public string Name { get; init; } = "";

        /// <summary>Its type code, such as Sb, SBc, E3, S0, cD, Irr, dSph or Ring; it decides the shape.</summary>
        public string Type { get; init; } = "";

        /// <summary>The shape its map is drawn with, from <see cref="Type"/>.</summary>
        public GalaxyShape Shape { get; init; }

        /// <summary>Its part in the cluster.</summary>
        public GalaxyRole Role { get; init; }

        /// <summary>The index of the major a satellite is bound to; -1 for every other galaxy.</summary>
        public int Host { get; init; }

        /// <summary>Across, in cluster units from the centre.</summary>
        public double X { get; init; }

        /// <summary>Up, in cluster units from the centre.</summary>
        public double Y { get; init; }

        /// <summary>Its radius on the cluster map, in cluster units, for drawing; grows with its system count.</summary>
        public double Size { get; init; }

        /// <summary>How many systems it has.</summary>
        public int Systems { get; init; }

        /// <summary>How old its stars are; old galaxies have more old regions.</summary>
        public StellarAge Age { get; init; }

        /// <summary>How rich its stars are in metals.</summary>
        public GalaxyRichness Richness { get; init; }

        /// <summary>What its central black hole is doing.</summary>
        public CoreActivity CoreActivity { get; init; }
    }

    /// <summary>A link between two galaxies of a cluster.</summary>
    public sealed record ClusterLink
    {
        /// <summary>The lower galaxy index.</summary>
        public int A { get; init; }

        /// <summary>The higher galaxy index.</summary>
        public int B { get; init; }

        /// <summary>Gate, wormhole or tether.</summary>
        public LinkTier Tier { get; init; }

        /// <summary>The distance between the two galaxies on the cluster map, in cluster units.</summary>
        public double Length { get; init; }
    }

    /// <summary>One end of a cluster link, as the galaxy it starts from sees it.</summary>
    public sealed record GalaxyGate
    {
        /// <summary>The index in the cluster of the galaxy it leads to.</summary>
        public int Galaxy { get; init; }

        /// <summary>The name of the galaxy it leads to.</summary>
        public string Name { get; init; } = "";

        /// <summary>Gate, wormhole or tether.</summary>
        public LinkTier Tier { get; init; }

        /// <summary>The index of the system in this galaxy where it opens.</summary>
        public int System { get; init; }

        /// <summary>For a filament, the index in the universe of the cluster it leads to; -1 for a link inside the cluster.</summary>
        public int Cluster { get; init; } = -1;
    }

    /// <summary>The universe's sky landmark as one galaxy sees it (plan A13).</summary>
    public sealed record SkyLandmark
    {
        /// <summary>The landmark galaxy's name.</summary>
        public string Name { get; init; } = "";

        /// <summary>The landmark galaxy's address.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its direction on the universe map, in whole degrees counter-clockwise from +x (0 to 359).</summary>
        public int Bearing { get; init; }

        /// <summary>How far away it is, in light-years, to the nearest thousand.</summary>
        public long LightYears { get; init; }
    }

    /// <summary>A cluster's galaxies, each generated with its context on first read and kept.</summary>
    internal sealed class LazyGalaxies : IReadOnlyList<Galaxy>
    {
        private readonly Address _cluster;
        private readonly GalaxyContext[] _contexts;
        private readonly GeneratorOptions _options;
        private readonly Galaxy?[] _made;
        private readonly GeneratorHooks? _hooks;

        public LazyGalaxies(Address cluster, GalaxyContext[] contexts, GeneratorOptions options, GeneratorHooks? hooks)
        {
            _cluster = cluster;
            _contexts = contexts;
            _options = options;
            _made = new Galaxy?[contexts.Length];
            _hooks = hooks;
        }

        public int Count => _made.Length;

        /// <summary>What the cluster decided for each galaxy (for tests).</summary>
        public IReadOnlyList<GalaxyContext> Contexts => _contexts;

        // Generation is deterministic, so two threads reading at once at worst generate the same galaxy twice.
        public Galaxy this[int index]
        {
            get
            {
                if (index < 0 || index >= _made.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, $"This cluster has {_made.Length} galaxies, numbered 0 to {_made.Length - 1}; you asked for {index}.");
                }

                return _made[index] ??= GalaxyGenerator.Generate(_cluster.Child("galaxy", index), _options, _contexts[index], _hooks);
            }
        }

        public IEnumerator<Galaxy> GetEnumerator()
        {
            for (var i = 0; i < _made.Length; i++)
            {
                yield return this[i];
            }
        }

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
