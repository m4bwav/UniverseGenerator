namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy cluster level's golden fields, listed explicitly: the cluster map and links, and the fields the cluster
    /// level added to <see cref="Galaxy"/> (which <see cref="GalaxyJson"/> does not write, so galaxy.json never changes).
    /// </summary>
    internal static class ClusterJson
    {
        public static void Write(JsonWriter w, GalaxyCluster c)
        {
            w.BeginObject();
            w.Name("address").String(c.Address).Name("name").String(c.Name).Name("kind").String(c.Kind.ToString());
            w.Name("age").String(c.Age.ToString()).Name("radius").Number(c.Radius, 3);
            w.Name("map").BeginArray();
            foreach (var e in c.Map)
            {
                w.BeginObject().Name("address").String(e.Address).Name("name").String(e.Name).Name("type").String(e.Type);
                w.Name("shape").String(e.Shape.ToString()).Name("role").String(e.Role.ToString()).Name("host").Int(e.Host);
                w.Name("x").Number(e.X, 3).Name("y").Number(e.Y, 3).Name("size").Number(e.Size, 3).Name("systems").Int(e.Systems);
                w.Name("age").String(e.Age.ToString()).Name("richness").String(e.Richness.ToString()).Name("core").String(e.CoreActivity.ToString());
                w.EndObject();
            }

            w.EndArray();
            w.Name("links").BeginArray();
            foreach (var l in c.Links)
            {
                w.BeginArray().Int(l.A).Int(l.B).String(l.Tier.ToString()).Number(l.Length, 3).EndArray();
            }

            w.EndArray().EndObject();
        }

        /// <summary>The galaxy fields the cluster level added.</summary>
        public static void WriteGalaxy(JsonWriter w, Galaxy g)
        {
            w.BeginObject();
            w.Name("address").String(g.Address).Name("name").String(g.Name).Name("type").String(g.Type);
            w.Name("descriptor").String(g.Descriptor).Name("age").String(g.Age.ToString()).Name("richness").String(g.Richness.ToString());
            w.Name("core").String(g.CoreActivity.ToString()).Name("coreHazardRadius").Number(g.CoreHazardRadius, 3);
            w.Name("systems").Int(g.Map.Count);
            w.Name("gates").BeginArray();
            foreach (var gate in g.Gates)
            {
                w.BeginArray().Int(gate.Galaxy).String(gate.Name).String(gate.Tier.ToString()).Int(gate.System).EndArray();
            }

            w.EndArray().EndObject();
        }

        public static string Text(GalaxyCluster c)
        {
            var w = new JsonWriter(indented: false);
            Write(w, c);
            return w.ToString();
        }

        public static string GalaxyText(Galaxy g)
        {
            var w = new JsonWriter(indented: false);
            w.BeginArray();
            WriteGalaxy(w, g);
            GalaxyJson.Write(w, g);
            w.EndArray();
            return w.ToString();
        }
    }
}
