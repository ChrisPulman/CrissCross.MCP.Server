# CrissCross MCP Server Implementation Plan

> **For Hermes:** Use subagent-driven-development skill to implement this plan task-by-task.

**Goal:** Build a C# stdio MCP server that helps developers and coding agents generate correct CrissCross setup, navigation, view models, controls, review diagnostics, and framework-specific starter projects for WPF, Avalonia, MAUI, and WinForms.

**Architecture:** Keep the MCP transport thin and deterministic. Put all CrissCross knowledge, validation, template metadata, and template rendering in a testable core library; expose that core through MCP tools, resources, and prompts from a small console host. Generate starter project files from declarative template manifests and plain text/Scriban templates so tests can validate every output without starting the MCP transport.

**Tech Stack:** .NET 10, C# 14/latest, `ModelContextProtocol` 1.3.0, `Microsoft.Extensions.Hosting` 10.0.8, `Microsoft.Extensions.Logging.Console` 10.0.8, TUnit 1.45.22 with Microsoft Testing Platform, optional Scriban for template rendering.

---

## Current context / recovery notes

- Target repository path: `/mnt/c/Projects/GitHub/ChrisPulman/CrissCross.MCP.Server`.
- Read-only source reference path: `/mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross`.
- Hard rule: never create, delete, or modify files under `/mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross`. In plain text for checklist tooling: never create, delete, or modify files under /mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross.
- Parent handoff `t_a230bc0f` observed target baseline: `README.md`, `LICENSE`, `.gitignore`, branch `main`, commit `de4c2fe`, with pre-existing line-ending-only working-tree modifications.
- This planning run added only this plan file under `docs/plans/`. Before implementation, the engineer must run the baseline commands below and preserve any user-owned files already present.
- Parent handoff `t_c3d8df92` is the source knowledge map. Treat its API names and source paths as the first implementation reference.

Baseline commands to run before any code changes:

```bash
cd /mnt/c/Projects/GitHub/ChrisPulman/CrissCross.MCP.Server
pwd
find . -maxdepth 3 -type f | sort
if git rev-parse --is-inside-work-tree >/dev/null 2>&1; then git status --short && git rev-parse --abbrev-ref HEAD && git rev-parse --short HEAD; fi

git -C /mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross status --short
git -C /mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross rev-parse --abbrev-ref HEAD
git -C /mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross rev-parse --short HEAD
"/mnt/c/Program Files/dotnet/dotnet.exe" --info
```

Expected source repo identity for this plan: `master@52ca28d` or newer. If source APIs have materially changed, update the knowledge catalogs and tests before implementing later phases.

---

## Scope boundaries

### In scope

- A net10.0 stdio MCP server executable.
- A transport-independent core library for:
  - CrissCross package/platform matrix.
  - Startup recipes.
  - Navigation recipes.
  - Control/state-model guidance.
  - Code/snippet review diagnostics.
  - Template Wizard manifest/options/validation/rendering.
- MCP tools, resources, and prompts backed by the core library.
- Template Wizard for four framework targets: `Avalonia`, `MAUI`, `WinForms`, `WPF`.
- Template Wizard modes:
  - `NavigationOnly`: creates navigation/VM wiring with minimal UI host/shell.
  - `NavigationAndUi`: creates navigation plus starter CrissCross UI controls and shared state/view models.
- Test-first implementation with exact RED/GREEN commands recorded in each task handoff.
- Documentation under `docs/` and README updates in the target repo only.

### Out of scope for first implementation

- Running or modifying the CrissCross source repository.
- Publishing NuGet packages.
- HTTP/SSE MCP transport.
- A graphical wizard UI.
- Compiling generated WPF/MAUI/WinForms/Avalonia starter projects on all TFMs in CI. The first milestone validates file manifests and source text; optional generated-project compilation can be a later card.
- Deep parsing of arbitrary C# syntax. The first validator is deterministic pattern-based review with conservative diagnostics.

---

## Architecture decisions

1. **Thin MCP host, rich core.** `src/CrissCross.McpServer` contains `Program.cs` and MCP adapter classes only. `src/CrissCross.McpServer.Core` contains all behavior and is fully unit tested.
2. **No stdout diagnostics.** The stdio host reserves stdout for MCP JSON-RPC. Logging must go to stderr through `AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace)`. No `Console.WriteLine` in host/tools.
3. **Deterministic data first.** Catalogs are committed as strongly typed records plus JSON/Markdown resource files. Avoid scraping the source repo at runtime.
4. **Template Wizard produces a manifest.** Every generated starter includes a `TemplateGenerationResult` with `GeneratedFile` entries, validation diagnostics, and user-facing next steps. Actual file writes are opt-in; tool calls can return files as text unless a future workflow explicitly permits writing.
5. **Use platform-specific template sets.** Keep shared VM/navigation snippets in `Templates/shared`, platform startup/shell/resources in `Templates/{wpf,avalonia,maui,winforms}`.
6. **Conservative validator.** Prefer false-positive warnings over generating known-bad CrissCross code. The validator must catch banned `RxApp`, missing startup builders, missing host registration, host-name ambiguity, wrong page-vs-VM navigation APIs, platform types in core, deep mutation of snapshot state models, missing view registrations, duplicate/unknown contracts, and STA notes for WPF tests.

