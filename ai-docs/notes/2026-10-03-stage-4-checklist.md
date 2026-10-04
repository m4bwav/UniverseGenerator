---
title: "Stage 4 checklist for Mark: public repository, Trusted Publishing, 1.0.0-beta.1 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2026-11-03
tags: [universegenerator, stage-4, release, nuget, trusted-publishing, checklist, public]
summary: "Mark's steps for Stage 4, in order: merge stage4-prep, re-run the scan, move CI off the self-hosted runner and make the repository public, the nuget.org Trusted Publishing policy (new package allowed), the nuget environment, date the CHANGELOG, tag v1.0.0-beta.1 after ci is green, approve, verify (1.0.0-beta.1 published and verified 2026-10-03, results in step 7); read before any of them or before releasing 1.0.0"
---

# Stage 4 checklist

## Summary

What Mark does to release UniverseGenerator 1.0.0-beta.1 to nuget.org, in order. Steps marked **(agent)** a session can do when Mark says so; everything else needs his hands or his accounts. Checked on 2026-10-03: the package ID `UniverseGenerator` is free on nuget.org (flat container 404, search 0 hits); the repository is private, with no environments and no Actions variables; the nuget.org Trusted Publishing doc (updated 2026-09-01) matches the plan and the package-modernize template, so nothing below differs from what the plan expected.

## 1. Merge the preparation

- [x] Review and merge the `stage4-prep` pull request (the [history scan](2026-10-03-history-scan.md), `release.yml`, `scripts/check-package.sh`), after `ci` is green.

## 2. Make the repository public

