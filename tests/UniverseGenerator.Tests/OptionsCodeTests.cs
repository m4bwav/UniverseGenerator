using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>GeneratorOptions.ToCode and FromCode, and links (an address plus a code) through Universe.Link and Universe.At.</summary>
    public class OptionsCodeTests
    {
        [Test]
        public void The_defaults_have_an_empty_code()
        {
            Assert.That(Preset.Default.ToCode(), Is.Empty);
            Assert.That(GeneratorOptions.FromCode(""), Is.EqualTo(Preset.Default));
            Assert.That(GeneratorOptions.FromCode("?"), Is.EqualTo(Preset.Default));
        }

        [Test]
        public void A_code_lists_the_changed_settings_in_a_fixed_order()
        {
            var options = Preset.SpaceOpera with { Shape = GalaxyShape.Barred, Systems = 120 };
            Assert.That(options.ToCode(), Is.EqualTo("systems=120&shape=barred&weirdness=10"));
            Assert.That(Preset.Plausible.ToCode(), Is.EqualTo("starmix=plausible&weirdness=1"));
        }

        [Test]
        public void Every_preset_round_trips()
        {
            foreach (var preset in Presets())
            {
                Assert.That(GeneratorOptions.FromCode(preset.ToCode()), Is.EqualTo(preset), preset.ToCode());
                Assert.That(GeneratorOptions.FromCode("?" + preset.ToCode()), Is.EqualTo(preset));
            }
        }

        [Test]
        public void Every_setting_has_a_name_in_the_code_and_round_trips()
        {
            // A new setting must be added to OptionsCode; this test fails until it is.
            var properties = typeof(GeneratorOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite).ToList();
            Assert.That(properties, Is.Not.Empty);
            foreach (var property in properties)
            {
                var changed = WithOtherValue(property);
                var code = changed.ToCode();
                Assert.That(code, Is.Not.Empty, $"{property.Name} is missing from OptionsCode");
                Assert.That(code, Does.Not.Contain("&"), $"{property.Name} wrote more than one setting");
                Assert.That(GeneratorOptions.FromCode(code), Is.EqualTo(changed), code);
            }
        }

        [Test]
        public void Every_choice_of_every_enum_round_trips()
        {
            foreach (var shape in All<GalaxyShape>())
            {
                var o = Preset.Default with { Shape = shape };
                Assert.That(GeneratorOptions.FromCode(o.ToCode()), Is.EqualTo(o));
            }

            foreach (var epoch in All<Epoch>())
            {
                var o = Preset.Default with { Epoch = epoch };
                Assert.That(GeneratorOptions.FromCode(o.ToCode()), Is.EqualTo(o));
            }

            foreach (var kind in All<ClusterKind>())
            {
                var o = Preset.Default with { ClusterKind = kind };
                Assert.That(GeneratorOptions.FromCode(o.ToCode()), Is.EqualTo(o));
            }
        }

        [TestCase("size=3", "\"size\" is not a setting; the settings are systems")]
        [TestCase("systems=10&systems=20", "\"systems\" appears twice.")]
        [TestCase("systems", "\"systems\" is not a name=value pair")]
        [TestCase("systems=", "\"systems=\" is not a name=value pair")]
        [TestCase("=3", "\"=3\" is not a name=value pair")]
        [TestCase("systems=012", "systems must be a whole number such as 12; got \"012\".")]
        [TestCase("systems=1x", "systems must be a whole number such as 12; got \"1x\".")]
        [TestCase("systems=0", "Systems must be 1 to 2000; you asked for 0.")]
        [TestCase("shape=Spiral", "shape must be one of auto, spiral, barred, elliptical, ring, irregular, colliding, starburst, clustered; got \"Spiral\".")]
        [TestCase("epoch=ancient", "epoch must be one of auto, young, mature, old; got \"ancient\".")]
        public void A_bad_code_is_refused_with_a_readable_message(string code, string message)
        {
            Assert.That(() => GeneratorOptions.FromCode(code), Throws.ArgumentException.With.Message.StartWith(message));
        }

        [Test]
        public void A_null_code_is_refused()
        {
            Assert.That(() => GeneratorOptions.FromCode(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void A_link_carries_the_options_to_Universe_At()
        {
            var options = Preset.SpaceOpera with { Systems = 120, Shape = GalaxyShape.Barred };
            var planet = Galaxy.Generate("my-seed", options).System(31).Planets[1];
            var link = Universe.Link(planet.Address, options);
            Assert.That(link, Is.EqualTo("v1-my-seed/galaxy/system/31/planet/1?systems=120&shape=barred&weirdness=10"));
            Assert.That(((Planet)Universe.At(link)).Summary, Is.EqualTo(planet.Summary));
        }

        [Test]
        public void A_link_with_the_defaults_is_the_address()
        {
            var galaxy = Galaxy.Generate("my-seed");
            Assert.That(Universe.Link(galaxy.Address, Preset.Default), Is.EqualTo(galaxy.Address));
            Assert.That(((Galaxy)Universe.At(galaxy.Address + "?")).Name, Is.EqualTo(galaxy.Name));
        }

        [Test]
        public void A_seed_with_question_marks_and_ampersands_still_makes_a_working_link()
        {
            var options = Preset.Default with { Systems = 20 };
            var galaxy = Galaxy.Generate("what?a&b=c", options);
            var link = Universe.Link(galaxy.System(3).Address, options);
            Assert.That(link, Does.StartWith("v1-what%3Fa%26b%3Dc/galaxy/system/3?systems=20"));
            Assert.That(((StarSystem)Universe.At(link)).Name, Is.EqualTo(galaxy.System(3).Name));
        }

        [Test]
        public void A_link_and_options_together_are_refused()
        {
            Assert.That(() => Universe.At("v1-my-seed/galaxy?systems=20", Preset.Default), Throws.ArgumentException.With.Message.StartWith("This link carries its options"));
        }

        [Test]
        public void Link_refuses_a_bad_address_or_bad_options()
        {
            Assert.That(() => Universe.Link("my-seed", Preset.Default), Throws.ArgumentException);
            Assert.That(() => Universe.Link("v1-my-seed/galaxy", Preset.Default with { Systems = 0 }), Throws.ArgumentException);
            Assert.That(() => Universe.Link("v1-my-seed/galaxy", null!), Throws.ArgumentNullException);
        }

        // Enum.GetValues<T>() does not exist on net48.
#pragma warning disable CA2263
        private static T[] All<T>()
            where T : struct, Enum => (T[])Enum.GetValues(typeof(T));
#pragma warning restore CA2263

        private static GeneratorOptions[] Presets() =>
            typeof(Preset).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(p => (GeneratorOptions)p.GetValue(null)!).ToArray();

        /// <summary>The defaults with one setting changed to another valid value, through reflection (tests only).</summary>
        private static GeneratorOptions WithOtherValue(PropertyInfo property)
        {
            var copy = Preset.Default with { };
            var current = property.GetValue(copy);
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            object[] candidates;
            if (type == typeof(int))
            {
                var v = current is int i ? i : 0;
                candidates = new object[] { v + 1, v - 1, 2, 3 };
            }
            else if (type == typeof(bool))
            {
                candidates = new object[] { !(current is bool b && b) };
            }
            else if (type.IsEnum)
            {
                candidates = Enum.GetValues(type).Cast<object>().Where(e => !e.Equals(current)).ToArray();
            }
            else
            {
                Assert.Fail($"teach WithOtherValue the type of {property.Name} ({type.Name})");
                return copy;
            }

            foreach (var candidate in candidates)
            {
                var changed = Preset.Default with { };
                property.SetValue(changed, candidate);
                try
                {
                    changed.Validate();
                    return changed;
                }
                catch (ArgumentException)
                {
                }
            }

            Assert.Fail($"no other valid value found for {property.Name}");
            return copy;
        }
    }
}