---

## Final solution/project/test structure

All paths are under `/mnt/c/Projects/GitHub/ChrisPulman/CrissCross.MCP.Server`.

```text
CrissCross.MCP.Server.slnx
Directory.Build.props
Directory.Packages.props
global.json
README.md
docs/
  architecture.md
  template-wizard.md
  plans/
    2026-05-21-crisscross-mcp-server-implementation-plan.md
src/
  CrissCross.McpServer/
    CrissCross.McpServer.csproj
    Program.cs
    Mcp/
      CrissCrossTools.cs
      CrissCrossResources.cs
      CrissCrossPrompts.cs
      McpResultMapper.cs
  CrissCross.McpServer.Core/
    CrissCross.McpServer.Core.csproj
    Catalog/
      CrissCrossKnowledgeCatalog.cs
      PackageCatalog.cs
      PlatformCatalog.cs
      ControlCatalog.cs
      StateModelCatalog.cs
      StartupRecipeCatalog.cs
      NavigationRecipeCatalog.cs
    Knowledge/
      packages.json
      startup-recipes.json
      navigation-recipes.md
      controls.json
      state-models.json
      anti-patterns.json
    Review/
      CrissCrossSnippetReviewer.cs
      ReviewRule.cs
      ReviewDiagnostic.cs
      ReviewSeverity.cs
    Templates/
      TemplateCatalog.cs
      TemplateRenderer.cs
      TemplateValidator.cs
      TemplateWizard.cs
      Models/
        FrameworkTarget.cs
        WizardMode.cs
        TemplateOptionSet.cs
        TemplateManifest.cs
        TemplateFileSpec.cs
        GeneratedFile.cs
        TemplateGenerationRequest.cs
        TemplateGenerationResult.cs
        ValidationDiagnostic.cs
        ValidationSeverity.cs
      shared/
        ViewModels/AppShellViewModel.cs.scriban
        ViewModels/HomeViewModel.cs.scriban
        Navigation/NavigationRegistry.cs.scriban
      wpf/
        App.xaml.scriban
        App.xaml.cs.scriban
        MainWindow.xaml.scriban
        MainWindow.xaml.cs.scriban
        Views/HomeView.xaml.scriban
        Project.csproj.scriban
      avalonia/
        App.axaml.scriban
        App.axaml.cs.scriban
        Program.cs.scriban
        MainWindow.axaml.scriban
        MainWindow.axaml.cs.scriban
        Views/HomeView.axaml.scriban
        Project.csproj.scriban
      maui/
        App.xaml.scriban
        App.xaml.cs.scriban
        MauiProgram.cs.scriban
        AppShell.xaml.scriban
        AppShell.xaml.cs.scriban
        Views/HomePage.xaml.scriban
        Project.csproj.scriban
      winforms/
        Program.cs.scriban
        MainForm.cs.scriban
        MainForm.Designer.cs.scriban
        Views/HomeView.cs.scriban
        Views/HomeView.Designer.cs.scriban
        Project.csproj.scriban
    Resources/
      ResourceRouter.cs
tests/
  CrissCross.McpServer.Tests/
    CrissCross.McpServer.Tests.csproj
    Catalog/
      PackageCatalogTests.cs
      StartupRecipeCatalogTests.cs
      NavigationRecipeCatalogTests.cs
      ControlCatalogTests.cs
    Review/
      CrissCrossSnippetReviewerTests.cs
    Templates/
      TemplateCatalogTests.cs
      TemplateWizardTests.cs
      TemplateRendererSnapshotTests.cs
      TemplateValidatorTests.cs
    Mcp/
      McpToolAdapterTests.cs
      McpResourceAdapterTests.cs
      McpPromptAdapterTests.cs
    TestData/
      ExpectedTemplates/
        wpf-navigation-only.txt
        wpf-navigation-and-ui.txt
        avalonia-navigation-only.txt
        avalonia-navigation-and-ui.txt
        maui-navigation-only.txt
        maui-navigation-and-ui.txt
        winforms-navigation-only.txt
        winforms-navigation-and-ui.txt
```

Project references:

- `src/CrissCross.McpServer/CrissCross.McpServer.csproj` references `src/CrissCross.McpServer.Core/CrissCross.McpServer.Core.csproj` and packages `ModelContextProtocol`, `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging.Console`.
- `tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj` references both source projects only if adapter tests need MCP types; otherwise prefer testing the core and adapter classes directly.
- The core library should not depend on `ModelContextProtocol`.

Central package versions:

```xml
<Project>
  <ItemGroup>
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="10.0.8" />
    <PackageVersion Include="Microsoft.Extensions.Logging.Console" Version="10.0.8" />
    <PackageVersion Include="ModelContextProtocol" Version="1.3.0" />
    <PackageVersion Include="Scriban" Version="6.3.0" />
    <PackageVersion Include="TUnit" Version="1.45.22" />
    <PackageVersion Include="TUnit.Assertions" Version="1.45.22" />
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.5.1" />
  </ItemGroup>
</Project>
```

