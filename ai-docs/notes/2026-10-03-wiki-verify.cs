#:package UniverseGenerator@1.0.0
#:property PublishAot=false
// wiki-verify for UniverseGenerator 1.0.0: prints every output the wiki's pages show, run against
// the PUBLISHED package from nuget.org, never the working tree. Keep the filled-in copy in the
// repository as ai-docs/notes/<date>-wiki-verify.cs, and its output beside it as
// ai-docs/notes/<date>-wiki-verify.out.txt with LF line endings (Console.WriteLine writes CRLF on
// Windows: tr -d '\r'), so the next release can run it again and diff (L-019, L-105).
//
// `wikiwright.py scaffold nuget ID VERSION --namespace NS --type T` writes this file filled in and cut to the
// sections the survey calls for: --children (the pages' examples as whole programs, run in child apps per
// framework and dependency set), --requests (the stand-in proxy and the .invalid gate), --fsharp (dotnet fsi),
// --tool ID:COMMAND (a dotnet tool's install and terminal transcripts). The template is never cut by hand.
//
// Run it from a folder outside any project cone (a scratch folder; file-based apps pick up
// Directory.Build.props and global.json from parent folders), with TEMP and TMP pointing into that folder
// (file-based builds go to <TEMP>/dotnet/runfile otherwise; from Git Bash export them as C:/ paths):
//   dotnet build wiki-verify.cs && dotnet run --no-build wiki-verify.cs > out.txt
// (a first plain `dotnet run` prints its build warnings into out.txt). Two runs must print the
// same thing: no times, paths or random values (L-022, L-111); Show() masks what Masks lists.
//
// PublishAot=false: .NET 10 file-based apps enable native AOT by default, which turns off
// reflection-based System.Text.Json and makes such calls throw even under dotnet run.
// Two copies of this file must not run at once from one folder: they share build output.
//
// Every case prints "## <label>" and then its output. Paste outputs into the pages exactly as
// printed; a page never shows output this program did not produce. A value shown in a code
// comment on a page is quoted (// "Marguerita") or written // => value, so
// `wikiwright.py outputs` can check it (L-110). The page's PowerShell and F# snippets run from
// here too, through Run() and Fsi(), so their output is in the saved file.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UniverseGeneration;

Console.OutputEncoding = Encoding.UTF8;
var scratch = (string?)AppContext.GetData("EntryPointFileDirectoryPath") ?? Environment.CurrentDirectory;
var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
Masks.Pairs.AddRange([(scratch, "<scratch>"), (Environment.GetEnvironmentVariable("NUGET_PACKAGES") ?? Path.Combine(home, ".nuget", "packages"), "<nuget>"),
    (Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar), "<temp>"), (home, "<home>")]);
// Text with a shape rather than a fixed value, as a regular expression and its token, masked after the pairs:
// an external program's heap addresses, for example ffmpeg's "[mp3 @ 0x55d0c1a2b3c0]".
// Masks.Patterns.Add((@"@ (0x)?[0-9a-f]{8,16}\]", "@ <address>]"));

// What loaded: the runtime, the package, then each dependency whose version the pages name, by assembly name
// ("Autofac", "Castle.Core"). Installed() prints the informational version, since an assembly version can stay put
// across releases (Castle.Core is 5.0.0.0 in 5.1.1 and 5.2.1), and the framework of the lib folder that loaded, from
// TargetFrameworkAttribute (Assembly.Location is the bin folder a build copied the DLL to). A dependency range the
// package declares is run at its ends: one children config per end (the children section).
string[] dependencies = [];
var asm = typeof(Galaxy).Assembly;
Show("installed", RuntimeInformation.FrameworkDescription + "\n"
    + string.Join("\n", new[] { asm }.Concat(dependencies.Select(name => Assembly.Load(name))).Select(Installed)));

