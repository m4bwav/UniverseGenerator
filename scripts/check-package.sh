#!/usr/bin/env bash
# The package holds exactly what it should and declares no dependencies. Called by ci.yml and release.yml on the
# package each one built:
#     dotnet pack src/UniverseGenerator -c Release -o artifacts
#     bash scripts/check-package.sh artifacts
set -euo pipefail

dir=${1:?usage: check-package.sh ARTIFACTS_DIR}
nupkg=$(ls "$dir"/UniverseGenerator.[0-9]*.nupkg)
expected=$'README.md\nUniverseGenerator.nuspec\nicon.png\nlib/net10.0/UniverseGenerator.dll\nlib/net10.0/UniverseGenerator.xml\nlib/netstandard2.0/UniverseGenerator.dll\nlib/netstandard2.0/UniverseGenerator.xml'
actual=$(unzip -Z1 "$nupkg" | grep -Ev '^(_rels/|package/|\[Content_Types\])' | LC_ALL=C sort)
if [ "$actual" != "$expected" ]; then
  echo "::error::unexpected package contents"
  diff <(echo "$expected") <(echo "$actual") || true
  exit 1
fi
nuspec=$(unzip -p "$nupkg" UniverseGenerator.nuspec | tr -d '\r\n')
deps=$(echo "$nuspec" | { grep -o '<dependency id="[^"]*"' || true; })
if [ -n "$deps" ]; then
  echo "::error::the package must have no dependencies: $deps"
  exit 1
fi
echo "package contents and dependencies as expected: $(basename "$nupkg")"
