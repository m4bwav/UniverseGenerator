#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>
    /// Post-processors that change objects as they are generated, such as adding fields to <c>Custom</c>, renaming a
    /// system or turning a planet into a colony. Each takes the generated object and returns the one to use (usually
    /// <c>with { ... }</c>). Pass the same hooks to <c>Generate</c> and to <see cref="Universe.At(string, GeneratorOptions?, GeneratorHooks?)"/>:
    /// hooks are code, so a link cannot carry them, and <c>At</c> runs them again on every object it regenerates.
    /// Hooks run on what the generator made; they never change what it draws, so the seed promise holds for the input
    /// they see. A hook must give the same result for the same input, or <c>At</c> cannot regenerate what it made.
    /// </summary>
    /// <remarks>
    /// Hooks are kept out of <see cref="GeneratorOptions"/> on purpose: options are a value (two equal options make the
    /// same universe) with a text code for links, and a delegate has neither value equality nor a text form.
    /// </remarks>
    public sealed class GeneratorHooks
    {
        /// <summary>Runs on every planet, in a system or alone, before its system's <see cref="OnSystem"/>.</summary>
        public Func<Planet, Planet>? OnPlanet { get; init; }

        /// <summary>Runs on every star system, in a galaxy, a void or alone, after <see cref="OnPlanet"/> ran on its planets; its moons, belts and stations are its to change.</summary>
        public Func<StarSystem, StarSystem>? OnSystem { get; init; }

        /// <summary>Runs on every galaxy, alone or in a cluster; its systems run their own hooks when first read.</summary>
        public Func<Galaxy, Galaxy>? OnGalaxy { get; init; }

        /// <summary>Runs on every group or cluster of galaxies, alone or in a universe.</summary>
        public Func<GalaxyCluster, GalaxyCluster>? OnCluster { get; init; }

        /// <summary>Runs on the universe; its clusters have run theirs, and its voids' systems run theirs when first read.</summary>
        public Func<Universe, Universe>? OnUniverse { get; init; }
    }

    /// <summary>Applies <see cref="GeneratorHooks"/> where each object is first made.</summary>
    internal static class Hook
    {
        public static Planet Planet(GeneratorHooks? hooks, Planet planet) =>
            hooks?.OnPlanet is null ? planet : NotNull(hooks.OnPlanet(planet), nameof(GeneratorHooks.OnPlanet));

        public static StarSystem System(GeneratorHooks? hooks, StarSystem system)
        {
            if (hooks is null)
            {
                return system;
            }

            if (hooks.OnPlanet != null && system.Planets.Count > 0)
            {
                var planets = new Planet[system.Planets.Count];
                for (var i = 0; i < planets.Length; i++)
                {
                    planets[i] = Planet(hooks, system.Planets[i]);
                }

                system = system with { Planets = planets };
            }

            return hooks.OnSystem is null ? system : NotNull(hooks.OnSystem(system), nameof(GeneratorHooks.OnSystem));
        }

        public static Galaxy Galaxy(GeneratorHooks? hooks, Galaxy galaxy) =>
            hooks?.OnGalaxy is null ? galaxy : NotNull(hooks.OnGalaxy(galaxy), nameof(GeneratorHooks.OnGalaxy));

        public static GalaxyCluster Cluster(GeneratorHooks? hooks, GalaxyCluster cluster) =>
            hooks?.OnCluster is null ? cluster : NotNull(hooks.OnCluster(cluster), nameof(GeneratorHooks.OnCluster));

        public static Universe Universe(GeneratorHooks? hooks, Universe universe) =>
            hooks?.OnUniverse is null ? universe : NotNull(hooks.OnUniverse(universe), nameof(GeneratorHooks.OnUniverse));

        private static T NotNull<T>(T? value, string hook)
            where T : class =>
            value ?? throw new InvalidOperationException($"The {hook} hook returned null; return the object it was given, or a changed copy.");
    }
}
