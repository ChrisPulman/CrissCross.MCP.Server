using System.Text.RegularExpressions;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

internal static partial class TemplateWizardEngine
{
    private static readonly Regex SafeIdentifier = SafeIdentifierRegex();
    private static readonly Regex SafeDottedIdentifier = SafeDottedIdentifierRegex();

    public static TemplateGenerationResult Generate(TemplateGenerationRequest request)
    {
        var diagnostics = Validate(request).ToList();
        var files = diagnostics.Any(diagnostic => diagnostic.Severity == ValidationSeverity.Error)
            ? Array.Empty<GeneratedFile>()
            : GenerateFiles(request).ToArray();

        var packages = RequiredPackages(request.Target, request.Mode);
        var nextSteps = new[]
        {
            $"Review {files.Length} generated-file previews and {diagnostics.Count} diagnostics before writing files for {request.AppName}.",
            $"Add/restore the required CrissCross packages: {string.Join(", ", packages)}.",
            "Use crisscross_review_code_snippet on edited generated files before committing.",
            "Run dotnet restore, dotnet build, and platform-specific UI smoke tests in the generated project."
        };

        return new TemplateGenerationResult(request, files, diagnostics, nextSteps);
    }

    public static IReadOnlyList<string> RequiredPackages(FrameworkTarget target, WizardMode mode)
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

