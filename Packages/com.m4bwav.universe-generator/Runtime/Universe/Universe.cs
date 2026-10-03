#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>
    /// Entry points that work across levels. <see cref="At"/> regenerates any object from its address alone, and gives
    /// the same result as generating the level it belongs to and walking down to it (plan D17).
    /// </summary>
    public static class Universe
    {
        /// <summary>
        /// Regenerates the object at <paramref name="address"/>, such as <c>v1-my-seed/galaxy/system/31/planet/2</c>: a
        /// <see cref="Galaxy"/>, <see cref="StarSystem"/>, <see cref="Planet"/>, <see cref="Moon"/> or <see cref="Station"/>.
        /// An address does not carry options, so pass the ones the object was generated with.
        /// </summary>
        /// <exception cref="ArgumentException">The address cannot be read, names a level or object that does not exist, or an option is out of range; the message says which.</exception>
        public static object At(string address, GeneratorOptions? options = null)
        {
            var o = options ?? Preset.Default;
            o.Validate();
            if (!Address.TryParse(address, out var parsed, out var error))
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
            switch (a.Root)
            {
                case "galaxy":
                    var galaxy = GalaxyGenerator.Generate(where, o);
                    if (path.Count == 0)
                    {
                        return galaxy;
                    }

                    system = Pick(galaxy.Systems, path, step++, "system", where);
                    where = where.Child("system", path[0].Index);
                    break;
                case "system":
                    system = StarSystemGenerator.Generate(where, SystemContext.Alone, o);
                    break;
                default:
                    throw new ArgumentException($"\"{a.Root}\" is not a level this package generates; an address starts from galaxy or system, as in v1-my-seed/galaxy/system/3.", nameof(address));
            }

            if (step == path.Count)
            {
                return system;
            }

            object found;
            if (path[step].Label == "station")
            {
                found = Pick(system.Stations, path, step++, "station", where);
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

            if (step < path.Count)
            {
                throw new ArgumentException($"Nothing in this package lies below {Describe(path, step)}; the address goes on with \"{path[step].Label}/{path[step].Index.ToString(CultureInfo.InvariantCulture)}\".", nameof(address));
            }

            return found;
        }

        private static T Pick<T>(IReadOnlyList<T> items, IReadOnlyList<(string Label, int Index)> path, int step, string label, Address address)
        {
            var (actual, index) = path[step];
            if (actual != label)
            {
                var expected = label == "planet" ? "planet or station" : label;
                throw new ArgumentException($"Below {address} comes a {expected}; the address has \"{actual}\".", nameof(address));
            }

            if (index >= items.Count)
            {
                var range = items.Count == 0 ? "none" : $"numbered 0 to {items.Count - 1}";
                throw new ArgumentException($"{address} has {items.Count} {label}s ({range}); the address asks for {label} {index.ToString(CultureInfo.InvariantCulture)}. Were the same options passed as when it was generated?", nameof(address));
            }

            return items[index];
        }

        private static string Describe(IReadOnlyList<(string Label, int Index)> path, int step) =>
            step == 0 ? "the level" : "a " + path[step - 1].Label;
    }
}