If package restore shows newer stable versions, update `Directory.Packages.props` in one commit and record the version change in the handoff.

---

## MCP server surface

### Tools

Implement these as MCP tools in `CrissCrossTools` backed by core services:

1. `crisscross_get_package_matrix(platform?, targetFramework?)`
   - Returns packages, TFMs, required platform flags, namespace URIs, and source reference paths.
   - Platforms: `core`, `wpf`, `wpf-ui`, `avalonia`, `avalonia-ui`, `maui`, `maui-ui`, `winforms`.
2. `crisscross_get_startup_recipe(platform, uiMode?)`
   - Returns exact startup calls, resource dictionaries, package references, and gotchas.
   - Must include WPF `.WithWpf().BuildApp()`, MAUI `.WithMaui().BuildApp()`, WinForms `.WithWinForms().BuildApp()`, Avalonia `.UseReactiveUI(...)`.
3. `crisscross_get_navigation_recipe(kind, platform?, hostName?, contract?)`
   - `kind`: `navigation-only`, `viewmodel-host`, `page-navigation`, `navigation-view`.
   - Returns when to use `NavigationRegistry` versus `IViewModelRoutedViewHost`/`SetMainNavigationHost`.
4. `crisscross_find_control(platform, nameOrFeature)`
   - Maps user terms to CrissCross controls, source paths, required package, state model, and example snippet.
5. `crisscross_generate_viewmodel(feature, className, namespace, navigationMode?)`
   - Returns a `RxObject`-based VM snippet using `RaiseAndSetIfChanged`, `ReactiveCommand.CreateFromTask`, `ObservableAsPropertyHelper`, immutable state replacement, and `RxSchedulers` where needed.
6. `crisscross_generate_navigation_registry(mappingSpec)`
   - Generates `NavigationRegistry.Register...` snippet with contracts, duplicate/unknown-contract notes, and a matching test snippet.
7. `crisscross_review_code_snippet(code, platform?, projectKind?)`
   - Returns deterministic `ReviewDiagnostic` entries for known incorrect patterns.
8. `crisscross_generate_project_starter(request)`
   - Entry point for the Template Wizard. Returns `TemplateGenerationResult` with file paths/content, diagnostics, and next commands.
9. `crisscross_explain_error(message, platform?)`
   - Maps common CrissCross errors to fixes: no navigation host, host name not set, missing view registration, duplicate contract, unknown contract, wrong page type.

### Resources

Implement resources in `CrissCrossResources` using URI routing:

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

Resource content should be Markdown or JSON text generated from core catalogs. Tests should verify stable content includes source-backed API names.

### Prompts

Implement prompts in `CrissCrossPrompts`:

1. `generate-crisscross-app`
   - Inputs: platform, app name, namespace, wizard mode, initial screens, include controls.
   - Prompt instructs agent to call `crisscross_generate_project_starter` first, then inspect diagnostics before emitting code.
2. `generate-crisscross-viewmodel`
   - Enforces `RxObject`, `RaiseAndSetIfChanged`, `ReactiveCommand.CreateFromTask`, `ObservableAsPropertyHelper`, no `RxApp`.
3. `wire-crisscross-navigation`
   - Chooses `NavigationRegistry` or platform host flow based on user intent.
4. `review-crisscross-code`
   - Instructs agent to call `crisscross_review_code_snippet` and fix every error diagnostic before finalizing.
5. `implement-crisscross-control-usage`
   - Guides control selection and state model replacement pattern.

---

## Template Wizard design

### User-facing request model

`TemplateGenerationRequest` fields:

```csharp
public sealed record TemplateGenerationRequest(
    string AppName,
    string RootNamespace,
    FrameworkTarget Target,
    WizardMode Mode,
    string TargetFramework,
    string? HostName,
    IReadOnlyList<string> Screens,
    IReadOnlyList<string> IncludeControls,
    bool IncludeTests,
    bool IncludeReadme,
    bool UseCentralPackageManagement,
    bool OverwriteExistingFiles);
```

### Framework targets

```csharp
public enum FrameworkTarget
{
    Avalonia,
    Maui,
    WinForms,
    Wpf
}
```

Each target has a `TemplateManifest`:

- `FrameworkTarget Target`
- `string DefaultTargetFramework`
- `IReadOnlyList<string> SupportedTargetFrameworks`
- `IReadOnlyList<string> RequiredCrissCrossPackages`
- `IReadOnlyList<string> OptionalCrissCrossPackages`
- `IReadOnlyList<TemplateFileSpec> Files`
- `IReadOnlyList<string> StartupRequirements`
- `IReadOnlyList<string> ValidationRules`

Suggested default TFMs:

- Avalonia: `net10.0`
- MAUI: `net10.0` for first generated text, with manifest notes for platform TFMs.
- WinForms: `net10.0-windows10.0.19041.0`
- WPF: `net10.0-windows10.0.19041.0`

### Wizard modes

```csharp
public enum WizardMode
{
    NavigationOnly,
    NavigationAndUi
}
```

`NavigationOnly` output must include:

