using System.Collections.Generic;
using UniverseGeneration;

namespace UniverseGeneration.Samples
{
    /// <summary>
    /// The sample's generation code, with no engine types, so the package's tests compile and run it on .NET and
    /// .NET Framework (the sample cannot rot). GalaxyPrinter shows these lines in the Unity console.
    /// </summary>
    public static class GalaxyReport
    {
        /// <summary>A heading for the galaxy, then one line per system and one per faction.</summary>
        public static IEnumerable<string> Lines(string seed, int systems)
        {
            var galaxy = Galaxy.Generate(seed, Preset.Default with { Systems = systems });
            yield return $"{galaxy.Name}: {galaxy.Descriptor}, {galaxy.Systems.Count} systems";
            foreach (var system in galaxy.Systems)
            {
                yield return $"{system.Name}: {system.Descriptor}, danger {system.Danger} ({system.Address})";
            }

            foreach (var faction in galaxy.Factions)
            {
                yield return faction.Description;
            }
        }
    }
}
