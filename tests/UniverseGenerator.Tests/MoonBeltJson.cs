namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The moon and belt level's golden fields, listed explicitly: a field added later goes into a new golden file, so
    /// this one never changes (AGENTS.md, the seed promise). The fields moons and belts already had are in
    /// <see cref="SystemJson"/>.
    /// </summary>
    internal static class MoonBeltJson
    {
        public static void Moon(JsonWriter w, Moon m)
        {
            w.BeginObject();
            w.Name("address").String(m.Address).Name("descriptor").String(m.Descriptor);
            w.Name("distance").Number(m.Distance, 0).Name("period").Number(m.Period, 3).Name("composition").String(m.Composition.ToString());
            w.Name("density").Number(m.Density, 2).Name("mass").Number(m.Mass, 6).Name("gravity").Number(m.Gravity, 3).Name("escapeVelocity").Number(m.EscapeVelocity, 2);
            w.Name("insolation").Number(m.Insolation, 4).Name("albedo").Number(m.Albedo, 2).Name("tidalHeating").Number(m.TidalHeating, 4);
            w.Name("temperature").Number(m.Temperature, 0).Name("dayTemperature").Number(m.DayTemperature, 0).Name("nightTemperature").Number(m.NightTemperature, 0);
            w.Name("atmosphere");
            PlanetJson.Atmosphere(w, m.Atmosphere);
            w.Name("water").Number(m.Water, 2).Name("ice").Number(m.Ice, 2).Name("subsurfaceOcean").Bool(m.SubsurfaceOcean);
            w.Name("rotation").Number(m.Rotation, 1).Name("tilt").Number(m.Tilt, 1);
            w.Name("bands");
            PlanetJson.Bands(w, m.Bands);
            w.Name("biomes");
            PlanetJson.Biomes(w, m.Biomes);
            w.Name("life").String(m.Life.ToString()).Name("flora").Int(m.Flora).Name("fauna").Int(m.Fauna);
            w.Name("traits");
            PlanetJson.Strings(w, m.Traits);
            w.Name("anomaly").String(m.Anomaly);
            w.Name("resources");
            PlanetJson.Resources(w, m.Resources);
            w.Name("hazard").Int(m.Hazard).Name("hazards");
            PlanetJson.Strings(w, m.Hazards);
            w.Name("similarity").Number(m.Similarity, 2).Name("habitability").String(m.Habitability.ToString());
            w.Name("summary").String(m.Summary);
            w.EndObject();
        }

        public static void Belt(JsonWriter w, Belt b)
        {
            var c = b.Composition;
            w.BeginObject();
            w.Name("address").String(b.Address).Name("index").Int(b.Index).Name("name").String(b.Name);
            w.Name("composition").BeginObject().Name("silicate").Int(c.Silicate).Name("carbonaceous").Int(c.Carbonaceous)
                .Name("metal").Int(c.Metal).Name("ice").Int(c.Ice).EndObject();
            w.Name("mass").Number(b.Mass, 6).Name("largestBody").Number(b.LargestBody, 0).Name("temperature").Number(b.Temperature, 0);
            w.Name("resources");
            PlanetJson.Resources(w, b.Resources);
            w.Name("summary").String(b.Summary);
            w.EndObject();
        }

        public static string Text(Moon m)
        {
            var w = new JsonWriter(indented: false);
            w.BeginArray();
            SystemJson.Moon(w, m);
            Moon(w, m);
            w.EndArray();
            return w.ToString();
        }

        public static string Text(Belt b)
        {
            var w = new JsonWriter(indented: false);
            w.BeginArray();
            SystemJson.Belt(w, b);
            Belt(w, b);
            w.EndArray();
            return w.ToString();
        }
    }
}
