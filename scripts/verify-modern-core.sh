#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project="$root/tests/Arthea.Core.SmokeTests/Arthea.Core.SmokeTests.csproj"
legacy="$root/tests/Arthea.Legacy.Characterization/Arthea.Legacy.Characterization.csproj"

# The core and legacy characterization are dependency-free; the isolated BSON reader
# uses a pinned, official MongoDB.Bson package and has its own integration test executable.
dotnet restore "$project" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$project" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Core.SmokeTests/bin/Debug/net10.0/Arthea.Core.SmokeTests.dll"

dotnet restore "$legacy" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$legacy" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Legacy.Characterization/bin/Debug/net10.0/Arthea.Legacy.Characterization.dll"

bson="$root/tests/Arthea.LegacyBson.Tests/Arthea.LegacyBson.Tests.csproj"
dotnet restore "$bson" --locked-mode -p:UseAppHost=false
dotnet build "$bson" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.LegacyBson.Tests/bin/Debug/net10.0/Arthea.LegacyBson.Tests.dll"

social="$root/tests/Arthea.Social.SmokeTests/Arthea.Social.SmokeTests.csproj"
dotnet restore "$social" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$social" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Social.SmokeTests/bin/Debug/net10.0/Arthea.Social.SmokeTests.dll"

python3 -B -m unittest discover -s "$root/tools/tests" -v
python3 -B "$root/tools/check_config_hygiene.py"
