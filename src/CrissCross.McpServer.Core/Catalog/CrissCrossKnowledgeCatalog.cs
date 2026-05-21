using System.Text;
using System.Text.RegularExpressions;
using CrissCross.McpServer.Core.Review;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Catalog;

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

public sealed class CrissCrossKnowledgeCatalog
{
    private readonly IReadOnlyList<PackageInfo> _packages;
    private readonly IReadOnlyList<StartupRecipe> _startupRecipes;
    private readonly IReadOnlyList<NavigationRecipe> _navigationRecipes;
    private readonly IReadOnlyList<ControlInfo> _controls;
    private readonly CrissCrossSnippetReviewer _reviewer;

    private CrissCrossKnowledgeCatalog(
        IReadOnlyList<PackageInfo> packages,
        IReadOnlyList<StartupRecipe> startupRecipes,
        IReadOnlyList<NavigationRecipe> navigationRecipes,
        IReadOnlyList<ControlInfo> controls,
        CrissCrossSnippetReviewer reviewer)
    {
        _packages = packages;
        _startupRecipes = startupRecipes;
        _navigationRecipes = navigationRecipes;
        _controls = controls;
        _reviewer = reviewer;
    }

    public static CrissCrossKnowledgeCatalog CreateDefault() => new(
        CreatePackages(),
        CreateStartupRecipes(),
        CreateNavigationRecipes(),
        CreateControls(),
        new CrissCrossSnippetReviewer());

