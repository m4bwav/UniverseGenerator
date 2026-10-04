---
title: "OpenUPM submission: values for Mark's form"
kind: note
status: active
date: 2026-10-04
stale_after: 2027-01-04
tags: [universegenerator, stage-5, openupm, unity, submission]
summary: "read when submitting the package to OpenUPM or checking its build: the exact form values, which tags will build, what OpenUPM checks, the LICENSE gap in the tarball, and how users install afterwards"
---

# OpenUPM submission

Researched on 2026-10-04 from OpenUPM's docs and source: openupm.com/docs/adding-upm-package.html (page source changed 2026-06-19), troubleshooting-build-errors.html, getting-started.html and getting-started-cli.html (2026-05), the form `PackageAddLayout.vue` in openupm/openupm-next, the data validator, `.mergify.yml` and `data/topics.yml` in openupm/openupm, and `findPackage.js` in openupm/openupm-pipelines. Unity's package naming rules come from docs.unity3d.com/Manual/cus-naming.html (6000.6). **Mark submits; the agent never does.**

## What Mark does

1. Open https://openupm.com/packages/add/ while signed in to GitHub. The form forks openupm/openupm and opens a pull request that adds `data/packages/com.m4bwav.universe-generator.yml`.
2. Fill in:

| Field | Value |
|---|---|
| Repository | `m4bwav/UniverseGenerator` |
| Branch | `master` |
| package.json path | `Packages/com.m4bwav.universe-generator/package.json` (pick it from the dropdown) |
| License | MIT (the YAML needs the exact SPDX name, "MIT License") |
| Hunter | `m4bwav` |
| Tracking mode | `git` (tags) |
| Topics | Procedural Generation (`procedural-generation`), only that one: OpenUPM treats unrelated topics as spam |
| README | `master:README.md` |
| Min version | `1.0.0` (recommended, so `v1.0.0-beta.1` is not published; otherwise it builds harmlessly under a prerelease dist-tag) |
| Cover image URL | the raw GitHub URL of `docs/images/openupm-cover.png` pinned to its commit, https://raw.githubusercontent.com/m4bwav/UniverseGenerator/6e87ad4586ce6e32986bb3619d2cc25e3e8656db/docs/images/openupm-cover.png ([the cover note](2026-10-04-openupm-cover-image.md)) |
| Git tag prefix, tag ignore | empty |

   The name, display name and description come from package.json.
3. A first-time contributor's pull request waits for a moderator to approve its CI run, usually within 24 hours. After that, Mergify merges it automatically, because the pull request adds only one data file, the scope is not a big vendor's, and the data validation passes.
4. The build takes 15 to 30 minutes. The package page then appears at https://openupm.com/packages/com.m4bwav.universe-generator/. The next session checks the page and the build log.

## Why it should build

- OpenUPM shallow-clones each semver tag and walks the tree for the first `package.json` whose `name` matches, so the subfolder is fine (error E800 when none matches).
- The tag must equal package.json's version (E811). `v1.0.0` holds 1.0.0, and `v1.0.0-beta.1` holds 1.0.0-beta.1.
- package.json parses, has a name and a semver version, and is not private. There are no lifecycle scripts.
- The name meets OpenUPM's rules (three or more parts, lowercase) and Unity's (`<tld>.<org>`, no "unity" in it, under 50 characters). OpenUPM does not check domain ownership, and `com.m4bwav` is not on its blocked-scopes list.
- Policy: open-source licence; a release within three months (1.0.0, 2026-10-03); not a test package. A package that is also on NuGet must use its own scope, which `com.m4bwav` is.
- Size limit 512 MB; the package folder is about 0.6 MB. OpenUPM does not check `.meta` files, `Samples~` or the `unity` field.

## The LICENSE gap

The build runs `npm pack` inside the package folder, so the repository-root LICENSE is **not** in the published tarball. The folder has no LICENSE, README or CHANGELOG; package.json's `licensesUrl` points at GitHub. That is not an OpenUPM rule, but MIT asks for the notice to travel with copies. A pull request adds `LICENSE.md` (with its `.meta`) to the package folder, and it ships with the next release. Submitting now is fine: 1.0.0 builds without it.

## How users install, once listed

- CLI: `npm install -g openupm-cli`, then `openupm add com.m4bwav.universe-generator`.
- By hand, in `Packages/manifest.json`: a scoped registry named `package.openupm.com`, with url `https://package.openupm.com` and scopes `["com.m4bwav.universe-generator"]`, plus `"com.m4bwav.universe-generator": "1.0.0"` in `dependencies`.

The README's Unity line (PR #45) and the wiki's Unity page (Stage 7) switch to OpenUPM once the page is live.

Related: builds on [Stage 5 Unity checks](2026-10-03-stage-5-unity-checks.md) and [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (Stage 5).