- Project file with required CrissCross platform package.
- Startup builder call for target platform.
- A shell/host file.
- `HomeViewModel : RxObject`.
- A platform view/control/page for `HomeViewModel`.
- Navigation registration or host setup.
- Comments explaining host names and contracts.

`NavigationAndUi` output includes everything in `NavigationOnly` plus:

- Platform UI package (`CrissCross.WPF.UI`, `CrissCross.Avalonia.UI`, `CrissCross.Maui.UI` where applicable).
- Resource dictionaries/theme setup.
- Starter controls such as `CommandButton`, `BusyOverlay`, `SearchBox`, `DataPager`, `ValidationSummary`, `ThemeSwitcher` when available for the platform.
- Shared state models and VM properties using replacement semantics, not deep mutation.

### Target-specific template expectations

#### WPF

- Include `UseWPF=true` and `net10.0-windows10.0.19041.0`.
- Startup snippet must include `RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();`.
- XAML namespace URI: `https://github.com/reactivemarbles/CrissCross`.
- Navigation host: `CrissCross.WPF.ViewModelRoutedViewHost` or `NavigationWindow<TViewModel>`.
- UI mode includes `<rxNav:CrissCrossWpfDictionary />` and selected WPF.UI controls.
- Validator warns if WPF UI tests omit STA guidance.

#### Avalonia

- Include `net10.0`.
- Startup snippet must include Avalonia `AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().UseReactiveUI(...)`.
- Style include: `avares://CrissCross.Avalonia/Themes/Index.axaml`.
- Navigation host: `CrissCross.Avalonia.ViewModelRoutedViewHost`, `NavigationWindow`, or `NavigationUserControl`.
- UI mode includes Avalonia UI resource/style include and control snippets.

#### MAUI

- Include `UseMaui=true`.
- Startup snippet must include `RxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp();` and `UseCrissCrossMauiUi()` in UI mode.
- Navigation host: `CrissCross.MAUI.NavigationShell`.
- UI mode includes `Resources.UseCrissCrossMauiUiResources();` and controls available under `CrissCross.Maui.UI`.

#### WinForms

- Include `UseWindowsForms=true` and `net10.0-windows10.0.19041.0`.
- Startup snippet must include `ApplicationConfiguration.Initialize(); RxAppBuilder.CreateReactiveUIBuilder().WithWinForms().BuildApp(); Application.Run(new MainForm());`.
- Navigation host: `CrissCross.WinForms.ViewModelRoutedViewHost` or `NavigationForm<TViewModel>`.
- UI mode is limited to navigation/control host patterns available in WinForms; do not invent WPF/Avalonia UI controls.
- Validator enforces non-empty `HostName` before `Setup()` for WinForms hosts.

---

## Core data model

Create immutable records so tool responses, resource rendering, validation, and tests share one contract.

```csharp
public sealed record PackageInfo(
    string Id,
    string DisplayName,
    FrameworkTarget? Platform,
    IReadOnlyList<string> TargetFrameworks,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> SourcePaths,
    IReadOnlyList<string> Notes);

public sealed record StartupRecipe(
    FrameworkTarget Target,
    WizardMode? Mode,
    string Title,
    IReadOnlyList<string> RequiredPackages,
    IReadOnlyList<string> RequiredFiles,
    string CodeSnippet,
    IReadOnlyList<string> Gotchas,
    IReadOnlyList<string> SourceReferences);

public sealed record NavigationRecipe(
    string Kind,
    FrameworkTarget? Target,
    string Summary,
    string CodeSnippet,
    IReadOnlyList<string> RequiredSetup,
    IReadOnlyList<string> CommonFailures,
    IReadOnlyList<string> SourceReferences);

public sealed record ControlInfo(
    string Name,
    FrameworkTarget Target,
    string PackageId,
    string? StateModel,
    IReadOnlyList<string> Features,
    IReadOnlyList<string> SourcePaths,
    string UsageSnippet);

public sealed record ReviewDiagnostic(
    string RuleId,
    ReviewSeverity Severity,
    string Message,
    string? Fix,
    string? SourceReference);

public sealed record TemplateFileSpec(
    string RelativePath,
    string TemplatePath,
    bool IncludeInNavigationOnly,
    bool IncludeInNavigationAndUi,
    IReadOnlyList<string> RequiredControls);

public sealed record GeneratedFile(
    string RelativePath,
    string Content,
    bool IsExecutable,
    string? SourceTemplate);

public sealed record ValidationDiagnostic(
    string RuleId,
    ValidationSeverity Severity,
    string Message,
    string? FilePath,
    string? SuggestedFix);

public sealed record TemplateGenerationResult(
    TemplateGenerationRequest Request,
    IReadOnlyList<GeneratedFile> Files,
    IReadOnlyList<ValidationDiagnostic> Diagnostics,
    IReadOnlyList<string> NextSteps);
```

Validation severity enums should be shared with MCP mapping but kept in core:

```csharp
public enum ValidationSeverity { Info, Warning, Error }
public enum ReviewSeverity { Info, Warning, Error }
```

---

## Dependency ordering

Phase graph:

