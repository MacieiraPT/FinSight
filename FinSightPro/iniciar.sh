#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

URL="https://localhost:7080"
( sleep 4 && (xdg-open "$URL" 2>/dev/null || open "$URL" 2>/dev/null) ) &
dotnet run --project FinSightPro.Web --launch-profile FinSightPro.Web
