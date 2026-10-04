#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UniverseGeneration
{
    /// <summary>
    /// Helpers for the <c>Custom</c> fields every generated object carries: your own text values by name, empty until a
    /// <see cref="GeneratorHooks"/> hook (or your code) sets them, and written by <c>ToJson</c> with the names in ordinal
    /// order. <c>planet with { Custom = planet.Custom.With("owner", "red") }</c> adds one.
    /// </summary>
    public static class CustomFields
    {
        /// <summary>No fields: the value of every <c>Custom</c> until something sets it.</summary>
        public static IReadOnlyDictionary<string, string> Empty { get; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal));

        /// <summary>A copy of <paramref name="fields"/> with <paramref name="name"/> set to <paramref name="value"/>; the original is unchanged.</summary>
        /// <exception cref="ArgumentNullException">A name or value is null.</exception>
        public static IReadOnlyDictionary<string, string> With(this IReadOnlyDictionary<string, string> fields, string name, string value)
        {
            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value), "A custom field holds text; use \"\" for an empty value.");
            }

            var copy = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in fields)
            {
                copy[pair.Key] = pair.Value;
            }

            copy[name] = value;
            return new ReadOnlyDictionary<string, string>(copy);
        }
    }
}