// ----- the pages' examples as whole programs, run in child apps: one set per config, each example in its own process -----
// An example is a whole program as the page shows it: using lines, statements, then the types it declares. The
// types stay global, so a printed type name (a cache key, a container's message) is the page's, with no wrapper
// namespace. Each example is its own file in a child app (#:include), its statements wrapped in a method; examples
// that declare the same type name go to separate child apps. An example that starts with its own #: lines (a
// file-based app on the page) runs as its own app with them (L-143). Every example runs in its own process, so
// process-global state (MemoryCache.Default, a static field, a container) never carries over from another one.
// Children build without the proxy variables, so their restores reach nuget.org.
// A config is a target framework and extra #: lines: "Autofac@9.3.4" adds `#:package Autofac@9.3.4` (the ends of a
// dependency range the package declares, one config each), "UniverseGenerator@1.0.1" runs an old version, a line
// starting with #: passes through. "net48" runs .NET Framework 4.8 (Windows only) on the package's net4x or
// netstandard2.0 build, "net8.0" a netstandard2.0 build. A raw string holding """ needs """" around the example.
var configs = new List<Config>
{
    new("net10.0"),
    new("net48"),
};
var examples = new List<Example>
{
    new("home: the first example", """
        using UniverseGeneration;

        var galaxy = Galaxy.Generate("my-seed");
        foreach (var system in galaxy.Systems.Take(3))
            Console.WriteLine($"{system.Name}: {system.Star.Class} star, {system.Planets.Count} planets, danger {system.Danger}");
        """),
    new("getting started: a galaxy at a glance", """
        using UniverseGeneration;

        var galaxy = Galaxy.Generate("my-seed");
        Console.WriteLine(galaxy.Name);
        Console.WriteLine(galaxy.Descriptor);
        Console.WriteLine($"{galaxy.Shape}, {galaxy.Map.Count} systems, {galaxy.Lanes.Count} lanes, {galaxy.Regions.Count} regions");

        var system = galaxy.System(0);
        Console.WriteLine(system.Descriptor);
        Console.WriteLine(system.Star.Description);
        foreach (var planet in system.Planets)
            Console.WriteLine($"  {planet.Name}: {planet.Descriptor}");
        """),
    new("getting started: every level on its own", """
        using UniverseGeneration;

        Console.WriteLine(Universe.Generate("my-seed").Name);
        Console.WriteLine(GalaxyCluster.Generate("my-seed").Name);
        Console.WriteLine(Galaxy.Generate("my-seed").Name);
        Console.WriteLine(StarSystem.Generate("my-seed").Descriptor);
        Console.WriteLine(Planet.Generate("my-seed").Summary);
        Console.WriteLine(StarName.Generate("my-seed").Name);
        """),
    new("getting started: number seeds", """
        using UniverseGeneration;

        var a = Galaxy.Generate(42);
        var b = Galaxy.Generate("42");
        Console.WriteLine(a.Address);
        Console.WriteLine(a.ToJson() == b.ToJson());
        """),
    new("same seed: twice, and a part alone", """
        using UniverseGeneration;

        var first = Galaxy.Generate("my-seed").ToJson(children: true);
        var second = Galaxy.Generate("my-seed").ToJson(children: true);
        Console.WriteLine(first == second);

        var galaxy = Galaxy.Generate("my-seed");
        var inPlace = galaxy.System(31);
        var alone = (StarSystem)Universe.At(inPlace.Address);
        Console.WriteLine(inPlace.ToJson() == alone.ToJson());
        """),
    new("same seed: a fingerprint", """
        using System.Security.Cryptography;
        using System.Text;
        using UniverseGeneration;

        var json = Galaxy.Generate("my-seed").ToJson(children: true);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        Console.WriteLine($"{json.Length} characters, SHA-256 {BitConverter.ToString(hash).Replace("-", "").Substring(0, 16)}...");
        """),
    new("same seed: seeds that look alike", """
        using UniverseGeneration;

        foreach (var seed in new[] { "my-seed", "My-Seed", "my-seed ", "hello world" })
        {
            var galaxy = Galaxy.Generate(seed);
            Console.WriteLine($"\"{seed}\": {galaxy.Address}, {galaxy.Name}, first system {galaxy.Map[0].Name}");
        }
        """),
    new("same seed: options are part of the answer", """
        using UniverseGeneration;

        var plain = Galaxy.Generate("my-seed");
        var shifted = Galaxy.Generate("my-seed", Preset.Default with { DangerShift = 2 });
        var bigger = Galaxy.Generate("my-seed", Preset.Default with { Systems = 61 });
        Console.WriteLine($"{plain.Map[0].Name}, {shifted.Map[0].Name}, {bigger.Map[0].Name}");
        Console.WriteLine($"danger {plain.Map[0].Danger}, {shifted.Map[0].Danger}, {bigger.Map[0].Danger}");
        Console.WriteLine(plain.Map.Zip(shifted.Map, (a, b) => a.Name == b.Name && a.X == b.X).All(same => same));
        """),
    new("same seed: hooks never move a draw", """
        using UniverseGeneration;

        var hooks = new GeneratorHooks { OnSystem = s => s with { Name = "New " + s.Name } };
        var plain = Galaxy.Generate("my-seed").System(0);
        var hooked = Galaxy.Generate("my-seed", null, hooks).System(0);
        Console.WriteLine($"{plain.Name} / {hooked.Name}");
        Console.WriteLine(plain.Star == hooked.Star && plain.Planets.Count == hooked.Planets.Count);
        """),
    new("plausibility: stars and their zones", """
        using UniverseGeneration;

        foreach (var system in Galaxy.Generate("my-seed").Systems.Take(4))
        {
            var star = system.Star;
            Console.WriteLine(FormattableString.Invariant($"{system.Name}: {star.SpectralType}, {star.Mass} Suns, luminosity {star.Luminosity}, radius {star.Radius}, {star.Temperature} K, {star.Colour}"));
            Console.WriteLine(FormattableString.Invariant($"  habitable zone {system.HabitableZoneInner} to {system.HabitableZoneOuter} au, frost line {system.FrostLine} au"));
        }
        """),
    new("plausibility: planets agree with their star", """
        using UniverseGeneration;

        var system = Galaxy.Generate("my-seed").System(0);
        foreach (var p in system.Planets)
            Console.WriteLine(FormattableString.Invariant($"{p.Name}: {p.Kind}, {p.Zone}, {p.Orbit} au, {p.Radius} Earth radii, {Math.Round(p.Temperature)} K, eccentricity {p.Eccentricity}"));
        """),
    new("plausibility: the star mix of two presets", """
        using UniverseGeneration;

        foreach (var (name, options) in new[] { ("Default", Preset.Default), ("Plausible", Preset.Plausible) })
        {
            var galaxy = Galaxy.Generate("my-seed", options with { Systems = 2000 });
            var counts = galaxy.Map.GroupBy(m => m.StarClass).OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                .Select(g => $"{g.Key} {g.Count()}");
            Console.WriteLine($"{name}: {string.Join(", ", counts)}");
        }
        """),
    new("errors: what is refused", """
        using UniverseGeneration;

        void Try(string what, Action action)
        {
            try
            {
                action();
                Console.WriteLine($"{what}: no error");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{what}: {e.GetType().Name}: {e.Message}");
            }
        }

        Try("Systems = 0", () => Galaxy.Generate("my-seed", Preset.Default with { Systems = 0 }));
        Try("Systems = 2001", () => Galaxy.Generate("my-seed", Preset.Default with { Systems = 2001 }));
        Try("Weirdness = 101", () => Galaxy.Generate("my-seed", Preset.Default with { Weirdness = 101 }));
        Try("Arms = 5", () => Galaxy.Generate("my-seed", Preset.Default with { Arms = 5 }));
        Try("DangerShift = 6", () => Galaxy.Generate("my-seed", Preset.Default with { DangerShift = 6 }));
        Try("a 201-character seed", () => Galaxy.Generate(new string('a', 201)));
        Try("an empty seed", () => Galaxy.Generate(""));
        Try("a null seed", () => Galaxy.Generate((string)null!));
        Try("System(60) of 60", () => Galaxy.Generate("my-seed").System(60));
        Try("an unknown level", () => Universe.At("v1-my-seed/galaxy/nebula/0"));
        Try("system 60 of 60", () => Universe.At("v1-my-seed/galaxy/system/60"));
        Try("generator version 2", () => Universe.At("v2-my-seed/galaxy"));
        Try("a link and options", () => Universe.At("v1-my-seed/galaxy?systems=120", Preset.Default));
        Try("an unknown option code", () => GeneratorOptions.FromCode("systems=120&colour=red"));
        Try("unregistered tables", () => Galaxy.Generate("my-seed", Preset.Default with { Tables = "nobody-registered-this" }));
        Try("StarName count 0", () => StarName.Generate("my-seed", 0));
        Try("StarName count 1001", () => StarName.Generate("my-seed", 1001));
        Try("tables from broken JSON", () => GeneratorTables.FromJson("{"));
        """),
    new("errors: warnings, never errors", """
        using UniverseGeneration;

        var ring = Preset.Default with { Shape = GalaxyShape.Ring, Arms = 3 };
        foreach (var w in ring.Check())
            Console.WriteLine($"{w.Code}: {w.Message}");

        var small = Preset.Default with { Shape = GalaxyShape.Spiral, Systems = 12 };
        foreach (var w in small.Check())
            Console.WriteLine($"{w.Code}: {w.Message}");

        var barren = Preset.Default with { MaxPlanetsPerSystem = 0, Require = Guarantee.GardenWorld };
        foreach (var w in Galaxy.Generate("my-seed", barren).Warnings)
            Console.WriteLine($"{w.Code}: {w.Message}");
        """),
    new("recipes: save and restore a run", """
        using UniverseGeneration;

        var options = Preset.Roguelike with { Names = NameStyle.Invented };
        var code = options.ToCode();
        Console.WriteLine(code);

        var restored = GeneratorOptions.FromCode(code);
        Console.WriteLine(restored == options);
        var galaxy = Galaxy.Generate("run-2817", restored);
        Console.WriteLine(Universe.Link(galaxy.System(5).Address, restored));
        """),
    new("recipes: compare the presets", """
        using UniverseGeneration;

        var presets = new[]
        {
            ("Default", Preset.Default), ("SpaceOpera", Preset.SpaceOpera), ("Plausible", Preset.Plausible), ("Pocket", Preset.Pocket),
            ("Roguelike", Preset.Roguelike), ("Cozy", Preset.Cozy), ("Epic", Preset.Epic),
        };
        foreach (var (name, options) in presets)
        {
            var g = Galaxy.Generate("my-seed", options);
            Console.WriteLine($"{name}: {g.Map.Count} systems, {g.Shape}, {g.Lanes.Count} lanes, {g.Map.Count(m => m.Danger >= 8)} at danger 8 or more, code \"{options.ToCode()}\"");
        }
        """),
    new("recipes: draw the map as SVG", """
        using System.Text;
        using UniverseGeneration;

        var galaxy = Galaxy.Generate("my-seed");
        var r = galaxy.Radius;
        var svg = new StringBuilder();
        svg.AppendLine(FormattableString.Invariant($"<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"{-r} {-r} {2 * r} {2 * r}\">"));
        foreach (var lane in galaxy.Lanes)
        {
            var a = galaxy.Map[lane.A];
            var b = galaxy.Map[lane.B];
            var colour = lane.Bridge ? "red" : "grey";
            svg.AppendLine(FormattableString.Invariant($"<line x1=\"{a.X}\" y1=\"{a.Y}\" x2=\"{b.X}\" y2=\"{b.Y}\" stroke=\"{colour}\" />"));
        }

        foreach (var m in galaxy.Map)
            svg.AppendLine(FormattableString.Invariant($"<circle cx=\"{m.X}\" cy=\"{m.Y}\" r=\"8\"><title>{m.Name}, danger {m.Danger}</title></circle>"));
        svg.AppendLine("</svg>");
        File.WriteAllText("galaxy.svg", svg.ToString());
        Console.WriteLine(svg.ToString().Split('\n')[0].TrimEnd());
        Console.WriteLine($"{galaxy.Lanes.Count} lanes, {galaxy.Lanes.Count(l => l.Bridge)} of them bridges");
        """),
    new("recipes: travel times", """
        using UniverseGeneration;

        var galaxy = Galaxy.Generate("my-seed");
        foreach (var lane in galaxy.Lanes.Take(3))
        {
            var a = galaxy.Map[lane.A];
            var b = galaxy.Map[lane.B];
            var length = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
            Console.WriteLine($"{a.Name} to {b.Name}: {Math.Round(Distances.LightYears(MapLevel.Galaxy, length))} light-years, {Distances.LaneDays(length)} days");
        }
        """),
    new("recipes: find a seed", """
        using UniverseGeneration;

        var seed = Enumerable.Range(0, 1000).Select(i => "colony-" + i).First(s =>
            Galaxy.Generate(s, Preset.Pocket).Systems.Any(sys => sys.Danger <= 2 && sys.Planets.Any(p => p.Kind == PlanetKind.Garden)));
        Console.WriteLine(seed);
        """),
    new("recipes: export to a file", """
        using UniverseGeneration;

        var system = Galaxy.Generate("my-seed").System(0);
        var json = system.ToJson(indented: true);
        File.WriteAllText("system.json", json);
        foreach (var line in json.Split('\n').Take(6))
            Console.WriteLine(line.TrimEnd());
        """),
    new("recipes: the type field of each export", """
        using UniverseGeneration;

        var universe = Universe.Generate("my-seed");
        var cluster = universe.Cluster(0);
        var galaxy = cluster.Galaxy(0);
        var system = galaxy.System(0);
        foreach (var json in new[] { universe.ToJson(), cluster.ToJson(), universe.Voids[0].ToJson(), galaxy.ToJson(), system.ToJson(), system.Planets[0].ToJson() })
            Console.WriteLine(json.Substring(0, json.IndexOf(",\"address\"")) + "}");
        Console.WriteLine(system.Address);
        """),
    new("recipes: your own system names", """
        using UniverseGeneration;

        var names = new[] { "Avalon", "Brigid", "Caer Sidi" };
        var hooks = new GeneratorHooks
        {
            OnSystem = s => int.TryParse(s.Address.Split('/').Last(), out var i) && i < names.Length ? s with { Name = names[i] } : s,
        };
        var galaxy = Galaxy.Generate("my-seed", null, hooks);
        foreach (var s in galaxy.Systems.Take(4))
            Console.WriteLine($"{s.Name}, first planet {s.Planets.FirstOrDefault()?.Name ?? "none"}");
        """),
    new("recipes: an ocean-heavy temperate zone", """
        using UniverseGeneration;

        Console.WriteLine(string.Join(", ", GeneratorTables.Ids));
        var temperate = GeneratorTables.Default.Table("planet-kinds.temperate");
        Console.WriteLine(string.Join(", ", temperate.Entries.Select(e => $"{e.Name} {e.Weight}")));

        var wet = GeneratorTables.Default.With("wet-worlds", new WeightTable
        {
            Id = "planet-kinds.temperate",
            Entries = temperate.Entries.Select(e => e.Name == "Ocean" ? e with { Weight = e.Weight * 4 } : e).ToArray(),
        });
        GeneratorTables.Register(wet);

        int Oceans(GeneratorOptions o) => Galaxy.Generate("my-seed", o).Systems.SelectMany(s => s.Planets).Count(p => p.Kind == PlanetKind.Ocean);
        Console.WriteLine($"{Oceans(Preset.Default)} ocean worlds, then {Oceans(Preset.Default with { Tables = "wet-worlds" })}");
        """),
    new("versions: every system of my-seed", """
        using System.Security.Cryptography;
        using System.Text;
        using UniverseGeneration;

        var galaxy = Galaxy.Generate("my-seed");
        var text = string.Join("\n", galaxy.Systems.Select(s =>
            $"{s.Name} {s.Star.SpectralType} {s.Danger} {s.Descriptor} | " + string.Join(" | ", s.Planets.Select(p => p.Summary))));
        using var sha = SHA256.Create();
        var hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").Substring(0, 16);
        Console.WriteLine($"{galaxy.Systems.Count} systems, {text.Length} characters, SHA-256 {hash}...");
        Console.WriteLine(galaxy.Systems[0].Planets[0].Summary);
        """),
};
Func<Child, string, string> runCase = (child, label) => RunExample(child, label, null, scratch);

