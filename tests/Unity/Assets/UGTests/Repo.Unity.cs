using System;
using System.IO;
using UnityEngine;

namespace UniverseGeneration.Tests
{
    /// <summary>
    /// Unity's stand-in for tests/UniverseGenerator.Tests/Repo.cs. The golden files are read from Resources, where
    /// make_project.py copies them, so the same comparison runs in the Editor (Mono) and in players (IL2CPP). A golden
    /// file is never written here.
    /// </summary>
    internal static class Repo
    {
        private const string GoldenPrefix = "tests/Golden/v1/";

        /// <summary>The repository root on the machine that made the project (RepoRoot.g.cs); Editor tests only.</summary>
        public static string Root => RepoRoot.Value;

        public static string PathTo(string relative) => Path.Combine(Root, relative.Replace('/', Path.DirectorySeparatorChar));

        public static void AssertGolden(string relative, string actual)
        {
            if (!relative.StartsWith(GoldenPrefix, StringComparison.Ordinal) || !relative.EndsWith(".json", StringComparison.Ordinal))
            {
                throw new GoldenMismatchException($"Not a v1 golden file: {relative}");
            }

            var name = relative.Substring(GoldenPrefix.Length, relative.Length - GoldenPrefix.Length - ".json".Length);
            var asset = Resources.Load<TextAsset>("Golden/v1/" + name);
            if (asset == null)
            {
                throw new GoldenMismatchException($"{relative} is missing from Resources; run make_project.py again.");
            }

            var expected = asset.text;
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

            throw new GoldenMismatchException($"{relative} differs at line {line + 1}:\n  golden: {(line < e.Length ? e[line] : "(end)")}\n  actual: {(line < a.Length ? a[line] : "(end)")}");
        }
    }

    /// <summary>A golden comparison failed. Thrown rather than NUnit's Assert, so players can run the checks without a test runner.</summary>
    public sealed class GoldenMismatchException : Exception
    {
        public GoldenMismatchException(string message)
            : base(message)
        {
        }
    }

    /// <summary>Where TestContext.Out.WriteLine goes in the copied tests (make_project.py rewrites the calls).</summary>
    internal static class UGLog
    {
        public static void WriteLine(string line) => Debug.Log(line);
    }
}