1. Repository/scaffold setup.
2. Core data contracts.
3. Knowledge catalogs.
4. Review validator.
5. Template manifests and validation.
6. Template renderer and wizard output.
7. MCP tool/resource/prompt adapters.
8. Host wiring and MCP discovery probe.
9. Documentation and README.

Do not start MCP adapter work before core catalog/review/template tests pass. Do not start host/discovery before adapter tests pass.

---

## Test-first task order

All commands below are run from:

```bash
cd /mnt/c/Projects/GitHub/ChrisPulman/CrissCross.MCP.Server
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
```

If the shell does not preserve `DOTNET`, paste the full quoted path in each command.

### Task 1: Scaffold solution and test project

**Objective:** Create buildable empty solution/project/test structure.

**Files:**
- Create: `global.json`
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `CrissCross.MCP.Server.slnx`
- Create: `src/CrissCross.McpServer/CrissCross.McpServer.csproj`
- Create: `src/CrissCross.McpServer.Core/CrissCross.McpServer.Core.csproj`
- Create: `tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj`
- Create: `tests/CrissCross.McpServer.Tests/SmokeTests.cs`

**RED:** Create `SmokeTests.cs` referencing `CrissCross.McpServer.Core.CrissCrossKnowledgeCatalog.CreateDefault()` before it exists.

Run:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/CoreCatalogCanBeCreated"
```

Expected RED: compile failure because `CrissCrossKnowledgeCatalog` does not exist.

**GREEN:** Add minimal `CrissCrossKnowledgeCatalog` class with `CreateDefault()` returning an empty object.

Run:

```bash
"$DOTNET" restore CrissCross.MCP.Server.slnx
"$DOTNET" build CrissCross.MCP.Server.slnx -c Release --no-restore -warnaserror
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/CoreCatalogCanBeCreated"
```

Expected GREEN: build succeeds and filtered test passes.

Commit:

```bash
git add global.json Directory.Build.props Directory.Packages.props CrissCross.MCP.Server.slnx src tests
git commit -m "chore: scaffold crisscross mcp server solution"
```

### Task 2: Add core data contracts

**Objective:** Define immutable records/enums for packages, recipes, controls, review diagnostics, templates, and generation results.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Catalog/*.cs`
- Create: `src/CrissCross.McpServer.Core/Review/*.cs`
- Create: `src/CrissCross.McpServer.Core/Templates/Models/*.cs`
- Test: `tests/CrissCross.McpServer.Tests/Templates/TemplateModelTests.cs`

**RED:** Test that `TemplateGenerationResult` holds request, files, diagnostics, and next steps immutably.

Run:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/TemplateGenerationResultCarriesFilesDiagnosticsAndNextSteps"
```

Expected RED: compile failure for missing records.

**GREEN:** Add the records/enums from the data model section. Keep constructors simple; no behavior yet.

Run same filtered test, then:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release
```

Expected GREEN: all current tests pass.

### Task 3: Implement package/platform catalog

**Objective:** Return source-backed package matrix for CrissCross package family.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Knowledge/packages.json`
- Modify: `src/CrissCross.McpServer.Core/Catalog/PackageCatalog.cs`
- Modify: `src/CrissCross.McpServer.Core/Catalog/CrissCrossKnowledgeCatalog.cs`
- Test: `tests/CrissCross.McpServer.Tests/Catalog/PackageCatalogTests.cs`

**RED:** Tests assert package IDs and target notes include at least:

- `CrissCross`
- `CrissCross.WPF`
- `CrissCross.WPF.UI`
- `CrissCross.Avalonia`
- `CrissCross.Avalonia.UI`
- `CrissCross.MAUI`
- `CrissCross.Maui.UI`
- `CrissCross.WinForms`

Run:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/PackageCatalogListsKnownCrissCrossPackages"
```

Expected RED: empty/missing catalog failure.

**GREEN:** Load embedded JSON or initialize strongly typed package entries. Include source references from parent handoff, e.g. `src/CrissCross.WPF/CrissCross.WPF.csproj`.

Run filtered test and full tests.

### Task 4: Implement startup recipe catalog

**Objective:** Return platform-specific startup instructions and resource setup.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Knowledge/startup-recipes.json`
- Create/modify: `src/CrissCross.McpServer.Core/Catalog/StartupRecipeCatalog.cs`
- Test: `tests/CrissCross.McpServer.Tests/Catalog/StartupRecipeCatalogTests.cs`

**RED:** Write four tests:

- WPF recipe contains `.WithWpf().BuildApp()` and `CrissCrossWpfDictionary`.
- Avalonia recipe contains `.UseReactiveUI` and `avares://CrissCross.Avalonia/Themes/Index.axaml`.
- MAUI recipe contains `.WithMaui().BuildApp()` and `UseCrissCrossMauiUiResources` for UI mode.
- WinForms recipe contains `ApplicationConfiguration.Initialize()` and `.WithWinForms().BuildApp()`.

Run each filtered test individually, record RED failures, then run all four.

**GREEN:** Add minimal recipe data and retrieval methods.