var children = configs.Select(config => BuildChild(scratch, config, examples, dependencies)).ToList();

foreach (var child in children)
{
    Show($"installed ({child.Name})", RunExample(child, "installed", null, scratch));
    foreach (var example in examples.Where(e => !e.Harness))
    {
        Show($"{example.Label} ({child.Name})", runCase(child, example.Label));
    }
}

// The same example on the previous release, in its own child app (its 1.0.0 members would not build there).
var previous = BuildChild(scratch, new Config("net10.0", ["UniverseGenerator@1.0.0-beta.1"]), examples.Where(e => e.Label.StartsWith("versions:")).ToList(), dependencies);
Show("installed (net10.0 with 1.0.0-beta.1)", RunExample(previous, "installed", null, scratch));
foreach (var example in examples.Where(e => e.Label.StartsWith("versions:")))
{
    Show($"{example.Label} (net10.0 with 1.0.0-beta.1)", RunExample(previous, example.Label, null, scratch));
}

// ----- the cases: one per example on the wiki, labelled by page -----
// Show("getting started: the first call", ...);
// What a page's code prints with Console.WriteLine, line for line:
// Show("recipes: a loop", Captured(() => { for (var i = 0; i < 3; i++) Console.WriteLine(i); }));
// An unseeded example: print it only when it is a possible answer:
// Possible("home: unseeded examples", ("Prophetstown", SomeGenerator.Names.Contains));
// Errors: Catch() prints the exception type and message the page quotes.
// Show("null input", Catch(() => ...));

