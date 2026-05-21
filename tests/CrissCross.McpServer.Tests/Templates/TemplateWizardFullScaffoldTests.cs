using System.Text.RegularExpressions;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

public sealed class TemplateWizardFullScaffoldTests
{
    private static readonly Regex BindingRegex = new("\\{Binding\\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);
    private static readonly Regex PublicPropertyRegex = new("public\\s+(?:[\\w<>,\\s?\\[\\].]+)\\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\\s*(?:\\{|=>)", RegexOptions.Compiled);

    [Test]
    public async Task TemplateWizardGeneratesCompleteProjectPreviewForEveryFrameworkAndMode()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();
        foreach (var target in Enum.GetValues<FrameworkTarget>())
        {
            foreach (var mode in Enum.GetValues<WizardMode>())
            {
                var request = RequestFor(target, mode);
                var result = catalog.GenerateProjectStarter(request);
                var paths = result.Files.Select(file => file.RelativePath).ToArray();
                var combined = string.Join("\n---FILE---\n", result.Files.Select(file => file.Content));

                await Assert.That(result.Diagnostics.Where(diagnostic => diagnostic.Severity == ValidationSeverity.Error)).IsEmpty();
                await Assert.That(paths).Contains($"{request.AppName}.csproj");
                await Assert.That(paths).Contains("Directory.Packages.props");
                await Assert.That(paths).Contains("README.md");
                await Assert.That(paths).Contains("ViewModels/HomeViewModel.cs");
                await Assert.That(paths).Contains("ViewModels/SettingsViewModel.cs");
                await Assert.That(paths).Contains("Tests/GeneratedTemplateSmokeTests.cs");
                await Assert.That(paths).IsEquivalentTo(paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
                await Assert.That(result.Files.All(file => !Path.IsPathRooted(file.RelativePath) && file.SourceTemplate is not null)).IsTrue();
                await Assert.That(combined).Contains("HomeViewModel");
                await Assert.That(combined).Contains("SettingsViewModel");
                await Assert.That(combined).Contains(request.HostName!);
                await Assert.That(combined).Contains(ExpectedStartupCall(target));

                if (mode == WizardMode.NavigationAndUi)
                {
                    await Assert.That(paths.Any(path => path.Contains("ControlsGallery", StringComparison.OrdinalIgnoreCase))).IsTrue();
                    await Assert.That(combined).Contains(ExpectedUiMarker(target));
                }
                else
                {
                    await Assert.That(paths.Any(path => path.Contains("ControlsGallery", StringComparison.OrdinalIgnoreCase))).IsFalse();
                }
            }
        }
    }

    [Test]
    public async Task TemplateWizardProducesFrameworkSpecificNavigationAndUiControlPreviews()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        var wpf = catalog.GenerateProjectStarter(RequestFor(FrameworkTarget.Wpf, WizardMode.NavigationAndUi));
        var avalonia = catalog.GenerateProjectStarter(RequestFor(FrameworkTarget.Avalonia, WizardMode.NavigationAndUi));
        var maui = catalog.GenerateProjectStarter(RequestFor(FrameworkTarget.Maui, WizardMode.NavigationAndUi));
        var winForms = catalog.GenerateProjectStarter(RequestFor(FrameworkTarget.WinForms, WizardMode.NavigationAndUi));

        await Assert.That(FileContent(wpf, "Views/ControlsGalleryView.xaml")).Contains("ui:CommandButton");
        await Assert.That(FileContent(wpf, "Views/ControlsGalleryView.xaml")).Contains("ui:SearchBox");
        await Assert.That(FileContent(wpf, "App.xaml")).Contains("ui:ControlsDictionary");

        await Assert.That(FileContent(avalonia, "Views/ControlsGalleryView.axaml")).Contains("controls:CommandButton");
        await Assert.That(FileContent(avalonia, "Views/ControlsGalleryView.axaml")).Contains("controls:SearchBox");
        await Assert.That(FileContent(avalonia, "App.axaml")).Contains("avares://CrissCross.Avalonia.UI/Themes/Index.axaml");

        await Assert.That(FileContent(maui, "Views/ControlsGalleryView.xaml")).Contains("mauiui:CommandButton");
        await Assert.That(FileContent(maui, "Views/ControlsGalleryView.xaml")).Contains("mauiui:SearchBox");
        await Assert.That(FileContent(maui, "App.xaml.cs")).Contains("UseCrissCrossMauiUiResources");

        await Assert.That(FileContent(winForms, "Views/ControlsGalleryForm.cs")).Contains("ReactiveCommand.CreateFromTask");
        await Assert.That(FileContent(winForms, "Views/ControlsGalleryForm.cs")).Contains("new Button");
        await Assert.That(winForms.Diagnostics.Any(diagnostic => diagnostic.RuleId == "TPL006" && diagnostic.Severity == ValidationSeverity.Info)).IsTrue();
    }

