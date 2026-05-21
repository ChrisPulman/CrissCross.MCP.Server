# CrissCross.MCP.Server

<!-- mcp-name: io.github.chrispulman/crisscross-mcp-server -->

C# stdio MCP server for developers and coding agents working with the ReactiveMarbles CrissCross package family.

The server keeps MCP transport thin and exposes deterministic, source-backed knowledge from the CrissCross repository through tools, resources, and prompts. It does not modify the CrissCross source repository.

The server is implemented in C# on `net10.0` using `ModelContextProtocol` `1.3.0`.

## Quick Install

Click to install in your preferred environment:

[![VS Code - Install CrissCross MCP](https://img.shields.io/badge/VS_Code-Install_CrissCross_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=crisscross-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.CrissCross.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)
[![VS Code Insiders - Install CrissCross MCP](https://img.shields.io/badge/VS_Code_Insiders-Install_CrissCross_MCP-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=crisscross-mcp-server&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.CrissCross.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D&quality=insiders)
[![Visual Studio - Install CrissCross MCP](https://img.shields.io/badge/Visual_Studio-Install_CrissCross_MCP-5C2D91?style=flat-square&logo=visualstudio&logoColor=white)](https://vs-open.link/mcp-install?%7B%22name%22%3A%22CP.CrissCross.Mcp.Server%22%2C%22type%22%3A%22stdio%22%2C%22command%22%3A%22dnx%22%2C%22args%22%3A%5B%22CP.CrissCross.Mcp.Server%400.*%22%2C%22--yes%22%5D%7D)

Note:
- These install links are prepared for the NuGet package identity `CP.CrissCross.Mcp.Server`.
- If the latest package has not been published yet, use the manual source-build configuration below.

Manual MCP configuration using NuGet:

```json
{
  "mcpServers": {
    "crisscross-mcp-server": {
      "type": "stdio",
      "command": "dnx",
      "args": [
        "CP.CrissCross.Mcp.Server@0.*",
        "--yes"
      ]
    }
  }
}
```

## Requirements

- .NET 10 SDK
- An MCP-capable client such as VS Code, Visual Studio, Claude Desktop, or any MCP 1.x host
- Node/npm only for optional MCP discovery probing with `mcporter`.

## Build and test

Run from the repository root:

```bash
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
"$DOTNET" restore CrissCross.MCP.Server.slnx
"$DOTNET" build CrissCross.MCP.Server.slnx -c Release --no-restore -warnaserror
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release
```

## Run as an MCP stdio server

Stdout is reserved for MCP JSON-RPC. All diagnostics are routed to stderr by `Microsoft.Extensions.Logging.Console` with `LogToStandardErrorThreshold = Trace`.

```bash
"/mnt/c/Program Files/dotnet/dotnet.exe" "$(wslpath -w "$PWD/src/CrissCross.McpServer/bin/Release/net10.0/CrissCross.McpServer.dll")"
```

Convenience wrapper:

```bash
scripts/run-crisscross-mcp.sh
```

MCP discovery probe:

```bash
npm_config_cache="$PWD/.npm-cache" npx -y mcporter list --stdio "$PWD/scripts/run-crisscross-mcp.sh" --name crisscross
```

### Manual source configuration

```json
{
  "mcpServers": {
    "crisscross-mcp-server": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "/path/to/CrissCross.MCP.Server/src/CrissCross.McpServer/CrissCross.McpServer.csproj"
      ]
    }
  }
}
```

## Tools

- `crisscross_get_package_matrix(platform?, targetFramework?)`
- `crisscross_get_startup_recipe(platform, uiMode?)`
- `crisscross_get_navigation_recipe(kind, platform?, hostName?, contract?)`
- `crisscross_find_control(platform, nameOrFeature)`
- `crisscross_generate_viewmodel(feature, className, namespace, navigationMode?)`
- `crisscross_generate_navigation_registry(mappingSpec)`
- `crisscross_review_code_snippet(code, platform?, projectKind?)`
- `crisscross_generate_project_starter(platform, mode, appName, rootNamespace, screensCsv?, controlsCsv?, targetFramework?, hostName?)`
- `crisscross_explain_error(message, platform?)`

## Resources

The core resource router supports these URI families:

- `crisscross://packages/matrix`
- `crisscross://startup/{wpf|avalonia|maui|winforms}`
- `crisscross://navigation/core`
- `crisscross://navigation/{navigation-only|viewmodel-host|page-navigation|navigation-view}`
- `crisscross://controls/{platform}`
- `crisscross://controls/{platform}/{controlName}`
- `crisscross://state-models`
- `crisscross://templates/{platform}/{mode}`
- `crisscross://quality/anti-patterns`
- `crisscross://quality/testing`

## Prompt guardrails

Prompt helpers instruct agents to call the CrissCross tools before emitting code, avoid `RxApp`, prefer `RxObject`, `RaiseAndSetIfChanged`, `ReactiveCommand.CreateFromTask`, `ObservableAsPropertyHelper`, `RxSchedulers`, and use replacement semantics for state models.

## Codex and agent skill

This repository includes a Codex skill at [`skills/crisscross/SKILL.md`](skills/crisscross/SKILL.md). It gives Codex and compatible agents a compact operating guide for using this MCP server well:

- search CrissCross package and startup guidance before emitting code
- choose the correct navigation pattern for each platform
- use source-backed controls and replacement state-model semantics
- review generated snippets with `crisscross_review_code_snippet`
- treat `crisscross_generate_project_starter` output as preview-only until the caller writes files

To install the skill for Codex on Windows:

```powershell
$source = ".\skills\crisscross"
$target = Join-Path $env:USERPROFILE ".codex\skills\crisscross"
New-Item -ItemType Directory -Force $target | Out-Null
Copy-Item -Path "$source\*" -Destination $target -Recurse -Force
```

Agents that support repository-local skills can use the file directly. Agents that do not support skills can still follow the workflow in `SKILL.md`.

## Template Wizard

`crisscross_generate_project_starter` is a full preview-only Template Wizard for WPF, Avalonia, MAUI, and WinForms. It supports `navigation-only` and `navigation-and-ui`, validates unsafe names/TFMs/controls, and returns `TemplateGenerationResult` JSON with diagnostics, next steps, source-template provenance, and complete generated-file previews. It does not write files.

Example MCP call:

```json
{
  "tool": "crisscross_generate_project_starter",
  "arguments": {
    "platform": "avalonia",
    "mode": "navigation-and-ui",
    "appName": "SampleApp",
    "rootNamespace": "SampleApp",
    "screensCsv": "Home,Settings",
    "controlsCsv": "CommandButton,SearchBox"
  }
}
```

Example template resource:

```text
crisscross://templates/wpf/navigation-and-ui
```

Generated previews include project/CPM files, framework startup files, CrissCross navigation hosts/registrations, one view model and view per screen, optional smoke tests/readme, and UI-mode controls gallery files. UI mode uses `CrissCross.WPF.UI`, `CrissCross.Avalonia.UI`, `CrissCross.Maui.UI`, or a WinForms reactive controls form with an informational diagnostic because WinForms has no separate CrissCross UI package.

See `docs/template-wizard.md` for file-set details, validation diagnostics, and additional MCP examples.

## MCP metadata

Server metadata file:
- `.mcp/server.json`

Current working identifiers:
- MCP server name: `io.github.chrispulman/crisscross-mcp-server`
- package id: `CP.CrissCross.Mcp.Server`
- tool command: `crisscross-mcp-server`
- version: `0.1.0`

Before publishing a new package version, update:
- `version.json`
- `.mcp/server.json`
- install badge package range if the major version changes