// ----- the pages' PowerShell and F# snippets, run as written -----
// Show("getting started: PowerShell", Run("pwsh", ["-NoProfile", "-NonInteractive", "-Command", """
//     Add-Type -Path "$env:USERPROFILE/.nuget/packages/universegenerator/1.0.0/lib/netstandard2.0/<assembly>.dll"
//     ...
//     """]));

static void Show(string label, object? value, params (string Text, string Token)[] extra)
{
    Console.WriteLine($"## {label}");
    Console.WriteLine(Mask(value?.ToString() ?? "<null>", extra));
    Console.WriteLine();
}

// Prints each value only when its check passes, one per line, so a page's example of a random
// call is shown to be a possible answer (L-111 `membership-for-random`).
static void Possible(string label, params (string Value, Func<string, bool> OnList)[] cases)
{
    Show(label + " (each checked against the list it comes from)",
        string.Join("\n", cases.Select(c => c.OnList(c.Value) ? c.Value : "NOT ON THE LIST: " + c.Value)));
}

// What the code prints with Console.WriteLine, line for line, LF endings.
static string Captured(Action action)
{
    var original = Console.Out;
    var writer = new StringWriter { NewLine = "\n" };
    Console.SetOut(writer);
    try
    {
        action();
    }
    finally
    {
        Console.SetOut(original);
    }

    return writer.ToString().TrimEnd('\n');
}

