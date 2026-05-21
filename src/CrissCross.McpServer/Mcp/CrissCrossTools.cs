using System.ComponentModel;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

/// <summary>
/// MCP tools that expose CrissCross package, startup, navigation, review, and template guidance.
/// </summary>
[McpServerToolType]
public sealed class CrissCrossTools
{
    /// <summary>
    /// Returns CrissCross package metadata filtered by platform or target framework.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="platform">Optional platform filter such as WPF, Avalonia, MAUI, or WinForms.</param>
    /// <param name="targetFramework">Optional target framework filter.</param>
    /// <returns>A JSON payload containing matching package metadata.</returns>
    [McpServerTool(Name = "crisscross_get_package_matrix")]
    [Description("Return CrissCross packages, target frameworks, dependencies, source paths, and setup notes.")]
    public static string crisscross_get_package_matrix(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Optional platform filter such as wpf, avalonia, maui, or winforms.")] string? platform = null,
        [Description("Optional target framework filter such as net10.0 or net10.0-windows10.0.19041.0.")] string? targetFramework = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return McpResultMapper.ToJson(catalog.GetPackageMatrix(platform, targetFramework));
    }

    /// <summary>
    /// Returns the startup recipe for a CrissCross platform.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <param name="uiMode">Optional wizard mode or UI-mode hint.</param>
    /// <returns>A JSON payload containing startup calls, packages, files, and gotchas.</returns>
    [McpServerTool(Name = "crisscross_get_startup_recipe")]
    [Description("Return exact CrissCross startup calls, resource dictionaries, package references, and gotchas for a platform.")]
    public static string crisscross_get_startup_recipe(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Platform such as wpf, avalonia, maui, or winforms.")] string platform,
        [Description("Optional mode such as navigation-only or navigation-and-ui.")] string? uiMode = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return McpResultMapper.ToJson(catalog.GetStartupRecipe(platform, uiMode));
    }

    /// <summary>
    /// Returns guidance for a CrissCross navigation pattern.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="kind">The navigation recipe kind.</param>
    /// <param name="platform">Optional platform hint.</param>
    /// <param name="hostName">Optional hosted-navigation host name.</param>
    /// <param name="contract">Optional navigation contract.</param>
    /// <returns>A JSON payload containing the selected navigation recipe.</returns>
    [McpServerTool(Name = "crisscross_get_navigation_recipe")]
    [Description("Return navigation guidance for NavigationRegistry, hosted view-model navigation, page navigation, or NavigationView flows.")]
    public static string crisscross_get_navigation_recipe(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Recipe kind such as navigation-only, viewmodel-host, page-navigation, or navigation-view.")] string kind,
        [Description("Optional platform such as wpf, avalonia, maui, or winforms.")] string? platform = null,
        [Description("Optional hosted navigation host name.")] string? hostName = null,
        [Description("Optional navigation contract.")] string? contract = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return McpResultMapper.ToJson(catalog.GetNavigationRecipe(kind, ParseOptionalTarget(platform), hostName, contract));
    }

