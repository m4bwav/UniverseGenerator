#!/usr/bin/env python3
"""Write a throwaway Unity 6 project that checks the package in Unity (tests/Unity/README.md).

Kinds:
  tests  the package from a git tag (or --local), the repository's NUnit tests compiled in Unity as the assembly
         UniverseGenerator.Tests (so InternalsVisibleTo applies), the golden files as Resources, and a runner that
         repeats every golden check in players (IL2CPP).
  size   a near-empty project for the WebGL size delta; --with-package adds the package and one call to
         Galaxy.Generate, without it the project has no package at all.

The project lives outside the repository and is never committed. Standard library only.
"""

import argparse
import json
import os
import re
import shutil
import subprocess
import sys

REPO = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
HERE = os.path.join(REPO, "tests", "Unity")
PACKAGE = "com.m4bwav.universe-generator"
GIT_URL = "https://github.com/m4bwav/UniverseGenerator.git?path=Packages/" + PACKAGE
TESTS = os.path.join(REPO, "tests", "UniverseGenerator.Tests")
SKIPPED = {
    "Repo.cs",  # replaced by Repo.Unity.cs
    "RepoRulesTests.cs",  # repository rules, not runtime behaviour; .NET CI covers them (and C# 11 raw strings)
}
# The repository's tests use NUnit 4; Unity ships a custom NUnit 3.5. Each rewrite keeps the assertion's meaning.
NUNIT35_REWRITES = [
    ("TestContext.Out.WriteLine(", "UGLog.WriteLine("),  # no test context in players
    (".Using<Lane>((a, b) => a == b)", ".Using<Lane>((a, b) => a == b ? 0 : 1)"),  # 3.5 has Comparison<T>, not Func<T, T, bool>
    ("Does.Not.Contain(g.Shape)", "Has.No.Member(g.Shape)"),  # 3.5's Does.Contain takes a substring only
]
EXTRA_SOURCES = [
    "samples/ConsoleSample/Examples.cs",
    "Packages/" + PACKAGE + "/Samples~/GalaxyPrinter/GalaxyReport.cs",
]


def unity_exe(version, override):
    if override:
        return override
    if sys.platform == "win32":
        return rf"C:\Program Files\Unity\Hub\Editor\{version}\Editor\Unity.exe"
    if sys.platform == "darwin":
        return f"/Applications/Unity/Hub/Editor/{version}/Unity.app/Contents/MacOS/Unity"
    return os.path.expanduser(f"~/Unity/Hub/Editor/{version}/Editor/Unity")


def builtin_version(exe, name):
    """The version of a package the Editor ships (com.unity.test-framework), read from its BuiltInPackages folder."""
    base = os.path.dirname(exe)
    for root in (os.path.join(base, "Data"), os.path.join(os.path.dirname(base), "Resources")):
        manifest = os.path.join(root, "Resources", "PackageManager", "BuiltInPackages", name, "package.json")
        if not os.path.exists(manifest):
            manifest = os.path.join(root, "PackageManager", "BuiltInPackages", name, "package.json")
        if os.path.exists(manifest):
            with open(manifest, encoding="utf-8") as f:
                return json.load(f)["version"]
    sys.exit(f"cannot find the built-in {name} next to {exe}")


def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def copy_lf(src, dst, transform=None):
    with open(src, encoding="utf-8") as f:
        text = f.read().replace("\r\n", "\n")
    if transform:
        text = transform(text)
    write(dst, text)


def create(exe, project):
    if os.path.exists(os.path.join(project, "Packages", "manifest.json")):
        return
    log = project + ".create.log"
    print(f"creating {project} (log {log})")
    subprocess.run([exe, "-batchmode", "-quit", "-nographics", "-createProject", project, "-logFile", log], check=True)


def set_dependencies(project, add, remove=()):
    path = os.path.join(project, "Packages", "manifest.json")
    with open(path, encoding="utf-8") as f:
        manifest = json.load(f)
    deps = manifest["dependencies"]
    for name in remove:
        deps.pop(name, None)
    deps.update(add)
    manifest["dependencies"] = dict(sorted(deps.items()))
    write(path, json.dumps(manifest, indent=2) + "\n")


