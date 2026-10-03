#!/usr/bin/env python3
"""The size gate (plan D19, ai-docs/notes/2026-10-02-package-size-budget.md): measure the package against the budget.

    dotnet pack src/UniverseGenerator -c Release -o artifacts
    python scripts/size-gate.py artifacts

Prints every metric with its band. Exit 1 when any metric is red; yellow passes but prints a warning line, which the
pull request and the maintainer must hear about first (AGENTS.md). WebGL size and cold start are measured elsewhere
(Unity build in Stage 5, BenchmarkDotNet).
"""
import io
import pathlib
import sys
import tarfile
import zipfile

ROOT = pathlib.Path(__file__).resolve().parent.parent
PACKAGE = ROOT / "Packages" / "com.m4bwav.universe-generator"
KB = 1024
MB = 1024 * 1024

# metric: (green below, red above); between them is yellow.
BUDGET = {
    "nupkg": (1 * MB, 3 * MB),
    "largest DLL": (750 * KB, int(1.5 * MB)),
    "UPM unpacked": (2 * MB, 5 * MB),
    "UPM compressed": (500 * KB, int(1.5 * MB)),
    "Runtime .cs files": (150, 300),
    "Runtime lines": (25_000, 50_000),
}


def band(metric: str, value: int) -> str:
    green, red = BUDGET[metric]
    return "green" if value < green else "red" if value > red else "yellow"


def show(value: int, metric: str) -> str:
    if "files" in metric or "lines" in metric:
        return f"{value:,}"
    return f"{value / KB:,.1f} KB"


def upm_sizes() -> tuple[int, int]:
    """What OpenUPM packs: the package folder without hidden or ~ entries, as a gzipped tarball."""
    unpacked = 0
    buffer = io.BytesIO()
    with tarfile.open(fileobj=buffer, mode="w:gz", compresslevel=9) as tar:
        for path in sorted(PACKAGE.rglob("*")):
            rel = path.relative_to(PACKAGE)
            if any(p.startswith(".") or p.endswith("~") for p in rel.parts) or not path.is_file():
                continue
            unpacked += path.stat().st_size
            tar.add(path, arcname=f"package/{rel.as_posix()}")
    return unpacked, len(buffer.getvalue())


def main() -> int:
    artifacts = pathlib.Path(sys.argv[1] if len(sys.argv) > 1 else ROOT / "artifacts")
    nupkgs = sorted(p for p in artifacts.glob("UniverseGenerator.*.nupkg") if not p.name.endswith(".snupkg"))
    if not nupkgs:
        print(f"no UniverseGenerator nupkg in {artifacts}; run dotnet pack first")
        return 2
    nupkg = nupkgs[-1]
    with zipfile.ZipFile(nupkg) as z:
        dlls = [i.file_size for i in z.infolist() if i.filename.endswith(".dll")]
    runtime = list((PACKAGE / "Runtime").rglob("*.cs"))
    lines = sum(len(p.read_bytes().splitlines()) for p in runtime)
    unpacked, compressed = upm_sizes()

    values = {
        "nupkg": nupkg.stat().st_size,
        "largest DLL": max(dlls),
        "UPM unpacked": unpacked,
        "UPM compressed": compressed,
        "Runtime .cs files": len(runtime),
        "Runtime lines": lines,
    }
    worst = "green"
    print(f"size gate for {nupkg.name}")
    for metric, value in values.items():
        b = band(metric, value)
        green, red = BUDGET[metric]
        print(f"  {metric:<18} {show(value, metric):>12}  {b:<6} (green under {show(green, metric)}, red over {show(red, metric)})")
        if b == "red" or (b == "yellow" and worst == "green"):
            worst = b
    if worst == "yellow":
        print("WARNING: a metric is yellow; say so first in the pull request and tell the maintainer (AGENTS.md).")
    if worst == "red":
        print("FAILED: a metric is red; split the content into an add-on package or stop (plan D19, D21).")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
