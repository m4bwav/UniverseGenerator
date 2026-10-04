using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UniverseGeneration.Tests
{
    /// <summary>The repository rules of AGENTS.md that a test can check.</summary>
    public class RepoRulesTests
    {
        private const string Package = "Packages/com.m4bwav.universe-generator";

        // Determinism rules (kb/rules/determinism.md) and the Unity rules (no engine types, no reflection) as source patterns.
        private static readonly (string Pattern, string Why)[] s_forbidden =
        {
            (@"\bSystem\.Random\b|\bnew Random\(", "System.Random: no promise across .NET versions, and it folds the sign"),
            (@"\bUnityEngine\b", "the Runtime asmdef has noEngineReferences"),
            (@"\.GetHashCode\(", "string.GetHashCode is randomised per process on .NET"),
            (@"\bGuid\.NewGuid\b", "identifiers come from addresses"),
            (@"\bMath\.(Exp|Log|Log10|Pow|Sin|Cos|Tan|Asin|Acos|Atan|Atan2|Sinh|Cosh|Tanh|Cbrt)\(", "C-library maths differ by platform; use DMath"),
            (@"\bMath\.Round\(|\bMidpointRounding\b", "Math.Round(x, n, mode) differs by runtime; use DMath.Round"),
            (@"\bfloat\b", "decisions and output use double and integers"),
            (@"\bSystem\.Reflection\b|\bActivator\.|\bType\.GetType\(", "no reflection (trimming, IL2CPP)"),
            (@"\bDateTime\.(Now|UtcNow)\b|\bEnvironment\.TickCount\b", "output depends only on the seed"),
        };

        [Test]
        public void Runtime_source_uses_no_forbidden_api()
        {
            var problems = (
                from file in Directory.GetFiles(Repo.PathTo(Package + "/Runtime"), "*.cs", SearchOption.AllDirectories)
                let lines = File.ReadAllLines(file)
                from i in Enumerable.Range(0, lines.Length)
                let code = Regex.Replace(lines[i], @"^\s*///?.*$|""(?:[^""\\]|\\.)*""", "")
                from rule in s_forbidden
                where Regex.IsMatch(code, rule.Pattern)
                select $"{Path.GetFileName(file)}:{i + 1}: {rule.Why}: {lines[i].Trim()}").ToList();
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void Every_package_file_and_folder_has_a_meta_file_with_a_unique_guid()
        {
            var root = Repo.PathTo(Package);
            var assets = Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
                .Where(p => !p.EndsWith(".meta", System.StringComparison.Ordinal))
                .Where(p => !p.Substring(root.Length).Split('/', '\\').Any(part => part.Length > 0 && (part[part.Length - 1] == '~' || part[0] == '.')))
                .ToList();
            var missing = assets.Where(p => !File.Exists(p + ".meta")).ToList();
            Assert.That(missing, Is.Empty, "run python scripts/add-meta.py");
            var guids = assets.Select(p => Regex.Match(File.ReadAllText(p + ".meta"), @"^guid: ([0-9a-f]{32})$", RegexOptions.Multiline).Groups[1].Value).ToList();
            Assert.That(guids, Has.None.Empty);
            Assert.That(guids, Is.Unique);
        }

        [Test]
        public void The_unity_assembly_is_in_the_upm_package_only()
        {
            // The NuGet build compiles the Runtime folder alone; Unity/ references UnityEngine and the Runtime assembly.
            var project = File.ReadAllText(Repo.PathTo("src/UniverseGenerator/UniverseGenerator.csproj"));
            Assert.That(Regex.Matches(project, "<Compile Include=\"([^\"]+)\"").Cast<Match>().Select(m => m.Groups[1].Value), Is.EqualTo(new[] { "$(RuntimeFolder)**/*.cs" }));
            Assert.That(project, Does.Contain("Packages/com.m4bwav.universe-generator/Runtime/</RuntimeFolder>"));
            var asmdef = File.ReadAllText(Repo.PathTo(Package + "/Unity/UniverseGenerator.Unity.asmdef"));
            Assert.That(asmdef, Does.Contain("\"UniverseGenerator\"").And.Contain("\"noEngineReferences\": false"));
            Assert.That(Directory.GetFiles(Repo.PathTo(Package + "/Unity"), "*.cs").Select(Path.GetFileName), Is.EquivalentTo(new[] { "TablesAsset.cs", "UnityVectors.cs" }));
        }

        [Test]
        public void The_unity_package_and_the_nuget_package_have_one_version()
        {
            var csproj = File.ReadAllText(Repo.PathTo("src/UniverseGenerator/UniverseGenerator.csproj"));
            var manifest = File.ReadAllText(Repo.PathTo(Package + "/package.json"));
            var nuget = Regex.Match(csproj, "<Version>([^<]+)</Version>").Groups[1].Value;
            var upm = Regex.Match(manifest, "\"version\": \"([^\"]+)\"").Groups[1].Value;
            Assert.That(upm, Is.EqualTo(nuget));
        }

        [Test]
        public void Source_files_use_lf_line_ends()
        {
            var withCr = new[] { Package, "src", "samples", "tests/UniverseGenerator.Tests" }
                .SelectMany(d => Directory.GetFiles(Repo.PathTo(d), "*.*", SearchOption.AllDirectories))
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar) && !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                // NuGet writes lock files with CRLF on Windows; .gitattributes stores them LF.
                .Where(f => Path.GetFileName(f) != "packages.lock.json")
                .Where(f => File.ReadAllBytes(f).Contains((byte)13))
                .ToList();
            Assert.That(withCr, Is.Empty);
        }
    }
}
