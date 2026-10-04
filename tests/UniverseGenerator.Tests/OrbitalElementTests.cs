using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>Planets' eccentricity, inclination and periapsis angle (orbital-elements.json), on their own stream.</summary>
    public class OrbitalElementTests
    {
        [Test]
        public void Orbital_elements_match_the_golden_file()
        {
            var w = new JsonWriter(indented: true);
            w.BeginObject().Name("generatorVersion").Int(GeneratorVersion.Current);
            w.Name("alone").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "-42", "", "Andromeda 7", "s1", "s2", "s3" })
            {
                Write(w, Planet.Generate(seed));
            }

            w.EndArray().Name("inSystems").BeginArray();
            foreach (var seed in new[] { "my-seed", "42", "s1", "s2", "s3", "s4" })
            {
                foreach (var p in StarSystem.Generate(seed).Planets)
                {
                    Write(w, p);
                }
            }

            foreach (var p in Galaxy.Generate("my-seed").System(31).Planets)
            {
                Write(w, p);
            }

            w.EndArray().EndObject();
            Repo.AssertGolden("tests/Golden/v1/orbital-elements.json", w.ToString());
        }

        [Test]
        public void Orbits_never_cross_and_stay_in_range()
        {
            var multi = new List<double>();
            for (var s = 0; s < 3000; s++)
            {
                var planets = StarSystem.Generate("e" + s).Planets.OrderBy(p => p.Orbit).ToList();
                foreach (var p in planets)
                {
                    Assert.That(p.Eccentricity, Is.InRange(0.0, 0.6), p.Address);
                    Assert.That(p.Inclination, Is.InRange(0.0, 90.0), p.Address);
                    Assert.That(p.PeriapsisAngle, Is.GreaterThanOrEqualTo(0.0).And.LessThan(360.0), p.Address);
                    if (planets.Count > 1)
                    {
                        multi.Add(p.Eccentricity);
                    }
                }

                for (var i = 0; i + 1 < planets.Count; i++)
                {
                    var (a, b) = (planets[i], planets[i + 1]);
                    Assert.That(a.Orbit * (1 + a.Eccentricity), Is.LessThan(b.Orbit * (1 - b.Eccentricity)), $"{a.Address} crosses {b.Address}");
                }
            }

            // Kepler's multi-planet systems: a mean near 0.05 (Rayleigh sigma 0.05 has mean 0.063 before the caps).
            Assert.That(multi.Average(), Is.InRange(0.03, 0.07));
        }

        [Test]
        public void A_planet_alone_is_more_eccentric_and_hot_planets_are_nearly_circular()
        {
            var alone = Enumerable.Range(0, 2000).Select(i => Planet.Generate("lone" + i)).ToList();
            Assert.That(alone.Average(p => p.Eccentricity), Is.InRange(0.2, 0.35));
            var hot = alone.Where(p => p.Orbit < 0.05).ToList();
            Assert.That(hot, Is.Not.Empty);
            Assert.That(hot.Max(p => p.Eccentricity), Is.LessThan(0.3));
        }

        [Test]
        public void The_elements_are_the_same_alone_and_in_the_galaxy()
        {
            var galaxy = Galaxy.Generate("my-seed");
            var planets = galaxy.System(12).Planets;
            var planet = planets[planets.Count - 1];
            var again = (Planet)Universe.At(planet.Address);
            Assert.That((again.Eccentricity, again.Inclination, again.PeriapsisAngle), Is.EqualTo((planet.Eccentricity, planet.Inclination, planet.PeriapsisAngle)));
        }

        [Test]
        public void The_key_filter_removes_keys_anywhere_and_keeps_the_rest()
        {
            var keys = new HashSet<string> { "x", "drop" };
            Assert.That(JsonKeyFilter.Without("{\"x\":1,\"y\":2}", keys), Is.EqualTo("{\"y\":2}"));
            Assert.That(JsonKeyFilter.Without("{\"y\":2,\"x\":1}", keys), Is.EqualTo("{\"y\":2}"));
            Assert.That(JsonKeyFilter.Without("{\"a\":[{\"drop\":{\"q\":[1,\"x\"]},\"k\":\"x\"}],\"x\":\"a\\\"b\"}", keys), Is.EqualTo("{\"a\":[{\"k\":\"x\"}]}"));
            Assert.That(JsonKeyFilter.Without("{\"x\":1}", keys), Is.EqualTo("{}"));
        }

        private static void Write(JsonWriter w, Planet p)
        {
            w.BeginObject().Name("address").String(p.Address).Name("eccentricity").Number(p.Eccentricity, 3);
            w.Name("inclination").Number(p.Inclination, 1).Name("periapsisAngle").Number(p.PeriapsisAngle, 1).EndObject();
        }
    }
}
