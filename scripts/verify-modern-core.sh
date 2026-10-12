#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project="$root/tests/Arthea.Core.SmokeTests/Arthea.Core.SmokeTests.csproj"
legacy="$root/tests/Arthea.Legacy.Characterization/Arthea.Legacy.Characterization.csproj"

# No external packages are needed by the extracted core or its smoke executable.
dotnet restore "$project" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$project" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Core.SmokeTests/bin/Debug/net10.0/Arthea.Core.SmokeTests.dll"

dotnet restore "$legacy" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$legacy" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Legacy.Characterization/bin/Debug/net10.0/Arthea.Legacy.Characterization.dll"

python3 -B -m unittest discover -s "$root/tools/tests" -v
python3 -B "$root/tools/check_config_hygiene.py"
