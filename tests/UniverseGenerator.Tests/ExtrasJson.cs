namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy extras' golden fields, listed explicitly (AGENTS.md, the seed promise): factions with each system's holder
    /// and border, points of interest, hazards, monuments and beacons.
    /// </summary>
    internal static class ExtrasJson
    {
        public static void Write(JsonWriter w, Galaxy g)
        {
            w.BeginObject().Name("address").String(g.Address);
            w.Name("factions").BeginArray();
            foreach (var f in g.Factions)
            {
                w.BeginObject().Name("name").String(f.Name).Name("kind").String(f.Kind.ToString()).Name("capital").Int(f.Capital);
                w.Name("systems").Int(f.Systems).Name("description").String(f.Description).EndObject();
            }

            w.EndArray();
            w.Name("holders").BeginArray();
            foreach (var m in g.Map)
            {
                if (m.Faction is int f)
                {
                    w.BeginArray().Int(f).Bool(m.Contested).EndArray();
                }
                else
                {
                    w.Null();
                }
            }

            w.EndArray();
            w.Name("pointsOfInterest").BeginArray();
            foreach (var p in g.PointsOfInterest)
            {
                w.BeginObject().Name("kind").String(p.Kind.ToString()).Name("system").Int(p.System).Name("name").String(p.Name).Name("text").String(p.Text);
                w.Name("chainStep").Int(p.ChainStep).Name("chainLength").Int(p.ChainLength).EndObject();
            }

            w.EndArray();
            w.Name("hazards").BeginArray();
            foreach (var h in g.Hazards)
            {
                w.BeginObject().Name("kind").String(h.Kind.ToString()).Name("name").String(h.Name).Name("x").Number(h.X, 3).Name("y").Number(h.Y, 3);
                w.Name("radius").Number(h.Radius, 3).Name("systems").BeginArray();
                foreach (var s in h.Systems)
                {
                    w.Int(s);
                }

                w.EndArray().Name("effect").String(h.Effect).EndObject();
            }

            w.EndArray();
            w.Name("monuments").BeginArray();
            foreach (var m in g.Monuments)
            {
                w.BeginObject().Name("region").Int(m.Region).Name("system").Int(m.System).Name("kind").String(m.Kind.ToString());
                w.Name("name").String(m.Name).Name("text").String(m.Text).EndObject();
            }

            w.EndArray();
            w.Name("beacons").BeginArray();
            foreach (var b in g.Beacons)
            {
                w.BeginObject().Name("system").Int(b.System).Name("kind").String(b.Kind.ToString()).Name("name").String(b.Name).Name("text").String(b.Text).EndObject();
            }

            w.EndArray().EndObject();
        }

        public static string Text(Galaxy g)
        {
            var w = new JsonWriter(indented: false);
            Write(w, g);
            return w.ToString();
        }
    }
}