    /// <summary>
    /// Finds a CrissCross UI control for a platform by name or feature text.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <param name="nameOrFeature">A control name or feature search term.</param>
    /// <returns>A JSON payload containing the matching control, or <see langword="null"/> when no control matches.</returns>
    [McpServerTool(Name = "crisscross_find_control")]
    [Description("Find a CrissCross control by platform and feature/name.")]
    public static string crisscross_find_control(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Platform such as wpf, avalonia, maui, or winforms.")] string platform,
        [Description("Control name or feature such as SearchBox, paging, or validation.")] string nameOrFeature)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return McpResultMapper.ToJson(catalog.FindControl(platform, nameOrFeature));
    }

    /// <summary>
    /// Generates a CrissCross view-model snippet for an agent to adapt.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="feature">The feature name used in the generated snippet.</param>
    /// <param name="className">The generated view-model class name.</param>
    /// <param name="namespace">The generated view-model namespace.</param>
    /// <param name="navigationMode">Optional navigation-mode hint for caller context.</param>
    /// <returns>A C# view-model snippet.</returns>
    [McpServerTool(Name = "crisscross_generate_viewmodel")]
    [Description("Generate a CrissCross RxObject view model snippet with ReactiveCommand and replacement-state guidance.")]
    public static string crisscross_generate_viewmodel(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Feature name to place in the generated view-model title.")] string feature,
        [Description("Safe C# class name for the generated view model.")] string className,
        [Description("Safe C# namespace for the generated view model.")] string @namespace,
        [Description("Optional navigation mode hint.")] string? navigationMode = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return catalog.GenerateViewModel(feature, className, @namespace, navigationMode);
    }

    /// <summary>
    /// Generates a CrissCross navigation registry snippet.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="mappingSpec">A caller-supplied description of the intended view-model/view mappings.</param>
    /// <returns>A C# navigation registry snippet.</returns>
    [McpServerTool(Name = "crisscross_generate_navigation_registry")]
    [Description("Generate a NavigationRegistry snippet from a mapping specification.")]
    public static string crisscross_generate_navigation_registry(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Description of view-model, view, and contract mappings to include as guidance.")] string mappingSpec)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return catalog.GenerateNavigationRegistry(mappingSpec);
    }

    /// <summary>
    /// Reviews a C# snippet for deterministic CrissCross anti-patterns.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="code">The C# snippet to review.</param>
    /// <param name="platform">Optional platform context.</param>
    /// <param name="projectKind">Optional project-kind context.</param>
    /// <returns>A JSON payload containing review diagnostics.</returns>
    [McpServerTool(Name = "crisscross_review_code_snippet")]
    [Description("Review a C# snippet for deterministic CrissCross anti-pattern diagnostics.")]
    public static string crisscross_review_code_snippet(
        CrissCrossKnowledgeCatalog catalog,
        [Description("C# code snippet to review.")] string code,
        [Description("Optional platform such as wpf, avalonia, maui, or winforms.")] string? platform = null,
        [Description("Optional project kind such as core or wpf-ui-page.")] string? projectKind = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return McpResultMapper.ToJson(catalog.ReviewCodeSnippet(code, ParseOptionalTarget(platform), projectKind));
    }

    /// <summary>
    /// Generates preview-only starter project files for a CrissCross platform and mode.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <param name="mode">The wizard mode.</param>
    /// <param name="appName">The generated application/project name.</param>
    /// <param name="rootNamespace">The generated root namespace.</param>
    /// <param name="screensCsv">Comma-separated screen names.</param>
    /// <param name="controlsCsv">Comma-separated control names for UI mode.</param>
    /// <param name="targetFramework">Optional target framework override.</param>
    /// <param name="hostName">Optional navigation host name.</param>
    /// <returns>A JSON payload containing diagnostics, next steps, and generated-file previews.</returns>
    [McpServerTool(Name = "crisscross_generate_project_starter")]
    [Description("Generate a starter project manifest and file contents for a CrissCross platform/mode. Does not write files.")]
    public static string crisscross_generate_project_starter(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Platform such as wpf, avalonia, maui, or winforms.")] string platform,
        [Description("Mode such as navigation-only or navigation-and-ui.")] string mode,
        [Description("Safe C# project/application name.")] string appName,
        [Description("Safe C# root namespace.")] string rootNamespace,
        [Description("Comma-separated screen names.")] string screensCsv = "Home",
        [Description("Comma-separated controls to include in UI mode.")] string controlsCsv = "",
        [Description("Optional target framework override.")] string? targetFramework = null,
        [Description("Optional CrissCross navigation host name.")] string? hostName = "MainHost")
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var target = ParseTarget(platform);
        var wizardMode = ParseMode(mode);
        var request = new TemplateGenerationRequest(
            appName,
            rootNamespace,
            target,
            wizardMode,
            targetFramework ?? DefaultTfm(target),
            hostName,
            SplitCsv(screensCsv),
            SplitCsv(controlsCsv),
            IncludeTests: true,
            IncludeReadme: true,
            UseCentralPackageManagement: true,
            OverwriteExistingFiles: false);

        return McpResultMapper.ToJson(catalog.GenerateProjectStarter(request), "TemplateGenerationResult<GeneratedFile>");
    }

    /// <summary>
    /// Explains a common CrissCross error and likely fix.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog.</param>
    /// <param name="message">The error message or symptom text.</param>
    /// <param name="platform">Optional platform context.</param>
    /// <returns>A concise explanation and fix direction.</returns>
    [McpServerTool(Name = "crisscross_explain_error")]
    [Description("Explain common CrissCross errors and likely fixes.")]
    public static string crisscross_explain_error(
        CrissCrossKnowledgeCatalog catalog,
        [Description("Error message or symptom text to explain.")] string message,
        [Description("Optional platform such as wpf, avalonia, maui, or winforms.")] string? platform = null)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        return catalog.ExplainError(message, ParseOptionalTarget(platform));
    }

    private static IReadOnlyList<string> SplitCsv(string? csv) => string.IsNullOrWhiteSpace(csv)
        ? Array.Empty<string>()
        : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static FrameworkTarget ParseTarget(string platform) => platform.Replace("-ui", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
    {
        "wpf" => FrameworkTarget.Wpf,
        "avalonia" => FrameworkTarget.Avalonia,
        "maui" => FrameworkTarget.Maui,
        "winforms" => FrameworkTarget.WinForms,
        _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, "Unknown CrissCross platform.")
    };

    private static FrameworkTarget? ParseOptionalTarget(string? platform) => string.IsNullOrWhiteSpace(platform) ? null : ParseTarget(platform);

    private static WizardMode ParseMode(string mode) => mode.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
    {
        "navigationonly" => WizardMode.NavigationOnly,
        "navigationandui" => WizardMode.NavigationAndUi,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown CrissCross wizard mode.")
    };

    private static string DefaultTfm(FrameworkTarget target) => target is FrameworkTarget.Wpf or FrameworkTarget.WinForms
        ? "net10.0-windows10.0.19041.0"
        : "net10.0";
}
