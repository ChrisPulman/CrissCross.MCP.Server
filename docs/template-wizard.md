# Template Wizard

The Template Wizard exposes deterministic CrissCross starter-project previews through MCP. It validates input, returns diagnostics and next steps, and previews complete file contents; it never writes generated files to disk.

## Entry point

MCP tool:

```text
crisscross_generate_project_starter(platform, mode, appName, rootNamespace, screensCsv?, controlsCsv?, targetFramework?, hostName?)
```

Core extension point:

```csharp
public interface ITemplateWizard
{
    TemplateGenerationResult Generate(TemplateGenerationRequest request);
}
```

Template resource:

```text
crisscross://templates/{wpf|avalonia|maui|winforms}/{navigation-only|navigation-and-ui}
```

The resource returns a framework/mode manifest, package guidance, and sample generated preview paths. The tool returns `TemplateGenerationResult` JSON with:

- `request`: normalized request values.
- `files`: generated-file previews with relative path, content, and `sourceTemplate` provenance.
- `diagnostics`: errors, warnings, and info messages.
- `nextSteps`: restore/build/review guidance for the caller.

## Supported combinations

Eight target/mode combinations are generated and tested:

- WPF `NavigationOnly`
- WPF `NavigationAndUi`
- Avalonia `NavigationOnly`
- Avalonia `NavigationAndUi`
- MAUI `NavigationOnly`
- MAUI `NavigationAndUi`
- WinForms `NavigationOnly`
- WinForms `NavigationAndUi`

## Generated scaffold shape

All valid requests include these shared preview files:

- `{AppName}.csproj`
- `Directory.Packages.props` when central package management is requested
- `ViewModels/{Screen}ViewModel.cs` for every screen in `screensCsv`
- `ViewModels/NavigationRegistration.cs`
- `Tests/GeneratedTemplateSmokeTests.cs` when tests are requested
- `README.md` when documentation is requested

Framework-specific navigation files:

| Framework | Navigation previews |
| --- | --- |
| WPF | `App.xaml`, `App.xaml.cs`, `MainWindow.xaml`, `MainWindow.xaml.cs`, `Views/{Screen}View.xaml` |
| Avalonia | `Program.cs`, `App.axaml`, `App.axaml.cs`, `Views/MainWindow.axaml`, `Views/MainWindow.axaml.cs`, `Views/{Screen}View.axaml` |
| MAUI | `MauiProgram.cs`, `App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`, `Views/{Screen}View.xaml` |
| WinForms | `Program.cs`, `MainForm.cs`, `Views/{Screen}View.cs` |

UI mode adds:

| Framework | UI previews |
| --- | --- |
| WPF | `ViewModels/ControlsGalleryViewModel.cs`, `Views/ControlsGalleryView.xaml`, `CrissCross.WPF.UI`, `ui:ControlsDictionary`, `ui:ThemesDictionary` |
| Avalonia | `ViewModels/ControlsGalleryViewModel.cs`, `Views/ControlsGalleryView.axaml`, `CrissCross.Avalonia.UI`, `avares://CrissCross.Avalonia.UI/Themes/Index.axaml` |
| MAUI | `ViewModels/ControlsGalleryViewModel.cs`, `Views/ControlsGalleryView.xaml`, `CrissCross.Maui.UI`, `Resources.UseCrissCrossMauiUiResources()` |
| WinForms | `ViewModels/ControlsGalleryViewModel.cs`, `Views/ControlsGalleryForm.cs`; diagnostic `TPL006` explains that WinForms has no separate CrissCross UI package |

## Example MCP calls

Navigation-only WPF starter:

```json
{
  "tool": "crisscross_generate_project_starter",
  "arguments": {
    "platform": "wpf",
    "mode": "navigation-only",
    "appName": "SampleApp",
    "rootNamespace": "SampleApp",
    "screensCsv": "Home,Settings",
    "targetFramework": "net10.0-windows10.0.19041.0",
    "hostName": "MainHost"
  }
}
```

Avalonia navigation + UI starter:

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

Template manifest resource:

```text
crisscross://templates/maui/navigation-and-ui
```

## Validation diagnostics

- `TPL001`: app name is required.
- `TPL002`: root namespace must be a valid C# namespace.
- `TPL003`: WPF and WinForms require a Windows TFM such as `net10.0-windows10.0.19041.0`.
- `TPL004`: unknown or unsupported controls are warnings, not silent generation.
- `TPL005`: app name must be a safe project identifier without spaces/path traversal.
- `TPL006`: WinForms UI mode uses reactive WinForms controls because there is no separate CrissCross UI package.
- `TPL007`: host name must be a non-empty safe identifier.
- `TPL008`: screens must be present and safe C# identifiers.

## Usage guidance for agents

1. Call `crisscross://templates/{platform}/{mode}` to inspect the expected file set.
2. Call `crisscross_generate_project_starter` with explicit `screensCsv`, `hostName`, and target framework.
3. Review `diagnostics`; do not write files when any diagnostic has severity `Error`.
4. If files are edited after preview, call `crisscross_review_code_snippet` on changed snippets before committing.
5. Build and run platform-specific UI smoke tests in the generated project.
