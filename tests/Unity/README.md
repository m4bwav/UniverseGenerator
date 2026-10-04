# Unity checks

The seed promise covers Unity Mono and IL2CPP (AGENTS.md). These files build a throwaway Unity 6 project outside the repository and check the package there. Nothing in the project is committed. Results so far: `ai-docs/notes/2026-10-03-stage-5-unity-checks.md`.

- `make_project.py`: writes the project (standard-library Python).
- `Assets/UGTests/Repo.Unity.cs`: Unity's stand-in for the tests' `Repo` class. It reads the golden files from Resources and never writes them.
- `Assets/Editor/UGBuild.cs`: batchmode entry points (sample import and run, the WebGL golden and size players, a Windows IL2CPP player).

## The tests project

```
python tests/Unity/make_project.py tests <project-dir> --editor 6000.6.4f1   # package from git, tag v1.0.0
python tests/Unity/make_project.py tests <project-dir> --local               # this working tree's package
```

It installs the package by git URL at the tag, as users will. It also copies `tests/UniverseGenerator.Tests/*.cs` (without `RepoRulesTests.cs`) into an assembly named `UniverseGenerator.Tests`, so the runtime's `InternalsVisibleTo` applies. The golden files go in as Resources, and a generated `GoldenRunner` calls every test method that checks a golden file. The repository's tests use NUnit 4, and Unity ships a custom NUnit 3.5, so a short table of rewrites in `make_project.py` keeps each assertion's meaning. Add to it when a new test uses a newer NUnit API.

Then, with `Unity` the editor executable:

```
Unity -batchmode -quit -nographics -projectPath <dir> -executeMethod UGBuild.ImportSample -logFile import.log
Unity -batchmode -quit -nographics -projectPath <dir> -executeMethod UGBuild.RunSample -logFile sample.log
Unity -batchmode -nographics -projectPath <dir> -runTests -testPlatform PlayMode -testResults results.xml -logFile tests.log
```

Look for `error CS` and `warning CS` in the first log, `UG-SAMPLE` lines in the second, and `failed="0"` in `results.xml`. The test assembly runs on every platform, so Unity runs it as PlayMode tests. `-testPlatform EditMode` finds no tests.

## Golden checks in IL2CPP

```
Unity -batchmode -quit -nographics -projectPath <dir> -buildTarget WebGL -executeMethod UGBuild.WebGLGolden -ugOut <build-dir> -logFile webgl.log
python -m http.server 8765 --bind 127.0.0.1 --directory <build-dir>
```

Open `http://127.0.0.1:8765/` and read the browser console. Expect `UG-GOLDEN PASS` for every check, then `UG-GOLDEN DONE pass=N fail=0`. WebGL always compiles through IL2CPP, so this needs only the Web Build Support module. Golden builds pass `BuildOptions.IncludeTestAssemblies`, because NUnit goes into a player only with test assemblies.

`UGBuild.WindowsIL2CPP` (with `-buildTarget Win64`) builds a desktop player that runs the same checks and quits with exit code 0 or 1. Run it with `-logFile`. It needs the Windows Build Support (IL2CPP) module and Visual Studio's C++ workload.

## WebGL size delta

```
python tests/Unity/make_project.py size <size-dir> --editor 6000.5.8f1                  # no package
Unity -batchmode -quit -nographics -projectPath <size-dir> -buildTarget WebGL -executeMethod UGBuild.WebGLSize -ugOut <without> -logFile without.log
python tests/Unity/make_project.py size <size-dir> --editor 6000.5.8f1 --with-package   # package and one Galaxy.Generate
Unity -batchmode -quit -nographics -projectPath <size-dir> -buildTarget WebGL -executeMethod UGBuild.WebGLSize -ugOut <with> -logFile with.log
```

Compare the files in the two `Build` folders. The budget row (`ai-docs/notes/2026-10-02-package-size-budget.md`) is the added Brotli size: green under 300 KB, yellow 0.3 to 1 MB, red over 1 MB.