// Runs a command (a page's snippet in another host, a child app) and returns what it printed, LF
// line endings. env: a value sets a variable, null removes it (ProxyEnv).
static string Run(string file, string[] args, Dictionary<string, string?>? env = null, string? cwd = null)
{
    var (stdout, stderr, exit) = Start(file, args, env, cwd);
    var err = stderr.Trim();   // fsi pads its warnings and errors with blank lines
    var text = stdout.TrimEnd() + (err.Length > 0 ? "\n--- stderr\n" + err : "") + (exit != 0 ? "\n--- exit " + exit : "");
    return text.Replace("\r\n", "\n").TrimStart('\n');
}

// Starts a process with both output streams read at once (a child that writes much to stderr cannot fill
// that pipe and stall) and stdin redirected and closed at once, so a child that asks reads end of input
// instead of waiting on this program's console. commandLine replaces args (cmd's own quoting, in Term()).
static (string Stdout, string Stderr, int Exit) Start(string file, string[] args, Dictionary<string, string?>? env = null, string? cwd = null, string? commandLine = null)
{
    var info = new ProcessStartInfo(file)
    {
        RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false,
        StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8, WorkingDirectory = cwd ?? "",
    };
    if (commandLine is not null) info.Arguments = commandLine;
    foreach (var arg in args)
    {
        info.ArgumentList.Add(arg);
    }

    foreach (var (name, value) in env ?? [])
    {
        if (value is null) info.Environment.Remove(name); else info.Environment[name] = value;
    }

    using var p = Process.Start(info)!;
    p.StandardInput.Close();
    var stderr = p.StandardError.ReadToEndAsync();
    var stdout = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
    return (stdout, stderr.Result, p.ExitCode);
}