    [Test]
    public async Task TemplateWizardNavigationAndUiBindingsResolveAgainstGeneratedControlGalleryViewModel()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();
        var targets = new[]
        {
            (Target: FrameworkTarget.Wpf, ViewPath: "Views/ControlsGalleryView.xaml"),
            (Target: FrameworkTarget.Avalonia, ViewPath: "Views/ControlsGalleryView.axaml"),
            (Target: FrameworkTarget.Maui, ViewPath: "Views/ControlsGalleryView.xaml")
        };

        foreach (var (target, viewPath) in targets)
        {
            var result = catalog.GenerateProjectStarter(RequestFor(target, WizardMode.NavigationAndUi));
            var view = FileContent(result, viewPath);
            var viewModel = FileContent(result, "ViewModels/ControlsGalleryViewModel.cs");
            var viewModelProperties = PublicPropertyRegex.Matches(viewModel)
                .Select(match => match.Groups["name"].Value)
                .ToHashSet(StringComparer.Ordinal);
            var missingBindings = BindingRegex.Matches(view)
                .Select(match => match.Groups["name"].Value)
                .Distinct(StringComparer.Ordinal)
                .Where(binding => !viewModelProperties.Contains(binding))
                .Select(binding => $"{target}:{binding}")
                .ToArray();

            await Assert.That(missingBindings).IsEmpty();
        }
    }

    [Test]
    public async Task TemplateWizardMauiScreenViewsDoNotReferenceUndeclaredViewModelNamespace()
    {
        var result = CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(RequestFor(FrameworkTarget.Maui, WizardMode.NavigationAndUi));
        var unresolvedViewModelPrefixes = result.Files
            .Where(file => file.RelativePath.StartsWith("Views/", StringComparison.OrdinalIgnoreCase)
                && file.RelativePath.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase)
                && file.Content.Contains("viewModels:", StringComparison.Ordinal)
                && !file.Content.Contains("xmlns:viewModels=", StringComparison.Ordinal))
            .Select(file => file.RelativePath)
            .ToArray();

        await Assert.That(unresolvedViewModelPrefixes).IsEmpty();
    }

    [Test]
    public async Task TemplateWizardRejectsUnsafeNamesAndScreenNamesBeforePreviewingFiles()
    {
        var request = RequestFor(FrameworkTarget.Avalonia, WizardMode.NavigationOnly) with
        {
            AppName = "../Bad App",
            RootNamespace = "Bad-App",
            Screens = new[] { "Home", "../Admin" },
            HostName = ""
        };

        var result = CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request);

        await Assert.That(result.Files).IsEmpty();
        await Assert.That(result.Diagnostics.Any(diagnostic => diagnostic.RuleId == "TPL002")).IsTrue();
        await Assert.That(result.Diagnostics.Any(diagnostic => diagnostic.RuleId == "TPL005")).IsTrue();
        await Assert.That(result.Diagnostics.Any(diagnostic => diagnostic.RuleId == "TPL007")).IsTrue();
        await Assert.That(result.Diagnostics.Any(diagnostic => diagnostic.RuleId == "TPL008")).IsTrue();
    }

    private static TemplateGenerationRequest RequestFor(FrameworkTarget target, WizardMode mode) => TestRequests.ValidWpf() with
    {
        Target = target,
        Mode = mode,
        TargetFramework = target is FrameworkTarget.Wpf or FrameworkTarget.WinForms ? "net10.0-windows10.0.19041.0" : "net10.0",
        HostName = "MainHost",
        Screens = new[] { "Home", "Settings" },
        IncludeControls = mode == WizardMode.NavigationAndUi ? new[] { "CommandButton", "SearchBox" } : Array.Empty<string>()
    };

    private static string FileContent(TemplateGenerationResult result, string relativePath) =>
        result.Files.Single(file => string.Equals(file.RelativePath, relativePath, StringComparison.OrdinalIgnoreCase)).Content;

    private static string ExpectedStartupCall(FrameworkTarget target) => target switch
    {
        FrameworkTarget.Wpf => ".WithWpf().BuildApp()",
        FrameworkTarget.Avalonia => ".UseReactiveUI",
        FrameworkTarget.Maui => ".WithMaui().BuildApp()",
        FrameworkTarget.WinForms => ".WithWinForms().BuildApp()",
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };

    private static string ExpectedUiMarker(FrameworkTarget target) => target switch
    {
        FrameworkTarget.Wpf => "CrissCross.WPF.UI",
        FrameworkTarget.Avalonia => "CrissCross.Avalonia.UI",
        FrameworkTarget.Maui => "CrissCross.Maui.UI",
        FrameworkTarget.WinForms => "WinForms has no separate CrissCross UI package",
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
    };
}
