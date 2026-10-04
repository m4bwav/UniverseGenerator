using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniverseGeneration.Unity
{
    /// <summary>
    /// Editable <see cref="GeneratorTables"/> as a Unity asset: create one with Assets > Create > Universe Generator >
    /// Tables, edit the weights in the Inspector (a new asset starts as a copy of the built-in tables), then call
    /// <see cref="Register"/> before generating and put the returned id in <see cref="GeneratorOptions.Tables"/>.
    /// A JSON file written by <see cref="GeneratorTables.ToJson"/> can be used instead of the lists.
    /// </summary>
    [CreateAssetMenu(fileName = "UniverseTables", menuName = "Universe Generator/Tables")]
    public sealed class TablesAsset : ScriptableObject
    {
        [Tooltip("The id links and GeneratorOptions.Tables name: lowercase letters, digits and dashes.")]
        public string id = "my-tables";

        [Tooltip("Optional: JSON from GeneratorTables.ToJson. When set, it is read instead of the tables below.")]
        public TextAsset json;

        [Tooltip("The weight tables; a table left out keeps its built-in weights, a choice left out has weight 0.")]
        public List<Table> tables = new List<Table>();

        /// <summary>These tables as a <see cref="GeneratorTables"/>; throws with the reason when an id, name or weight is not allowed.</summary>
        public GeneratorTables ToTables()
        {
            if (json != null)
            {
                return GeneratorTables.FromJson(json.text);
            }

            var list = new List<WeightTable>();
            foreach (var table in tables)
            {
                var entries = new List<WeightEntry>();
                foreach (var entry in table.entries)
                {
                    entries.Add(new WeightEntry { Name = entry.name, Weight = entry.weight });
                }

                list.Add(new WeightTable { Id = table.id, Entries = entries });
            }

            return new GeneratorTables(id, list);
        }

        /// <summary>Registers these tables and returns their id, for <c>Preset.Default with { Tables = id }</c>.</summary>
        public string Register()
        {
            var built = ToTables();
            GeneratorTables.Register(built);
            return built.Id;
        }

        /// <summary>Replaces the lists with <paramref name="source"/>'s tables, every choice listed; the id is kept.</summary>
        public void CopyFrom(GeneratorTables source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            tables = new List<Table>();
            foreach (var table in source.Tables)
            {
                var copy = new Table { id = table.Id };
                foreach (var entry in table.Entries)
                {
                    copy.entries.Add(new Entry { name = entry.Name, weight = entry.Weight });
                }

                tables.Add(copy);
            }
        }

        // A new asset starts as a copy of the built-in tables, so every table and choice is there to edit.
        private void Reset() => CopyFrom(GeneratorTables.Default);

        /// <summary>One weight table as Unity serializes it.</summary>
        [Serializable]
        public sealed class Table
        {
            public string id = "";
            public List<Entry> entries = new List<Entry>();
        }

        /// <summary>One choice and its weight.</summary>
        [Serializable]
        public sealed class Entry
        {
            public string name = "";
            public int weight;
        }
    }
}