Run:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/StartupRecipe*"
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release
```

### Task 5: Implement navigation recipe catalog

**Objective:** Explain navigation-only and navigation+UI host flows without confusing APIs.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Knowledge/navigation-recipes.md`
- Create/modify: `src/CrissCross.McpServer.Core/Catalog/NavigationRecipeCatalog.cs`
- Test: `tests/CrissCross.McpServer.Tests/Catalog/NavigationRecipeCatalogTests.cs`

**RED:** Tests assert:

- `navigation-only` recipe includes `NavigationRegistry` and `IBidirectionalNavigator`.
- `viewmodel-host` recipe includes `IViewModelRoutedViewHost`, `SetMainNavigationHost`, and host-name guidance.
- `page-navigation` recipe references WPF.UI `INavigationService`/`IPageService` and does not claim it is the same as VM-host navigation.

Run filtered tests and record failures.

**GREEN:** Add recipes with source references:

- `src/CrissCross/Navigation/NavigationRegistry.cs`
- `src/CrissCross/Navigation/IBidirectionalNavigator.cs`
- `src/CrissCross/ViewModelRoutedViewHostMixins.cs`
- `src/CrissCross.WPF.UI/NavigationService.cs`
- `src/CrissCross.WPF.UI/Services/PageService.cs`

### Task 6: Implement control and state-model catalogs

**Objective:** Provide searchable guidance for shared controls and state models.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Knowledge/controls.json`
- Create: `src/CrissCross.McpServer.Core/Knowledge/state-models.json`
- Create/modify: `src/CrissCross.McpServer.Core/Catalog/ControlCatalog.cs`
- Create/modify: `src/CrissCross.McpServer.Core/Catalog/StateModelCatalog.cs`
- Test: `tests/CrissCross.McpServer.Tests/Catalog/ControlCatalogTests.cs`

**RED:** Tests assert:

- `SearchBox`, `DataPager`, `ValidationSummary`, `ThemeSwitcher`, and `CommandButton` resolve for WPF/Avalonia/MAUI where available.
- WPF-only entries do not appear as available WinForms controls unless backed by source.
- State-model guidance says replace immutable/snapshot state values rather than deep-mutating nested values.

**GREEN:** Add minimal source-backed entries first; expand entries after tests cover them.

### Task 7: Implement snippet reviewer

**Objective:** Detect common incorrect CrissCross generation patterns.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Review/CrissCrossSnippetReviewer.cs`
- Create: `src/CrissCross.McpServer.Core/Review/ReviewRule.cs`
- Test: `tests/CrissCross.McpServer.Tests/Review/CrissCrossSnippetReviewerTests.cs`

**RED:** Add one test per rule:

1. `ReviewerRejectsRxAppUsage`
2. `ReviewerWarnsWhenWpfStartupBuildAppIsMissing`
3. `ReviewerWarnsWhenAvaloniaUseReactiveUiIsMissing`
4. `ReviewerWarnsWhenNavigationHostRegistrationIsMissing`
5. `ReviewerWarnsOnEmptyHostNameInHostedNavigation`
6. `ReviewerWarnsOnPlatformTypesInCoreProject`
7. `ReviewerWarnsOnDeepStateMutation`
8. `ReviewerWarnsWhenWpfUiPageNavigationUsesViewModelHostApi`

Run one filtered test, verify RED, implement minimal rule, verify GREEN, repeat. Do not implement all rules before watching the first test fail.

Final command:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/Reviewer*"
```

### Task 8: Implement Template Wizard manifest and validator

**Objective:** Define target/mode manifests and validate request combinations before rendering.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Templates/TemplateCatalog.cs`
- Create: `src/CrissCross.McpServer.Core/Templates/TemplateValidator.cs`
- Create: `src/CrissCross.McpServer.Core/Templates/{wpf,avalonia,maui,winforms,shared}/...` placeholder template files
- Test: `tests/CrissCross.McpServer.Tests/Templates/TemplateCatalogTests.cs`
- Test: `tests/CrissCross.McpServer.Tests/Templates/TemplateValidatorTests.cs`

**RED:** Tests assert exactly eight valid target/mode combinations:

- WPF NavigationOnly
- WPF NavigationAndUi
- Avalonia NavigationOnly
- Avalonia NavigationAndUi
- MAUI NavigationOnly
- MAUI NavigationAndUi
- WinForms NavigationOnly
- WinForms NavigationAndUi

Additional RED tests:

- Empty `AppName` returns error diagnostic.
- Invalid root namespace returns error diagnostic.
- WinForms/WPF non-windows TFM returns error diagnostic.
- Unknown control for target returns warning diagnostic.

**GREEN:** Add manifest entries and validator logic only sufficient to pass tests.

### Task 9: Implement template renderer and wizard output

**Objective:** Generate file manifests/content for all target/mode combinations.

**Files:**
- Create: `src/CrissCross.McpServer.Core/Templates/TemplateRenderer.cs`
- Create: `src/CrissCross.McpServer.Core/Templates/TemplateWizard.cs`
- Fill template files under `Templates/`
- Test: `tests/CrissCross.McpServer.Tests/Templates/TemplateWizardTests.cs`
- Test: `tests/CrissCross.McpServer.Tests/Templates/TemplateRendererSnapshotTests.cs`
- Test data: `tests/CrissCross.McpServer.Tests/TestData/ExpectedTemplates/*.txt`

