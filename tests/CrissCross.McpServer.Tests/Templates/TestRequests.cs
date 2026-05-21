using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

internal static class TestRequests
{
    public static TemplateGenerationRequest ValidWpf() => new(
        AppName: "SampleApp",
        RootNamespace: "SampleApp",
        Target: FrameworkTarget.Wpf,
        Mode: WizardMode.NavigationOnly,
        TargetFramework: "net10.0-windows10.0.19041.0",
        HostName: "MainHost",
        Screens: new[] { "Home" },
        IncludeControls: Array.Empty<string>(),
        IncludeTests: true,
        IncludeReadme: true,
        UseCentralPackageManagement: true,
        OverwriteExistingFiles: false);

    public static TemplateGenerationRequest ValidMauiUi() => ValidWpf() with
    {
        Target = FrameworkTarget.Maui,
        Mode = WizardMode.NavigationAndUi,
        TargetFramework = "net10.0",
        IncludeControls = new[] { "CommandButton", "SearchBox" }
    };
}
