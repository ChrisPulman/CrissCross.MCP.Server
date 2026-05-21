using System.ComponentModel;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

[McpServerToolType]
public static class CrissCrossTools
{
    [McpServerTool]
    [Description("Return CrissCross packages, target frameworks, dependencies, source paths, and setup notes.")]
    public static string crisscross_get_package_matrix(string? platform = null, string? targetFramework = null) =>
        McpResultMapper.ToJson(Catalog().GetPackageMatrix(platform, targetFramework));

    [McpServerTool]
    [Description("Return exact CrissCross startup calls, resource dictionaries, package references, and gotchas for a platform.")]
    public static string crisscross_get_startup_recipe(string platform, string? uiMode = null) =>
        McpResultMapper.ToJson(Catalog().GetStartupRecipe(platform, uiMode));

    [McpServerTool]
    [Description("Return navigation guidance for NavigationRegistry, hosted view-model navigation, page navigation, or NavigationView flows.")]
    public static string crisscross_get_navigation_recipe(string kind, string? platform = null, string? hostName = null, string? contract = null) =>
        McpResultMapper.ToJson(Catalog().GetNavigationRecipe(kind, ParseOptionalTarget(platform), hostName, contract));

    [McpServerTool]
    [Description("Find a CrissCross control by platform and feature/name.")]
    public static string crisscross_find_control(string platform, string nameOrFeature) =>
        McpResultMapper.ToJson(Catalog().FindControl(platform, nameOrFeature));

    [McpServerTool]
    [Description("Generate a CrissCross RxObject view model snippet with ReactiveCommand and replacement-state guidance.")]
    public static string crisscross_generate_viewmodel(string feature, string className, string @namespace, string? navigationMode = null) =>
        Catalog().GenerateViewModel(feature, className, @namespace, navigationMode);

    [McpServerTool]
    [Description("Generate a NavigationRegistry snippet from a mapping specification.")]
    public static string crisscross_generate_navigation_registry(string mappingSpec) =>
        Catalog().GenerateNavigationRegistry(mappingSpec);

    [McpServerTool]
    [Description("Review a C# snippet for deterministic CrissCross anti-pattern diagnostics.")]
    public static string crisscross_review_code_snippet(string code, string? platform = null, string? projectKind = null) =>
        McpResultMapper.ToJson(Catalog().ReviewCodeSnippet(code, ParseOptionalTarget(platform), projectKind));

    [McpServerTool]
    [Description("Generate a starter project manifest and file contents for a CrissCross platform/mode. Does not write files.")]
    public static string crisscross_generate_project_starter(
        string platform,
        string mode,
        string appName,
        string rootNamespace,
        string screensCsv = "Home",
        string controlsCsv = "",
        string? targetFramework = null,
        string? hostName = "MainHost")
    {
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

        return McpResultMapper.ToJson(Catalog().GenerateProjectStarter(request), "TemplateGenerationResult<GeneratedFile>");
    }

    [McpServerTool]
    [Description("Explain common CrissCross errors and likely fixes.")]
    public static string crisscross_explain_error(string message, string? platform = null) =>
        Catalog().ExplainError(message, ParseOptionalTarget(platform));

    private static CrissCrossKnowledgeCatalog Catalog() => CrissCrossKnowledgeCatalog.CreateDefault();

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