def package_source(args):
    if args.local:
        return "file:" + os.path.join(REPO, "Packages", PACKAGE).replace("\\", "/")
    return f"{GIT_URL}#{args.ref}"


def golden_methods():
    """(class, method, setup) for every test method that calls Repo.AssertGolden; setup is a [OneTimeSetUp] method."""
    found = []
    for name in sorted(os.listdir(TESTS)):
        if not name.endswith(".cs") or name in SKIPPED:
            continue
        with open(os.path.join(TESTS, name), encoding="utf-8") as f:
            text = f.read()
        cls = re.search(r"\bclass (\w+)", text)
        setup = re.search(r"\[OneTimeSetUp\]\s*public void (\w+)\(\)", text)
        heads = list(re.finditer(r"public (?:static )?void (\w+)\(\)", text))
        for i, head in enumerate(heads):
            body = text[head.end(): heads[i + 1].start() if i + 1 < len(heads) else len(text)]
            if "Repo.AssertGolden(" in body:
                found.append((cls.group(1), head.group(1), setup.group(1) if setup else None))
    return found


def runner_source(methods):
    calls = []
    for cls, method, setup in methods:
        pre = f"t.{setup}(); " if setup else ""
        calls.append(f'            Check(r, "{cls}.{method}", () => {{ var t = new {cls}(); {pre}t.{method}(); }});')
    return GENERATED_HEADER + RUNNER.replace("//CALLS", "\n".join(calls)).replace("//COUNT", str(len(methods)))


GENERATED_HEADER = "// Generated by tests/Unity/make_project.py; do not edit.\n"

RUNNER = """using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UniverseGeneration.Tests
{
    /// <summary>Every golden check, called directly, for players where no test runner exists (IL2CPP).</summary>
    public static class GoldenRunner
    {
        private sealed class Results
        {
            public int Pass;
            public int Fail;
        }

        public static int Run()
        {
#if ENABLE_IL2CPP
            const string backend = "IL2CPP";
#else
            const string backend = "Mono";
#endif
            Debug.Log($"UG-GOLDEN START {Application.platform} {backend} {Application.unityVersion} 64-bit {Environment.Is64BitProcess}, //COUNT checks");
            var r = new Results();
//CALLS
            var watch = Stopwatch.StartNew();
            Galaxy.Generate("warm-up");
            watch.Restart();
            for (var i = 0; i < 5; i++)
            {
                Galaxy.Generate("my-seed");
            }

            Debug.Log($"UG-GOLDEN TIMING default galaxy {watch.Elapsed.TotalMilliseconds / 5:0.0} ms (mean of 5, warm)");
            Debug.Log($"UG-GOLDEN DONE pass={r.Pass} fail={r.Fail}");
            return r.Fail;
        }

        private static void Check(Results r, string name, Action check)
        {
            try
            {
                check();
                r.Pass++;
                Debug.Log("UG-GOLDEN PASS " + name);
            }
            catch (Exception e)
            {
                r.Fail++;
                Debug.Log("UG-GOLDEN FAIL " + name + ": " + e.GetType().Name + ": " + e.Message);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RunInPlayers()
        {
            if (Application.isEditor)
            {
                return;
            }

            var failed = Run();
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Application.Quit(failed == 0 ? 0 : 1);
            }
        }
    }
}
"""

SIZE_PROBE = """using UnityEngine;
using UniverseGeneration;

/// <summary>The size check's only use of the package: one default galaxy at start.</summary>
public static class SizeProbe
{
    [RuntimeInitializeOnLoadMethod]
    private static void Go() => Debug.Log("systems " + Galaxy.Generate("my-seed").Systems.Count);
}
"""


