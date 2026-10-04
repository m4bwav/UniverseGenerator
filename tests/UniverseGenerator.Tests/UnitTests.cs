using NUnit.Framework;

// Enum.GetValues<T>() does not exist on net48.
#pragma warning disable CA2263

namespace UniverseGeneration.Tests
{
    /// <summary>Units.Convert and Units.Map: known values, round trips, and agreement with the generator's own figures.</summary>
    public class UnitTests
    {
        [Test]
        public void Known_lengths_convert()
        {
            Assert.That(Units.Convert(1, LengthUnit.AstronomicalUnits, LengthUnit.Kilometres), Is.EqualTo(149597870.7));
            Assert.That(Units.Convert(1, LengthUnit.Parsecs, LengthUnit.LightYears), Is.EqualTo(3.26156).Within(1e-5));
            Assert.That(Units.Convert(1, LengthUnit.LightYears, LengthUnit.AstronomicalUnits), Is.EqualTo(63241.08).Within(0.01));
            Assert.That(Units.Convert(1, LengthUnit.JupiterRadii, LengthUnit.EarthRadii), Is.EqualTo(10.97).Within(0.01));
            Assert.That(Units.Convert(1, LengthUnit.SolarRadii, LengthUnit.AstronomicalUnits), Is.EqualTo(0.00465).Within(0.00001));
        }

        [Test]
        public void Known_masses_times_and_temperatures_convert()
        {
            Assert.That(Units.Convert(1, MassUnit.SolarMasses, MassUnit.EarthMasses), Is.EqualTo(332950).Within(100));
            Assert.That(Units.Convert(1, MassUnit.JupiterMasses, MassUnit.EarthMasses), Is.EqualTo(317.83).Within(1e-9));
            Assert.That(Units.Convert(365.25, TimeUnit.Days, TimeUnit.Years), Is.EqualTo(1));
            Assert.That(Units.Convert(2, TimeUnit.Days, TimeUnit.Hours), Is.EqualTo(48));
            Assert.That(Units.Convert(273.15, TemperatureUnit.Kelvin, TemperatureUnit.Celsius), Is.EqualTo(0).Within(1e-9));
            Assert.That(Units.Convert(100, TemperatureUnit.Celsius, TemperatureUnit.Fahrenheit), Is.EqualTo(212).Within(1e-9));
            Assert.That(Units.Convert(-40, TemperatureUnit.Fahrenheit, TemperatureUnit.Celsius), Is.EqualTo(-40).Within(1e-9));
        }

        [Test]
        public void Every_unit_round_trips()
        {
            foreach (LengthUnit a in System.Enum.GetValues(typeof(LengthUnit)))
            {
                foreach (LengthUnit b in System.Enum.GetValues(typeof(LengthUnit)))
                {
                    Assert.That(Units.Convert(Units.Convert(7.5, a, b), b, a), Is.EqualTo(7.5).Within(1e-9), $"{a} {b}");
                }
            }

            foreach (TemperatureUnit a in System.Enum.GetValues(typeof(TemperatureUnit)))
            {
                foreach (TemperatureUnit b in System.Enum.GetValues(typeof(TemperatureUnit)))
                {
                    Assert.That(Units.Convert(Units.Convert(250, a, b), b, a), Is.EqualTo(250).Within(1e-9), $"{a} {b}");
                }
            }
        }

        [Test]
        public void Map_units_follow_the_distance_scale()
        {
            Assert.That(Units.Map(MapLevel.Galaxy, 1000, LengthUnit.LightYears), Is.EqualTo(50000));
            Assert.That(Units.Map(MapLevel.Universe, 1, LengthUnit.Parsecs), Is.EqualTo(100000 / 3.26156377716743).Within(1e-6));
        }

        [Test]
        public void An_unknown_unit_is_refused()
        {
            Assert.That(() => Units.Convert(1, (LengthUnit)99, LengthUnit.Kilometres), Throws.InstanceOf<System.ArgumentOutOfRangeException>());
        }

        [Test]
        public void A_planets_values_convert_to_what_the_generator_also_states()
        {
            var planet = Planet.Generate("my-seed");
            var moon = planet.Moons[0];
            // The moon's distance is stored in km and its orbit in planet radii: the two agree through Units.
            Assert.That(Units.Convert(moon.Orbit * planet.Radius, LengthUnit.EarthRadii, LengthUnit.Kilometres), Is.EqualTo(moon.Distance).Within(moon.Distance * 0.01));
        }
    }
}
