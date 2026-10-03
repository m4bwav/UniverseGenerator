using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy cluster level's golden file: fixed seeds and options, the fields of <see cref="ClusterJson"/>, every
    /// galaxy's cluster fields, the map of two galaxies of each cluster (the last, and the smallest with an active core) with
    /// their first system in full, and the cluster fields of the lone galaxies of galaxy.json, which that file leaves out.
    /// </summary>
    public class GoldenClusterTests
    {
        [Test]
        public void Clusters_match_the_golden_file()
        {
            var cases = new (string Seed, GeneratorOptions Options)[]
            {
                ("my-seed", Preset.Default),
                ("42", Preset.Default),
                ("Virgo", Preset.Default with { ClusterKind = ClusterKind.Cluster }),
                ("local", Preset.Default with { ClusterKind = ClusterKind.Group }),
                ("small", Preset.Plausible with { ClusterKind = ClusterKind.Cluster, Systems = 20 }),
                ("one", Preset.Default with { ClusterKind = ClusterKind.Group, Systems = 1 }),
            };

            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("clusters").BeginArray();
            foreach (var (seed, options) in cases)
            {
                var c = GalaxyCluster.Generate(seed, options);
                w.BeginObject().Name("cluster");
                ClusterJson.Write(w, c);
                w.Name("galaxies").BeginArray();
                foreach (var g in c.Galaxies)
                {
                    ClusterJson.WriteGalaxy(w, g);
                }

                w.EndArray();
                var active = c.Map.Where(e => e.CoreActivity != CoreActivity.Quiet).OrderBy(e => e.Systems).ThenBy(e => e.Index).FirstOrDefault();
                var shown = active == null || active.Index == c.Map.Count - 1 ? new[] { c.Map.Count - 1 } : new[] { active.Index, c.Map.Count - 1 };
                w.Name("maps").BeginArray();
                foreach (var i in shown)
                {
                    var g = c.Galaxy(i);
                    w.BeginObject().Name("galaxy");
                    GalaxyJson.Write(w, g);
                    w.Name("system");
                    SystemJson.Write(w, g.System(0));
                    w.EndObject();
                }

                w.EndArray().EndObject();
            }

            w.EndArray();
            w.Name("loneGalaxies").BeginArray();
            foreach (var (seed, options) in GoldenGalaxyTests.Cases)
            {
                ClusterJson.WriteGalaxy(w, Galaxy.Generate(seed, options));
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/cluster.json", w.ToString());
        }
    }
}
