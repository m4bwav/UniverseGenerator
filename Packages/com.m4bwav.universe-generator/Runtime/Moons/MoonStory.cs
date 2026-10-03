#nullable enable
using System.Collections.Generic;

namespace UniverseGeneration
{
    /// <summary>
    /// A moon's traits, its own hazards and its descriptor. A trait is offered only when the data supports it and a hazard
    /// only follows from the data, so text never contradicts the generated facts.
    /// </summary>
    internal static class MoonStory
    {
        public static string[] Traits(Pcg32 rng, Moon m, Planet planet)
        {
            var k = m.Kind;
            var giant = PlanetDetail.IsGiant(planet.Kind);
            var air = (m.Atmosphere.Pressure ?? 0) >= 0.01;
            var icy = k == MoonKind.Ice || k == MoonKind.Ocean;
            var rocky = k == MoonKind.Barren || k == MoonKind.Ice;

            // Every trait in a fixed order with its weight; a weight of 0 means the data does not support it.
            var candidates = new (string Trait, int Weight)[]
            {
                ("active volcanoes", k == MoonKind.Volcanic ? 10 : 0),
                ("sulphur plains", k == MoonKind.Volcanic ? 8 : 0),
                ("lava lakes", k == MoonKind.Volcanic ? 6 : 0),
                ("ice geysers", icy && m.TidalHeating >= 0.01 && m.Temperature < 200 ? 10 : 0),
                ("a cracked ice shell", k == MoonKind.Ocean ? 10 : 0),
                ("an ocean under the ice", m.SubsurfaceOcean ? 6 : 0),
                ("a sublimating crust", icy && m.Temperature >= 200 ? 10 : 0),
                ("an orange haze", k == MoonKind.Hazy ? 10 : 0),
                ("methane lakes", air && PlanetDetail.Has(m.Atmosphere.Gases, Gas.Methane) && m.Temperature >= 90 && m.Temperature <= 112 ? 10 : 0),
                ("hydrocarbon dunes", k == MoonKind.Hazy ? 6 : 0),
                ("ancient craters", rocky ? 8 : 0),
                ("a two-toned face", rocky ? 3 : 0),
                ("deep canyons", rocky || k == MoonKind.Ocean ? 4 : 0),
                ("a giant crater", 2),
                ("a captured asteroid", k == MoonKind.Barren && m.Radius < 300 ? 8 : 0),
                ("the giant fills its sky", giant && m.Orbit < 10 ? 8 : 0),
                ("eclipses every orbit", giant && m.Orbit < 30 ? 5 : 0),
                ("rings across its sky", planet.Rings ? 6 : 0),
                ("tidal flexing", m.TidalHeating >= 0.05 && k != MoonKind.Volcanic ? 6 : 0),
                ("auroras", air && giant ? 3 : 0),
                ("towering forests", m.Flora >= 4 ? 8 : 0),
                ("great herds", m.Fauna >= 4 ? 8 : 0),
                ("glowing seas", m.Water > 0 && m.Life >= LifeLevel.Simple ? 6 : 0),
                ("scattered archipelagos", m.Water > 0 && m.Water < 0.95 ? 6 : 0),
                ("fungal jungles", k == MoonKind.Garden ? 4 : 0),
            };

            return PlanetStory.Pick(rng, candidates);
        }

        /// <summary>
        /// The hazards a moon has that a planet does not: its giant's radiation (unless at least half a bar of air shields
        /// the surface, as Titan's does), eruptions, geysers, moonquakes.
        /// </summary>
        public static List<(string Text, int Tier)> Hazards(Moon m, Planet planet)
        {
            var list = new List<(string Text, int Tier)>();
            var exposed = (m.Atmosphere.Pressure ?? 0) < 0.5;
            if (exposed && planet.Kind == PlanetKind.GasGiant && m.Orbit < 15)
            {
                list.Add(("the giant's radiation belts", 4));
            }
            else if (exposed && ((planet.Kind == PlanetKind.GasGiant && m.Orbit < 30) || (planet.Kind == PlanetKind.IceGiant && m.Orbit < 10)))
            {
                list.Add(("radiation from the giant", 3));
            }

            if (m.Kind == MoonKind.Volcanic)
            {
                list.Add(("eruptions", 2));
            }

            foreach (var t in m.Traits)
            {
                if (t == "ice geysers")
                {
                    list.Add(("geysers", 2));
                }
            }

            if (m.TidalHeating >= 0.1 && m.Kind != MoonKind.Volcanic)
            {
                list.Add(("moonquakes", 2));
            }

            return list;
        }

        /// <summary>"a hazy moon of a ringed gas giant".</summary>
        public static string Descriptor(MoonKind kind, Planet planet)
        {
            string word;
            switch (kind)
            {
                case MoonKind.Barren: word = "barren"; break;
                case MoonKind.Ice: word = "ice"; break;
                case MoonKind.Volcanic: word = "volcanic"; break;
                case MoonKind.Ocean: word = "ocean"; break;
                case MoonKind.Hazy: word = "hazy"; break;
                default: word = "garden"; break;
            }

            return Story.WithArticle(word + " moon") + " of " + Story.WithArticle((planet.Rings ? "ringed " : "") + Story.PlanetNoun(planet.Kind));
        }
    }
}
