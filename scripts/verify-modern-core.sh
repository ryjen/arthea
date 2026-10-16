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

# Independently reported core cases; keep the original smoke suite until
# equivalent coverage is deliberately demonstrated.
contracts="$root/tests/Arthea.Core.ContractTests/Arthea.Core.ContractTests.csproj"
dotnet restore "$contracts" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$contracts" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Core.ContractTests/bin/Debug/net10.0/Arthea.Core.ContractTests.dll"
contract_dll="$root/tests/Arthea.Core.ContractTests/bin/Debug/net10.0/Arthea.Core.ContractTests.dll"
dotnet "$contract_dll" --list | grep -Fx "caller changes cannot mutate graph"
selected_output="$(dotnet "$contract_dll" --case "caller changes cannot mutate graph")"
grep -Fx "PASS caller changes cannot mutate graph" <<< "$selected_output"
grep -Fx "Core contract cases: 1 executed, 0 failed." <<< "$selected_output"
set +e
dotnet "$contract_dll" --case "this contract does not exist"
unknown_status=$?
set -e
if [[ "$unknown_status" -ne 2 ]]; then
  echo "Unknown contract case must exit 2, got $unknown_status" >&2
  exit 1
fi

dotnet restore "$legacy" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$legacy" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Legacy.Characterization/bin/Debug/net10.0/Arthea.Legacy.Characterization.dll"

# Original-source command dispatcher characterization, using synthetic only
# character, social, ability and command types. No server or database access.
legacy_command="$root/tests/Arthea.Legacy.CommandCharacterization/Arthea.Legacy.CommandCharacterization.csproj"
dotnet restore "$legacy_command" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$legacy_command" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Legacy.CommandCharacterization/bin/Debug/net10.0/Arthea.Legacy.CommandCharacterization.dll"

# Original exit-editor one-way and two-way link behavior with synthetic model/data shims.
# No legacy database, network listener or original Framework server.
legacy_exits="$root/tests/Arthea.Legacy.ExitCharacterization/Arthea.Legacy.ExitCharacterization.csproj"
dotnet restore "$legacy_exits" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$legacy_exits" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Legacy.ExitCharacterization/bin/Debug/net10.0/Arthea.Legacy.ExitCharacterization.dll"

bson="$root/tests/Arthea.LegacyBson.Tests/Arthea.LegacyBson.Tests.csproj"
dotnet restore "$bson" --locked-mode -p:UseAppHost=false
dotnet build "$bson" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.LegacyBson.Tests/bin/Debug/net10.0/Arthea.LegacyBson.Tests.dll"

social="$root/tests/Arthea.Social.SmokeTests/Arthea.Social.SmokeTests.csproj"
dotnet restore "$social" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$social" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Social.SmokeTests/bin/Debug/net10.0/Arthea.Social.SmokeTests.dll"

sessions="$root/tests/Arthea.Session.SmokeTests/Arthea.Session.SmokeTests.csproj"
dotnet restore "$sessions" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$sessions" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Session.SmokeTests/bin/Debug/net10.0/Arthea.Session.SmokeTests.dll"

transport="$root/tests/Arthea.Transport.SmokeTests/Arthea.Transport.SmokeTests.csproj"
dotnet restore "$transport" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$transport" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Transport.SmokeTests/bin/Debug/net10.0/Arthea.Transport.SmokeTests.dll"

gmcp="$root/tests/Arthea.Gmcp.SmokeTests/Arthea.Gmcp.SmokeTests.csproj"
dotnet restore "$gmcp" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$gmcp" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Gmcp.SmokeTests/bin/Debug/net10.0/Arthea.Gmcp.SmokeTests.dll"

demo="$root/samples/Arthea.LocalDemo/Arthea.LocalDemo.csproj"
dotnet restore "$demo" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$demo" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/samples/Arthea.LocalDemo/bin/Debug/net10.0/Arthea.LocalDemo.dll" --self-check

journal="$root/tests/Arthea.Persistence.SmokeTests/Arthea.Persistence.SmokeTests.csproj"
dotnet restore "$journal" --locked-mode -p:UseAppHost=false
dotnet build "$journal" --no-restore -warnaserror -p:UseAppHost=false
dotnet "$root/tests/Arthea.Persistence.SmokeTests/bin/Debug/net10.0/Arthea.Persistence.SmokeTests.dll"

# Compile the optional benchmark without running timing-sensitive measurements in CI.
probe="$root/samples/Arthea.ContentionProbe/Arthea.ContentionProbe.csproj"
dotnet restore "$probe" --ignore-failed-sources -p:UseAppHost=false
dotnet build "$probe" --configuration Release --no-restore -warnaserror -p:UseAppHost=false

python3 -B -m unittest discover -s "$root/tools/tests" -v
python3 -B "$root/tools/check_config_hygiene.py"
# Scope: blocked sample screen paths in the committed candidate only, not Git history.
python3 -B "$root/tools/check_publication_hygiene.py"
# Informational inventory only; does not grant licenses or fail on missing notices.
python3 -B "$root/tools/inventory_third_party.py"
