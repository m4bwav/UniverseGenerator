using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Batchmode entry points for the scratch projects that make_project.py writes (tests/Unity/README.md):
/// importing and running the package sample, and the player builds for the golden check and the size check.
/// Arguments come after the method: -ugOut &lt;folder&gt;.
/// </summary>
public static class UGBuild
{
    private const string Package = "com.m4bwav.universe-generator";
    private const string Scene = "Assets/Scenes/Main.unity";

    /// <summary>Imports the Galaxy printer sample into Assets/Samples (it compiles on the next start).</summary>
    public static void ImportSample()
    {
        var samples = Sample.FindByPackage(Package, null).ToList();
        Debug.Log($"UG-SAMPLE found {samples.Count} sample(s): {string.Join(", ", samples.Select(s => s.displayName))}");
        foreach (var sample in samples)
        {
            if (!sample.Import(Sample.ImportOptions.OverridePreviousImports))
            {
                Fail($"could not import the sample {sample.displayName}");
            }
        }
    }

    /// <summary>Runs the imported sample's report for my-seed and logs it, as GalaxyPrinter does in Play mode.</summary>
    public static void RunSample()
    {
        var printer = FindType("GalaxyPrinter");
        var report = FindType("UniverseGeneration.Samples.GalaxyReport");
        if (printer == null || report == null || !typeof(MonoBehaviour).IsAssignableFrom(printer))
        {
            Fail("the sample's GalaxyPrinter or GalaxyReport did not compile");
        }

        var lines = ((System.Collections.Generic.IEnumerable<string>)report.GetMethod("Lines").Invoke(null, new object[] { "my-seed", 60 })).ToList();
        Debug.Log($"UG-SAMPLE {lines.Count} lines");
        foreach (var line in lines.Take(3))
        {
            Debug.Log("UG-SAMPLE " + line);
        }
    }

    /// <summary>A WebGL player (always IL2CPP) that runs the golden checks at start and logs UG-GOLDEN lines to the browser console.</summary>
    public static void WebGLGolden()
    {
        var target = NamedBuildTarget.WebGL;
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Low);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithoutStacktrace;
        Build(BuildTarget.WebGL, "golden");
    }

    /// <summary>The release-like WebGL player for the size budget: Brotli, stripping High, IL2CPP optimised for size.</summary>
    public static void WebGLSize()
    {
        var target = NamedBuildTarget.WebGL;
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.High);
        PlayerSettings.SetIl2CppCodeGeneration(target, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        Build(BuildTarget.WebGL, "size");
    }

    /// <summary>A Windows x64 IL2CPP player that runs the golden checks and quits (needs the Windows IL2CPP module and a C++ toolchain).</summary>
    public static void WindowsIL2CPP()
    {
        var target = NamedBuildTarget.Standalone;
        PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(target, ManagedStrippingLevel.Low);
        Build(BuildTarget.StandaloneWindows64, "golden");
    }

    private static void Build(BuildTarget target, string kind)
    {
        var output = Arg("-ugOut") ?? Fail("missing -ugOut <folder>");
        EnsureScene();
        var options = new BuildPlayerOptions
        {
            scenes = new[] { Scene },
            locationPathName = target == BuildTarget.StandaloneWindows64 ? System.IO.Path.Combine(output, "UGGolden.exe") : output,
            target = target,
            options = kind == "golden" ? BuildOptions.IncludeTestAssemblies : BuildOptions.None, // NUnit ships only with test builds
        };
        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;
        Debug.Log($"UG-BUILD {kind} {target} {summary.result} {summary.totalSize} bytes, {summary.totalErrors} errors, {summary.totalWarnings} warnings, {summary.totalTime.TotalSeconds:0} s");
        if (summary.result != BuildResult.Succeeded)
        {
            Fail("the build failed");
        }
    }

    private static void EnsureScene()
    {
        if (System.IO.File.Exists(Scene))
        {
            return;
        }

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, Scene);
    }

    private static Type FindType(string fullName) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).FirstOrDefault(t => t != null);

    private static string Arg(string name)
    {
        var args = Environment.GetCommandLineArgs();
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }

    private static string Fail(string message)
    {
        Debug.LogError("UG-FAIL " + message);
        EditorApplication.Exit(1);
        throw new InvalidOperationException(message);
    }
}
