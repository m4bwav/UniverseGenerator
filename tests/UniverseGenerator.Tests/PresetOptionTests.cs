using System.Linq;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The presets and the Arms, ExtraLanes and DangerShift options: what each changes, and that the defaults change nothing.</summary>
    public class PresetOptionTests
    {
        private static readonly string[] Seeds = { "my-seed", "42", "Andromeda 7", "spiral", "bar", "x", "y", "z" };

        [Test]
        public void The_default_values_of_the_new_options_give_the_same_galaxy()
        {
            foreach (var seed in Seeds)
            {
                var plain = GalaxyJson.Text(Galaxy.Generate(seed));
                var spelled = GalaxyJson.Text(Galaxy.Generate(seed, Preset.Default with { Arms = null, ExtraLanes = 25, DangerShift = 0 }));
                Assert.That(spelled, Is.EqualTo(plain), seed);
            }
        }

        [Test]
        public void Arms_sets_the_arm_count_of_spirals_and_bars()
        {
            foreach (var arms in new[] { 2, 3, 4 })
            {
                Assert.That(Galaxy.Generate("spiral", Preset.Default with { Shape = GalaxyShape.Spiral, Systems = 120, Arms = arms }).Arms, Is.EqualTo(arms));
                Assert.That(Galaxy.Generate("bar", Preset.Default with { Shape = GalaxyShape.Barred, Systems = 120, Arms = arms }).Arms, Is.EqualTo(arms));
            }

            Assert.That(Galaxy.Generate("ring", Preset.Default with { Shape = GalaxyShape.Ring, Arms = 3 }).Arms, Is.Zero);
        }

        [Test]
        public void Arms_changes_only_the_map_not_the_regions_or_the_shape()
        {
            var drawn = Galaxy.Generate("spiral", Preset.Default with { Shape = GalaxyShape.Spiral, Systems = 120 });
            var other = drawn.Arms == 4 ? 2 : 4;
            var set = Galaxy.Generate("spiral", Preset.Default with { Shape = GalaxyShape.Spiral, Systems = 120, Arms = other });
            Assert.That(set.Shape, Is.EqualTo(drawn.Shape));
            Assert.That(set.Regions.Select(r => r.Name), Is.EqualTo(drawn.Regions.Select(r => r.Name)));
        }

        [Test]
        public void More_extra_lanes_give_more_lanes_and_the_same_systems()
        {
            foreach (var seed in Seeds)
            {
                var none = Galaxy.Generate(seed, Preset.Default with { ExtraLanes = 0 });
                var some = Galaxy.Generate(seed);
                var all = Galaxy.Generate(seed, Preset.Default with { ExtraLanes = 100 });
                Assert.That(none.Lanes.Count, Is.LessThanOrEqualTo(some.Lanes.Count), seed);
                Assert.That(some.Lanes.Count, Is.LessThanOrEqualTo(all.Lanes.Count), seed);
                Assert.That(none.Lanes.Count, Is.LessThan(all.Lanes.Count), seed);
                Assert.That(all.Map.Select(m => (m.X, m.Y)), Is.EqualTo(none.Map.Select(m => (m.X, m.Y))), seed);
            }
        }

        [TestCase(-5)]
        [TestCase(-2)]
        [TestCase(3)]
        [TestCase(5)]
        public void DangerShift_moves_every_systems_danger_within_1_to_10(int shift)
        {
            foreach (var seed in Seeds)
            {
                var plain = Galaxy.Generate(seed, Preset.Default with { ExtraLanes = 25 });
                var shifted = Galaxy.Generate(seed, Preset.Default with { DangerShift = shift });
                for (var i = 0; i < plain.Map.Count; i++)
                {
                    // The shift is applied before the 1 to 10 clamp, so a clamped value moves less; it never moves the other way.
                    var expected = plain.Map[i].Danger + shift;
                    Assert.That(shifted.Map[i].Danger, Is.InRange(1, 10));
                    if (expected >= 1 && expected <= 10 && plain.Map[i].Danger > 1 && plain.Map[i].Danger < 10)
                    {
                        Assert.That(shifted.Map[i].Danger, shift > 0 ? Is.GreaterThanOrEqualTo(plain.Map[i].Danger) : Is.LessThanOrEqualTo(plain.Map[i].Danger));
                    }
                }

                Assert.That(shifted.Map.Select(m => m.Name), Is.EqualTo(plain.Map.Select(m => m.Name)), seed);
            }
        }

        [Test]
        public void A_lone_system_shifts_its_drawn_danger()
        {
            var plain = StarSystem.Generate("my-seed");
            var safer = StarSystem.Generate("my-seed", Preset.Default with { DangerShift = -5 });
            var riskier = StarSystem.Generate("my-seed", Preset.Default with { DangerShift = 5 });
            Assert.That(safer.Danger, Is.EqualTo(System.Math.Max(1, plain.Danger - 5)));
            Assert.That(riskier.Danger, Is.EqualTo(System.Math.Min(10, plain.Danger + 5)));
        }

        [Test]
        public void The_presets_hold_what_they_promise()
        {
            Assert.That(Galaxy.Generate("my-seed", Preset.Pocket).Map, Has.Count.EqualTo(20));
            Assert.That(Galaxy.Generate("my-seed", Preset.Pocket).Systems.Max(s => s.Planets.Count), Is.LessThanOrEqualTo(6));
            Assert.That(Galaxy.Generate("my-seed", Preset.Roguelike).Map, Has.Count.EqualTo(30));
            Assert.That(Galaxy.Generate("my-seed", Preset.Cozy).Map, Has.Count.EqualTo(40));
            Assert.That(Galaxy.Generate("my-seed", Preset.Epic).Map, Has.Count.EqualTo(300));

            double Mean(GeneratorOptions o) => Seeds.SelectMany(s => Galaxy.Generate(s, o with { Systems = 60 }).Map).Average(m => m.Danger);
            Assert.That(Mean(Preset.Roguelike), Is.GreaterThan(Mean(Preset.Default) + 1));
            Assert.That(Mean(Preset.Cozy), Is.LessThan(Mean(Preset.Default) - 1.5));
        }

        [TestCase("Arms", 1)]
        [TestCase("Arms", 5)]
        [TestCase("ExtraLanes", -1)]
        [TestCase("ExtraLanes", 101)]
        [TestCase("DangerShift", 6)]
        [TestCase("DangerShift", -6)]
        public void Out_of_range_values_are_refused(string name, int value)
        {
            var o = name switch
            {
                "Arms" => Preset.Default with { Arms = value },
                "ExtraLanes" => Preset.Default with { ExtraLanes = value },
                _ => Preset.Default with { DangerShift = value },
            };
            Assert.That(() => Galaxy.Generate("my-seed", o), Throws.ArgumentException.With.Message.StartWith(name + " must be"));
        }
    }
}