def make_tests(args, exe, project):
    create(exe, project)
    set_dependencies(project, {
        PACKAGE: package_source(args),
        "com.unity.test-framework": builtin_version(exe, "com.unity.test-framework"),
    })
    root = os.path.join(project, "Assets", "UGTests")
    if os.path.exists(root):
        shutil.rmtree(root)

    def unity_safe(text):
        for old, new in NUNIT35_REWRITES:
            text = text.replace(old, new)
        # 3.5's Has.Count finds no Count property on the map collections; compare the count itself.
        text = re.sub(r"Assert\.That\(([^;]+?\.Map), Has\.Count\.EqualTo\(", r"Assert.That(\1.Count, Is.EqualTo(", text)
        return "#nullable enable\n" + text

    for name in sorted(os.listdir(TESTS)):
        if name.endswith(".cs") and name not in SKIPPED:
            copy_lf(os.path.join(TESTS, name), os.path.join(root, "Src", name), unity_safe)
    for rel in EXTRA_SOURCES:
        copy_lf(os.path.join(REPO, rel), os.path.join(root, "Src", os.path.basename(rel)), unity_safe)
    copy_lf(os.path.join(HERE, "Assets", "UGTests", "Repo.Unity.cs"), os.path.join(root, "Repo.Unity.cs"))
    repo_literal = REPO.replace('"', '""')
    write(os.path.join(root, "RepoRoot.g.cs"), GENERATED_HEADER +
          "namespace UniverseGeneration.Tests\n{\n    internal static class RepoRoot\n    {\n"
          f"        public const string Value = @\"{repo_literal}\";\n    }}\n}}\n")
    methods = golden_methods()
    write(os.path.join(root, "GoldenRunner.g.cs"), runner_source(methods))
    write(os.path.join(root, "UniverseGenerator.Tests.asmdef"), json.dumps({
        "name": "UniverseGenerator.Tests",
        "rootNamespace": "UniverseGeneration.Tests",
        "references": ["UniverseGenerator"],
        "includePlatforms": [],
        "excludePlatforms": [],
        "allowUnsafeCode": False,
        "overrideReferences": True,
        "precompiledReferences": ["nunit.framework.dll"],
        "autoReferenced": False,
        "defineConstraints": [],
        "versionDefines": [],
        "noEngineReferences": False,
    }, indent=4) + "\n")
    golden = os.path.join(REPO, "tests", "Golden", "v1")
    for name in sorted(os.listdir(golden)):
        dst = os.path.join(root, "Resources", "Golden", "v1", name)
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        shutil.copyfile(os.path.join(golden, name), dst)
    copy_lf(os.path.join(HERE, "Assets", "Editor", "UGBuild.cs"), os.path.join(project, "Assets", "Editor", "UGBuild.cs"))
    print(f"tests project ready: {project}; package {package_source(args)}; {len(methods)} golden checks")


def make_size(args, exe, project):
    create(exe, project)
    probe = os.path.join(project, "Assets", "SizeProbe.cs")
    if args.with_package:
        set_dependencies(project, {PACKAGE: package_source(args)})
        write(probe, SIZE_PROBE)
    else:
        set_dependencies(project, {}, remove=[PACKAGE])
        for path in (probe, probe + ".meta"):
            if os.path.exists(path):
                os.remove(path)
    copy_lf(os.path.join(HERE, "Assets", "Editor", "UGBuild.cs"), os.path.join(project, "Assets", "Editor", "UGBuild.cs"))
    print(f"size project ready: {project}; package {'yes' if args.with_package else 'no'}")


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    p.add_argument("kind", choices=["tests", "size"])
    p.add_argument("project", help="folder for the project, outside the repository")
    p.add_argument("--editor", default="6000.6.4f1", help="Unity editor version (default 6000.6.4f1)")
    p.add_argument("--unity", help="path to the Unity executable, when it is not in the Hub's default folder")
    p.add_argument("--ref", default="v1.0.0", help="git tag or commit of the package (default v1.0.0)")
    p.add_argument("--local", action="store_true", help="use this working tree's package folder instead of git")
    p.add_argument("--with-package", action="store_true", help="size kind: add the package and its probe")
    args = p.parse_args()
    project = os.path.abspath(args.project)
    if os.path.commonpath([project, REPO]) == REPO:
        sys.exit("the project must live outside the repository")
    exe = unity_exe(args.editor, args.unity)
    if not os.path.exists(exe):
        sys.exit(f"no Unity at {exe}; pass --unity")
    (make_tests if args.kind == "tests" else make_size)(args, exe, project)


if __name__ == "__main__":
    main()
