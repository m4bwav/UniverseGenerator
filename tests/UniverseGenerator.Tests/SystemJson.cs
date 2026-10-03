using System.Collections.Generic;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// The star system level's golden fields, listed explicitly: a field added later goes into a new golden file, so this
    /// one never changes (AGENTS.md, the seed promise).
    /// </summary>
    internal static class SystemJson
    {
        public static void Write(JsonWriter w, StarSystem s)
        {
            w.BeginObject();
            w.Name("address").String(s.Address).Name("name").String(s.Name);
            w.Name("age").String(s.Age.ToString()).Name("danger").Int(s.Danger);
            w.Name("star");
            Star(w, s.Star);
            w.Name("companion");
            if (s.Companion is null)
            {
                w.Null();
            }
            else
            {
                w.BeginObject().Name("orbit").String(s.Companion.Orbit.ToString()).Name("separation").Number(s.Companion.Separation, 4).Name("star");
                Star(w, s.Companion.Star);
                w.EndObject();
            }

            w.Name("habitableZone").BeginArray().Number(s.HabitableZoneInner, 4).Number(s.HabitableZoneOuter, 4).EndArray();
            w.Name("frostLine").Number(s.FrostLine, 4);
            w.Name("planets").BeginArray();
            foreach (var p in s.Planets)
            {
                Planet(w, p);
            }

            w.EndArray();
            w.Name("belts").BeginArray();
            foreach (var b in s.Belts)
            {
                w.BeginObject().Name("kind").String(b.Kind.ToString()).Name("inner").Number(b.Inner, 4).Name("outer").Number(b.Outer, 4).EndObject();
            }

            w.EndArray();
            w.Name("stations").BeginArray();
            foreach (var st in s.Stations)
            {
                w.BeginObject().Name("address").String(st.Address).Name("name").String(st.Name).Name("kind").String(st.Kind.ToString()).Name("planet");
                if (st.Planet is null)
                {
                    w.Null();
                }
                else
                {
                    w.Int(st.Planet.Value);
                }

                w.EndObject();
            }

            w.EndArray();
            w.Name("landmarks").BeginArray();
            foreach (var l in s.Landmarks)
            {
                w.BeginObject().Name("kind").String(l.Kind.ToString()).Name("outlier").Bool(l.Outlier).Name("text").String(l.Text).EndObject();
            }

            w.EndArray();
            w.Name("tags").BeginArray();
            foreach (var t in s.Tags)
            {
                w.String(t);
            }

            w.EndArray();
            w.Name("descriptor").String(s.Descriptor);
            w.EndObject();
        }

        public static string Text(StarSystem s)
        {
            var w = new JsonWriter(indented: false);
            Write(w, s);
            return w.ToString();
        }

        public static void Planet(JsonWriter w, Planet p)
        {
            w.BeginObject();
            w.Name("address").String(p.Address).Name("name").String(p.Name).Name("kind").String(p.Kind.ToString()).Name("zone").String(p.Zone.ToString());
            w.Name("orbit").Number(p.Orbit, 4).Name("period").Number(p.Period, 2).Name("mass").Number(p.Mass, 3).Name("radius").Number(p.Radius, 3);
            w.Name("rings").Bool(p.Rings).Name("descriptor").String(p.Descriptor);
            w.Name("moons").BeginArray();
            foreach (var m in p.Moons)
            {
                w.BeginObject().Name("name").String(m.Name).Name("kind").String(m.Kind.ToString()).Name("orbit").Number(m.Orbit, 2).Name("radius").Number(m.Radius, 0).EndObject();
            }

            w.EndArray().EndObject();
        }

        public static string Text(Planet p)
        {
            var w = new JsonWriter(indented: false);
            Planet(w, p);
            return w.ToString();
        }

        private static void Star(JsonWriter w, Star s)
        {
            w.BeginObject();
            w.Name("class").String(s.Class.ToString()).Name("spectralType").String(s.SpectralType);
            w.Name("mass").Number(s.Mass, 3).Name("luminosity").Number(s.Luminosity, 5).Name("radius").Number(s.Radius, 6);
            w.Name("temperature").Number(s.Temperature, 0).Name("colour").String(s.Colour).Name("description").String(s.Description);
            w.EndObject();
        }

        public static IEnumerable<string> Seeds(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return "s" + i.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }
}
