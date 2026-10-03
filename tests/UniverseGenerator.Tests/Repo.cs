using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>Paths in the repository, and the golden-file rule: compare, never overwrite.</summary>
    internal static class Repo
    {
        public static string Root
        {
            get
            {
                var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "global.json")))
                {
                    dir = dir.Parent;
                }

                return dir?.FullName ?? throw new InvalidOperationException("No global.json above the test directory.");
            }
        }

        public static string PathTo(string relative) => Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));

        /// <summary>
        /// Compares generated text with a committed golden file. A missing file is written only when UG_WRITE_GOLDEN=1
        /// (and the test then reports Inconclusive, so the new file is reviewed and committed); an existing file is never
        /// rewritten (AGENTS.md, the seed promise).
        /// </summary>
        public static void AssertGolden(string relative, string actual)
        {
            var path = PathTo(relative);
            if (!File.Exists(path))
            {
                if (Environment.GetEnvironmentVariable("UG_WRITE_GOLDEN") == "1")
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    File.WriteAllText(path, actual, new UTF8Encoding(false));
                    Assert.Inconclusive($"Wrote the new golden file {relative}; review it and commit it.");
                }

                Assert.Fail($"The golden file {relative} is missing. Run once with UG_WRITE_GOLDEN=1 to create it.");
            }

            var expected = File.ReadAllText(path, Encoding.UTF8);
            if (expected == actual)
            {
                return;
            }

            var e = expected.Split('\n');
            var a = actual.Split('\n');
            var line = 0;
            while (line < e.Length && line < a.Length && e[line] == a[line])
            {
                line++;
            }

            Assert.Fail($"{relative} differs at line {line + 1}:\n  golden: {(line < e.Length ? e[line] : "(end)")}\n  actual: {(line < a.Length ? a[line] : "(end)")}");
        }
    }
}