    public IReadOnlyList<PackageInfo> GetPackageMatrix(string? platform = null, string? targetFramework = null)
    {
        var target = TryParseTarget(platform);
        return _packages
            .Where(package => target is null || package.Platform == target || package.Platform is null)
            .Where(package => string.IsNullOrWhiteSpace(targetFramework) || package.TargetFrameworks.Any(tfm => string.Equals(tfm, targetFramework, StringComparison.OrdinalIgnoreCase)) || package.Notes.Any(note => note.Contains(targetFramework, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    public StartupRecipe GetStartupRecipe(FrameworkTarget target, WizardMode? mode = null) =>
        _startupRecipes.First(recipe => recipe.Target == target && (recipe.Mode == mode || recipe.Mode is null));

    public StartupRecipe GetStartupRecipe(string platform, string? uiMode = null) =>
        GetStartupRecipe(ParseTarget(platform), ParseMode(uiMode));

    public NavigationRecipe GetNavigationRecipe(string kind, FrameworkTarget? platform = null, string? hostName = null, string? contract = null)
    {
        var recipe = _navigationRecipes.First(candidate => string.Equals(candidate.Kind, kind, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(hostName) || !string.IsNullOrWhiteSpace(contract))
        {
            return recipe with
            {
                Summary = $"{recipe.Summary} Host name: {hostName ?? "<default>"}. Contract: {contract ?? "<none>"}."
            };
        }

        return recipe;
    }

    public ControlInfo? FindControl(FrameworkTarget target, string nameOrFeature) =>
        _controls.FirstOrDefault(control => control.Target == target &&
            (control.Name.Contains(nameOrFeature, StringComparison.OrdinalIgnoreCase) ||
             control.Features.Any(feature => feature.Contains(nameOrFeature, StringComparison.OrdinalIgnoreCase))));

    public ControlInfo? FindControl(string platform, string nameOrFeature) => FindControl(ParseTarget(platform), nameOrFeature);

    public string GetStateModelGuidance() =>
        "State models such as SearchQueryState, ValidationSummaryState, ThemePreferenceState, and CommandButtonStatus are snapshots: replace immutable/snapshot state values; do not deep-mutate nested values. Use RaiseAndSetIfChanged with a new state instance.";

    public IReadOnlyList<ReviewDiagnostic> ReviewCodeSnippet(string code, FrameworkTarget? platform = null, string? projectKind = null) =>
        _reviewer.Review(code, platform, projectKind);

    public IReadOnlyList<(FrameworkTarget Target, WizardMode Mode)> GetTemplateCombinations() =>
        Enum.GetValues<FrameworkTarget>().SelectMany(_ => Enum.GetValues<WizardMode>(), (target, mode) => (target, mode)).ToArray();

    public TemplateGenerationResult GenerateProjectStarter(TemplateGenerationRequest request) =>
        CrissCross.McpServer.Core.Templates.TemplateWizardEngine.Generate(request);

    public string GenerateViewModel(string feature, string className, string @namespace, string? navigationMode = null) => $$"""
        using System.Reactive;
        using System.Reactive.Linq;
        using ReactiveUI;
        using CrissCross;

        namespace {{@namespace}};

        public sealed class {{className}} : RxObject
        {
            private readonly ObservableAsPropertyHelper<bool> _isBusy;
            private string _title = "{{feature}}";

            public {{className}}()
            {
                Save = ReactiveCommand.CreateFromTask(SaveAsync);
                _isBusy = Save.IsExecuting.ToProperty(this, x => x.IsBusy, scheduler: RxSchedulers.MainThreadScheduler);
            }

            public string Title
            {
                get => _title;
                set => this.RaiseAndSetIfChanged(ref _title, value);
            }

            public bool IsBusy => _isBusy.Value;

            public ReactiveCommand<Unit, Unit> Save { get; }

            private static Task SaveAsync() => Task.CompletedTask;
        }
        """;

    public string GenerateNavigationRegistry(string mappingSpec) => $$"""
        var registry = new NavigationRegistry()
            .Register<HomeViewModel, HomeView>(contract: "Home");

        IBidirectionalNavigator navigator = registry.CreateNavigator(serviceProvider);
        // Mapping spec: {{mappingSpec}}
        // Validate duplicate contracts and unknown contracts in tests before navigation.
        """;

    public string ExplainError(string message, FrameworkTarget? platform = null)
    {
        if (message.Contains("host", StringComparison.OrdinalIgnoreCase))
        {
            return "No navigation host or host name ambiguity: configure HostName and call SetMainNavigationHost/Setup before navigation.";
        }

        if (message.Contains("view", StringComparison.OrdinalIgnoreCase))
        {
            return "Missing view registration: register view-model/view pairs through NavigationRegistry or platform view locator.";
        }

        return "Check CrissCross startup builder, package reference, view registration, and host-name configuration for the selected platform.";
    }

    private static IReadOnlyList<ValidationDiagnostic> ValidateTemplateRequest(TemplateGenerationRequest request)
    {
        var diagnostics = new List<ValidationDiagnostic>();
        if (string.IsNullOrWhiteSpace(request.AppName))
        {
            diagnostics.Add(new("TPL001", ValidationSeverity.Error, "AppName is required.", SuggestedFix: "Provide a non-empty app name."));
        }

        if (!Regex.IsMatch(request.RootNamespace, "^[A-Za-z_][A-Za-z0-9_.]*$"))
        {
            diagnostics.Add(new("TPL002", ValidationSeverity.Error, "RootNamespace must be a valid C# namespace.", SuggestedFix: "Start with a letter or underscore and use dot-separated identifiers."));
        }

        if ((request.Target == FrameworkTarget.Wpf || request.Target == FrameworkTarget.WinForms) && !request.TargetFramework.Contains("windows", StringComparison.OrdinalIgnoreCase))
        {
            diagnostics.Add(new("TPL003", ValidationSeverity.Error, "WPF and WinForms starters require a Windows target framework.", SuggestedFix: "Use net10.0-windows10.0.19041.0."));
        }

        foreach (var control in request.IncludeControls.Where(control => CreateControls().All(info => info.Target != request.Target || !string.Equals(info.Name, control, StringComparison.OrdinalIgnoreCase))))
        {
            diagnostics.Add(new("TPL004", ValidationSeverity.Warning, $"Control '{control}' is not source-backed for {request.Target}.", SuggestedFix: "Choose a control from crisscross_find_control for the target."));
        }

        return diagnostics;
    }

    private static IEnumerable<GeneratedFile> GenerateFiles(TemplateGenerationRequest request)
    {
        var ns = request.RootNamespace;
        var hostName = string.IsNullOrWhiteSpace(request.HostName) ? "MainHost" : request.HostName;
        yield return new GeneratedFile("ViewModels/HomeViewModel.cs", $$"""
            using ReactiveUI;
            using CrissCross;

            namespace {{ns}}.ViewModels;

            public sealed class HomeViewModel : RxObject
            {
                private string _title = "Home";
                public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
            }
            """, SourceTemplate: "Templates/shared/ViewModels/HomeViewModel.cs.scriban");

        switch (request.Target)
        {
            case FrameworkTarget.Wpf:
                yield return new GeneratedFile("App.xaml.cs", $$"""
                    using System.Windows;
                    using CrissCross;

                    namespace {{ns}};

                    public partial class App : Application
                    {
                        public App() => RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
                    }
                    """, SourceTemplate: "Templates/wpf/App.xaml.cs.scriban");
                yield return new GeneratedFile("MainWindow.xaml.cs", $$"""
                    using CrissCross;
                    using CrissCross.WPF;

                    namespace {{ns}};

                    public partial class MainWindow : NavigationWindow<ViewModels.HomeViewModel>
                    {
                        public MainWindow()
                        {
                            InitializeComponent();
                            NavigationFrame.HostName = "{{hostName}}";
                            this.SetMainNavigationHost(NavigationFrame);
                        }
                    }
                    """, SourceTemplate: "Templates/wpf/MainWindow.xaml.cs.scriban");
                if (request.Mode == WizardMode.NavigationAndUi)
                {
                    yield return new GeneratedFile("Themes/CrissCross.xaml", "<rxNav:CrissCrossWpfDictionary />\n<!-- CrissCross.WPF.UI CommandButton SearchBox DataPager ValidationSummary ThemeSwitcher -->", SourceTemplate: "Templates/wpf/CrissCross.xaml.scriban");
                }
                break;
            case FrameworkTarget.Avalonia:
                yield return new GeneratedFile("Program.cs", $$"""
                    using Avalonia;
                    using Avalonia.ReactiveUI;

                    namespace {{ns}};

                    internal static class Program
                    {
                        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().UseReactiveUI(b => { });
                    }
                    """, SourceTemplate: "Templates/avalonia/Program.cs.scriban");
                yield return new GeneratedFile("Views/HomeView.axaml", "<UserControl xmlns:rx=\"https://github.com/reactivemarbles/CrissCross\" />", SourceTemplate: "Templates/avalonia/Views/HomeView.axaml.scriban");
                if (request.Mode == WizardMode.NavigationAndUi)
                {
                    yield return new GeneratedFile("Styles/CrissCross.axaml", "<StyleInclude Source=\"avares://CrissCross.Avalonia/Themes/Index.axaml\" />\n<!-- CrissCross.Avalonia.UI controls -->", SourceTemplate: "Templates/avalonia/Styles/CrissCross.axaml.scriban");
                }
                break;
            case FrameworkTarget.Maui:
                yield return new GeneratedFile("MauiProgram.cs", $$"""
                    using CrissCross;
                    using CrissCross.MAUI;
                    using CrissCross.Maui.UI;

                    namespace {{ns}};

                    public static class MauiProgram
                    {
                        public static MauiApp CreateMauiApp()
                        {
                            var builder = MauiApp.CreateBuilder();
                            builder.UseMauiApp<App>();
                            Resources.UseCrissCrossMauiUiResources();
                            RxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp();
                            return builder.Build();
                        }
                    }
                    """, SourceTemplate: "Templates/maui/MauiProgram.cs.scriban");
                yield return new GeneratedFile("AppShell.xaml.cs", "using CrissCross.MAUI;\npublic partial class AppShell : NavigationShell { }", SourceTemplate: "Templates/maui/AppShell.xaml.cs.scriban");
                break;
            case FrameworkTarget.WinForms:
                yield return new GeneratedFile("Program.cs", $$"""
                    using CrissCross;
                    using System.Windows.Forms;

                    namespace {{ns}};

                    internal static class Program
                    {
                        [STAThread]
                        private static void Main()
                        {
                            ApplicationConfiguration.Initialize();
                            RxAppBuilder.CreateReactiveUIBuilder().WithWinForms().BuildApp();
                            Application.Run(new MainForm());
                        }
                    }
                    """, SourceTemplate: "Templates/winforms/Program.cs.scriban");
                yield return new GeneratedFile("MainForm.cs", "using CrissCross.WinForms;\npublic sealed partial class MainForm : NavigationForm<HomeViewModel> { }", SourceTemplate: "Templates/winforms/MainForm.cs.scriban");
                break;
        }
    }

    private static IReadOnlyList<string> RequiredPackages(FrameworkTarget target, WizardMode mode)
    {
        var packages = target switch
        {
            FrameworkTarget.Wpf => new List<string> { "CrissCross.WPF" },
            FrameworkTarget.Avalonia => new List<string> { "CrissCross.Avalonia" },
            FrameworkTarget.Maui => new List<string> { "CrissCross.MAUI" },
            FrameworkTarget.WinForms => new List<string> { "CrissCross.WinForms" },
            _ => new List<string> { "CrissCross" }
        };

        if (mode == WizardMode.NavigationAndUi)
        {
            packages.Add(target switch
            {
                FrameworkTarget.Wpf => "CrissCross.WPF.UI",
                FrameworkTarget.Avalonia => "CrissCross.Avalonia.UI",
                FrameworkTarget.Maui => "CrissCross.Maui.UI",
                FrameworkTarget.WinForms => "CrissCross.WinForms",
                _ => "CrissCross"
            });
        }

        return packages;
    }

    private static FrameworkTarget ParseTarget(string value) => TryParseTarget(value) ?? throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown CrissCross platform.");

    private static FrameworkTarget? TryParseTarget(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "core", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Replace("-ui", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
        {
            "wpf" => FrameworkTarget.Wpf,
            "avalonia" => FrameworkTarget.Avalonia,
            "maui" or "mauiui" => FrameworkTarget.Maui,
            "winforms" => FrameworkTarget.WinForms,
            _ => Enum.TryParse<FrameworkTarget>(value, ignoreCase: true, out var parsed) ? parsed : null
        };
    }

    private static WizardMode? ParseMode(string? value) => value?.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() switch
    {
        "navigationonly" => WizardMode.NavigationOnly,
        "navigationandui" or "ui" => WizardMode.NavigationAndUi,
        _ => null
    };

    private static IReadOnlyList<PackageInfo> CreatePackages() => new[]
    {
        new PackageInfo("CrissCross", "CrissCross core", null, new[] { "net10.0" }, Array.Empty<string>(), new[] { "src/CrissCross/CrissCross.csproj" }, new[] { "Core RxObject, RxSchedulers, NavigationRegistry, IViewModelRoutedViewHost abstractions." }),
        new PackageInfo("CrissCross.WPF", "CrissCross WPF", FrameworkTarget.Wpf, new[] { "net10.0-windows10.0.19041.0" }, new[] { "CrissCross" }, new[] { "src/CrissCross.WPF/CrissCross.WPF.csproj" }, new[] { "Requires UseWPF=true and RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp()." }),
        new PackageInfo("CrissCross.WPF.UI", "CrissCross WPF UI", FrameworkTarget.Wpf, new[] { "net10.0-windows10.0.19041.0" }, new[] { "CrissCross.WPF" }, new[] { "src/CrissCross.WPF.UI/CrissCross.WPF.UI.csproj" }, new[] { "Resource dictionaries and WPF UI controls such as SearchBox, DataPager, ValidationSummary, ThemeSwitcher, CommandButton." }),
        new PackageInfo("CrissCross.Avalonia", "CrissCross Avalonia", FrameworkTarget.Avalonia, new[] { "net10.0" }, new[] { "CrissCross" }, new[] { "src/CrissCross.Avalonia/CrissCross.Avalonia.csproj" }, new[] { "Use AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().UseReactiveUI(...)." }),
        new PackageInfo("CrissCross.Avalonia.UI", "CrissCross Avalonia UI", FrameworkTarget.Avalonia, new[] { "net10.0" }, new[] { "CrissCross.Avalonia" }, new[] { "src/CrissCross.Avalonia.UI/CrissCross.Avalonia.UI.csproj" }, new[] { "Style include avares://CrissCross.Avalonia/Themes/Index.axaml and controls." }),
        new PackageInfo("CrissCross.MAUI", "CrissCross MAUI", FrameworkTarget.Maui, new[] { "net10.0" }, new[] { "CrissCross" }, new[] { "src/CrissCross.MAUI/CrissCross.MAUI.csproj" }, new[] { "UseMaui=true and RxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp()." }),
        new PackageInfo("CrissCross.Maui.UI", "CrissCross MAUI UI", FrameworkTarget.Maui, new[] { "net10.0" }, new[] { "CrissCross.MAUI" }, new[] { "src/CrissCross.Maui.UI/CrissCross.Maui.UI.csproj" }, new[] { "UseCrissCrossMauiUiResources; controls include SearchBox, DataPager, ValidationSummary, ThemeSwitcher, CommandButton, BusyOverlay." }),
        new PackageInfo("CrissCross.WinForms", "CrissCross WinForms", FrameworkTarget.WinForms, new[] { "net10.0-windows10.0.19041.0" }, new[] { "CrissCross" }, new[] { "src/CrissCross.WinForms/CrissCross.WinForms.csproj" }, new[] { "UseWindowsForms=true; ApplicationConfiguration.Initialize plus .WithWinForms().BuildApp()." })
    };

    private static IReadOnlyList<StartupRecipe> CreateStartupRecipes() => new[]
    {
        new StartupRecipe(FrameworkTarget.Wpf, null, "WPF startup", new[] { "CrissCross.WPF" }, new[] { "App.xaml.cs", "<rxNav:CrissCrossWpfDictionary />" }, "RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();", new[] { "Add CrissCrossWpfDictionary for CrissCross resources; WPF UI tests need STA guidance." }, new[] { "src/CrissCross.WPF.Test/App.xaml.cs", "src/CrissCross.WPF/CrissCrossWpfDictionary.cs" }),
        new StartupRecipe(FrameworkTarget.Avalonia, null, "Avalonia startup", new[] { "CrissCross.Avalonia" }, new[] { "Program.cs", "avares://CrissCross.Avalonia/Themes/Index.axaml" }, "AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().UseReactiveUI(b => { });", new[] { "Keep UseReactiveUI on the Avalonia AppBuilder; include CrissCross Avalonia styles for UI mode." }, new[] { "src/CrissCross.Avalonia.Test.Desktop/Program.cs" }),
        new StartupRecipe(FrameworkTarget.Maui, WizardMode.NavigationAndUi, "MAUI startup with UI", new[] { "CrissCross.MAUI", "CrissCross.Maui.UI" }, new[] { "App.xaml.cs", "MauiProgram.cs" }, "Resources.UseCrissCrossMauiUiResources();\nRxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp();", new[] { "UseCrissCrossMauiUiResources is required for MAUI UI controls." }, new[] { "src/CrissCross.MAUI.Test/App.xaml.cs", "src/CrissCross.Maui.UI/MauiUiAppBuilderExtensions.cs" }),
        new StartupRecipe(FrameworkTarget.Maui, null, "MAUI startup", new[] { "CrissCross.MAUI" }, new[] { "App.xaml.cs", "MauiProgram.cs" }, "RxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp();", new[] { "NavigationShell is the MAUI IViewModelRoutedViewHost." }, new[] { "src/CrissCross.MAUI.Test/App.xaml.cs" }),
        new StartupRecipe(FrameworkTarget.WinForms, null, "WinForms startup", new[] { "CrissCross.WinForms" }, new[] { "Program.cs" }, "ApplicationConfiguration.Initialize();\nRxAppBuilder.CreateReactiveUIBuilder().WithWinForms().BuildApp();\nApplication.Run(new MainForm());", new[] { "Set non-empty HostName before hosted navigation Setup()." }, new[] { "src/CrissCross.WinForms.Test/Program.cs" })
    };

    private static IReadOnlyList<NavigationRecipe> CreateNavigationRecipes() => new[]
    {
        new NavigationRecipe("navigation-only", null, "NavigationRegistry maps view-models to views and creates an IBidirectionalNavigator.", "var registry = new NavigationRegistry().Register<HomeViewModel, HomeView>(contract: \"Home\");\nIBidirectionalNavigator navigator = registry.CreateNavigator(serviceProvider);", new[] { "CrissCross package", "registered view-model/view pairs" }, new[] { "duplicate contracts", "unknown contracts" }, new[] { "src/CrissCross/Navigation/NavigationRegistry.cs", "src/CrissCross/Navigation/IBidirectionalNavigator.cs" }),
        new NavigationRecipe("viewmodel-host", null, "Use IViewModelRoutedViewHost for hosted VM navigation; configure a stable host name before navigating.", "IViewModelRoutedViewHost host = NavigationFrame;\nthis.SetMainNavigationHost(host);\n// HostName = \"MainHost\" when multiple hosts exist.", new[] { "platform host control", "SetMainNavigationHost" }, new[] { "host name ambiguity", "missing Setup" }, new[] { "src/CrissCross/ViewModelRoutedViewHostMixins.cs", "src/CrissCross/IViewModelRoutedViewHost.cs" }),
        new NavigationRecipe("page-navigation", FrameworkTarget.Wpf, "WPF.UI page navigation uses INavigationService/IPageService and is not the same as VM-host navigation.", "public MainWindow(IPageService pageService, INavigationService navigationService)\n{\n    navigationService.SetPageService(pageService);\n    navigationService.Navigate(typeof(HomePage));\n}", new[] { "CrissCross.WPF.UI", "IPageService", "INavigationService" }, new[] { "using SetMainNavigationHost for page navigation" }, new[] { "src/CrissCross.WPF.UI/NavigationService.cs", "src/CrissCross.WPF.UI/Services/PageService.cs" }),
        new NavigationRecipe("navigation-view", null, "NavigationView controls present page/service navigation and should not be confused with NavigationRegistry VM routing.", "// Resolve INavigationService and IPageService, then wire the NavigationView root.", new[] { "UI package" }, new[] { "mixing contracts with page tags" }, new[] { "src/CrissCross.Avalonia.UI/Controls/NavigationView" })
    };

    private static IReadOnlyList<ControlInfo> CreateControls() => new[]
    {
        Control("SearchBox", FrameworkTarget.Wpf, "CrissCross.WPF.UI", "SearchQueryState", "src/CrissCross.WPF.UI/Controls/SearchBox/SearchBox.cs"),
        Control("DataPager", FrameworkTarget.Wpf, "CrissCross.WPF.UI", null, "src/CrissCross.WPF.UI/Controls/DataPager/DataPager.cs"),
        Control("ValidationSummary", FrameworkTarget.Wpf, "CrissCross.WPF.UI", "ValidationSummaryState", "src/CrissCross.WPF.UI/Controls/ValidationSummary/ValidationSummary.cs"),
        Control("ThemeSwitcher", FrameworkTarget.Wpf, "CrissCross.WPF.UI", "ThemePreferenceState", "src/CrissCross.WPF.UI/Controls/ThemeSwitcher/ThemeSwitcher.cs"),
        Control("CommandButton", FrameworkTarget.Wpf, "CrissCross.WPF.UI", "CommandButtonStatus", "src/CrissCross.WPF.UI/Controls/CommandButton/CommandButton.cs"),
        Control("BusyOverlay", FrameworkTarget.Wpf, "CrissCross.WPF.UI", null, "src/CrissCross.WPF.UI/Controls/BusyOverlay/BusyOverlay.cs"),
        Control("SearchBox", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", "SearchQueryState", "src/CrissCross.Avalonia.UI/Controls/SearchBox/SearchBox.cs"),
        Control("DataPager", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", null, "src/CrissCross.Avalonia.UI/Controls/DataPager/DataPager.cs"),
        Control("ValidationSummary", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", "ValidationSummaryState", "src/CrissCross.Avalonia.UI/Controls/ValidationSummary/ValidationSummary.cs"),
        Control("ThemeSwitcher", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", "ThemePreferenceState", "src/CrissCross.Avalonia.UI/Controls/ThemeSwitcher/ThemeSwitcher.cs"),
        Control("CommandButton", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", "CommandButtonStatus", "src/CrissCross.Avalonia.UI/Controls/CommandButton/CommandButton.cs"),
        Control("BusyOverlay", FrameworkTarget.Avalonia, "CrissCross.Avalonia.UI", null, "src/CrissCross.Avalonia.UI/Controls/BusyOverlay/BusyOverlay.cs"),
        Control("SearchBox", FrameworkTarget.Maui, "CrissCross.Maui.UI", "SearchQueryState", "src/CrissCross.Maui.UI/Controls/SearchBox.cs"),
        Control("DataPager", FrameworkTarget.Maui, "CrissCross.Maui.UI", null, "src/CrissCross.Maui.UI/Controls/DataPager.cs"),
        Control("ValidationSummary", FrameworkTarget.Maui, "CrissCross.Maui.UI", "ValidationSummaryState", "src/CrissCross.Maui.UI/Controls/ValidationSummary.cs"),
        Control("ThemeSwitcher", FrameworkTarget.Maui, "CrissCross.Maui.UI", "ThemePreferenceState", "src/CrissCross.Maui.UI/Controls/ThemeSwitcher.cs"),
        Control("CommandButton", FrameworkTarget.Maui, "CrissCross.Maui.UI", "CommandButtonStatus", "src/CrissCross.Maui.UI/Controls/CommandButton.cs"),
        Control("BusyOverlay", FrameworkTarget.Maui, "CrissCross.Maui.UI", null, "src/CrissCross.Maui.UI/Controls/BusyOverlay.cs")
    };

    private static ControlInfo Control(string name, FrameworkTarget target, string package, string? stateModel, string source) =>
        new(name, target, package, stateModel, new[] { name, "reactive", stateModel ?? "control" }, new[] { source }, $"<!-- Use {name} from {package}; bind by replacing {stateModel ?? "view-model"} state snapshots. -->");
}
