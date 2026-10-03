using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy extras' golden file: the extras of the eight galaxies of galaxy.json, of two galaxies of a cluster (the
    /// first with an active core, and the last), and of the merging pair's first galaxy, whose tidal frontier seats pirates.
    /// </summary>
    public class GoldenExtrasTests
    {
        [Test]
        public void Extras_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("galaxies").BeginArray();
            foreach (var (seed, options) in GoldenGalaxyTests.Cases)
            {
                ExtrasJson.Write(w, Galaxy.Generate(seed, options));
            }

            var cluster = GalaxyCluster.Generate("Virgo", Preset.Default with { ClusterKind = ClusterKind.Cluster });
            ExtrasJson.Write(w, cluster.Galaxies.First(g => g.CoreActivity != CoreActivity.Quiet));
            ExtrasJson.Write(w, cluster.Galaxy(cluster.Galaxies.Count - 1));
            var universe = Universe.Generate("my-seed");
            ExtrasJson.Write(w, universe.Cluster(universe.Merger.Cluster).Galaxy(0));
            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/galaxy-extras.json", w.ToString());
        }
    }
}
