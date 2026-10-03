---
title: "History scan before going public (N8, 2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2026-11-03
tags: [universegenerator, stage-4, security, history-scan, secrets, gitleaks, public]
summary: "read before making the repository public, or to re-run the scan: no secrets in any commit (gitleaks 8.30.1 and a regex pass), local paths and one second author address in old commits judged harmless (already public elsewhere, no rewrite), current tree clean; how to re-run it"
---

# History scan before going public

Plan item N8 and AGENTS.md ("Before this repository goes public, scan the whole history"). Run on 2026-10-03 on branch `stage4-prep` (off `master` at 73d07ec): every ref, local and remote (`master`, `stage3-core`, `origin/docs/stop-2-ruled`), 32 commits.

## Summary

**No secret anywhere in the history, and the current tree is clean.** Nothing needs rotating. The history keeps a few local paths and a second author address of Mark's; each is already public in other m4bwav repositories, so going public exposes nothing new and no history rewrite is needed (history was not rewritten and nothing was force-pushed). Mark makes the final call in the [Stage 4 checklist](2026-10-03-stage-4-checklist.md).

## How it ran

| Tool | What it read | Result |
|---|---|---|
| gitleaks 8.30.1 (official Windows x64 release from GitHub, sha256 checked against the release's checksums file, run from the session's scratch folder; nothing installed system-wide) | `gitleaks git --log-opts=--all --redact`: 30 commits with their own diffs (the two merge commits add none) | no leaks |
| gitleaks 8.30.1 | `gitleaks dir --redact .`: the working tree | no leaks |
| `scripts/history-scan.py` (stdlib Python, committed with this note) | the patch of every commit on every ref, merges against each parent (`git log -p --all -m`); every commit's author, committer and message; the tracked files at HEAD | history: 5 findings below; current tree: 0 |

The regex pass looks for local paths (user profile, the maintainer's drive, the runner folder), the vault path, LAN addresses, internal hostnames, email addresses other than the public author address on the commits, secret-shaped assignments, private key blocks, known token prefixes (GitHub, npm, nuget.org, AWS, OpenAI-style, Slack, Google), connection strings, and a list of private names kept in the private sidecar (`--names-file`, never committed). Its patterns were probed with a right and a wrong example of each kind before the run.

## Findings in history (each kind, commit and file; values are not repeated here)

| Kind | Commits | File | Status |
|---|---|---|---|
| Local path: the maintainer's projects drive | 9586758, cbc409b, 0961fa8, 32df421 (merge), aed392d, 73d07ec (merge) | `ai-docs/next-session-prompt.md` | Fixed in the current tree on this branch; the same kind of path is already public in DotNetRandomNameGenerator's and DotNetJsonPrettyPrinter's docs |
| Local path: the maintainer's user profile | aed392d, 73d07ec (merge) | `ai-docs/next-session-prompt.md` | Fixed in the current tree; the user name is the public GitHub handle |
| Vault path: the package-modernize overlay folder | aed392d, 73d07ec (merge) | `ai-docs/next-session-prompt.md` | Fixed in the current tree; names a folder, holds nothing |
| Local path: the self-hosted runner's folder | 60d8809, 52ec910, 32df421 (merge), aed392d, 73d07ec (merge) | `ai-docs/HANDOFF.md`, `ai-docs/next-session-prompt.md` | Fixed in the current tree; the folder is now in the private sidecar |
| Email address other than the public author one (Mark's own second address) | 73d07ec (author of the GitHub web merge of PR #2) | commit metadata | Stays: GitHub's web merge used the account's primary address. The same address is already in the commit metadata of public m4bwav/everlast, package-modernize, wikiwright and evergreen-protocol. Future web merges add it again unless Mark turns on "Keep my email addresses private" in GitHub's email settings (checklist) |

## Checked and not a finding

- **Secrets of any kind**: none in any commit, message or file (both tools).
- **People other than Mark**: none. The GitHub handles in `kb/sources/` and `kb/features/feature-matrix.md` (authors of public libraries and asset packs the research cites) and in `ai-docs/notes/2026-10-02-galaxy-generator-library-or-web-tool.md` are public project credits, not private names.
- **LAN addresses and internal hosts**: none.
- **The game's name** (SpaceDeckBuilder2, a private repository) appears in AGENTS.md, CHANGELOG, `kb/` and `ai-docs/`, with one editor-script path from the game in the Stage 0 survey note. That is intentional: the plan and the CHANGELOG say openly that the generator was extracted from the game, and none of it names a person, host or credential. The game's own history was never copied (plan, "Security").
- **`tests/Golden/legacy/`**: the game's generator output, numbers only.

## Re-run before going public

Commits land between this scan and the switch to public, so run both again on the final `master`:

```
gitleaks git --log-opts=--all --redact .
python scripts/history-scan.py --names-file <the names file in the private sidecar>
```

Expect no gitleaks finding, `current: 0 distinct finding(s)` and exit 0, and in history only the findings above plus the lines this branch removed (they show as removals in its commits).

## Re-run on the final master (2026-10-03)

On `master` at 272b0e0 (the merge of `stage4-prep`), 37 commits on every ref, before the repository went public:

| Tool | Result |
|---|---|
| gitleaks 8.30.1 (still the latest release; official Windows x64 zip, sha256 checked, run from the session's scratch folder), `git --log-opts=--all --redact` | 34 commits with their own diffs, no leaks |
| gitleaks 8.30.1, `dir --redact .` | no leaks |
| `scripts/history-scan.py --names-file` (the private list) | history: the same five kinds as the table above, with two more commits each: 9586a58 and the merge 272b0e0, which hold the removals of those lines; current: 0, exit 0 |

**Nothing new.** The first run of the re-run also reported a connection string and a second vault path, both in `scripts/history-scan.py` itself: its own patterns (the Azure storage connection string prefix and the package-modernize overlay folder) as they were added in 9586a58. The script already skipped its own file in the current tree; it now skips it in history too, and the re-run gives the result above. No private name, LAN address or other email address appears; the merge of `stage4-prep` carries the public author address. Clean, so step 2 of the [Stage 4 checklist](2026-10-03-stage-4-checklist.md) can go on.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (N8, Stage 4); see also [Stage 4 checklist](2026-10-03-stage-4-checklist.md), [Stop 2 questions](2026-10-03-stop-2-questions.md).
