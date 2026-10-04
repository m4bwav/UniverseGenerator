#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniverseGeneration
{
    /// <summary>One choice of a weight table and its weight; a choice with weight 0 is never drawn.</summary>
    public sealed record WeightEntry
    {
        /// <summary>The choice, such as <c>M</c> (a star class) or <c>Garden</c> (a planet kind).</summary>
        public string Name { get; init; } = "";

        /// <summary>Its weight, 0 to 1,000,000; the chance of a choice is its weight over the table's total.</summary>
        public int Weight { get; init; }
    }

    /// <summary>A weight table the generator draws from, by id, such as <c>star-classes.mature</c>.</summary>
    public sealed record WeightTable
    {
        /// <summary>Which table this is; <see cref="GeneratorTables.Ids"/> lists them.</summary>
        public string Id { get; init; } = "";

        /// <summary>The choices; names come from the built-in table, and a name left out has weight 0.</summary>
        public IReadOnlyList<WeightEntry> Entries { get; init; } = Array.Empty<WeightEntry>();
    }

    /// <summary>
    /// A set of weight tables, the data behind some of the generator's choices: which star classes a region holds (by its
    /// age, and for <see cref="StarMix.Plausible"/>), which rocky planet kinds each orbit zone holds, and the moons of
    /// giant planets. Load one from JSON (<see cref="FromJson"/>), or change <see cref="Default"/> with <see cref="With"/>,
    /// then <see cref="Register"/> it and name it in <see cref="GeneratorOptions.Tables"/>; a link then carries the id,
    /// and <c>Universe.At</c> finds the same tables by it in any program that registered them. The built-in tables give
    /// exactly the output of a generator without tables.
    /// </summary>
    public sealed class GeneratorTables
    {
        /// <summary>The longest id accepted.</summary>
        public const int MaxIdLength = 40;

        /// <summary>The format name written by <see cref="ToJson"/>.</summary>
        public const string Schema = "universe-generator-tables/1";

        private static readonly object s_lock = new object();
        private static readonly Dictionary<string, GeneratorTables> s_registered = new Dictionary<string, GeneratorTables>(StringComparer.Ordinal);
        private static GeneratorTables? s_default;

        private readonly int[][] _weights;

        /// <summary>Makes a set of tables; every table not given keeps its built-in weights.</summary>
        /// <exception cref="ArgumentException">The id is not lowercase letters, digits and dashes; a table id is unknown or repeated; a name is not a choice of its table or is repeated; a weight is out of range; or a table has no weight above 0.</exception>
        public GeneratorTables(string id, IEnumerable<WeightTable> tables)
        {
            CheckId(id);
            if (tables is null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            _weights = new int[Ids.Count][];
            var list = new WeightTable[Ids.Count];
            foreach (var table in tables)
            {
                if (table is null)
                {
                    throw new ArgumentException("A table is null.", nameof(tables));
                }

                var t = IndexOf(table.Id);
                if (_weights[t] != null)
                {
                    throw new ArgumentException($"Table \"{table.Id}\" appears twice.", nameof(tables));
                }

                _weights[t] = Compile(t, table);
                list[t] = Canonical(t, _weights[t]);
            }

            for (var t = 0; t < Ids.Count; t++)
            {
                if (_weights[t] is null)
                {
                    _weights[t] = (int[])s_builtIn[t].Clone();
                    list[t] = Canonical(t, _weights[t]);
                }
            }

            Id = id;
            Tables = list;
        }

        /// <summary>The ids of the tables the generator reads, in the order <see cref="Tables"/> lists them.</summary>
        public static IReadOnlyList<string> Ids { get; } = new[]
        {
            "star-classes.young", "star-classes.mature", "star-classes.old", "star-classes.plausible",
            "planet-kinds.hot", "planet-kinds.warm", "planet-kinds.temperate", "planet-kinds.cold", "moon-kinds.giant",
        };

        /// <summary>The built-in tables, with the id <c>default</c>; registered from the start.</summary>
        public static GeneratorTables Default
        {
            get
            {
                lock (s_lock)
                {
                    return s_default ??= new GeneratorTables("default", Array.Empty<WeightTable>());
                }
            }
        }

        /// <summary>The name a program registers these tables under and a link carries, such as <c>my-mod</c>.</summary>
        public string Id { get; }

        /// <summary>Every table, in the order of <see cref="Ids"/>, each listing every choice of its table.</summary>
        public IReadOnlyList<WeightTable> Tables { get; }

        /// <summary>The table with id <paramref name="id"/>.</summary>
        /// <exception cref="ArgumentException">There is no table with that id; the message lists them.</exception>
        public WeightTable Table(string id) => Tables[IndexOf(id)];

        /// <summary>A copy with a new id and <paramref name="tables"/> in place of the tables with the same ids.</summary>
        public GeneratorTables With(string id, params WeightTable[] tables)
        {
            var merged = new List<WeightTable>(Tables);
            foreach (var table in tables ?? Array.Empty<WeightTable>())
            {
                merged[IndexOf(table?.Id)] = table!;
            }

            return new GeneratorTables(id, merged);
        }

        /// <summary>
        /// Makes these tables findable by <see cref="Id"/>, so <see cref="GeneratorOptions.Tables"/> and links can name
        /// them; registering the same id again replaces the tables (the built-in <c>default</c> cannot be replaced).
        /// Every program that regenerates an object must register the same tables under the same id.
        /// </summary>
        /// <exception cref="ArgumentException">The id is <c>default</c>.</exception>
        public static void Register(GeneratorTables tables)
        {
            if (tables is null)
            {
                throw new ArgumentNullException(nameof(tables));
            }

            if (tables.Id == "default")
            {
                throw new ArgumentException("The id \"default\" belongs to the built-in tables; register yours under another id.", nameof(tables));
            }

            lock (s_lock)
            {
                s_registered[tables.Id] = tables;
            }
        }

        /// <summary>The tables registered under <paramref name="id"/>, or null.</summary>
        public static GeneratorTables? Find(string id)
        {
            if (id == "default")
            {
                return Default;
            }

            lock (s_lock)
            {
                return id != null && s_registered.TryGetValue(id, out var tables) ? tables : null;
            }
        }

        /// <summary>
        /// Reads tables from JSON written by <see cref="ToJson"/>: <c>{"id": "my-mod", "tables": [{"id": "star-classes.young",
        /// "entries": [{"name": "M", "weight": 300}]}]}</c>. Tables left out keep their built-in weights; a <c>schema</c> key is allowed.
        /// </summary>
        /// <exception cref="FormatException">The text is not JSON of that shape; the message gives the position.</exception>
        /// <exception cref="ArgumentException">The JSON is well formed but a value is not allowed (see the constructor).</exception>
        public static GeneratorTables FromJson(string json)
        {
            if (json is null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var r = new JsonReader(json);
            string? id = null;
            var tables = new List<WeightTable>();
            r.ReadObject(key =>
            {
                switch (key)
                {
                    case "schema":
                        var schema = r.ReadString();
                        if (schema != Schema)
                        {
                            throw r.Error($"the schema is \"{schema}\"; this package reads \"{Schema}\"");
                        }

                        break;
                    case "id":
                        id = r.ReadString();
                        break;
                    case "tables":
                        r.ReadArray(() => tables.Add(ReadTable(r)));
                        break;
                    default:
                        throw r.Error($"\"{key}\" is not a key of a table set; the keys are schema, id and tables");
                }
            });
            r.End();
            return new GeneratorTables(id ?? throw r.Error("the table set has no \"id\""), tables);
        }

        /// <summary>These tables as JSON that <see cref="FromJson"/> reads back: the schema, the id, then every table in full.</summary>
        public string ToJson(bool indented = false)
        {
            var w = new JsonWriter(indented);
            w.BeginObject().Name("schema").String(Schema).Name("id").String(Id).Name("tables").BeginArray();
            foreach (var table in Tables)
            {
                JsonExport.Write(w, table, false);
            }

            w.EndArray().EndObject();
            return w.ToString();
        }

        /// <summary>The tables <paramref name="options"/> name, compiled; the built-in ones when it names none.</summary>
        internal static GeneratorTables For(GeneratorOptions options) =>
            options.Tables is null ? Default
            : Find(options.Tables) ?? throw new InvalidOperationException($"No tables are registered as \"{options.Tables}\"; call GeneratorTables.Register before generating.");

        // Table indexes, in the order of Ids.
        internal const int StarYoung = 0, StarMature = 1, StarOld = 2, StarPlausible = 3, PlanetHot = 4, PlanetWarm = 5, PlanetTemperate = 6, PlanetCold = 7, GiantMoons = 8;

        /// <summary>Table <paramref name="table"/>'s weights in the order of its built-in choices, as the generator draws them.</summary>
        internal int[] Weights(int table) => _weights[table];

        internal static void CheckId(string? id)
        {
            if (id is null)
            {
                throw new ArgumentNullException(nameof(id), "Tables need an id such as \"my-mod\".");
            }

            var ok = id.Length > 0 && id.Length <= MaxIdLength && id[0] != '-';
            foreach (var c in id)
            {
                ok &= (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-';
            }

            if (!ok)
            {
                throw new ArgumentException($"A tables id is 1 to {MaxIdLength} lowercase letters, digits and dashes, not starting with a dash, such as \"my-mod\"; got \"{id}\".", nameof(id));
            }
        }

        // The built-in weights and the names of their choices, read from the generators' own arrays.
        private static readonly int[][] s_builtIn =
        {
            StarGenerator.GameYoung, StarGenerator.GameMature, StarGenerator.GameOld, StarGenerator.Plausible,
            StarSystemGenerator.HotWeights, StarSystemGenerator.WarmWeights, StarSystemGenerator.TemperateWeights, StarSystemGenerator.ColdWeights,
            StarSystemGenerator.GiantMoonWeights,
        };

        private static string[] Names(int table)
        {
            if (table <= StarPlausible)
            {
                return Strings(StarGenerator.Classes);
            }

            switch (table)
            {
                case PlanetHot: return Strings(StarSystemGenerator.HotKinds);
                case PlanetWarm: return Strings(StarSystemGenerator.WarmKinds);
                case PlanetTemperate: return Strings(StarSystemGenerator.TemperateKinds);
                case PlanetCold: return Strings(StarSystemGenerator.ColdKinds);
                default: return Strings(StarSystemGenerator.GiantMoonKinds);
            }
        }

        private static string[] Strings<T>(T[] values)
            where T : struct
        {
            var names = new string[values.Length];
            for (var i = 0; i < names.Length; i++)
            {
                names[i] = values[i].ToString()!;
            }

            return names;
        }

        private static int IndexOf(string? id)
        {
            for (var t = 0; t < Ids.Count; t++)
            {
                if (Ids[t] == id)
                {
                    return t;
                }
            }

            throw new ArgumentException($"\"{id}\" is not a table; the tables are {string.Join(", ", Ids)}.", nameof(id));
        }

        private static int[] Compile(int t, WeightTable table)
        {
            var names = Names(t);
            var weights = new int[names.Length];
            var seen = new bool[names.Length];
            var total = 0;
            foreach (var entry in table.Entries ?? Array.Empty<WeightEntry>())
            {
                var at = Array.IndexOf(names, entry?.Name);
                if (at < 0)
                {
                    throw new ArgumentException($"\"{entry?.Name}\" is not a choice of {table.Id}; its choices are {string.Join(", ", names)}.", nameof(table));
                }

                if (seen[at])
                {
                    throw new ArgumentException($"{table.Id} lists \"{entry!.Name}\" twice.", nameof(table));
                }

                if (entry!.Weight < 0 || entry.Weight > 1_000_000)
                {
                    throw new ArgumentException($"{table.Id}: the weight of {entry.Name} must be 0 to 1,000,000; got {entry.Weight.ToString(CultureInfo.InvariantCulture)}.", nameof(table));
                }

                seen[at] = true;
                weights[at] = entry.Weight;
                total += entry.Weight;
            }

            if (total == 0)
            {
                throw new ArgumentException($"{table.Id} needs at least one weight above 0.", nameof(table));
            }

            return weights;
        }

        private static WeightTable Canonical(int t, int[] weights)
        {
            var names = Names(t);
            var entries = new WeightEntry[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                entries[i] = new WeightEntry { Name = names[i], Weight = weights[i] };
            }

            return new WeightTable { Id = Ids[t], Entries = entries };
        }

        private static WeightTable ReadTable(JsonReader r)
        {
            string? id = null;
            var entries = new List<WeightEntry>();
            r.ReadObject(key =>
            {
                switch (key)
                {
                    case "id":
                        id = r.ReadString();
                        break;
                    case "entries":
                        r.ReadArray(() =>
                        {
                            string? name = null;
                            int? weight = null;
                            r.ReadObject(k =>
                            {
                                switch (k)
                                {
                                    case "name": name = r.ReadString(); break;
                                    case "weight": weight = r.ReadInt(); break;
                                    default: throw r.Error($"\"{k}\" is not a key of an entry; the keys are name and weight");
                                }
                            });
                            entries.Add(new WeightEntry
                            {
                                Name = name ?? throw r.Error("an entry has no \"name\""),
                                Weight = weight ?? throw r.Error("an entry has no \"weight\""),
                            });
                        });
                        break;
                    default:
                        throw r.Error($"\"{key}\" is not a key of a table; the keys are id and entries");
                }
            });
            return new WeightTable { Id = id ?? throw r.Error("a table has no \"id\""), Entries = entries };
        }
    }
}
