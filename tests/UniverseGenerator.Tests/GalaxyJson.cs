namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The galaxy level's golden fields, listed explicitly: a field added later goes into a new golden file, so this one
    /// never changes (AGENTS.md, the seed promise).
    /// </summary>
    internal static class GalaxyJson
    {
        public static void Write(JsonWriter w, Galaxy g)
        {
            w.BeginObject();
            w.Name("address").String(g.Address).Name("shape").String(g.Shape.ToString()).Name("arms").Int(g.Arms);
            w.Name("radius").Number(g.Radius, 3).Name("core").Int(g.Core);
            w.Name("regions").BeginArray();
            foreach (var r in g.Regions)
            {
                w.BeginObject().Name("name").String(r.Name).Name("age").String(r.Age.ToString()).Name("theme").String(r.Theme).Name("centre").Int(r.Centre).EndObject();
            }

            w.EndArray();
            w.Name("map").BeginArray();
            foreach (var m in g.Map)
            {
                w.BeginObject().Name("name").String(m.Name).Name("x").Number(m.X, 3).Name("y").Number(m.Y, 3).Name("star").String(m.StarClass.ToString());
                w.Name("region").Int(m.Region).Name("hops").Int(m.Hops).Name("danger").Int(m.Danger).Name("chokepoint").Bool(m.Chokepoint).EndObject();
            }

            w.EndArray();
            w.Name("lanes").BeginArray();
            foreach (var l in g.Lanes)
            {
                w.BeginArray().Int(l.A).Int(l.B).Bool(l.Bridge).EndArray();
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
