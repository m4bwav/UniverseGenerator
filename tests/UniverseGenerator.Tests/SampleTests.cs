using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConsoleSample;
using NUnit.Framework;
using UniverseGeneration.Samples;

namespace UniverseGeneration.Tests
{
    /// <summary>The README's examples and the samples, so none of them can rot.</summary>
    public class SampleTests
    {
        private const string UnitySample = "Packages/com.m4bwav.universe-generator/Samples~/GalaxyPrinter";

        [Test]
        public void Every_readme_code_block_is_an_example_in_the_console_sample()
        {
            var readme = File.ReadAllText(Repo.PathTo("README.md"));
            var blocks = Regex.Matches(readme, "```csharp\n(.*?)```", RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => Normalise(m.Groups[1].Value.Split('\n').Where(l => !l.StartsWith("using ", StringComparison.Ordinal))))
                .ToList();
            var examples = File.ReadAllText(Repo.PathTo("samples/ConsoleSample/Examples.cs"));
            var regions = Regex.Matches(examples, @"#region readme\n(.*?)\n\s*#endregion", RegexOptions.Singleline)
                .Cast<Match>()
                .Select(m => Normalise(m.Groups[1].Value.Split('\n')))
                .ToList();
            Assert.That(blocks, Is.EqualTo(regions), "copy the examples from samples/ConsoleSample/Examples.cs into the README, in order");
        }

        [Test]
        public void The_readme_examples_run_and_print_what_the_readme_says()
        {
            var output = Run(Examples.All);
            Assert.That(output, Does.StartWith("HD 147927: K star, 6 planets, danger 9\nHD 165595: M star, 1 planets, danger 6\nHD 28101: K star, 6 planets, danger 6\n"));
            Assert.That(output, Does.Contain("\na ringed ice giant far from a red dwarf\nv1-my-seed/galaxy/system/31/planet/1\nTrue\n"));
            Assert.That(output, Does.Contain("Systems must be 1 to 2000; you asked for 0."));
            Assert.That(output, Does.Contain("\n15 of 30 systems at danger 8 or more\n6 of 40 systems at danger 8 or more\n"));
            Assert.That(output, Does.Contain("\nv1-my-seed/galaxy/system/13/planet/1\nv1-my-seed/galaxy?systems=20&planets=6&require=garden,blackhole\n"));
            Assert.That(output, Does.Contain("\nA truce holds the frontier, and someone is working to break it.\n"));
            Assert.That(output, Does.Contain("\nan empire ruled from HD 31487, holding 17 systems\n"));
            Assert.That(output, Does.Contain("\nHD 45528 (K)\nHD 19029 (F)\nKepler-1971 (F)\n"));
            Assert.That(output, Does.EndWith("; HD 147927 e I\nv1-my-seed/galaxy/system/31?systems=120\nTrue\n  \"address\": \"v1-my-seed/planet\",\nTrue\npirates\npirates\n229 million km out, -37 C\n30660\n"));
        }

        [Test]
        public void The_unity_sample_reports_a_galaxy()
        {
            var lines = GalaxyReport.Lines("my-seed", 60).ToList();
            var galaxy = Galaxy.Generate("my-seed");
            Assert.That(lines, Has.Count.EqualTo(1 + 60 + galaxy.Factions.Count));
            Assert.That(lines[0], Does.StartWith("Wyvern Galaxy: a ring galaxy, 60 systems"));
            Assert.That(lines[1], Does.EndWith("(v1-my-seed/galaxy/system/0)"));
        }

        [Test]
        public void The_unity_sample_is_listed_in_the_package_and_uses_only_the_report()
        {
            var manifest = File.ReadAllText(Repo.PathTo("Packages/com.m4bwav.universe-generator/package.json"));
            Assert.That(manifest, Does.Contain("\"path\": \"Samples~/GalaxyPrinter\""));
            var printer = File.ReadAllText(Repo.PathTo(UnitySample + "/GalaxyPrinter.cs"));
            Assert.That(printer, Does.Contain("GalaxyReport.Lines(seed, systems)"));
            Assert.That(Directory.GetFiles(Repo.PathTo(UnitySample)).Select(Path.GetFileName), Is.EquivalentTo(new[] { "GalaxyPrinter.cs", "GalaxyReport.cs" }));
        }

        private static string Normalise(IEnumerable<string> lines)
        {
            var list = lines.Select(l => l.TrimEnd()).ToList();
            while (list.Count > 0 && list[0].Length == 0)
            {
                list.RemoveAt(0);
            }

            while (list.Count > 0 && list[list.Count - 1].Length == 0)
            {
                list.RemoveAt(list.Count - 1);
            }

            var indent = list.Where(l => l.Length > 0).Min(l => l.Length - l.TrimStart().Length);
            return string.Join("\n", list.Select(l => l.Length == 0 ? l : l.Substring(indent)));
        }

        private static string Run(Action action)
        {
            var before = Console.Out;
            using var writer = new StringWriter { NewLine = "\n" };
            Console.SetOut(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(before);
            }

            return writer.ToString();
        }
    }
}
