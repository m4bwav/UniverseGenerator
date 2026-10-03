#nullable enable
using System;
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>The overall form of a galaxy's map.</summary>
    public enum GalaxyShape
    {
        /// <summary>Drawn from the seed among the shapes that read at the requested count: spirals and bars from 80 systems.</summary>
        Auto,

        /// <summary>Two to four logarithmic arms around a bright core.</summary>
        Spiral,

        /// <summary>Arms that start from the ends of a central bar.</summary>
        Barred,

        /// <summary>A smooth oval, densest in the middle.</summary>
        Elliptical,

        /// <summary>A ring of systems around a sparse middle and a small core.</summary>
        Ring,

        /// <summary>A few uneven clumps.</summary>
        Irregular,
    }

    /// <summary>
    /// A galaxy: a map of star systems joined by lanes, grouped into named regions, with danger growing away from the
    /// core. Positions are in game units within <see cref="Radius"/> of the centre. <c>Galaxy.Generate("my-seed")</c>
    /// makes one; each system's details are generated when first read, and equal what its address alone regenerates.
    /// </summary>
    public sealed record Galaxy
    {
        /// <summary>Where it is, such as <c>v1-my-seed/galaxy</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, such as "Halcyon Galaxy"; unique within a cluster.</summary>
        public string Name { get; init; } = "";

        /// <summary>Its type code, such as Sb, SBc, E3, S0, cD, Irr, dSph or Ring (plan U6). Alone, it is read from the shape.</summary>
        public string Type { get; init; } = "";

        /// <summary>The galaxy in words: "an old giant elliptical galaxy, rich in metals, with a quasar at its core".</summary>
        public string Descriptor { get; init; } = "";

        /// <summary>How old its stars are overall; old galaxies hold more old regions. Mature for a galaxy alone.</summary>
        public StellarAge Age { get; init; }

        /// <summary>How rich its stars are in metals: rich galaxies have more giant planets. Normal for a galaxy alone.</summary>
        public GalaxyRichness Richness { get; init; }

        /// <summary>What its central black hole is doing; Quiet for a galaxy alone.</summary>
        public CoreActivity CoreActivity { get; init; }

        /// <summary>The radius around the centre, in game units, where an active core's radiation raises danger; 0 when quiet.</summary>
        public double CoreHazardRadius { get; init; }

        /// <summary>Its links to other galaxies of its cluster, in the cluster's link order; empty for a galaxy alone.</summary>
        public IReadOnlyList<GalaxyGate> Gates { get; init; } = Array.Empty<GalaxyGate>();

        /// <summary>Where the universe's sky landmark appears from here; null for a galaxy outside a universe, and for the landmark itself.</summary>
        public SkyLandmark? Landmark { get; init; }

        /// <summary>The shape the map was drawn with (never <see cref="GalaxyShape.Auto"/>).</summary>
        public GalaxyShape Shape { get; init; }

        /// <summary>The number of spiral arms; 0 for shapes without arms.</summary>
        public int Arms { get; init; }

        /// <summary>The radius in game units; every system lies within it of the centre (0, 0).</summary>
        public double Radius { get; init; }

        /// <summary>The index of the core system, the one nearest the centre; hops and danger count from it.</summary>
        public int Core { get; init; }

        /// <summary>Every system as the map shows it: name, position, star class, region, hops, danger. Cheap to read.</summary>
        public IReadOnlyList<MapEntry> Map { get; init; } = Array.Empty<MapEntry>();

        /// <summary>The lanes between systems, each listed once with the lower index first.</summary>
        public IReadOnlyList<Lane> Lanes { get; init; } = Array.Empty<Lane>();

        /// <summary>The named regions; the first is centred on the core.</summary>
        public IReadOnlyList<GalaxyRegion> Regions { get; init; } = Array.Empty<GalaxyRegion>();

        /// <summary>Every system in full, in the order of <see cref="Map"/>; each is generated when first read.</summary>
        public IReadOnlyList<StarSystem> Systems { get; init; } = Array.Empty<StarSystem>();

        /// <summary>System <paramref name="index"/> in full, the same as <c>Systems[index]</c>.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The galaxy has no system with that index; the message gives the range.</exception>
        public StarSystem System(int index)
        {
            if (index < 0 || index >= Systems.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"This galaxy has {Systems.Count} systems, numbered 0 to {Systems.Count - 1}; you asked for {index}.");
            }

            return Systems[index];
        }

        /// <summary>Generates a galaxy from any seed text, such as "my-seed".</summary>
        /// <exception cref="ArgumentException">The seed is too long or an option is out of range; the message says which.</exception>
        public static Galaxy Generate(string seed, GeneratorOptions? options = null)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            return GalaxyGenerator.Generate(new Address(GeneratorVersion.Current, GeneratorOptions.CheckSeed(seed), "galaxy"), o, GalaxyContext.Alone);
        }

        /// <summary>Generates a galaxy from a number; the same as passing the number's digits as text.</summary>
        public static Galaxy Generate(long seed, GeneratorOptions? options = null) =>
            Generate(GeneratorOptions.SeedText(seed), options);
    }

    /// <summary>A system as the galaxy map shows it, without its planets.</summary>
    public sealed record MapEntry
    {
        /// <summary>Its index in the galaxy, as in <c>galaxy.System(index)</c>.</summary>
        public int Index { get; init; }

        /// <summary>Its address, such as <c>v1-my-seed/galaxy/system/31</c>.</summary>
        public string Address { get; init; } = "";

        /// <summary>Its name, unique in the galaxy; the same as the full system's.</summary>
        public string Name { get; init; } = "";

        /// <summary>Across, in game units from the centre.</summary>
        public double X { get; init; }

        /// <summary>Up, in game units from the centre.</summary>
        public double Y { get; init; }

        /// <summary>The class of its primary star.</summary>
        public StarClass StarClass { get; init; }

        /// <summary>The index of its region in <see cref="Galaxy.Regions"/>.</summary>
        public int Region { get; init; }

        /// <summary>Lanes on the shortest route to the core.</summary>
        public int Hops { get; init; }

        /// <summary>How dangerous it is, 1 (safe) to 10 (deadly): grows with hops from the core, varied by region and system.</summary>
        public int Danger { get; init; }

        /// <summary>True when every route between some other systems passes through this one.</summary>
        public bool Chokepoint { get; init; }
    }

    /// <summary>A lane between two systems of a galaxy.</summary>
    public sealed record Lane
    {
        /// <summary>The lower system index.</summary>
        public int A { get; init; }

        /// <summary>The higher system index.</summary>
        public int B { get; init; }

        /// <summary>True when this lane is the only route between the two parts of the map it joins.</summary>
        public bool Bridge { get; init; }
    }

    /// <summary>A named region of a galaxy, such as "Vela Reach", with an age and a story theme.</summary>
    public sealed record GalaxyRegion
    {
        /// <summary>Its index in <see cref="Galaxy.Regions"/>.</summary>
        public int Index { get; init; }

        /// <summary>Its name, unique in the galaxy.</summary>
        public string Name { get; init; } = "";

        /// <summary>How old its stars are; old regions hold more giants and white dwarfs.</summary>
        public StellarAge Age { get; init; }

        /// <summary>A story theme, such as "frontier" or "precursor ruins".</summary>
        public string Theme { get; init; } = "";

        /// <summary>The index of the system at its centre.</summary>
        public int Centre { get; init; }
    }

    /// <summary>Builds a galaxy record from its layout, and gives each system its context (plan D17).</summary>
    internal static class GalaxyGenerator
    {
        /// <summary>The reach of a merging pair's tidal frontier around its tidal link, in game units.</summary>
        public const double FrontierRadius = 300;

        public const string FrontierTheme = "tidal frontier";

        public static Galaxy Generate(Address address, GeneratorOptions options, GalaxyContext context)
        {
            var seed = address.ObjectSeed;
            var layout = GalaxyLayout.Generate(seed, context.Systems ?? options.Systems, context.Shape ?? options.Shape, context.Tuning);
            var n = layout.Count;

            var regions = new GalaxyRegion[layout.Regions.Count];
            for (var r = 0; r < regions.Length; r++)
            {
                var (name, age, theme, centre) = layout.Regions[r];
                regions[r] = new GalaxyRegion { Index = r, Name = name, Age = age, Theme = theme, Centre = centre };
            }

            // Cluster links open at the core (gates) or at the system farthest out towards the other galaxy.
            var gates = new GalaxyGate[context.Links.Count];
            var frontier = -1;
            for (var k = 0; k < gates.Length; k++)
            {
                var (other, otherName, tier, dx, dy, cluster) = context.Links[k];
                var system = layout.Core;
                if (tier != LinkTier.Gate)
                {
                    var farthest = double.MinValue;
                    for (var i = 0; i < n; i++)
                    {
                        var reach = layout.X[i] * dx + layout.Y[i] * dy;
                        if (reach > farthest)
                        {
                            farthest = reach;
                            system = i;
                        }
                    }
                }

                gates[k] = new GalaxyGate { Galaxy = other, Name = otherName, Tier = tier, System = system, Cluster = cluster };
                if (tier == LinkTier.Tidal && context.Frontier)
                {
                    frontier = system;
                }
            }

            // A merging pair's tidal frontier (plan A12): lawless, 2 more danger within 300 units of the tidal link, and
            // its region themed for it. Values only; no stream draws differently.
            var danger = layout.Danger;
            if (frontier >= 0)
            {
                danger = (int[])layout.Danger.Clone();
                for (var i = 0; i < n; i++)
                {
                    double fx = layout.X[i] - layout.X[frontier], fy = layout.Y[i] - layout.Y[frontier];
                    if (fx * fx + fy * fy <= FrontierRadius * FrontierRadius)
                    {
                        danger[i] = Math.Min(10, danger[i] + 2);
                    }
                }

                var r = layout.Region[frontier];
                regions[r] = regions[r] with { Theme = FrontierTheme };
            }

            // The skeleton pass: each system's region age and danger come from the map, and its name, drawn from its own
            // stream for its star's class, is redrawn from the same stream until no earlier system has it.
            var map = new MapEntry[n];
            var contexts = new SystemContext[n];
            var names = new HashSet<string>(StringComparer.Ordinal);
            var richness = context.Richness == GalaxyRichness.Rich ? 160 : context.Richness == GalaxyRichness.Poor ? 50 : 100;
            for (var i = 0; i < n; i++)
            {
                var systemSeed = GalaxyLayout.SystemSeed(seed, i);
                var age = regions[layout.Region[i]].Age;
                var (star, _) = StarGenerator.Roll(Seeds.Stream(systemSeed, "star"), age, options.StarMix);
                var name = UniqueName(Seeds.Stream(systemSeed, "names"), star.Class, names, i);
                contexts[i] = new SystemContext(name, age, danger[i], richness);
                map[i] = new MapEntry
                {
                    Index = i,
                    Address = address.Child("system", i).ToString(),
                    Name = name,
                    X = layout.X[i],
                    Y = layout.Y[i],
                    StarClass = star.Class,
                    Region = layout.Region[i],
                    Hops = layout.Hops[i],
                    Danger = danger[i],
                    Chokepoint = layout.Chokepoint[i],
                };
            }

            var lanes = new Lane[layout.Lanes.Count];
            for (var l = 0; l < lanes.Length; l++)
            {
                var (a, b, bridge) = layout.Lanes[l];
                lanes[l] = new Lane { A = a, B = b, Bridge = bridge };
            }

            var type = context.Type ?? GalaxyTypes.Derive(layout.Shape, layout.Pitch, layout.Ellipse);
            return new Galaxy
            {
                Address = address.ToString(),
                Name = context.Name ?? ClusterGenerator.LoneName(Seeds.Stream(seed, "name")),
                Type = type,
                Descriptor = GalaxyTypes.Describe(type, context.Age, context.Richness, context.CoreActivity, context.Host),
                Age = context.Age,
                Richness = context.Richness,
                CoreActivity = context.CoreActivity,
                CoreHazardRadius = ClusterGenerator.HazardRadius(context.CoreActivity),
                Gates = gates,
                Landmark = context.Landmark,
                Shape = layout.Shape,
                Arms = layout.Shape == GalaxyShape.Spiral || layout.Shape == GalaxyShape.Barred ? layout.Arms : 0,
                Radius = GalaxyLayout.Radius,
                Core = layout.Core,
                Map = map,
                Lanes = lanes,
                Regions = regions,
                Systems = new LazySystems(address, contexts, options),
            };
        }

        private static string UniqueName(Pcg32 rng, StarClass c, HashSet<string> used, int index)
        {
            for (var tries = 0; tries < 100; tries++)
            {
                var name = StarNames.Draw(rng, c);
                if (used.Add(name))
                {
                    return name;
                }
            }

            // Out of reach in practice (hundreds of thousands of catalogue names); kept so generation never loops forever.
            var fallback = StarNames.Draw(rng, c) + " " + (index + 1).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
            used.Add(fallback);
            return fallback;
        }
    }

    /// <summary>A galaxy's systems, each generated with its context on first read and kept.</summary>
    internal sealed class LazySystems : IReadOnlyList<StarSystem>
    {
        private readonly Address _galaxy;
        private readonly SystemContext[] _contexts;
        private readonly GeneratorOptions _options;
        private readonly StarSystem?[] _made;

        public LazySystems(Address galaxy, SystemContext[] contexts, GeneratorOptions options)
        {
            _galaxy = galaxy;
            _contexts = contexts;
            _options = options;
            _made = new StarSystem?[contexts.Length];
        }

        public int Count => _made.Length;

        // Generation is deterministic, so two threads reading at once at worst generate the same system twice.
        public StarSystem this[int index]
        {
            get
            {
                if (index < 0 || index >= _made.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, $"This galaxy has {_made.Length} systems, numbered 0 to {_made.Length - 1}; you asked for {index}.");
                }

                return _made[index] ??= StarSystemGenerator.Generate(_galaxy.Child("system", index), _contexts[index], _options);
            }
        }

        public IEnumerator<StarSystem> GetEnumerator()
        {
            for (var i = 0; i < _made.Length; i++)
            {
                yield return this[i];
            }
        }

        global::System.Collections.IEnumerator global::System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