        return packages.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static IEnumerable<ValidationDiagnostic> Validate(TemplateGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AppName))
        {
            yield return new("TPL001", ValidationSeverity.Error, "AppName is required.", SuggestedFix: "Provide a non-empty app name.");
        }
        else if (!SafeIdentifier.IsMatch(request.AppName))
        {
            yield return new("TPL005", ValidationSeverity.Error, "AppName must be a safe project identifier without spaces, path separators, or traversal.", SuggestedFix: "Use a value such as SampleApp.");
        }

        if (string.IsNullOrWhiteSpace(request.RootNamespace) || !SafeDottedIdentifier.IsMatch(request.RootNamespace))
        {
            yield return new("TPL002", ValidationSeverity.Error, "RootNamespace must be a valid C# namespace.", SuggestedFix: "Start with a letter or underscore and use dot-separated identifiers.");
        }

        if ((request.Target == FrameworkTarget.Wpf || request.Target == FrameworkTarget.WinForms) && !request.TargetFramework.Contains("windows", StringComparison.OrdinalIgnoreCase))
        {
            yield return new("TPL003", ValidationSeverity.Error, "WPF and WinForms starters require a Windows target framework.", SuggestedFix: "Use net10.0-windows10.0.19041.0.");
        }

        if (request.Screens.Count == 0)
        {
            yield return new("TPL008", ValidationSeverity.Error, "At least one screen is required so the wizard can generate navigation registrations.", SuggestedFix: "Provide screensCsv such as Home,Settings.");
        }

        foreach (var screen in request.Screens)
        {
            if (string.IsNullOrWhiteSpace(screen) || !SafeIdentifier.IsMatch(screen))
            {
                yield return new("TPL008", ValidationSeverity.Error, $"Screen '{screen}' must be a safe C# identifier.", SuggestedFix: "Use screen names such as Home,Settings,Details.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.HostName) || !SafeIdentifier.IsMatch(request.HostName))
        {
            yield return new("TPL007", ValidationSeverity.Error, "HostName must be a non-empty safe identifier for CrissCross hosted navigation.", SuggestedFix: "Use MainHost unless the project has multiple hosts.");
        }

        foreach (var control in request.IncludeControls.Where(control => !SupportedControls(request.Target).Contains(control, StringComparer.OrdinalIgnoreCase)))
        {
            yield return new("TPL004", ValidationSeverity.Warning, $"Control '{control}' is not source-backed for {request.Target}.", SuggestedFix: "Choose CommandButton, SearchBox, DataPager, ValidationSummary, ThemeSwitcher, or BusyOverlay for WPF/Avalonia/MAUI.");
        }

        if (request.Target == FrameworkTarget.WinForms && request.Mode == WizardMode.NavigationAndUi)
        {
            yield return new("TPL006", ValidationSeverity.Info, "WinForms has no separate CrissCross UI package; the wizard previews a reactive WinForms controls form plus CrissCross.WinForms navigation setup.");
        }
    }

    private static IEnumerable<GeneratedFile> GenerateFiles(TemplateGenerationRequest request)
    {
        yield return File($"{request.AppName}.csproj", ProjectFile(request), "Templates/shared/Project.csproj.scriban");
        if (request.UseCentralPackageManagement)
        {
            yield return File("Directory.Packages.props", CentralPackages(request), "Templates/shared/Directory.Packages.props.scriban");
        }

        foreach (var screen in NormalizedScreens(request))
        {
            yield return File($"ViewModels/{screen}ViewModel.cs", ScreenViewModel(request, screen), "Templates/shared/ViewModels/ScreenViewModel.cs.scriban");
        }

        yield return File("ViewModels/NavigationRegistration.cs", NavigationRegistration(request), "Templates/shared/ViewModels/NavigationRegistration.cs.scriban");

        foreach (var file in FrameworkFiles(request))
        {
            yield return file;
        }

        if (request.Mode == WizardMode.NavigationAndUi)
        {
            yield return File("ViewModels/ControlsGalleryViewModel.cs", ControlsGalleryViewModel(request), "Templates/shared/ViewModels/ControlsGalleryViewModel.cs.scriban");
            foreach (var file in UiFiles(request))
            {
                yield return file;
            }
        }

        if (request.IncludeTests)
        {
            yield return File("Tests/GeneratedTemplateSmokeTests.cs", SmokeTests(request), "Templates/shared/Tests/GeneratedTemplateSmokeTests.cs.scriban");
        }

        if (request.IncludeReadme)
        {
            yield return File("README.md", Readme(request), "Templates/shared/README.md.scriban");
        }
    }

    private static IEnumerable<GeneratedFile> FrameworkFiles(TemplateGenerationRequest request) => request.Target switch
    {
        FrameworkTarget.Wpf => WpfFiles(request),
        FrameworkTarget.Avalonia => AvaloniaFiles(request),
        FrameworkTarget.Maui => MauiFiles(request),
        FrameworkTarget.WinForms => WinFormsFiles(request),
        _ => Array.Empty<GeneratedFile>()
    };

    private static IEnumerable<GeneratedFile> WpfFiles(TemplateGenerationRequest request)
    {
        yield return File("App.xaml", $$"""
            <Application
                x:Class="{{request.RootNamespace}}.App"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:ui="https://github.com/reactivemarbles/CrissCross.ui"
                StartupUri="MainWindow.xaml">
                <Application.Resources>
                    <ResourceDictionary>
                        <ResourceDictionary.MergedDictionaries>
                            {{WpfResourceDictionaries(request)}}
                        </ResourceDictionary.MergedDictionaries>
                    </ResourceDictionary>
                </Application.Resources>
            </Application>
            """, "Templates/wpf/App.xaml.scriban");
        yield return File("App.xaml.cs", $$"""
            using System.Windows;
            using CrissCross;

            namespace {{request.RootNamespace}};

            public partial class App : Application
            {
                public App() => RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp();
            }
            """, "Templates/wpf/App.xaml.cs.scriban");
        yield return File("MainWindow.xaml", $$"""
            <Window
                x:Class="{{request.RootNamespace}}.MainWindow"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:cc="https://github.com/reactivemarbles/CrissCross"
                Title="{{request.AppName}}" Height="720" Width="1080">
                <Grid>
                    <cc:ViewModelRoutedViewHost x:Name="NavigationFrame" HostName="{{request.HostName}}" />
                </Grid>
            </Window>
            """, "Templates/wpf/MainWindow.xaml.scriban");
        yield return File("MainWindow.xaml.cs", $$"""
            using CrissCross;

            namespace {{request.RootNamespace}};

            public partial class MainWindow : NavigationWindow<ViewModels.HomeViewModel>
            {
                public MainWindow()
                {
                    InitializeComponent();
                    NavigationFrame.HostName = "{{request.HostName}}";
                    this.SetMainNavigationHost(NavigationFrame);
                    _ = ViewModels.NavigationRegistration.CreateRegistry();
                }
            }
            """, "Templates/wpf/MainWindow.xaml.cs.scriban");

        foreach (var screen in NormalizedScreens(request))
        {
            yield return File($"Views/{screen}View.xaml", WpfScreenView(request, screen), "Templates/wpf/Views/ScreenView.xaml.scriban");
        }
    }

    private static IEnumerable<GeneratedFile> AvaloniaFiles(TemplateGenerationRequest request)
    {
        yield return File("Program.cs", $$"""
            using Avalonia;
            using Avalonia.ReactiveUI;

            namespace {{request.RootNamespace}};

            internal static class Program
            {
                [STAThread]
                public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

                public static AppBuilder BuildAvaloniaApp() =>
                    AppBuilder.Configure<App>()
                        .UsePlatformDetect()
                        .WithInterFont()
                        .UseReactiveUI(_ => { });
            }
            """, "Templates/avalonia/Program.cs.scriban");
        yield return File("App.axaml", $$"""
            <Application
                x:Class="{{request.RootNamespace}}.App"
                xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
                <Application.Styles>
                    <FluentTheme />
                    <StyleInclude Source="avares://CrissCross.Avalonia/Themes/Index.axaml" />
                    {{AvaloniaUiStyleInclude(request)}}
                </Application.Styles>
            </Application>
            """, "Templates/avalonia/App.axaml.scriban");
        yield return File("App.axaml.cs", $$"""
            using Avalonia;
            using Avalonia.Controls.ApplicationLifetimes;
            using Avalonia.Markup.Xaml;

            namespace {{request.RootNamespace}};

            public partial class App : Application
            {
                public override void Initialize() => AvaloniaXamlLoader.Load(this);

                public override void OnFrameworkInitializationCompleted()
                {
                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow = new Views.MainWindow();
                    }

                    base.OnFrameworkInitializationCompleted();
                }
            }
            """, "Templates/avalonia/App.axaml.cs.scriban");
        yield return File("Views/MainWindow.axaml", $$"""
            <nav:NavigationWindow
                x:Class="{{request.RootNamespace}}.Views.MainWindow"
                xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:nav="https://github.com/reactivemarbles/CrissCross"
                Name="{{request.HostName}}"
                Title="{{request.AppName}}"
                Width="1080" Height="720" />
            """, "Templates/avalonia/Views/MainWindow.axaml.scriban");
        yield return File("Views/MainWindow.axaml.cs", $$"""
            using CrissCross;
            using ReactiveUI;

            namespace {{request.RootNamespace}}.Views;

            public partial class MainWindow : NavigationWindow<ViewModels.HomeViewModel>
            {
                public MainWindow()
                {
                    InitializeComponent();
                    Name = "{{request.HostName}}";
                    this.WhenActivated(_ => this.NavigateToView<ViewModels.HomeViewModel>());
                }
            }
            """, "Templates/avalonia/Views/MainWindow.axaml.cs.scriban");

        foreach (var screen in NormalizedScreens(request))
        {
            yield return File($"Views/{screen}View.axaml", AvaloniaScreenView(request, screen), "Templates/avalonia/Views/ScreenView.axaml.scriban");
        }
    }

    private static IEnumerable<GeneratedFile> MauiFiles(TemplateGenerationRequest request)
    {
        yield return File("MauiProgram.cs", $$"""
            using CrissCross;
            using CrissCross.MAUI;
            {{MauiUiUsing(request)}}

            namespace {{request.RootNamespace}};

            public static class MauiProgram
            {
                public static MauiApp CreateMauiApp()
                {
                    var builder = MauiApp.CreateBuilder();
                    builder.UseMauiApp<App>();
                    {{MauiUiResourceCall(request)}}
                    RxAppBuilder.CreateReactiveUIBuilder().WithMaui().BuildApp();
                    return builder.Build();
                }
            }
            """, "Templates/maui/MauiProgram.cs.scriban");
        yield return File("App.xaml", $$"""
            <?xml version="1.0" encoding="UTF-8" ?>
            <Application
                x:Class="{{request.RootNamespace}}.App"
                xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml" />
            """, "Templates/maui/App.xaml.scriban");
        yield return File("App.xaml.cs", $$"""
            {{MauiUiUsing(request)}}

            namespace {{request.RootNamespace}};

            public partial class App : Application
            {
                public App()
                {
                    InitializeComponent();
                    {{MauiUiResourceCall(request)}}
                    MainPage = new AppShell { Name = "{{request.HostName}}" };
                }
            }
            """, "Templates/maui/App.xaml.cs.scriban");
        yield return File("AppShell.xaml", $$"""
            <?xml version="1.0" encoding="UTF-8" ?>
            <nav:NavigationShell
                x:Class="{{request.RootNamespace}}.AppShell"
                xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                xmlns:nav="clr-namespace:CrissCross.MAUI;assembly=CrissCross.MAUI"
                x:Name="{{request.HostName}}" />
            """, "Templates/maui/AppShell.xaml.scriban");
        yield return File("AppShell.xaml.cs", $$"""
            using CrissCross.MAUI;

            namespace {{request.RootNamespace}};

            public partial class AppShell : NavigationShell
            {
                public AppShell()
                {
                    InitializeComponent();
                    Name = "{{request.HostName}}";
                }
            }
            """, "Templates/maui/AppShell.xaml.cs.scriban");

        foreach (var screen in NormalizedScreens(request))
        {
            yield return File($"Views/{screen}View.xaml", MauiScreenView(request, screen), "Templates/maui/Views/ScreenView.xaml.scriban");
        }
    }

    private static IEnumerable<GeneratedFile> WinFormsFiles(TemplateGenerationRequest request)
    {
        yield return File("Program.cs", $$"""
            using CrissCross;
            using System.Windows.Forms;

            namespace {{request.RootNamespace}};

            internal static class Program
            {
                [STAThread]
                private static void Main()
                {
                    ApplicationConfiguration.Initialize();
                    RxAppBuilder.CreateReactiveUIBuilder().WithWinForms().BuildApp();
                    Application.Run(new MainForm { Name = "{{request.HostName}}" });
                }
            }
            """, "Templates/winforms/Program.cs.scriban");
        yield return File("MainForm.cs", $$"""
            using CrissCross;
            using CrissCross.WinForms;

            namespace {{request.RootNamespace}};

            public sealed partial class MainForm : NavigationForm<ViewModels.HomeViewModel>
            {
                public MainForm()
                {
                    Name = "{{request.HostName}}";
                    Text = "{{request.AppName}}";
                    _ = ViewModels.NavigationRegistration.CreateRegistry();
                }
            }
            """, "Templates/winforms/MainForm.cs.scriban");

        foreach (var screen in NormalizedScreens(request))
        {
            yield return File($"Views/{screen}View.cs", WinFormsScreenView(request, screen), "Templates/winforms/Views/ScreenView.cs.scriban");
        }
    }

    private static IEnumerable<GeneratedFile> UiFiles(TemplateGenerationRequest request) => request.Target switch
    {
        FrameworkTarget.Wpf => new[] { File("Views/ControlsGalleryView.xaml", WpfControlsGalleryView(request), "Templates/wpf/Views/ControlsGalleryView.xaml.scriban") },
        FrameworkTarget.Avalonia => new[] { File("Views/ControlsGalleryView.axaml", AvaloniaControlsGalleryView(request), "Templates/avalonia/Views/ControlsGalleryView.axaml.scriban") },
        FrameworkTarget.Maui => new[] { File("Views/ControlsGalleryView.xaml", MauiControlsGalleryView(request), "Templates/maui/Views/ControlsGalleryView.xaml.scriban") },
        FrameworkTarget.WinForms => new[] { File("Views/ControlsGalleryForm.cs", WinFormsControlsGalleryForm(request), "Templates/winforms/Views/ControlsGalleryForm.cs.scriban") },
        _ => Array.Empty<GeneratedFile>()
    };

    private static string ProjectFile(TemplateGenerationRequest request) => $$"""
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>WinExe</OutputType>
            <TargetFramework>{{request.TargetFramework}}</TargetFramework>
            <RootNamespace>{{request.RootNamespace}}</RootNamespace>
            <UseWPF>{{Bool(request.Target == FrameworkTarget.Wpf)}}</UseWPF>
            <UseWindowsForms>{{Bool(request.Target == FrameworkTarget.WinForms)}}</UseWindowsForms>
            <UseMaui>{{Bool(request.Target == FrameworkTarget.Maui)}}</UseMaui>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
          </PropertyGroup>
          <ItemGroup>
        {{string.Join("\n", RequiredPackages(request.Target, request.Mode).Select(package => $"    <PackageReference Include=\"{package}\" />"))}}
          </ItemGroup>
        </Project>
        """;

    private static string CentralPackages(TemplateGenerationRequest request) => $$"""
        <Project>
          <ItemGroup>
        {{string.Join("\n", RequiredPackages(request.Target, request.Mode).Select(package => $"    <PackageVersion Include=\"{package}\" Version=\"*\" />"))}}
          </ItemGroup>
        </Project>
        """;

    private static string ScreenViewModel(TemplateGenerationRequest request, string screen) => $$"""
        using System.Reactive;
        using ReactiveUI;
        using CrissCross;

        namespace {{request.RootNamespace}}.ViewModels;

        public sealed class {{screen}}ViewModel : RxObject
        {
            private string _title = "{{screen}}";

            public {{screen}}ViewModel()
            {
                NavigateHome = ReactiveCommand.Create(() => { });
            }

            public string Title
            {
                get => _title;
                set => this.RaiseAndSetIfChanged(ref _title, value);
            }

            public ReactiveCommand<Unit, Unit> NavigateHome { get; }
        }
        """;

    private static string NavigationRegistration(TemplateGenerationRequest request) => $$"""
        using CrissCross;

        namespace {{request.RootNamespace}}.ViewModels;

        public static class NavigationRegistration
        {
            public static NavigationRegistry CreateRegistry() => new NavigationRegistry()
        {{string.Join("\n", NormalizedScreens(request).Select((screen, index) => $"        .Register<{screen}ViewModel, {request.RootNamespace}.Views.{screen}View>(contract: \"{screen}\"){(index == NormalizedScreens(request).Count - 1 ? ";" : string.Empty)}"))}}

            public static IBidirectionalNavigator CreateNavigator(IServiceProvider serviceProvider) =>
                CreateRegistry().CreateNavigator(serviceProvider);
        }
        """;

    private static string ControlsGalleryViewModel(TemplateGenerationRequest request) => $$"""
        using System.Reactive;
        using System.Reactive.Linq;
        using ReactiveUI;
        using CrissCross;

        namespace {{request.RootNamespace}}.ViewModels;

        public sealed class ControlsGalleryViewModel : RxObject
        {
            private readonly ObservableAsPropertyHelper<bool> _isOperationRunning;
            private string _searchText = string.Empty;
            private CommandButtonStatus _commandState = CommandButtonStatus.Idle(canExecute: true);
            private SearchQueryState _searchState = new();

            public ControlsGalleryViewModel()
            {
                RunImportCommand = ReactiveCommand.CreateFromTask(RunImportAsync);
                SearchCommand = ReactiveCommand.Create<string>(query => SearchText = query);
                ClearSearchCommand = ReactiveCommand.Create(() => SearchText = string.Empty);
                _isOperationRunning = RunImportCommand.IsExecuting.ToProperty(this, x => x.IsOperationRunning, scheduler: RxSchedulers.MainThreadScheduler);
            }

            public string PlatformNotes => "{{UiMarker(request.Target)}} starter generated from source-backed CrissCross patterns.";
            public bool IsOperationRunning => _isOperationRunning.Value;
            public double CommandProgress => IsOperationRunning ? 0.5 : 0;
            public CommandButtonStatus CommandState { get => _commandState; set => this.RaiseAndSetIfChanged(ref _commandState, value); }
            public SearchQueryState SearchState { get => _searchState; set => this.RaiseAndSetIfChanged(ref _searchState, value); }
            public ValidationSummaryState ValidationSummary { get; } = new(Array.Empty<ValidationMessage>());
            public ThemePreferenceState ThemeState { get; } = new(ThemeChoice.System, ThemeChoice.Light, supportsHighContrast: true);
            public string SearchText { get => _searchText; set => this.RaiseAndSetIfChanged(ref _searchText, value); }
            public BusyOperation CurrentOperation => IsOperationRunning ? new BusyOperation("Importing", progress: CommandProgress) : new BusyOperation(string.Empty);
            public ReactiveCommand<Unit, Unit> RunImportCommand { get; }
            public ReactiveCommand<string, Unit> SearchCommand { get; }
            public ReactiveCommand<Unit, Unit> ClearSearchCommand { get; }

            private async Task RunImportAsync()
            {
                CommandState = CommandButtonStatus.Executing(canExecute: false);
                await Task.Delay(250).ConfigureAwait(false);
                CommandState = CommandButtonStatus.Succeeded(canExecute: true);
            }
        }
        """;

    private static string WpfScreenView(TemplateGenerationRequest request, string screen) => $$"""
        <UserControl
            x:Class="{{request.RootNamespace}}.Views.{{screen}}View"
            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
            <Grid Margin="24">
                <TextBlock FontSize="28" FontWeight="SemiBold" Text="{Binding Title}" />
            </Grid>
        </UserControl>
        """;

    private static string AvaloniaScreenView(TemplateGenerationRequest request, string screen) => $$"""
        <UserControl
            x:Class="{{request.RootNamespace}}.Views.{{screen}}View"
            xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
            <Grid Margin="24">
                <TextBlock FontSize="28" FontWeight="SemiBold" Text="{Binding Title}" />
            </Grid>
        </UserControl>
        """;

    private static string MauiScreenView(TemplateGenerationRequest request, string screen) => $$"""
        <?xml version="1.0" encoding="UTF-8" ?>
        <rxui:ReactiveContentPage
            x:Class="{{request.RootNamespace}}.Views.{{screen}}View"
            xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            xmlns:rxui="clr-namespace:ReactiveUI.Maui;assembly=ReactiveUI.Maui"
            xmlns:viewModels="clr-namespace:{{request.RootNamespace}}.ViewModels"
            x:TypeArguments="viewModels:{{screen}}ViewModel">
            <VerticalStackLayout Padding="24">
                <Label FontAttributes="Bold" FontSize="28" Text="{Binding Title}" />
            </VerticalStackLayout>
        </rxui:ReactiveContentPage>
        """;

    private static string WinFormsScreenView(TemplateGenerationRequest request, string screen) => $$"""
        using System.Windows.Forms;
        using ReactiveUI;

        namespace {{request.RootNamespace}}.Views;

        public sealed partial class {{screen}}View : UserControl, IViewFor<ViewModels.{{screen}}ViewModel>
        {
            public {{screen}}View()
            {
                Controls.Add(new Label { Dock = DockStyle.Top, Text = "{{screen}}", AutoSize = true });
            }

            public ViewModels.{{screen}}ViewModel? ViewModel { get; set; }
            object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (ViewModels.{{screen}}ViewModel?)value; }
        }
        """;

    private static string WpfControlsGalleryView(TemplateGenerationRequest request) => $$"""
        <UserControl
            x:Class="{{request.RootNamespace}}.Views.ControlsGalleryView"
            xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:ui="https://github.com/reactivemarbles/CrissCross.ui">
            <StackPanel Margin="24">
                <ui:CommandButton Command="{Binding RunImportCommand}" Content="Run deterministic import" IsExecuting="{Binding IsOperationRunning}" Progress="{Binding CommandProgress}" State="{Binding CommandState}" />
                <ui:BusyOverlay Operation="{Binding CurrentOperation}">
                    <Border MinHeight="72" Padding="12"><ui:TextBlock Text="CrissCross.WPF.UI BusyOverlay preview" /></Border>
                </ui:BusyOverlay>
                <ui:SearchBox PlaceholderText="Search telemetry" QueryState="{Binding SearchState}" SearchCommand="{Binding SearchCommand}" Text="{Binding SearchText, Mode=TwoWay}" />
                <ui:DataPager QueryState="{Binding SearchState}" SortKey="timestamp" />
                <ui:ValidationSummary SummaryState="{Binding ValidationSummary}" />
                <ui:ThemeSwitcher CurrentState="{Binding ThemeState}" />
            </StackPanel>
        </UserControl>
        """;

    private static string AvaloniaControlsGalleryView(TemplateGenerationRequest request) => $$"""
        <UserControl
            x:Class="{{request.RootNamespace}}.Views.ControlsGalleryView"
            xmlns="https://github.com/avaloniaui"
            xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
            xmlns:controls="using:CrissCross.Avalonia.UI.Controls">
            <controls:StackPanel Margin="24" Spacing="16">
                <controls:CommandButton Command="{Binding RunImportCommand}" Content="Run deterministic import" IsExecuting="{Binding IsOperationRunning}" Progress="{Binding CommandProgress}" State="{Binding CommandState}" />
                <controls:BusyOverlay Operation="{Binding CurrentOperation}">
                    <Border MinHeight="72" Padding="12"><controls:TextBlock Text="CrissCross.Avalonia.UI BusyOverlay preview" /></Border>
                </controls:BusyOverlay>
                <controls:SearchBox PlaceholderText="Search telemetry" QueryState="{Binding SearchState}" SearchCommand="{Binding SearchCommand}" Text="{Binding SearchText}" />
                <controls:DataPager QueryState="{Binding SearchState}" SortKey="timestamp" />
                <controls:ValidationSummary SummaryState="{Binding ValidationSummary}" />
                <controls:ThemeSwitcher CurrentState="{Binding ThemeState}" />
            </controls:StackPanel>
        </UserControl>
        """;

    private static string MauiControlsGalleryView(TemplateGenerationRequest request) => $$"""
        <?xml version="1.0" encoding="UTF-8" ?>
        <rxui:ReactiveContentPage
            x:Class="{{request.RootNamespace}}.Views.ControlsGalleryView"
            xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
            xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
            xmlns:mauiui="clr-namespace:CrissCross.Maui.UI.Controls;assembly=CrissCross.Maui.UI"
            xmlns:rxui="clr-namespace:ReactiveUI.Maui;assembly=ReactiveUI.Maui">
            <VerticalStackLayout Padding="24" Spacing="16">
                <mauiui:CommandButton Command="{Binding RunImportCommand}" Text="Run deterministic import" IsExecuting="{Binding IsOperationRunning}" Progress="{Binding CommandProgress}" State="{Binding CommandState}" />
                <mauiui:BusyOverlay Operation="{Binding CurrentOperation}" />
                <mauiui:SearchBox SearchState="{Binding SearchState}" SubmitCommand="{Binding SearchCommand}" />
                <mauiui:DataPager QueryState="{Binding SearchState}" SortKey="timestamp" />
                <mauiui:ValidationSummary SummaryState="{Binding ValidationSummary}" />
                <mauiui:ThemeSwitcher ThemeState="{Binding ThemeState}" />
            </VerticalStackLayout>
        </rxui:ReactiveContentPage>
        """;

    private static string WinFormsControlsGalleryForm(TemplateGenerationRequest request) => $$"""
        using System.Reactive;
        using System.Windows.Forms;
        using ReactiveUI;

        namespace {{request.RootNamespace}}.Views;

        public sealed class ControlsGalleryForm : Form
        {
            private readonly ViewModels.ControlsGalleryViewModel _viewModel = new();

            public ControlsGalleryForm()
            {
                Text = "WinForms has no separate CrissCross UI package - reactive controls preview";
                // ViewModel owns the ReactiveCommand.CreateFromTask pipeline; WinForms controls subscribe at the edge.
                var runButton = new Button { Text = "Run deterministic import", Dock = DockStyle.Top };
                runButton.Click += (_, _) => _viewModel.RunImportCommand.Execute(Unit.Default).Subscribe();
                Controls.Add(runButton);
                Controls.Add(new TextBox { PlaceholderText = "Search telemetry", Dock = DockStyle.Top });
            }
        }
        """;

    private static string SmokeTests(TemplateGenerationRequest request) => $$"""
        namespace {{request.RootNamespace}}.Tests;

        public sealed class GeneratedTemplateSmokeTests
        {
            [Test]
            public void GeneratedTemplateContainsStableNavigationHost()
            {
                const string hostName = "{{request.HostName}}";
                if (string.IsNullOrWhiteSpace(hostName))
                {
                    throw new InvalidOperationException("CrissCross navigation host names must be non-empty.");
                }
            }
        }
        """;

    private static string Readme(TemplateGenerationRequest request) => $$"""
        # {{request.AppName}}

        Generated CrissCross {{request.Target}} starter preview.

        Mode: {{request.Mode}}
        Host: {{request.HostName}}
        Packages: {{string.Join(", ", RequiredPackages(request.Target, request.Mode))}}

        Generated-file previews are returned by the MCP server only; callers decide where to write them.
        Run `dotnet restore`, `dotnet build`, platform UI smoke tests, and `crisscross_review_code_snippet` after edits.
        """;

    private static IReadOnlyList<string> NormalizedScreens(TemplateGenerationRequest request) =>
        request.Screens.Count == 0 ? new[] { "Home" } : request.Screens.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static IReadOnlyList<string> SupportedControls(FrameworkTarget target) => target == FrameworkTarget.WinForms
        ? Array.Empty<string>()
        : new[] { "CommandButton", "SearchBox", "DataPager", "ValidationSummary", "ThemeSwitcher", "BusyOverlay" };

    private static string WpfResourceDictionaries(TemplateGenerationRequest request) => request.Mode == WizardMode.NavigationAndUi
        ? "<ui:ControlsDictionary />\n                <ui:ThemesDictionary />"
        : "<!-- Add CrissCross.WPF.UI dictionaries when switching to NavigationAndUi mode. -->";

    private static string AvaloniaUiStyleInclude(TemplateGenerationRequest request) => request.Mode == WizardMode.NavigationAndUi
        ? "<StyleInclude Source=\"avares://CrissCross.Avalonia.UI/Themes/Index.axaml\" />"
        : "<!-- Add CrissCross.Avalonia.UI styles when switching to NavigationAndUi mode. -->";

    private static string MauiUiUsing(TemplateGenerationRequest request) => request.Mode == WizardMode.NavigationAndUi
        ? "using CrissCross.Maui.UI;"
        : string.Empty;

    private static string MauiUiResourceCall(TemplateGenerationRequest request) => request.Mode == WizardMode.NavigationAndUi
        ? "Resources.UseCrissCrossMauiUiResources();"
        : "// Add Resources.UseCrissCrossMauiUiResources() when switching to NavigationAndUi mode.";

    private static string UiMarker(FrameworkTarget target) => target switch
    {
        FrameworkTarget.Wpf => "CrissCross.WPF.UI",
        FrameworkTarget.Avalonia => "CrissCross.Avalonia.UI",
        FrameworkTarget.Maui => "CrissCross.Maui.UI",
        FrameworkTarget.WinForms => "WinForms has no separate CrissCross UI package",
        _ => "CrissCross"
    };

    private static GeneratedFile File(string relativePath, string content, string sourceTemplate) =>
        new(relativePath, Normalize(content), SourceTemplate: sourceTemplate);

    private static string Normalize(string value) => string.Join('\n', value.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').Select(line => line.TrimEnd())).Trim() + "\n";

    private static string Bool(bool value) => value ? "true" : "false";

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeIdentifierRegex();

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*(\\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeDottedIdentifierRegex();
}