**RED:** For each target/mode, assert generated files include required startup/shell/VM/view files and do not include files from other platforms.

Example filtered command:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/TemplateWizardGeneratesWpfNavigationOnlyFiles"
```

Expected RED: missing renderer/wizard.

**GREEN:** Implement renderer and wizard. Use simple token replacement first; introduce Scriban only when tests require conditionals/loops.

Snapshot expectations:

- WPF navigation-only contains `.WithWpf().BuildApp()` and `SetMainNavigationHost`.
- WPF UI contains `CrissCross.WPF.UI` and `CrissCrossWpfDictionary`.
- Avalonia navigation-only contains `.UseReactiveUI` and Avalonia host/view files.
- Avalonia UI contains `CrissCross.Avalonia.UI` resources/styles.
- MAUI navigation-only contains `.WithMaui().BuildApp()` and `NavigationShell`.
- MAUI UI contains `UseCrissCrossMauiUiResources`.
- WinForms navigation-only contains `ApplicationConfiguration.Initialize()` and `.WithWinForms().BuildApp()`.
- WinForms UI does not invent unavailable WPF/Avalonia controls.

### Task 10: Implement core service facade for MCP adapters

**Objective:** Provide one core service API for tools/resources/prompts.

**Files:**
- Create/modify: `src/CrissCross.McpServer.Core/Catalog/CrissCrossKnowledgeCatalog.cs`
- Create: `src/CrissCross.McpServer.Core/Resources/ResourceRouter.cs`
- Test: `tests/CrissCross.McpServer.Tests/Mcp/McpToolAdapterTests.cs` can start with core facade tests if MCP project is not wired yet.

**RED:** Tests assert methods return non-empty deterministic results:

- `GetPackageMatrix`
- `GetStartupRecipe`
- `GetNavigationRecipe`
- `FindControl`
- `ReviewCodeSnippet`
- `GenerateProjectStarter`
- `ReadResource("crisscross://packages/matrix")`

**GREEN:** Compose catalog/reviewer/template services behind facade methods.

### Task 11: Implement MCP tool adapters

**Objective:** Expose core facade through ModelContextProtocol tool attributes.

**Files:**
- Create: `src/CrissCross.McpServer/Mcp/CrissCrossTools.cs`
- Create: `src/CrissCross.McpServer/Mcp/McpResultMapper.cs`
- Test: `tests/CrissCross.McpServer.Tests/Mcp/McpToolAdapterTests.cs`

**RED:** Tests instantiate `CrissCrossTools` with a core facade and assert:

- Tool methods return serialized text/JSON that includes expected catalog data.
- `crisscross_generate_project_starter` returns generated file entries and diagnostics.
- No tool writes to stdout.

Use a stdout capture test if practical:

```csharp
var original = Console.Out;
using var writer = new StringWriter();
Console.SetOut(writer);
try { /* call tool */ }
finally { Console.SetOut(original); }
await Assert.That(writer.ToString()).IsEmpty();
```

**GREEN:** Implement tool methods. Use MCP SDK attributes verified from package IntelliSense/build, e.g. tool type/method attributes from `ModelContextProtocol.Server`. Keep method bodies one-line calls into core plus result mapping.

### Task 12: Implement MCP resources

**Objective:** Expose resource URIs through core `ResourceRouter`.

**Files:**
- Create: `src/CrissCross.McpServer/Mcp/CrissCrossResources.cs`
- Test: `tests/CrissCross.McpServer.Tests/Mcp/McpResourceAdapterTests.cs`

**RED:** Tests assert `crisscross://startup/wpf`, `crisscross://navigation/core`, and `crisscross://templates/wpf/navigation-only` return text containing expected source-backed guidance.

**GREEN:** Implement resource adapter and route URIs to `ResourceRouter`.

### Task 13: Implement MCP prompts

**Objective:** Provide guided prompts that direct agents to the right tool/resource first.

**Files:**
- Create: `src/CrissCross.McpServer/Mcp/CrissCrossPrompts.cs`
- Test: `tests/CrissCross.McpServer.Tests/Mcp/McpPromptAdapterTests.cs`

**RED:** Tests assert prompt text includes required tool names and anti-pattern guardrails:

- `generate-crisscross-app` mentions `crisscross_generate_project_starter`.
- `review-crisscross-code` mentions `crisscross_review_code_snippet`.
- `generate-crisscross-viewmodel` bans `RxApp` and requires `RxObject`.

**GREEN:** Implement prompt methods using SDK prompt attributes verified from package docs/build.

### Task 14: Wire stdio host

**Objective:** Make the console executable start the MCP server over stdio without stdout pollution.

**Files:**
- Create/modify: `src/CrissCross.McpServer/Program.cs`
- Modify: `src/CrissCross.McpServer/CrissCross.McpServer.csproj`
- Test: `tests/CrissCross.McpServer.Tests/Mcp/HostConfigurationTests.cs`

**RED:** Add tests for host setup if feasible without starting stdio:

- Host registers core facade.
- Logging configuration routes console logs to stderr threshold.
- Tool/resource/prompt adapter types are registered.

