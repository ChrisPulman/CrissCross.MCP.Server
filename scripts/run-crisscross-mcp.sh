#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DLL="$ROOT/src/CrissCross.McpServer/bin/Release/net10.0/CrissCross.McpServer.dll"
exec "/mnt/c/Program Files/dotnet/dotnet.exe" "$(wslpath -w "$DLL")"
