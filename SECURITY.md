# Security policy

## Reporting a problem

Open an issue or a pull request, or start a thread in [Discussions](https://github.com/m4bwav/UniverseGenerator/discussions) and I'll take a look. You can also report privately: open the repository's **Security** tab and choose **Report a vulnerability**.

A confirmed problem is fixed in a new release, and the advisory is published once the fix is on nuget.org. Affected versions are then marked deprecated on nuget.org with the fixed version as the alternate.

## Supported versions

Only the latest major version (1.x) gets security fixes.

## What this package is not

The generator makes no network calls, reads no files and uses no reflection; its only data is the star-name tables compiled into it. A seed is not a secret: anyone with a seed text and the generator version can reproduce everything generated from it, so never use a seed, or anything generated from one, as a password, token or other security value. The pseudo-random generator (PCG32) is chosen for speed and reproducibility and is not cryptographically secure.
