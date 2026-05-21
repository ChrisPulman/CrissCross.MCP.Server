# Architecture

## Shape

- `src/CrissCross.McpServer` is the thin MCP host and adapter layer.
- `src/CrissCross.McpServer.Core` contains deterministic catalogs, snippet review rules, resource routing, and Template Wizard extension points.
- `tests/CrissCross.McpServer.Tests` validates behavior without requiring a running MCP transport, then `mcporter` verifies real stdio tool discovery.

## Stdio discipline

The server uses `Host.CreateApplicationBuilder`, clears default logging providers, then adds console logging with `LogToStandardErrorThreshold = LogLevel.Trace`. This prevents diagnostic pollution on stdout, which is reserved for MCP JSON-RPC.

## Dependency injection

The host registers:

- `CrissCrossKnowledgeCatalog.CreateDefault()` as the core deterministic knowledge facade.
- `ResourceRouter` for `crisscross://` resource reads.
- MCP tools/resources/prompts from the server assembly.

## Knowledge sources

The target repository owns committed, deterministic knowledge surfaces. The CrissCross source repository at `/mnt/c/Projects/GitHub/ReactiveMarbles/CrissCross` is read-only reference material.

Source-backed APIs and files represented in this slice include:

- `RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp()` from WPF examples.
- Avalonia `.UseReactiveUI(...)` startup examples.
- MAUI `.WithMaui().BuildApp()` and `UseCrissCrossMauiUiResources()`.
- WinForms `ApplicationConfiguration.Initialize()` and `.WithWinForms().BuildApp()`.
- `NavigationRegistry`, `IBidirectionalNavigator`, `IViewModelRoutedViewHost`, and `SetMainNavigationHost`.
- WPF/Avalonia page navigation `INavigationService` and `IPageService`.
- UI controls such as `SearchBox`, `DataPager`, `ValidationSummary`, `ThemeSwitcher`, `CommandButton`, and `BusyOverlay` where source-backed.

## Failure modes guarded by tests

- Use of `RxApp` in generated snippets.
- Missing platform startup builders.
- Missing navigation host registration.
- Empty host names.
- Platform UI types leaking into core projects.
- Deep mutation of snapshot state models.
- Confusing WPF.UI page navigation with view-model-host navigation.
