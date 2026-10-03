#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>A cluster's part in its universe.</summary>
    public enum NodeRole
    {
        /// <summary>The home group, holding the home spiral.</summary>
        Home,

        /// <summary>The great cluster, holding the dying giant and the ring galaxy.</summary>
        GreatCluster,

        /// <summary>The group whose two big spirals are merging.</summary>
        Merger,

        /// <summary>Any other group or cluster; the farthest from home holds the distant quasar.</summary>
        Field,
    }

    /// <summary>What a universe landmark is.</summary>
    public enum UniverseLandmarkKind
    {
        /// <summary>The home spiral: a big spiral with a quiet core, where a story can start.</summary>
        Home,

        /// <summary>The dying giant: the great cluster's old central elliptical, its stars burning out.</summary>
        DyingGiant,

        /// <summary>A ring galaxy, left by a collision long ago.</summary>
        RingGalaxy,

        /// <summary>The distant quasar, seen in every galaxy's sky.</summary>
        Quasar,

        /// <summary>Two big spirals colliding, with a lawless frontier between them.</summary>
        Merger,
    }

    /// <summary>
    /// A small universe at game scale: four to seven galaxy groups and clusters joined by filaments, with named voids
    /// holding lone systems, four landmark galaxies, a merging pair and an epoch that sets every cluster's age.
    /// <c>Universe.Generate("my-seed")</c> makes one; <see cref="At"/> regenerates any object of any level from its address.
    /// Positions are in universe units (100,000 light-years) within about <see cref="Radius"/> of the centre.
    /// </summary>
    public sealed record Universe
    {
        /// <summary>Where it is, such as <c>v1-my-seed/universe</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "the Lantern Reach".</summary>
        public string Name { get; init; } = "";

        /// <summary>Its epoch, which every cluster in it takes as its age.</summary>
        public StellarAge Age { get; init; }

        /// <summary>The radius of the universe map in universe units.</summary>
        public double Radius { get; init; }

        /// <summary>Every group and cluster as the universe map shows it.</summary>
        public IReadOnlyList<UniverseNode> Nodes { get; init; } = Array.Empty<UniverseNode>();

        /// <summary>The filaments joining the clusters, each pair once with the lower index first, in index order.</summary>
        public IReadOnlyList<Filament> Filaments { get; init; } = Array.Empty<Filament>();

        /// <summary>The named voids, each holding a few lone systems.</summary>
        public IReadOnlyList<CosmicVoid> Voids { get; init; } = Array.Empty<CosmicVoid>();

        /// <summary>The landmarks: home spiral, dying giant, ring galaxy, distant quasar and the merger.</summary>
        public IReadOnlyList<UniverseLandmark> Landmarks { get; init; } = Array.Empty<UniverseLandmark>();

        /// <summary>The merging pair and the conflict on its frontier.</summary>
        public UniverseMerger Merger { get; init; } = new UniverseMerger();

        /// <summary>Every group and cluster, in the order of <see cref="Nodes"/>; their galaxies are generated when first read.</summary>
        public IReadOnlyList<GalaxyCluster> Clusters { get; init; } = Array.Empty<GalaxyCluster>();

        /// <summary>Cluster <paramref name="index"/>, the same as <c>Clusters[index]</c>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The universe has no cluster with that index; the message gives the range.</exception>
        public GalaxyCluster Cluster(int index)
        {
            if (index < 0 || index >= Clusters.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"This universe has {Clusters.Count} clusters, numbered 0 to {Clusters.Count - 1}; you asked for {index}.");
            }

            return Clusters[index];
        }

        /// <summary>Generates a universe from any seed text, such as "my-seed".</summary>
        /// <exception cref="ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static Universe Generate(string seed, GeneratorOptions? options = null)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            return UniverseGenerator.Generate(new Address(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "universe"), o);
        }

        /// <summary>Generates a universe from a number; the same as passing the number's digits as text.</summary>
        public static Universe Generate(long seed, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), options);

        /// <summary>
        /// A link to the object at <paramref name="address"/> made with <paramref name="options"/>: the address, then
        /// <c>?</c> and <see cref="GeneratorOptions.ToCode"/>, such as <c>v1-my-seed/galaxy/system/31?systems=120</c>, or the
        /// address alone for the defaults. <see cref="At"/> regenerates the object from the link with no options passed.
        /// </summary>
        /// <exception cref="ArgumentException">The address cannot be read, or an option is out of range.</exception>
        public static string Link(string address, GeneratorOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Validate();
            if (!UniverseGeneration.Address.TryParse(address, out _, out var error))
            {
                throw new ArgumentException(error, nameof(address));
            }

            var code = options.ToCode();
            return code.Length == 0 ? address : address + "?" + code;
        }

        /// <summary>
        /// Regenerates the object at <paramref name="address"/>, such as <c>v1-my-seed/galaxy/system/31/planet/2</c> or
        /// <c>v1-my-seed/planet</c> (a planet generated on its own): a <see cref="Universe"/> (<c>v1-my-seed/universe</c>),
        /// <see cref="GalaxyCluster"/> (alone, <c>v1-my-seed/cluster</c>, or <c>v1-my-seed/universe/cluster/2</c>), <see cref="CosmicVoid"/>
        /// (<c>v1-my-seed/universe/void/0</c>), <see cref="Galaxy"/> (alone, or <c>.../cluster/galaxy/2</c>), <see cref="StarSystem"/>,
        /// <see cref="Planet"/>, <see cref="Moon"/>, <see cref="Station"/> or <see cref="Belt"/> (<c>.../system/belt/0</c>).
        /// It gives the same result as generating the level it belongs to and walking down to it (plan D17).
        /// An address does not carry options, so pass the ones the object was generated with, or pass a link from
        /// <see cref="Link"/> (the address, <c>?</c> and the options' code), which carries them.
        /// </summary>
        /// <exception cref="ArgumentException">The address cannot be read, names a level or object that does not exist, or an option is out of range, or a link was passed together with options; the message says which.</exception>
        public static object At(string address, GeneratorOptions? options = null)
        {
            var q = address is null ? -1 : address.IndexOf('?');
            if (q >= 0)
            {
                if (options is not null)
                {
                    throw new ArgumentException("This link carries its options after the ?; pass a link or options, not both.", nameof(address));
                }

                options = GeneratorOptions.FromCode(address!.Substring(q + 1));
                address = address.Substring(0, q);
            }

            var o = options ?? Preset.Default;
            o.Validate();
            if (!UniverseGeneration.Address.TryParse(address, out var parsed, out var error))
            {
                throw new ArgumentException(error, nameof(address));
            }

            var a = parsed!;
            if (!GeneratorVersion.IsSupported(a.Version))
            {
                throw new ArgumentException($"This package generates version {GeneratorVersion.Current}; the address asks for version {a.Version}.", nameof(address));
            }

            GeneratorOptions.CheckSeed(a.Seed);
            var path = a.Path;
            var step = 0;
            var where = new Address(a.Version, a.Seed, a.Root);
            StarSystem system;
            GalaxyCluster cluster;
            switch (a.Root)
            {
                case "universe":
                    var universe = UniverseGenerator.Generate(where, o);
                    if (path.Count == 0)
                    {
                        return universe;
                    }

                    if (path[0].Label == "void")
                    {
                        var hole = Pick(universe.Voids, path, step++, "void", where);
                        where = where.Child("void", path[0].Index);
                        if (step == path.Count)
                        {
                            return hole;
                        }

                        system = Pick(hole.Systems, path, step++, "system", where);
                        where = where.Child("system", path[1].Index);
                        return Below(system, path, step, where);
                    }

                    cluster = Pick(universe.Clusters, path, step++, "cluster", where);
                    where = where.Child("cluster", path[0].Index);
                    break;
                case "cluster":
                    cluster = ClusterGenerator.Generate(where, o);
                    break;
                case "galaxy":
                    var galaxy = GalaxyGenerator.Generate(where, o, GalaxyContext.Alone);
                    if (path.Count == 0)
                    {
                        return galaxy;
                    }

                    system = Pick(galaxy.Systems, path, step++, "system", where);
                    where = where.Child("system", path[0].Index);
                    return Below(system, path, step, where);
                case "system":
                    return Below(StarSystemGenerator.Generate(where, SystemContext.Alone, o), path, step, where);
                case "planet":
                    var lone = PlanetGenerator.Generate(where, o);
                    if (path.Count == 0)
                    {
                        return lone;
                    }

                    var moon = Pick(lone.Moons, path, step++, "moon", where);
                    NothingBelow(path, step);
                    return moon;
                default:
                    throw new ArgumentException($"\"{a.Root}\" is not a level this package generates; an address starts from universe, cluster, galaxy, system or planet, as in v1-my-seed/galaxy/system/3.", nameof(address));
            }

            if (step == path.Count)
            {
                return cluster;
            }

            var member = Pick(cluster.Galaxies, path, step, "galaxy", where);
            where = where.Child("galaxy", path[step].Index);
            step++;
            if (step == path.Count)
            {
                return member;
            }

            system = Pick(member.Systems, path, step, "system", where);
            where = where.Child("system", path[step].Index);
            return Below(system, path, step + 1, where);
        }

        /// <summary>The system itself, or the planet, moon, station or belt the rest of the path names.</summary>
        private static object Below(StarSystem system, IReadOnlyList<(string Label, int Index)> path, int step, Address where)
        {
            if (step == path.Count)
            {
                return system;
            }

            object found;
            if (path[step].Label == "station")
            {
                found = Pick(system.Stations, path, step++, "station", where);
            }
            else if (path[step].Label == "belt")
            {
                found = Pick(system.Belts, path, step++, "belt", where);
            }
            else
            {
                var planet = Pick(system.Planets, path, step++, "planet", where);
                found = planet;
                if (step < path.Count)
                {
                    found = Pick(planet.Moons, path, step++, "moon", where.Child("planet", planet.Index));
                }
            }

            NothingBelow(path, step);
            return found;
        }

        // The parameter is named address so the exception can name it (CA2208): it is the path the address walks.
        private static void NothingBelow(IReadOnlyList<(string Label, int Index)> address, int step)
        {
            if (step < address.Count)
            {
                throw new ArgumentException($"Nothing in this package lies below {Describe(address, step)}; the address goes on with \"{address[step].Label}/{address[step].Index.ToString(CultureInfo.InvariantCulture)}\".", nameof(address));
            }
        }

        private static T Pick<T>(IReadOnlyList<T> items, IReadOnlyList<(string Label, int Index)> path, int step, string label, Address address)
        {
            var (actual, index) = path[step];
            if (actual != label)
            {
                var expected = label == "planet" ? "planet, station or belt" : label == "cluster" ? "cluster or void" : label;
                throw new ArgumentException($"Below {address} comes a {expected}; the address has \"{actual}\".", nameof(address));
            }

            if (index >= items.Count)
            {
                var range = items.Count == 0 ? "none" : $"numbered 0 to {items.Count - 1}";
                var plural = label == "galaxy" ? "galaxies" : label + "s";
                throw new ArgumentException($"{address} has {items.Count} {plural} ({range}); the address asks for {label} {index.ToString(CultureInfo.InvariantCulture)}. Were the same options passed as when it was generated?", nameof(address));
            }

            return items[index];
        }

        private static string Describe(IReadOnlyList<(string Label, int Index)> path, int step) =>
            step == 0 ? "the level" : "a " + path[step - 1].Label;
    }

    /// <summary>A group or cluster as the universe map shows it.</summary>
    public sealed record UniverseNode
    {
        /// <summary>Its index in the universe, as in <c>universe.Cluster(index)</c>.</summary>
        public int Index { get; init; }

        /// <summary>Its address, such as <c>v1-my-seed/universe/cluster/2</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, unique in the universe.</summary>
        public string Name { get; init; } = "";

        /// <summary>A group or a cluster.</summary>
        public ClusterKind Kind { get; init; }

        /// <summary>Its part in the universe.</summary>
        public NodeRole Role { get; init; }

        /// <summary>Across, in universe units from the centre.</summary>
        public double X { get; init; }

        /// <summary>Up, in universe units from the centre.</summary>
        public double Y { get; init; }

        /// <summary>How many galaxies it holds.</summary>
        public int Galaxies { get; init; }
    }

    /// <summary>A filament of the cosmic web between two clusters, opening in one galaxy of each.</summary>
    public sealed record Filament
    {
        /// <summary>The lower cluster index.</summary>
        public int A { get; init; }

        /// <summary>The higher cluster index.</summary>
        public int B { get; init; }

        /// <summary>Its length on the universe map, in universe units.</summary>
        public double Length { get; init; }

        /// <summary>The galaxy of cluster A where it opens.</summary>
        public int GalaxyA { get; init; }

        /// <summary>The galaxy of cluster B where it opens.</summary>
        public int GalaxyB { get; init; }
    }

    /// <summary>A void in the cosmic web: a wide empty space holding a few lone systems far from any galaxy.</summary>
    public sealed record CosmicVoid
    {
        /// <summary>Its index in the universe.</summary>
        public int Index { get; init; }

        /// <summary>Its address, such as <c>v1-my-seed/universe/void/0</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "the Hollow Void".</summary>
        public string Name { get; init; } = "";

        /// <summary>Across, in universe units from the centre.</summary>
        public double X { get; init; }

        /// <summary>Up, in universe units from the centre.</summary>
        public double Y { get; init; }

        /// <summary>Its radius in universe units: the room between it and the nearest cluster, filament, void or the map edge.</summary>
        public double Radius { get; init; }

        /// <summary>Its lone systems: old, quiet and poor in metals; each generated when first read.</summary>
        public IReadOnlyList<StarSystem> Systems { get; init; } = Array.Empty<StarSystem>();
    }

    /// <summary>One of the universe's landmarks.</summary>
    public sealed record UniverseLandmark
    {
        /// <summary>What it is.</summary>
        public UniverseLandmarkKind Kind { get; init; }

        /// <summary>The galaxy's name, or the collision's for the merger.</summary>
        public string Name { get; init; } = "";

        /// <summary>The galaxy's address, or the merging group's.</summary>
        public string Address { get; init; } = "";
    }

    /// <summary>The merging pair (plan U10) and the shared conflict on its tidal frontier (plan A12).</summary>
    public sealed record UniverseMerger
    {
        /// <summary>The collision's name, such as "the Kestrel-Vesper Collision".</summary>
        public string Name { get; init; } = "";

        /// <summary>The index of the merging group in the universe.</summary>
        public int Cluster { get; init; }

        /// <summary>The address of the first galaxy of the pair.</summary>
        public string GalaxyA { get; init; } = "";

        /// <summary>The address of the second galaxy of the pair.</summary>
        public string GalaxyB { get; init; } = "";

        /// <summary>The conflict both galaxies share on the frontier, as one sentence.</summary>
        public string Hook { get; init; } = "";
    }
}