static string Catch(Func<object?> action)
{
    try
    {
        return "returned: " + (action()?.ToString() ?? "<null>");
    }
    catch (Exception e)
    {
        return $"{e.GetType().Name}: {e.Message}";
    }
}

// Replaces what differs between machines and runs: Masks.Pairs and the call's extra pairs (a case's folder as
// <cwd>), longest first, each also with / separators; then each of Masks.Patterns. On Linux run with a
// distinctive TMPDIR (TMPDIR=/tmp/wiki-verify-tmp): masking a bare /tmp also rewrites content such as
// file:///tmp/a.png.
static string Mask(string text, params (string Text, string Token)[] extra)
{
    foreach (var (from, token) in Masks.Pairs.Concat(extra).Where(p => p.Text.Length > 1).OrderByDescending(p => p.Text.Length))
    {
        text = text.Replace(from, token).Replace(from.Replace(Path.DirectorySeparatorChar, '/'), token);
    }

    foreach (var (pattern, token) in Masks.Patterns)
    {
        text = Regex.Replace(text, pattern, token);
    }

    return text;
}

// "Name informational-version (file F, lib Framework,Version=vX)": see `dependencies` at the top. The file version
// is there for a build that never set an informational version, which then reads 1.0.0 (FFMpegCore 5.0.0.0).
static string Installed(Assembly assembly) => assembly.GetName().Name + " "
    + assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0]
    + " (file " + assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
    + ", lib " + (assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "without a TargetFrameworkAttribute") + ")";

static string Slug(string text) => string.Concat(text.Select(c => char.IsAsciiLetterOrDigit(c) ? c : '-'));

