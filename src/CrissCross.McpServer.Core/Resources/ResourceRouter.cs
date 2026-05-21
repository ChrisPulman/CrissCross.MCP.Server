using System.Text.Json;
using System.Text.Json.Serialization;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Resources;

/// <summary>
/// Routes <c>crisscross://</c> resource URIs to deterministic catalog-backed content.
/// </summary>
public sealed class ResourceRouter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly CrissCrossKnowledgeCatalog _catalog;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceRouter"/> class.
    /// </summary>
    /// <param name="catalog">The CrissCross knowledge catalog used to serve resources.</param>
    public ResourceRouter(CrissCrossKnowledgeCatalog catalog) => _catalog = catalog;

    /// <summary>
    /// Creates a router using the default CrissCross knowledge catalog.
    /// </summary>
    /// <returns>A default resource router.</returns>
    public static ResourceRouter CreateDefault() => new(CrissCrossKnowledgeCatalog.CreateDefault());

    /// <summary>
    /// Reads a resource by URI.
    /// </summary>
    /// <param name="uri">The full <c>crisscross://</c> resource URI.</param>
    /// <returns>The routed resource content.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the URI is not a known CrissCross resource.</exception>
    public string ReadResource(string uri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri);
        var parts = uri.Replace("crisscross://", string.Empty, StringComparison.OrdinalIgnoreCase).Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts is ["packages", "matrix"])
        {
            return JsonSerializer.Serialize(_catalog.GetPackageMatrix(), JsonOptions);
        }

        if (parts is ["startup", var startupPlatform])
        {
            return ToMarkdown(_catalog.GetStartupRecipe(startupPlatform));
        }

        if (parts is ["navigation", "core"])
        {
            return ToMarkdown(_catalog.GetNavigationRecipe("navigation-only"));
        }

        if (parts is ["navigation", var kind])
        {
            return ToMarkdown(_catalog.GetNavigationRecipe(kind));
        }

        if (parts is ["controls", var platformOnly])
        {
            var target = ParseTarget(platformOnly);
            return JsonSerializer.Serialize(_catalog.GetPackageMatrix(platformOnly).Concat(Array.Empty<PackageInfo>()).ToArray(), JsonOptions) + "\n" + string.Join("\n", ControlNames(target));
        }

        if (parts is ["controls", var controlPlatform, var controlName])
        {
            return JsonSerializer.Serialize(_catalog.FindControl(controlPlatform, controlName), JsonOptions);
        }

        if (parts is ["state-models"])
        {
            return _catalog.GetStateModelGuidance();
        }

        if (parts is ["templates", var templatePlatform, var mode])
        {
            var target = ParseTarget(templatePlatform);
            var wizardMode = ParseMode(mode);
            var sample = _catalog.GenerateProjectStarter(new TemplateGenerationRequest(
                AppName: "SampleApp",
                RootNamespace: "SampleApp",
                Target: target,
                Mode: wizardMode,
                TargetFramework: target is FrameworkTarget.Wpf or FrameworkTarget.WinForms ? "net10.0-windows10.0.19041.0" : "net10.0",
                HostName: "MainHost",
                Screens: new[] { "Home", "Settings" },
                IncludeControls: wizardMode == WizardMode.NavigationAndUi ? new[] { "CommandButton", "SearchBox" } : Array.Empty<string>(),
                IncludeTests: true,
                IncludeReadme: true,
                UseCentralPackageManagement: true,
                OverwriteExistingFiles: false));
            return ToMarkdown(templatePlatform, mode, sample);
        }

        if (parts is ["quality", "anti-patterns"])
        {
            return "# CrissCross anti-patterns\n- RxApp usage\n- Missing startup builder\n- Missing SetMainNavigationHost\n- Empty HostName\n- Deep state mutation";
        }

        if (parts is ["quality", "testing"])
        {
            return "# CrissCross testing\nUse TUnit/MTP and STA guidance for WPF UI tests. Validate generated file manifests before compiling platform starters.";
        }

        throw new InvalidOperationException($"Unknown CrissCross resource URI '{uri}'.");
    }

    private static string ToMarkdown(StartupRecipe recipe) => $"""
        # {recipe.Title}

        Packages: {string.Join(", ", recipe.RequiredPackages)}

        ```csharp
        {recipe.CodeSnippet}
        ```

        Files:
        {string.Join("\n", recipe.RequiredFiles.Select(file => $"- {file}"))}

        Gotchas:
        {string.Join("\n", recipe.Gotchas.Select(gotcha => $"- {gotcha}"))}
        """;

    private static string ToMarkdown(NavigationRecipe recipe) => $"""
        # {recipe.Kind}

        {recipe.Summary}

        ```csharp
        {recipe.CodeSnippet}
        ```

        Setup:
        {string.Join("\n", recipe.RequiredSetup.Select(item => $"- {item}"))}
        """;

    private static string ToMarkdown(string platform, string mode, TemplateGenerationResult result) => $"""
        # Template {platform}/{mode}

        Mode: {result.Request.Mode}
        Target framework: {result.Request.TargetFramework}
        Host: {result.Request.HostName}

        Use `crisscross_generate_project_starter` before emitting files. The wizard returns validation diagnostics, next steps, source-template names, and complete generated-file previews; it never writes files.

        Packages:
        {string.Join("\n", result.NextSteps.Where(step => step.Contains("packages", StringComparison.OrdinalIgnoreCase)).Select(step => $"- {step}"))}

        Sample generated preview paths:
        {string.Join("\n", result.Files.Select(file => $"- {file.RelativePath} ({file.SourceTemplate})"))}
        """;

    private static FrameworkTarget ParseTarget(string value) => value.ToLowerInvariant() switch
    {
        "wpf" => FrameworkTarget.Wpf,
        "avalonia" => FrameworkTarget.Avalonia,
        "maui" => FrameworkTarget.Maui,
        "winforms" => FrameworkTarget.WinForms,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown platform.")
    };

    private static WizardMode ParseMode(string value) => value.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
    {
        "navigationonly" => WizardMode.NavigationOnly,
        "navigationandui" => WizardMode.NavigationAndUi,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown wizard mode.")
    };

    private static IEnumerable<string> ControlNames(FrameworkTarget target) => target switch
    {
        FrameworkTarget.WinForms => Array.Empty<string>(),
        _ => new[] { "SearchBox", "DataPager", "ValidationSummary", "ThemeSwitcher", "CommandButton", "BusyOverlay" }
    };
}
