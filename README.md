# CrissCross.MCP.Server

C# stdio MCP server for developers and coding agents working with the ReactiveMarbles CrissCross package family.

The server keeps MCP transport thin and exposes deterministic, source-backed knowledge from the CrissCross repository through tools, resources, and prompts. It does not modify the CrissCross source repository.

## Requirements

- .NET 10 SDK. From WSL on this machine use:
  - `/mnt/c/Program Files/dotnet/dotnet.exe`
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
