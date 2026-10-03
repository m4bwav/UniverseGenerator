namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The universe level's golden fields, listed explicitly: the universe map, filaments, voids, landmarks and merger,
    /// and the fields the universe level added to <see cref="Galaxy"/> (which no other golden writer writes).
    /// </summary>
    internal static class UniverseJson
    {
        public static void Write(JsonWriter w, Universe u)
        {
            w.BeginObject();
            w.Name("address").String(u.Address).Name("name").String(u.Name).Name("age").String(u.Age.ToString()).Name("radius").Number(u.Radius, 3);
            w.Name("nodes").BeginArray();
            foreach (var n in u.Nodes)
            {
                w.BeginObject().Name("address").String(n.Address).Name("name").String(n.Name).Name("kind").String(n.Kind.ToString());
                w.Name("role").String(n.Role.ToString()).Name("x").Number(n.X, 3).Name("y").Number(n.Y, 3).Name("galaxies").Int(n.Galaxies);
                w.EndObject();
            }

            w.EndArray();
            w.Name("filaments").BeginArray();
            foreach (var f in u.Filaments)
            {
                w.BeginArray().Int(f.A).Int(f.B).Number(f.Length, 3).Int(f.GalaxyA).Int(f.GalaxyB).EndArray();
            }

            w.EndArray();
            w.Name("voids").BeginArray();
            foreach (var v in u.Voids)
            {
                w.BeginObject().Name("address").String(v.Address).Name("name").String(v.Name);
                w.Name("x").Number(v.X, 3).Name("y").Number(v.Y, 3).Name("radius").Number(v.Radius, 3);
                w.Name("systems").BeginArray();
                foreach (var s in v.Systems)
                {
                    SystemJson.Write(w, s);
                }

                w.EndArray().EndObject();
            }

            w.EndArray();
            w.Name("landmarks").BeginArray();
            foreach (var l in u.Landmarks)
            {
                w.BeginArray().String(l.Kind.ToString()).String(l.Name).String(l.Address).EndArray();
            }

            w.EndArray();
            var m = u.Merger;
            w.Name("merger").BeginObject().Name("name").String(m.Name).Name("cluster").Int(m.Cluster);
            w.Name("galaxyA").String(m.GalaxyA).Name("galaxyB").String(m.GalaxyB).Name("hook").String(m.Hook).EndObject();
            w.EndObject();
        }

        /// <summary>The galaxy fields the universe level added: the sky landmark and the cluster each gate leads to.</summary>
        public static void WriteGalaxy(JsonWriter w, Galaxy g)
        {
            w.BeginObject().Name("address").String(g.Address).Name("landmark");
            if (g.Landmark == null)
            {
                w.Null();
            }
            else
            {
                w.BeginArray().String(g.Landmark.Name).String(g.Landmark.Address).Int(g.Landmark.Bearing).Int(g.Landmark.LightYears).EndArray();
            }

            w.Name("gateClusters").BeginArray();
            foreach (var gate in g.Gates)
            {
                w.Int(gate.Cluster);
            }

            w.EndArray().EndObject();
        }

        public static string Text(Universe u)
        {
            var w = new JsonWriter(indented: false);
            Write(w, u);
            return w.ToString();
        }

        public static string GalaxyText(Galaxy g)
        {
            var w = new JsonWriter(indented: false);
            w.BeginArray();
            WriteGalaxy(w, g);
            ClusterJson.WriteGalaxy(w, g);
            GalaxyJson.Write(w, g);
            w.EndArray();
            return w.ToString();
        }
    }
}
