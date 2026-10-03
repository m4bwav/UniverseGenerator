#!/usr/bin/env python3
"""The history scan (plan N8): every commit on every ref, for what must not go public.

    python scripts/history-scan.py [--names-file FILE] [--json OUT]

Reads the patch of every commit on every ref (merges against each parent), every commit's author, committer and
message, and the tracked files at HEAD. Reports private-looking text by kind: local paths, the vault path, LAN
addresses and internal hosts, email addresses other than the public author one, secret-shaped assignments, private
key blocks, known token prefixes, connection strings, and the names in --names-file (one per line; the list of
private names stays out of the repository, in the maintainer's private notes). Secret-shaped matches are printed as
[redacted], never their value. Run gitleaks as well (`gitleaks git --log-opts=--all --redact .`); this pass covers
what gitleaks does not look for. Exit 1 when the current tree holds a finding, 0 otherwise; history findings are
reported for review, since history is not rewritten (ai-docs/notes/2026-10-03-history-scan.md).
"""
import argparse
import collections
import json
import re
import subprocess
import sys

SELF = "scripts/history-scan.py"  # its own patterns would match themselves
PUBLIC_EMAILS = {"m4bwav@gmail.com", "noreply@github.com"}
SECRET_KINDS = {"secret-shaped assignment", "private key block", "known token prefix", "connection string"}
KINDS = [
    ("local path: user profile", r"(?i)\b[a-z]:[\\/]+users[\\/]+[a-z0-9_.-]+|/c/users/[a-z0-9_.-]+|/home/[a-z0-9_-]+/"),
    ("local path: maintainer's drive", r"(?i)\b[a-z]:[\\/]+m4bwa\b|/[a-z]/m4bwa\b"),
    ("local path: runner folder", r"(?i)actions-runner-[a-z0-9]+"),
    ("vault path", r"(?i)everlast-vault[\\/]|Documents[\\/]+(Everlast|Evergreen)\b|\.package-modernize\b"),
    ("LAN address", r"\b(192\.168\.\d{1,3}\.\d{1,3}|10\.\d{1,3}\.\d{1,3}\.\d{1,3}|172\.(1[6-9]|2\d|3[01])\.\d{1,3}\.\d{1,3})\b"),
    ("internal host", r"(?i)\b[a-z0-9-]+\.(local|lan|internal|corp|home\.arpa)\b"),
    ("email address", r"\b[A-Za-z0-9][A-Za-z0-9._%+-]*@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}"),
    ("secret-shaped assignment",
     r"(?i)\b(api[_-]?key|secret|password|passwd|pwd|token|connection[_-]?string|account[_-]?key|sharedaccesskey)\b"
     r"\s*[:=]\s*['\"]?[A-Za-z0-9+/_\-]{12,}"),
    ("private key block", r"-----BEGIN [A-Z ]*PRIVATE KEY-----"),
    ("known token prefix",
     r"\b(gh[opsu]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|npm_[A-Za-z0-9]{30,}|oy2[a-z0-9]{40,}"
     r"|AKIA[0-9A-Z]{16}|sk-[A-Za-z0-9_-]{20,}|xox[abp]-[A-Za-z0-9-]{10,}|AIza[0-9A-Za-z_-]{30,})"),
    ("connection string",
     r"(?i)(Server|Data Source)=[^;\n]+;[^\n]*(Password|Pwd)=|DefaultEndpointsProtocol=|mongodb(\+srv)?://\S+@"
     r"|postgres(ql)?://\S+@"),
]


def git(repo, *args):
    return subprocess.run(["git", "-C", repo, *args], capture_output=True, check=True).stdout.decode("utf-8", "replace")


def matches(kinds, text):
    for kind, rx in kinds:
        for m in rx.finditer(text):
            value = m.group(0)
            if kind == "email address" and value.lower() in PUBLIC_EMAILS:
                continue
            yield kind, "[redacted]" if kind in SECRET_KINDS else value


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", default=".")
    parser.add_argument("--names-file", help="private names to look for, one per line (kept outside the repository)")
    parser.add_argument("--json", help="also write the findings to this file")
    args = parser.parse_args()

    kinds = [(k, re.compile(p)) for k, p in KINDS]
    if args.names_file:
        with open(args.names_file, encoding="utf-8") as f:
            names = [n.strip() for n in f if n.strip() and not n.startswith("#")]
        if names:
            kinds.append(("private name", re.compile(r"\b(" + "|".join(map(re.escape, names)) + r")\b")))
    else:
        print("note: no --names-file, so private names are not checked")

    history = collections.defaultdict(set)
    commit = path = None
    for line in git(args.repo, "log", "-p", "--all", "-m", "--no-color", "--format=@@COMMIT %h").splitlines():
        if line.startswith("@@COMMIT "):
            commit, path = line.split()[1], None
        elif line.startswith("+++ b/"):
            path = line[6:]
        elif line.startswith(("+", "-")) and not line.startswith(("+++ ", "--- ")) and path != SELF:
            for kind, value in matches(kinds, line):
                history[(kind, value)].add((commit, path))
    for block in git(args.repo, "log", "--all", "--format=@@COMMIT %h%n%an <%ae>%n%cn <%ce>%n%B").split("@@COMMIT ")[1:]:
        commit, _, rest = block.partition("\n")
        for kind, value in matches(kinds, rest):
            history[(kind, value)].add((commit, "(commit metadata)"))

    current = collections.defaultdict(set)
    for path in git(args.repo, "ls-files").splitlines():
        if path == SELF:
            continue
        try:
            with open(f"{args.repo}/{path}", "rb") as f:
                data = f.read()
        except OSError:
            continue
        if b"\0" in data[:8000]:
            continue  # binary
        for kind, value in matches(kinds, data.decode("utf-8", "replace")):
            current[(kind, value)].add(path)

    report = {
        "history": [{"kind": k, "match": v, "where": sorted(w)} for (k, v), w in sorted(history.items())],
        "current": [{"kind": k, "match": v, "where": sorted(w)} for (k, v), w in sorted(current.items())],
    }
    for section in ("history", "current"):
        print(f"{section}: {len(report[section])} distinct finding(s)")
        for item in report[section]:
            where = ", ".join(" ".join(w) if isinstance(w, tuple) else w for w in item["where"])
            print(f"  {item['kind']}: {item['match']} ({where})")
    if args.json:
        with open(args.json, "w", encoding="utf-8", newline="\n") as f:
            json.dump(report, f, indent=1)
    return 1 if report["current"] else 0


if __name__ == "__main__":
    sys.exit(main())
