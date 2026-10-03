#!/usr/bin/env python3
"""Give every file and folder in the Unity package a .meta file, and check the ones that exist.

Unity identifies assets by the GUID in their .meta; a GUID must never change once committed (AGENTS.md), so this
script only ever adds missing .meta files and never rewrites one. Run it after adding a file under Packages/.

    python scripts/add-meta.py          # add missing .meta files, then check
    python scripts/add-meta.py --check  # only check: exit 1 on a missing, orphaned or duplicate .meta
"""
import pathlib
import re
import sys
import uuid

ROOT = pathlib.Path(__file__).resolve().parent.parent
PACKAGE = ROOT / "Packages" / "com.m4bwav.universe-generator"

TAIL = "  userData: \n  assetBundleName: \n  assetBundleVariant: \n"


def meta_text(path: pathlib.Path, guid: str) -> str:
    head = f"fileFormatVersion: 2\nguid: {guid}\n"
    if path.is_dir():
        return head + "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n" + TAIL
    suffix = path.suffix.lower()
    if suffix == ".cs":
        return (head + "MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n"
                "  executionOrder: 0\n  icon: {instanceID: 0}\n" + TAIL)
    if suffix == ".asmdef":
        return head + "AssemblyDefinitionImporter:\n  externalObjects: {}\n" + TAIL
    if path.name == "package.json":
        return head + "PackageManifestImporter:\n  externalObjects: {}\n" + TAIL
    if suffix in (".md", ".txt", ".json", ".bytes"):
        return head + "TextScriptImporter:\n  externalObjects: {}\n" + TAIL
    return head + "DefaultImporter:\n  externalObjects: {}\n" + TAIL


def assets():
    for path in sorted(PACKAGE.rglob("*")):
        rel = path.relative_to(PACKAGE)
        # Unity skips names ending in ~ and hidden names, with everything below them.
        if any(part.endswith("~") or part.startswith(".") for part in rel.parts):
            continue
        if path.suffix == ".meta":
            continue
        yield path


def main() -> int:
    check_only = "--check" in sys.argv[1:]
    added = 0
    if not check_only:
        for path in assets():
            meta = path.with_name(path.name + ".meta")
            if not meta.exists():
                meta.write_bytes(meta_text(path, uuid.uuid4().hex).encode())
                added += 1
                print(f"added {meta.relative_to(ROOT).as_posix()}")

    problems = []
    guids = {}
    for path in assets():
        meta = path.with_name(path.name + ".meta")
        if not meta.exists():
            problems.append(f"missing {meta.relative_to(ROOT).as_posix()}")
            continue
        match = re.search(r"^guid: ([0-9a-f]{32})$", meta.read_text(encoding="utf-8"), re.M)
        if not match:
            problems.append(f"no guid in {meta.relative_to(ROOT).as_posix()}")
            continue
        if match.group(1) in guids:
            problems.append(f"duplicate guid in {meta.relative_to(ROOT).as_posix()} and {guids[match.group(1)]}")
        guids[match.group(1)] = meta.relative_to(ROOT).as_posix()
    for meta in PACKAGE.rglob("*.meta"):
        if not meta.with_name(meta.name[:-5]).exists():
            problems.append(f"orphaned {meta.relative_to(ROOT).as_posix()}")

    for p in problems:
        print(p)
    print(f"{len(guids)} assets with .meta files; {added} added; {len(problems)} problems")
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())