// Writes a config's examples into child apps under <scratch>/children/<config>, builds each WITHOUT the proxy
// variables and returns the command that runs each example by label, plus "installed" (the runtime and the
// versions that loaded). A child app that does not build stops the run; an example's own app that does not build
// is that example's output.
static Child BuildChild(string scratch, Config config, List<Example> examples, string[] dependencies)
{
    var root = Path.Combine(scratch, "children", Slug(config.Name));
    if (Directory.Exists(root)) Directory.Delete(root, true);
    var lines = (config.Lines ?? []).Select(l => l.StartsWith("#:") ? l : "#:package " + l).ToList();
    var runs = new Dictionary<string, (string[]? Command, string? Error)>();
    var batches = new List<(List<Example> Members, HashSet<string> Types)> { ([], []) };
    foreach (var example in examples)
    {
        var code = example.Code.Replace("\r\n", "\n");
        if (code.Split('\n').Any(l => l.StartsWith("#:")))
        {
            // The page's own file-based app, as written, with the config's lines it does not set itself.
            var named = code.Split('\n').Where(l => l.StartsWith("#:")).Select(l => l.Split('@', '=')[0]).ToHashSet();
            var head = new[] { "#:property TargetFramework=" + config.Framework, "#:property LangVersion=latest" }.Concat(lines)
                .Where(l => !named.Contains(l.Split('@', '=')[0])).Append("#:include harness.cs");
            runs.Add(example.Label, BuildApp(Path.Combine(root, Slug(example.Label)), "app", string.Join("\n", head) + "\n" + code, false));
            continue;
        }

        var types = TypeNames(code);
        var batch = batches.FindIndex(b => !b.Types.Overlaps(types));
        if (batch < 0) batches.Add(([], []));
        batches[batch < 0 ? batches.Count - 1 : batch].Members.Add(example);
        batches[batch < 0 ? batches.Count - 1 : batch].Types.UnionWith(types);
    }

    string[] main = lines.Any(l => l.StartsWith("#:package UniverseGenerator@", StringComparison.OrdinalIgnoreCase)) ? [] : ["#:package UniverseGenerator@1.0.0"];
    for (var b = 0; b < batches.Count; b++)
    {
        var dir = Path.Combine(root, "b" + b);
        Directory.CreateDirectory(dir);
        var cases = new StringBuilder();
        var includes = new StringBuilder();
        for (var i = 0; i < batches[b].Members.Count; i++)
        {
            var (usings, statements, types) = Split(batches[b].Members[i].Code);
            var scoped = Regex.Match(types, @"^namespace ([\w.]+);[ \t]*$", RegexOptions.Multiline);   // a block, after the wrapper
            types = scoped.Success ? types[..scoped.Index] + "namespace " + scoped.Groups[1].Value + " {" + types[(scoped.Index + scoped.Length)..] + "\n}" : types;
            File.WriteAllText(Path.Combine(dir, $"ex{i}.cs"), $"// example: {batches[b].Members[i].Label}\n{usings}\nstatic class WikiCase{i}\n{{\npublic static async System.Threading.Tasks.Task Run(string[] args)\n{{\n{statements}\n}}\n}}\n{types}\n");
            includes.Append($"#:include ex{i}.cs\n");
            cases.Append($"    case \"{i}\": await WikiCase{i}.Run(args); break;\n");
        }

        var source = string.Join("\n", new[] { "#:property TargetFramework=" + config.Framework, "#:property PublishAot=false", "#:property LangVersion=latest" }
            .Concat(main).Concat(lines).Append("#:include harness.cs")) + "\n" + includes + """
            using System;
            using System.Reflection;
            using System.Runtime.InteropServices;
            using System.Runtime.Versioning;

            switch (args.Length > 0 ? args[0] : "")
            {
                case "installed":
                    Console.WriteLine(RuntimeInformation.FrameworkDescription);
                    Console.WriteLine(Installed(typeof(global::UniverseGeneration.Galaxy).Assembly));
                    foreach (var name in new string[] { %DEPENDENCIES% })
                    {
                        try { Console.WriteLine(Installed(Assembly.Load(name))); }
                        catch (Exception e) { Console.WriteLine(name + ": " + e.GetType().Name); }
                    }

                    break;
            %CASES%    default:
                    Console.WriteLine("no example " + string.Join(" ", args));
                    break;
            }

            static string Installed(Assembly assembly) => assembly.GetName().Name + " "
                + assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion?.Split('+')[0]
                + " (file " + assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version
                + ", lib " + (assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "without a TargetFrameworkAttribute") + ")";

            """.Replace("%DEPENDENCIES%", string.Join(", ", dependencies.Select(d => '"' + d + '"'))).Replace("%CASES%", cases.ToString());
        var app = BuildApp(dir, "child", source, true);
        foreach (var (label, key) in batches[b].Members.Select((e, i) => (e.Label, i.ToString())).Prepend(("installed", "installed")).Skip(b == 0 ? 0 : 1))
        {
            runs.Add(label, (app.Command!.Append(key).ToArray(), null));
        }
    }

    return new Child(config.Name, runs);
}

