---
name: crisscross
description: "Use the CrissCross MCP server for source-backed CrissCross package, startup, navigation, controls, review, and preview-only project starter guidance across WPF, Avalonia, MAUI, and WinForms."
---

# CrissCross MCP Usage

Use this skill when a task involves CrissCross, ReactiveMarbles CrissCross packages, platform startup, hosted navigation, CrissCross UI controls, state models, code review, or generating a CrissCross starter preview.

## Default Workflow

1. Start with the MCP tools instead of relying on memory.
2. Call `crisscross_get_package_matrix` before choosing package references or target frameworks.
3. Call `crisscross_get_startup_recipe` for platform startup, resource dictionaries, and gotchas.
4. Call `crisscross_get_navigation_recipe` before wiring `NavigationRegistry`, hosted view-model navigation, WPF page navigation, or `NavigationView` flows.
5. Call `crisscross_find_control` before using a CrissCross UI control.
6. Call `crisscross_review_code_snippet` before finalizing generated or edited CrissCross code.
7. For new starter projects, call `crisscross_generate_project_starter` and use only the returned generated-file previews after reviewing diagnostics.

## Guardrails

- Do not use `RxApp` in generated CrissCross code.
- Prefer `RxObject`, `RaiseAndSetIfChanged`, `ReactiveCommand.CreateFromTask`, `ObservableAsPropertyHelper`, and `RxSchedulers`.
- Treat state models as replacement snapshots; do not deep-mutate nested state.
- Use a stable non-empty host name before hosted navigation setup.
- Keep `NavigationRegistry` view-model routing separate from WPF.UI page-service navigation.
- Keep platform UI types out of core projects.

## Tool Selection

- `crisscross_get_package_matrix`: choose packages, target frameworks, dependencies, and source references.
- `crisscross_get_startup_recipe`: get exact startup calls for WPF, Avalonia, MAUI, or WinForms.
- `crisscross_get_navigation_recipe`: select a navigation pattern and identify setup/failure points.
- `crisscross_find_control`: find source-backed controls and state-model guidance by platform.
- `crisscross_generate_viewmodel`: produce a compact CrissCross view-model snippet.
- `crisscross_generate_navigation_registry`: produce a `NavigationRegistry` snippet.
- `crisscross_review_code_snippet`: detect CrissCross anti-patterns before committing code.
- `crisscross_generate_project_starter`: return preview-only starter project files and diagnostics.
- `crisscross_explain_error`: map common CrissCross errors to likely fixes.

## Resources

Use `crisscross://` resources for read-only reference material:

- `crisscross://packages/matrix`
- `crisscross://startup/{wpf|avalonia|maui|winforms}`
- `crisscross://navigation/core`
- `crisscross://navigation/{navigation-only|viewmodel-host|page-navigation|navigation-view}`
- `crisscross://controls/{platform}`
- `crisscross://controls/{platform}/{controlName}`
- `crisscross://state-models`
- `crisscross://templates/{platform}/{navigation-only|navigation-and-ui}`
- `crisscross://quality/anti-patterns`
- `crisscross://quality/testing`

## Starter Generation

`crisscross_generate_project_starter` is preview-only. It returns file paths, contents, diagnostics, source-template names, and next steps; it does not write files. Review diagnostics first, then write the returned files only when the destination and overwrite behavior are clear from the user request.