- [x] **(agent)** Re-run the scan on the final `master`, as the [scan note](2026-10-03-history-scan.md) says ("Re-run before going public"). Go on only when it is clean. Done 2026-10-03 on 272b0e0: clean, nothing new (scan note, "Re-run on the final master").
- [ ] Decide on the history findings: the scan judged them harmless (local paths and your second commit address, all already public in other m4bwav repositories), so the recommendation is to keep the history as it is. Optional: GitHub, Settings, Emails, "Keep my email addresses private", so later web merges use the noreply address.
- [x] **Before** switching to public, take CI off your PC: a public repository's pull requests from forks could otherwise run code on the self-hosted runner. Done 2026-10-03: `RUNS_ON` was set to `"ubuntu-latest"`, then PR #5 (merged) made `ci.yml`'s build job the hosted Ubuntu 24.04, Windows and macOS matrix with no self-hosted fallback, and the agent deleted the `RUNS_ON` variable (nothing reads it; the repository has no Actions variables now). `ci` on `master` 558c544 (run 37145366186) passed on all three: 170 tests each on net10.0, net48 on Windows, size gate green (nupkg 373.2 KB, DLL 499.0 KB).
- [x] Remove the runner `universe` from the repository (Settings, Actions, Runners) and stop its scheduled task on your PC. Done by Mark 2026-10-03 (the runners list reads back empty; no workflow names `self-hosted`).
- [x] Make it public (done 2026-10-03: the agent ran the command below on Mark's explicit request, after checking the runner was gone; visibility reads back `PUBLIC`): Settings, General, Danger Zone, Change visibility, or `gh repo edit m4bwav/UniverseGenerator --visibility public --accept-visibility-change-consequences`.
- [x] **(agent)** Then the settings the free plan allows only on public repositories: secret scanning and push protection, private vulnerability reporting, workflow permissions read, the `master` ruleset (deletion and non-fast-forward blocked, required check `ci`, admin bypass) and a tag ruleset so only admins create `v*` tags (package-modernize templates `rulesets/`). The wiki (Stage 7) can be switched on too (not done; Stage 7).

Done 2026-10-03, every one accepted by the API on the free plan and read back with `gh api`:

| Setting | Read back |
|---|---|
| Secret scanning | `security_and_analysis.secret_scanning` enabled |
| Push protection | `secret_scanning_push_protection` enabled (non-provider patterns and validity checks stay off; dependabot security updates off) |
| Private vulnerability reporting | `private-vulnerability-reporting` `{"enabled":true}` |
| Workflow permissions | `default_workflow_permissions` read, `can_approve_pull_request_reviews` false |
| Ruleset `master` (id 24430276) | branch, active, `~DEFAULT_BRANCH`; rules deletion, non_fast_forward, required_status_checks [`ci`] (not strict); bypass: repository role 5 (admin), always. `rules/branches/master` lists all three. |
| Ruleset `Tags only by admins` (id 24430278) | tag, active, `~ALL` tags (covers `v*` and is stricter); rules creation, update, deletion; bypass admin, always. From the package-modernize template unchanged. |

The `master` ruleset as sent (the templates have no branch ruleset file):

```json
{"name": "master", "target": "branch", "enforcement": "active",
 "conditions": {"ref_name": {"include": ["~DEFAULT_BRANCH"], "exclude": []}},
 "rules": [{"type": "deletion"}, {"type": "non_fast_forward"},
   {"type": "required_status_checks", "parameters": {"strict_required_status_checks_policy": false,
     "required_status_checks": [{"context": "ci"}]}}],
 "bypass_actors": [{"actor_id": 5, "actor_type": "RepositoryRole", "bypass_mode": "always"}]}
```

`ci` is the summary job in `ci.yml` that needs every matrix leg, so one required check covers Ubuntu, Windows and macOS. The admin bypass means your own merges are never blocked; pull requests still show the check.

## 3. nuget.org Trusted Publishing policy

- [x] nuget.org, your user name, **Trusted Publishing**, add a policy (Mark said it was done, 2026-10-03; an agent cannot read it back, so the first release run is the test):

| Field | Value |
|---|---|
| Policy owner | you (your user, not an organization) |
| Repository Owner | `m4bwav` |
| Repository | `UniverseGenerator` |
| Workflow File | `release.yml` (file name only) |
| Environment | `nuget` |
| Scope | **push new packages and package versions** (the package does not exist yet, so "only new versions" would refuse the first push), glob `UniverseGenerator` |

- A new policy can start "temporarily active" for 7 days, until the first publish binds GitHub's repository and owner ids to it; with no publish in that window it goes inactive, and you can restart the window at any time. So create it shortly before step 6, or restart it then.
- Optional after the first release: narrow the scope to "push only new package versions" with the same glob.

## 4. The `nuget` environment

Needs the repository public (required reviewers are not available on private repositories on the free plan).

- [x] Done 2026-10-03: Mark created the environment, the agent added the reviewer and the tag rule with the commands below, Mark set `NUGET_USER` (the agent's secret write was refused by its permission settings) and turned off "Allow administrators to bypass configured protection rules". Read back: `required_reviewers` m4bwav, `branch_policy` with `v* tag`, secret `NUGET_USER` present, `can_admins_bypass` false. The profile name is the nuget.org owner of Mark's other six packages.
- Settings, Environments, New environment `nuget`: required reviewer **m4bwav**; Deployment branches and tags: "Selected branches and tags", rule `v*` of type tag; environment secret `NUGET_USER` = your nuget.org **profile name** (not your email). Or let an agent run it:

```
gh api -X PUT repos/m4bwav/UniverseGenerator/environments/nuget --input - <<'JSON'
{"reviewers":[{"type":"User","id":156112}],"deployment_branch_policy":{"protected_branches":false,"custom_branch_policies":true}}
JSON
gh api -X POST repos/m4bwav/UniverseGenerator/environments/nuget/deployment-branch-policies -f name='v*' -f type=tag
gh secret set NUGET_USER --env nuget -R m4bwav/UniverseGenerator    # paste the profile name when asked
```

(156112 is the user id of m4bwav, from `gh api users/m4bwav --jq .id`.)

## 5. The release commit

- [x] **(agent)** PR #10 (`release/1.0.0-beta.1`), merged by Mark 2026-10-03 (1776834; `ci` green on that `master` commit, run 37154206419, size gate matching the CHANGELOG; the compressed Unity number wobbles by about 0.1 KB between builds): heading dated 2026-10-03; size numbers checked against the size gate on `master` 04f3a16 (ci run 37151962628): nupkg 373.5 to 373.2 KB and Unity compressed 116.0 to 114.5 KB corrected, the rest unchanged, all green. A small pull request: the CHANGELOG heading `## [1.0.0-beta.1] - <date>` (it says "Unreleased" now; `release.yml` accepts that for a prerelease, but the release ritual dates it) with the size numbers checked again; `<Version>` stays `1.0.0-beta.1` (already set, and `package.json` matches). Merge after `ci` is green, then wait for `ci` green on `master`.

## 6. Tag and approve

- [x] Done 2026-10-03 by the agent on Mark's explicit instruction: `ci` green on `master` c4f4e9e (run 37154681055), tag `v1.0.0-beta.1` pushed on c4f4e9e, release run 37158022980 started.
- Only after `ci` is green on that `master` commit (tag only after green; a tag on a failing commit burns the version):

```
git fetch origin && git tag v1.0.0-beta.1 origin/master && git push origin v1.0.0-beta.1
```

- [x] Run 37158022980: build and test, attest, Windows (net48 and net10.0) all passed, then it waited at `push to nuget.org`. On Mark's explicit instruction the agent sent the approval through the API (`pending_deployments`, state approved). Confirmed by the next session (2026-10-03): every job passed, `push to nuget.org (after approval)` in 10 s (Trusted Publishing accepted; the log shows `Created` and "Your package was pushed." for the nupkg and the snupkg), then `GitHub Release` in 6 s.
- The `release` run checks the tag against `<Version>`, that the commit is on `master` and passed `ci`, builds, tests on Linux and Windows (net48 too), checks the package and the size gate, attests, then waits. Open the run, **Review deployments**, approve `nuget`. Nothing is on nuget.org before that click; after it the push is permanent (unlisting is the only undo).

## 7. Verify **(agent)**

Done 2026-10-03 for 1.0.0-beta.1, about ten minutes after the push; everything matched what this list expects.

- [x] Flat container lists it (usually within 15 minutes): `https://api.nuget.org/v3-flatcontainer/universegenerator/index.json` contains `1.0.0-beta.1`. Read back `{"versions": ["1.0.0-beta.1"]}`.
- [x] Registration says listed (can lag 25 minutes or more): `https://api.nuget.org/v3/registration5-gz-semver2/universegenerator/index.json` has the version with `"listed": true` (gzip; `curl --compressed`). Read back `1.0.0-beta.1`, listed true.
- [x] `dotnet nuget verify` on the downloaded nupkg (repository signature), the snupkg on the symbol server, `gh release view v1.0.0-beta.1` (prerelease, notes, nupkg and snupkg), `gh attestation verify --format json` on the run's artifact, and a fresh net10.0 and net48 console project restoring `1.0.0-beta.1 --prerelease` and printing the README's first example.

| Check | Result |
|---|---|
| `dotnet nuget verify --all` on the nupkg from the flat container (395,256 bytes) | Repository signature, NuGet.org Repository by Microsoft, certificate valid to 2027-05-18; content hash `t5ipovHk...` |
| Contents | README.md, `lib/net10.0` and `lib/netstandard2.0` DLL and XML docs; the netstandard2.0 DLL is byte-identical to the one in the run's attested artifact |
| snupkg | `www.nuget.org/api/v2/symbolpackage/UniverseGenerator/1.0.0-beta.1` redirects (302) to the symbol-packages CDN, which serves it (200, 78,455 bytes) |
| `gh release view v1.0.0-beta.1` | prerelease, not draft, notes from the CHANGELOG, assets the nupkg and the snupkg |
| `gh attestation verify` on the run's `release` artifact | nupkg verified: signer `release.yml@refs/tags/v1.0.0-beta.1`, run 37158022980 attempt 1. The snupkg has no attestation (HTTP 404), as designed: `release.yml` attests `artifacts/*.nupkg` only. The nupkg from nuget.org has a different digest (nuget.org adds its repository signature), so verify the run's artifact, not the download |
| Console projects (dotnet SDK 10.0.401, a scratch folder outside the repository, a `nuget.config` with only nuget.org, an empty `NUGET_PACKAGES` cache) | net10.0 (`dotnet add package UniverseGenerator --prerelease`, resolved 1.0.0-beta.1) and net48 (`--version 1.0.0-beta.1`, a .NET Framework exe) both print the README's first example: 60 lines starting `HD 147927: K star, 6 planets, danger 9`, `HD 165595: M star, 1 planets, danger 6`, `HD 28101: K star, 6 planets, danger 6`, as the README shows; the two outputs are byte-identical |

`dotnet add package` refuses `--version` and `--prerelease` together ("not supported in the same command"): use `--prerelease` alone (latest prerelease) or `--version 1.0.0-beta.1` alone.

- [x] Then 1.0.0 (after N1, plan "Before 1.0.0") the same way, and `PackageValidationBaselineVersion` 1.0.0 after it. Done 2026-10-03 (evening, US Central): on Mark's explicit instruction in the session the agent pushed tag `v1.0.0` on 3505547 (Mark's login bypassed the tag-creation ruleset, which the push reported) and approved the `nuget` deployment through the API. Release run 37173982448: build and test, Windows (net48 and net10.0), attest, `push to nuget.org (after approval)` in 10 s, `GitHub Release` in 8 s, all green. The baseline pull request #39 was merged by Mark on 2026-10-03 (bd86cf8; see the last table row).

Verified 2026-10-03 for 1.0.0, about fifteen minutes after the push (the beta.1 rows above stay):

| Check | Result |
|---|---|
| Flat container | `{"versions":["1.0.0-beta.1","1.0.0"]}`, about 15 minutes after the push |
| Registration (`curl --compressed`) | `1.0.0-beta.1` listed true, `1.0.0` listed true, at the same time as the flat container |
| `dotnet nuget verify --all` on the nupkg from the flat container (464,636 bytes) | exit 0; repository signature, NuGet.org Repository by Microsoft, certificate valid to 2027-05-18; content hash `Ttnj06HW...` |
| Contents | nuspec, README.md, icon.png, `lib/net10.0` and `lib/netstandard2.0` DLL and XML docs; both DLLs byte-identical to the run's attested artifact |
| snupkg | `www.nuget.org/api/v2/symbolpackage/UniverseGenerator/1.0.0` redirects (302) to the symbol-packages CDN, which serves it (200, 96,094 bytes, the same size as the release asset) |
| `gh release view v1.0.0` | a full release (not prerelease, not draft), notes from the CHANGELOG ("The first stable release..."), assets `UniverseGenerator.1.0.0.nupkg` (451,549 bytes) and `.snupkg` (96,094 bytes) |
| `gh attestation verify` on the run's `release` artifact | nupkg verified: signer `release.yml@refs/tags/v1.0.0`, run 37173982448 attempt 1; the snupkg has no attestation (HTTP 404), as designed |
| Console projects (dotnet SDK 10.0.401, a scratch folder outside the repository, a `nuget.config` with only nuget.org, empty `NUGET_PACKAGES` and `NUGET_HTTP_CACHE_PATH`) | net10.0 and net48 (`dotnet add package UniverseGenerator --version 1.0.0` alone; net48 a .NET Framework exe) both print the README's first example, 60 lines starting `HD 147927: K star, 6 planets, danger 9`; the two outputs are byte-identical |
| Package validation baseline (branch `chore/package-validation-baseline`, PR #39) | local `dotnet pack -c Release` downloaded the 1.0.0 nupkg from nuget.org as the baseline and passed, 0 warnings, 0 errors; `dotnet test -c Release` net10.0 272 passed, net48 271 passed. Merged by Mark (bd86cf8): `ci` run 37175151368 green on Linux, Windows and macOS, its pack step printed no warning, and a diagnostic pack on `master` (`-v diag`) sets the validator's baseline path to the cached nuget.org `universegenerator.1.0.0.nupkg`, 0 warnings |

The first `dotnet add package --version 1.0.0` failed with NU1102 ("Nearest version: 1.0.0-beta.1") minutes after the flat container already listed 1.0.0: NuGet's HTTP cache (`v3-cache`, about 30 minutes) still held the index from the beta.1 checks, and the failed add wrote no PackageReference. Point `NUGET_HTTP_CACHE_PATH` at an empty folder too (or run `dotnet nuget locals http-cache --clear`), then add again.
- [ ] Optional now that the first push bound the policy: narrow the Trusted Publishing scope to "push only new package versions" (step 3).

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (Stage 4, Security); see also [history scan](2026-10-03-history-scan.md), [package size budget](2026-10-02-package-size-budget.md).