// Writes harness.cs and <name>.cs into dir and builds them; returns the command that runs the app, or the build's
// errors. harness.cs runs first in every child (a module initializer) and holds what the examples may call.
static (string[]? Command, string? Error) BuildApp(string dir, string name, string source, bool mustBuild)
{
    Directory.CreateDirectory(dir);
    File.WriteAllText(Path.Combine(dir, name + ".cs"), source.Replace("\r\n", "\n"));
    File.WriteAllText(Path.Combine(dir, "harness.cs"), """
        global using static WikiHarness;
        using System;
        using System.Net;
        using System.Net.Http;
        using System.Text;
        using System.Threading.Tasks;

        static class WikiHarness
        {
            [System.Runtime.CompilerServices.ModuleInitializer]
            internal static void Start()
            {
                Console.OutputEncoding = Encoding.UTF8;

        """
        + """
            }

        """
        + """
        }

        #if NETFRAMEWORK
        namespace System.Runtime.CompilerServices
        {
            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            sealed class ModuleInitializerAttribute : Attribute { }
        }
        #endif

        """);
    var build = Run("dotnet", ["build", name + ".cs", "-o", "bin", "-nologo", "-v:q"], WithoutProxy(), dir);
    var framework = File.ReadAllText(Path.Combine(dir, name + ".cs")).Contains("TargetFramework=net4");
    var output = Path.Combine(dir, "bin", name + (framework ? ".exe" : ".dll"));
    string[] command = framework ? [output] : ["dotnet", output];
    if (File.Exists(output)) return (command, null);
    if (mustBuild) throw new Exception($"the child app in {dir} did not build:\n{build}");
    return (null, "build failed:\n" + string.Join("\n", build.Split('\n').Where(l => l.Contains(": error "))
        .Select(l => Regex.Replace(l.Trim(),@" \[[^\]]*\]$", "")).Distinct()));
}

// Runs one example (or "installed") of a child in its own process.
static string RunExample(Child child, string label, Dictionary<string, string?>? env, string cwd) =>
    !child.Runs.TryGetValue(label, out var run) ? "no example labelled " + label
    : run.Command is null ? run.Error! : Run(run.Command[0], run.Command[1..], env, cwd);

// The environment children build in: no proxy variables, so the restore reaches nuget.org (L-137).
static Dictionary<string, string?> WithoutProxy()
{
    var env = new Dictionary<string, string?> { ["DOTNET_NOLOGO"] = "1", ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1" };
    foreach (var name in new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "NO_PROXY" })
    {
        env[name] = env[name.ToLowerInvariant()] = null;
    }

    return env;
}

// An example's text in three parts: its using lines; its statements; the types it declares, from the first line
// at the margin that starts a type or a namespace (with the attribute and comment lines just above it).
static (string Usings, string Statements, string Types) Split(string code)
{
    var lines = code.Replace("\r\n", "\n").Split('\n');
    var at = 0;
    while (at < lines.Length && (lines[at].Trim().Length == 0 || lines[at].StartsWith("//")
        || Regex.IsMatch(lines[at], @"^(global )?using (static )?[A-Za-z_][\w.]*( = [\w.<>, ]+)?;\s*$")))
    {
        at++;
    }

    var start = Array.FindIndex(lines, at, l => Regex.IsMatch(l, @"^(\[[^\]]*\]\s*)*((public|internal|file|sealed|static|abstract|partial|readonly|ref|unsafe)\s+)*(class|interface|struct|enum|record|delegate|namespace)\b"));
    start = start < 0 ? lines.Length : start;
    while (start > at && (lines[start - 1].StartsWith("[") || lines[start - 1].StartsWith("//")))
    {
        start--;
    }

    return (string.Join("\n", lines[..at]), string.Join("\n", lines[at..start]), string.Join("\n", lines[start..]));
}

// The names of the types an example declares at the margin: two examples that share one go to separate apps.
static HashSet<string> TypeNames(string code) => Regex.Matches(Split(code).Types,
        @"^(?:\[[^\]]*\]\s*)*(?:(?:public|internal|file|sealed|static|abstract|partial|readonly|ref|unsafe)\s+)*(?:record\s+(?:class\s+|struct\s+)?|class\s+|interface\s+|struct\s+|enum\s+|delegate\s+[^\s(]+\s+)([A-Za-z_]\w*)",
        RegexOptions.Multiline).Select(m => m.Groups[1].Value).ToHashSet();

// A config: a target framework and extra #: lines ("Autofac@9.3.4" is a #:package line).
record Config(string Framework, string[]? Lines = null)
{
    public string Name => Lines is { Length: > 0 } ? Framework + " with " + string.Join(", ", Lines) : Framework;
}

// A page's example as a whole program; Harness marks one the harness runs itself (the gate), not a case.
record Example(string Label, string Code, bool Harness = false);

// A config's built apps: the command that runs each example by label, or why its app did not build.
record Child(string Name, Dictionary<string, (string[]? Command, string? Error)> Runs);

// The texts Mask() replaces, with their tokens; Patterns are regular expressions.
static class Masks
{
    public static readonly List<(string Text, string Token)> Pairs = [];
    public static readonly List<(string Pattern, string Token)> Patterns = [];
}