Run filtered host tests; expect missing host builder failure.

**GREEN:** Implement:

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
    options.LogToStandardErrorThreshold = LogLevel.Trace);

builder.Services.AddSingleton(CrissCrossKnowledgeCatalog.CreateDefault());
// Add core services/facades.
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<CrissCrossTools>()
    .WithResources<CrissCrossResources>()
    .WithPrompts<CrissCrossPrompts>();

await builder.Build().RunAsync();
```

If the SDK uses different exact registration method names for resources/prompts in version 1.3.0, use the package-correct names and update tests to verify discovery.

### Task 15: Add MCP discovery probe

**Objective:** Verify real MCP clients can discover server capabilities.

**Files:**
- No production files unless discovery exposes adapter issues.
- Optional script: `scripts/probe-mcp-discovery.ps1` or `scripts/probe-mcp-discovery.sh` under target repo.
- Docs: `docs/architecture.md` discovery section.

**RED:** Run discovery before host is wired or before expected tool count exists; record failure.

Probe command from parent handoff:

```bash
"$DOTNET" build src/CrissCross.McpServer/CrissCross.McpServer.csproj -c Release
npm_config_cache="$PWD/.npm-cache" npx -y mcporter list --stdio "'/mnt/c/Program Files/dotnet/dotnet.exe' '$PWD/src/CrissCross.McpServer/bin/Release/net10.0/CrissCross.McpServer.dll'" --name crisscross
```

Expected GREEN: discovered tools include `crisscross_get_package_matrix`, `crisscross_get_startup_recipe`, `crisscross_generate_project_starter`, and `crisscross_review_code_snippet`. If `mcporter` is unavailable, use the MCP Inspector or a small JSON-RPC stdio probe script and record the substitute command.

### Task 16: Documentation and README

**Objective:** Document server purpose, local development, tools/resources/prompts, Template Wizard, and verification.

**Files:**
- Modify/create: `README.md`
- Create: `docs/architecture.md`
- Create: `docs/template-wizard.md`

**RED:** Add documentation tests if possible, e.g. test that every documented tool name exists in the tool catalog or adapter constants.

Run:

```bash
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release -- --treenode-filter "/*/*/*/ReadmeDocumentsEveryTool"
```

Expected RED: missing docs or tool list mismatch.

**GREEN:** Update docs. Include all build/test/probe commands and stdio logging warning.

---

## Acceptance gates

| Phase | Required proof | Coherent files |
| --- | --- | --- |
| Scaffold | `dotnet restore`, `dotnet build -warnaserror`, smoke test pass | solution, props, csprojs, first test |
| Catalogs | Package/startup/navigation/control tests pass | `Catalog/`, `Knowledge/`, catalog tests |
| Review | Every reviewer rule has RED/GREEN evidence and tests pass | `Review/`, reviewer tests |
| Templates | Eight target/mode combinations pass manifest/render tests | `Templates/`, template tests, snapshots |
| MCP adapters | Tool/resource/prompt adapter tests pass | `src/CrissCross.McpServer/Mcp/` |
| Host | Host config tests pass and build succeeds | `Program.cs`, host project |
| Discovery | MCP probe lists expected capabilities | probe output in handoff |
| Docs | README/docs list exactly implemented surfaces | README, docs |

Full final verification command set:

```bash
cd /mnt/c/Projects/GitHub/ChrisPulman/CrissCross.MCP.Server
DOTNET="/mnt/c/Program Files/dotnet/dotnet.exe"
"$DOTNET" --info
"$DOTNET" restore CrissCross.MCP.Server.slnx
"$DOTNET" build CrissCross.MCP.Server.slnx -c Release --no-restore -warnaserror
"$DOTNET" test --project tests/CrissCross.McpServer.Tests/CrissCross.McpServer.Tests.csproj -c Release
"$DOTNET" build src/CrissCross.McpServer/CrissCross.McpServer.csproj -c Release
npm_config_cache="$PWD/.npm-cache" npx -y mcporter list --stdio "'/mnt/c/Program Files/dotnet/dotnet.exe' '$PWD/src/CrissCross.McpServer/bin/Release/net10.0/CrissCross.McpServer.dll'" --name crisscross
```

---

## Kill criteria

Stop and ask for human review if any of these occur:

- The target path contains unexpected user files or a non-matching repository history that would be overwritten by scaffold steps.
- The Windows dotnet SDK cannot restore/build net10.0 projects.
- `ModelContextProtocol` 1.3.0 does not expose resource/prompt registration APIs compatible with the adapter plan; propose the package-correct equivalent before continuing.
- Template Wizard output needs to write files outside the target repo.
- Generated starter projects require modifying the read-only CrissCross source repo.
- The MCP discovery probe cannot run and no substitute JSON-RPC probe can be installed or written under the target repo.

---

## Implementation handoff requirements

For every implementation card, record:

- Exact files changed.
- Exact RED command and failure reason.
- Exact GREEN command and passing result.
- Full final test command result.
- Any package version changes from this plan.
- Any source API changes discovered in `/mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross`.

For code-changing kanban work, leave a `review-required` handoff with changed files, tests run, and discovery output before marking terminal completion.
