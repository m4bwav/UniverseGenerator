using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The universe level's golden file: fixed seeds and options, the fields of <see cref="UniverseJson"/> (void systems in
    /// full), every cluster's map and links, every galaxy's cluster and universe fields, and the map of the merging pair's
    /// first galaxy (its tidal frontier) with the system where the tidal link opens.
    /// </summary>
    public class GoldenUniverseTests
    {
        internal static readonly (string Seed, GeneratorOptions Options)[] Cases =
        {
            ("my-seed", Preset.Default),
            ("42", Preset.Default),
            ("ancient", Preset.Default with { Epoch = Epoch.Old }),
            ("small", Preset.Plausible with { Epoch = Epoch.Young, Systems = 20 }),
        };

        [Test]
        public void Universes_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("universes").BeginArray();
            foreach (var (seed, options) in Cases)
            {
                var u = Universe.Generate(seed, options);
                w.BeginObject().Name("universe");
                UniverseJson.Write(w, u);
                w.Name("clusters").BeginArray();
                foreach (var c in u.Clusters)
                {
                    w.BeginObject().Name("cluster");
                    ClusterJson.Write(w, c);
                    w.Name("galaxies").BeginArray();
                    foreach (var g in c.Galaxies)
                    {
                        w.BeginArray();
                        UniverseJson.WriteGalaxy(w, g);
                        ClusterJson.WriteGalaxy(w, g);
                        w.EndArray();
                    }

                    w.EndArray().EndObject();
                }

                w.EndArray();
                var pair = u.Cluster(u.Merger.Cluster).Galaxy(0);
                w.Name("frontier").BeginObject().Name("galaxy");
                GalaxyJson.Write(w, pair);
                w.Name("system");
                foreach (var gate in pair.Gates)
                {
                    if (gate.Tier == LinkTier.Tidal)
                    {
                        SystemJson.Write(w, pair.System(gate.System));
                    }
                }

                w.EndObject().EndObject();
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/universe.json", w.ToString());
        }
    }
}
