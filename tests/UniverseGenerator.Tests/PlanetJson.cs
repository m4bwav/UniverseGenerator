using System.Collections.Generic;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The planet level's golden fields, listed explicitly: a field added later goes into a new golden file, so this
    /// one never changes (AGENTS.md, the seed promise). The fields a planet already had are in <see cref="SystemJson.Planet"/>.
    /// </summary>
    internal static class PlanetJson
    {
        public static void Detail(JsonWriter w, Planet p)
        {
            w.BeginObject();
            w.Name("address").String(p.Address).Name("composition").String(p.Composition.ToString());
            w.Name("gravity").Number(p.Gravity, 2).Name("escapeVelocity").Number(p.EscapeVelocity, 2).Name("density").Number(p.Density, 2);
            w.Name("insolation").Number(p.Insolation, 4).Name("albedo").Number(p.Albedo, 2);
            w.Name("temperature").Number(p.Temperature, 0).Name("dayTemperature").Number(p.DayTemperature, 0).Name("nightTemperature").Number(p.NightTemperature, 0);
            w.Name("atmosphere");
            Atmosphere(w, p.Atmosphere);
            w.Name("water").Number(p.Water, 2).Name("ice").Number(p.Ice, 2);
            w.Name("rotation").Number(p.Rotation, 1).Name("spin").String(p.Spin.ToString()).Name("tilt").Number(p.Tilt, 1);
            w.Name("bands");
            Bands(w, p.Bands);
            w.Name("biomes");
            Biomes(w, p.Biomes);
            w.Name("life").String(p.Life.ToString()).Name("flora").Int(p.Flora).Name("fauna").Int(p.Fauna);
            w.Name("traits");
            Strings(w, p.Traits);
            w.Name("anomaly").String(p.Anomaly);
            w.Name("resources");
            Resources(w, p.Resources);
            w.Name("hazard").Int(p.Hazard).Name("hazards");
            Strings(w, p.Hazards);
            w.Name("similarity").Number(p.Similarity, 2).Name("habitability").String(p.Habitability.ToString());
            w.Name("summary").String(p.Summary);
            w.EndObject();
        }

        public static void Atmosphere(JsonWriter w, Atmosphere a)
        {
            w.BeginObject().Name("class").String(a.Class.ToString()).Name("pressure");
            if (a.Pressure is null)
            {
                w.Null();
            }
            else
            {
                w.Number(a.Pressure.Value, 4);
            }

            w.Name("gases").BeginArray();
            foreach (var g in a.Gases)
            {
                w.String(g.ToString());
            }

            w.EndArray();
            w.Name("lightestGasKept").Number(a.LightestGasKept, 1).Name("breathable").Bool(a.Breathable).Name("why").String(a.Why).EndObject();
        }

        public static void Bands(JsonWriter w, IReadOnlyList<ClimateBand> bands)
        {
            w.BeginArray();
            foreach (var b in bands)
            {
                w.BeginArray().Int(b.From).Int(b.To).Number(b.Share, 4).Number(b.Temperature, 0).String(b.Biome.ToString()).EndArray();
            }

            w.EndArray();
        }

        public static void Biomes(JsonWriter w, IReadOnlyList<BiomeShare> biomes)
        {
            w.BeginArray();
            foreach (var b in biomes)
            {
                w.BeginArray().String(b.Biome.ToString()).Number(b.Share, 2).EndArray();
            }

            w.EndArray();
        }

        public static void Resources(JsonWriter w, ResourceGrades r) =>
            w.BeginObject().Name("metals").Int(r.Metals).Name("rareElements").Int(r.RareElements).Name("ices").Int(r.Ices)
                .Name("gases").Int(r.Gases).Name("organics").Int(r.Organics).EndObject();

        public static void Strings(JsonWriter w, IReadOnlyList<string> items)
        {
            w.BeginArray();
            foreach (var s in items)
            {
                w.String(s);
            }

            w.EndArray();
        }

        public static string Text(Planet p)
        {
            var w = new JsonWriter(indented: false);
            Detail(w, p);
            return w.ToString();
        }

        /// <summary>A lone planet: the fields it shares with planets in systems, then the planet level's.</summary>
        public static void Lone(JsonWriter w, Planet p)
        {
            w.BeginObject().Name("planet");
            SystemJson.Planet(w, p);
            w.Name("detail");
            Detail(w, p);
            w.EndObject();
        }

        public static string LoneText(Planet p)
        {
            var w = new JsonWriter(indented: false);
            Lone(w, p);
            return w.ToString();
        }
    }
}
