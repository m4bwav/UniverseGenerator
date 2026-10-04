#nullable enable
using System;

namespace UniverseGeneration
{
    /// <summary>Units of length. Generated values use au (orbits), Earth radii (planets), Suns (stars) and kilometres (moons, belts).</summary>
    public enum LengthUnit
    {
        /// <summary>Kilometres.</summary>
        Kilometres,

        /// <summary>Earth radii (6,371 km).</summary>
        EarthRadii,

        /// <summary>Jupiter radii (69,911 km).</summary>
        JupiterRadii,

        /// <summary>Solar radii (695,700 km, IAU nominal).</summary>
        SolarRadii,

        /// <summary>Astronomical units (149,597,870.7 km).</summary>
        AstronomicalUnits,

        /// <summary>Light-years.</summary>
        LightYears,

        /// <summary>Parsecs.</summary>
        Parsecs,
    }

    /// <summary>Units of mass. Generated values use Earths (planets, moons, belts) and Suns (stars).</summary>
    public enum MassUnit
    {
        /// <summary>Kilograms.</summary>
        Kilograms,

        /// <summary>Earth masses.</summary>
        EarthMasses,

        /// <summary>Jupiter masses (317.83 Earths).</summary>
        JupiterMasses,

        /// <summary>Solar masses.</summary>
        SolarMasses,
    }

    /// <summary>Units of temperature. Generated values use kelvin.</summary>
    public enum TemperatureUnit
    {
        /// <summary>Kelvin.</summary>
        Kelvin,

        /// <summary>Degrees Celsius.</summary>
        Celsius,

        /// <summary>Degrees Fahrenheit.</summary>
        Fahrenheit,
    }

    /// <summary>Units of time. Generated values use hours (rotation) and days (orbits).</summary>
    public enum TimeUnit
    {
        /// <summary>Hours.</summary>
        Hours,

        /// <summary>Days.</summary>
        Days,

        /// <summary>Julian years of 365.25 days.</summary>
        Years,
    }

    /// <summary>
    /// Conversions between the units the generated values are stored in and any other, on plain doubles (no units
    /// library, D19). Each field's summary names its unit, for example <c>Planet.Orbit</c> in au and <c>Planet.Radius</c>
    /// in Earths: <c>Units.Convert(planet.Orbit, LengthUnit.AstronomicalUnits, LengthUnit.Kilometres)</c>. Map coordinates
    /// convert through <see cref="Map"/>. Conversions use IAU nominal values and do not round.
    /// </summary>
    public static class Units
    {
        private const double KmPerLightYear = 9460730472580.8;

        /// <summary><paramref name="value"/> in <paramref name="from"/>, expressed in <paramref name="to"/>.</summary>
        public static double Convert(double value, LengthUnit from, LengthUnit to) => from == to ? value : value * Kilometres(from) / Kilometres(to);

        /// <summary><paramref name="value"/> in <paramref name="from"/>, expressed in <paramref name="to"/>.</summary>
        public static double Convert(double value, MassUnit from, MassUnit to) => from == to ? value : value * Kilograms(from) / Kilograms(to);

        /// <summary><paramref name="value"/> in <paramref name="from"/>, expressed in <paramref name="to"/>.</summary>
        public static double Convert(double value, TimeUnit from, TimeUnit to) => from == to ? value : value * Hours(from) / Hours(to);

        /// <summary><paramref name="value"/> in <paramref name="from"/>, expressed in <paramref name="to"/>.</summary>
        public static double Convert(double value, TemperatureUnit from, TemperatureUnit to)
        {
            if (from == to)
            {
                return value;
            }

            var kelvin = from switch
            {
                TemperatureUnit.Celsius => value + 273.15,
                TemperatureUnit.Fahrenheit => (value - 32) * 5 / 9 + 273.15,
                _ => value,
            };
            return to switch
            {
                TemperatureUnit.Celsius => kelvin - 273.15,
                TemperatureUnit.Fahrenheit => (kelvin - 273.15) * 9 / 5 + 32,
                _ => kelvin,
            };
        }

        /// <summary>
        /// <paramref name="units"/> of the map at <paramref name="level"/> (a galaxy, cluster or universe map's coordinates
        /// or distances) as a length in <paramref name="to"/>; the scale is <see cref="Distances"/>'.
        /// </summary>
        public static double Map(MapLevel level, double units, LengthUnit to) =>
            Convert(Distances.LightYears(level, units), LengthUnit.LightYears, to);

        private static double Kilometres(LengthUnit unit) => unit switch
        {
            LengthUnit.Kilometres => 1,
            LengthUnit.EarthRadii => 6371,
            LengthUnit.JupiterRadii => 69911,
            LengthUnit.SolarRadii => 695700,
            LengthUnit.AstronomicalUnits => 149597870.7,
            LengthUnit.LightYears => KmPerLightYear,
            LengthUnit.Parsecs => KmPerLightYear * 3.26156377716743,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Not a LengthUnit."),
        };

        private static double Kilograms(MassUnit unit) => unit switch
        {
            MassUnit.Kilograms => 1,
            MassUnit.EarthMasses => 5.9722e24,
            MassUnit.JupiterMasses => 5.9722e24 * 317.83,
            MassUnit.SolarMasses => 1.98847e30,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Not a MassUnit."),
        };

        private static double Hours(TimeUnit unit) => unit switch
        {
            TimeUnit.Hours => 1,
            TimeUnit.Days => 24,
            TimeUnit.Years => 24 * 365.25,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "Not a TimeUnit."),
        };
    }
}
