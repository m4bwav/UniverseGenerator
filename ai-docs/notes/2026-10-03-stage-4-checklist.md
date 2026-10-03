---
title: "Stage 4 checklist for Mark: public repository, Trusted Publishing, 1.0.0-beta.1 (2026-10-03)"
kind: note
status: active
date: 2026-10-03
verified: 2026-10-03
stale_after: 2026-11-03
tags: [universegenerator, stage-4, release, nuget, trusted-publishing, checklist, public]
summary: "Mark's steps for Stage 4, in order: merge stage4-prep, re-run the scan, move CI off the self-hosted runner and make the repository public, the nuget.org Trusted Publishing policy (new package allowed), the nuget environment, date the CHANGELOG, tag v1.0.0-beta.1 after ci is green, approve, verify; read before any of them"
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

- [ ] nuget.org, your user name, **Trusted Publishing**, add a policy:

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

- [ ] Settings, Environments, New environment `nuget`: required reviewer **m4bwav**; Deployment branches and tags: "Selected branches and tags", rule `v*` of type tag; environment secret `NUGET_USER` = your nuget.org **profile name** (not your email). Or let an agent run it:

```
gh api -X PUT repos/m4bwav/UniverseGenerator/environments/nuget --input - <<'JSON'
{"reviewers":[{"type":"User","id":156112}],"deployment_branch_policy":{"protected_branches":false,"custom_branch_policies":true}}
JSON
gh api -X POST repos/m4bwav/UniverseGenerator/environments/nuget/deployment-branch-policies -f name='v*' -f type=tag
gh secret set NUGET_USER --env nuget -R m4bwav/UniverseGenerator    # paste the profile name when asked
```

(156112 is the user id of m4bwav, from `gh api users/m4bwav --jq .id`.)

## 5. The release commit

- [ ] **(agent)** A small pull request: the CHANGELOG heading `## [1.0.0-beta.1] - <date>` (it says "Unreleased" now; `release.yml` accepts that for a prerelease, but the release ritual dates it) with the size numbers checked again; `<Version>` stays `1.0.0-beta.1` (already set, and `package.json` matches). Merge after `ci` is green, then wait for `ci` green on `master`.

## 6. Tag and approve

- [ ] Only after `ci` is green on that `master` commit (tag only after green; a tag on a failing commit burns the version):

```
git fetch origin && git tag v1.0.0-beta.1 origin/master && git push origin v1.0.0-beta.1
```

- [ ] The `release` run checks the tag against `<Version>`, that the commit is on `master` and passed `ci`, builds, tests on Linux and Windows (net48 too), checks the package and the size gate, attests, then waits. Open the run, **Review deployments**, approve `nuget`. Nothing is on nuget.org before that click; after it the push is permanent (unlisting is the only undo).

## 7. Verify **(agent)**

- [ ] Flat container lists it (usually within 15 minutes): `https://api.nuget.org/v3-flatcontainer/universegenerator/index.json` contains `1.0.0-beta.1`.
- [ ] Registration says listed (can lag 25 minutes or more): `https://api.nuget.org/v3/registration5-gz-semver2/universegenerator/index.json` has the version with `"listed": true` (gzip; `curl --compressed`).
- [ ] `dotnet nuget verify` on the downloaded nupkg (repository signature), the snupkg on the symbol server, `gh release view v1.0.0-beta.1` (prerelease, notes, nupkg and snupkg), `gh attestation verify --format json` on the run's artifact, and a fresh net10.0 and net48 console project restoring `1.0.0-beta.1 --prerelease` and printing the README's first example.
- [ ] Then 1.0.0 (after N1, plan "Before 1.0.0") the same way, and `PackageValidationBaselineVersion` 1.0.0 after it.

Related: builds on [the 1.0 plan](../plans/2026-10-02-universegenerator-1.0-plan.md) (Stage 4, Security); see also [history scan](2026-10-03-history-scan.md), [package size budget](2026-10-02-package-size-budget.md).
