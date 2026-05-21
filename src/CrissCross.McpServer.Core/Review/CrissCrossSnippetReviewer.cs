using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Review;

/// <summary>
/// Performs deterministic text-based review checks for common CrissCross anti-patterns.
/// </summary>
public sealed class CrissCrossSnippetReviewer
{
    /// <summary>
    /// Reviews a C# snippet for known CrissCross anti-patterns.
    /// </summary>
    /// <param name="code">The C# code snippet to review.</param>
    /// <param name="platform">Optional platform context.</param>
    /// <param name="projectKind">Optional project-kind context.</param>
    /// <returns>The diagnostics found in the snippet.</returns>
    public IReadOnlyList<ReviewDiagnostic> Review(string code, FrameworkTarget? platform = null, string? projectKind = null)
    {
        ArgumentNullException.ThrowIfNull(code);
        var diagnostics = new List<ReviewDiagnostic>();
        var text = code.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (code.Contains("RxApp", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC001", ReviewSeverity.Error, "Do not use ReactiveUI RxApp in CrissCross generated code.", "Use RxSchedulers and CrissCross builder-provided scheduling abstractions instead.", "src/CrissCross/RxSchedulers.cs"));
        }

        if (platform == FrameworkTarget.Wpf && code.Contains("Application", StringComparison.Ordinal) && !code.Contains(".WithWpf().BuildApp()", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC002", ReviewSeverity.Warning, "WPF startup is missing RxAppBuilder.CreateReactiveUIBuilder().WithWpf().BuildApp().", "Add the CrissCross WPF builder call in App startup.", "src/CrissCross.WPF.Test/App.xaml.cs"));
        }

        if (platform == FrameworkTarget.Avalonia && code.Contains("AppBuilder", StringComparison.Ordinal) && !code.Contains("UseReactiveUI", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC003", ReviewSeverity.Warning, "Avalonia startup is missing UseReactiveUI(...).", "Call .UseReactiveUI(b => { }) on the Avalonia AppBuilder.", "src/CrissCross.Avalonia.Test.Desktop/Program.cs"));
        }

        if (code.Contains("ViewModelRoutedViewHost", StringComparison.Ordinal) && !code.Contains("SetMainNavigationHost", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC004", ReviewSeverity.Warning, "Hosted view-model navigation requires host registration.", "Call SetMainNavigationHost with a configured IViewModelRoutedViewHost.", "src/CrissCross/ViewModelRoutedViewHostMixins.cs"));
        }

        if (text.Contains("HostName=string.Empty", StringComparison.Ordinal) || text.Contains("HostName=\"\"", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC005", ReviewSeverity.Warning, "Navigation host name is empty before Setup().", "Assign a stable non-empty HostName before Setup or registration.", "src/CrissCross.WinForms/ViewModelRoutedViewHost.cs"));
        }

        if (string.Equals(projectKind, "core", StringComparison.OrdinalIgnoreCase) && (code.Contains("Window", StringComparison.Ordinal) || code.Contains("Page", StringComparison.Ordinal) || code.Contains("ContentView", StringComparison.Ordinal)))
        {
            diagnostics.Add(new("CC006", ReviewSeverity.Warning, "Core projects should not reference platform UI types.", "Move platform views into WPF/Avalonia/MAUI/WinForms projects and keep core code view-model/state only.", null));
        }

        if (System.Text.RegularExpressions.Regex.IsMatch(code, @"\b[A-Za-z0-9_]+\.[A-Za-z0-9_]+\.[A-Za-z0-9_]+\s*="))
        {
            diagnostics.Add(new("CC007", ReviewSeverity.Warning, "State model appears to be deep-mutated.", "Replace immutable/snapshot state values with a new instance instead of mutating nested properties.", "src/CrissCross/SearchQueryState.cs"));
        }

        if (platform == FrameworkTarget.Wpf && string.Equals(projectKind, "wpf-ui-page", StringComparison.OrdinalIgnoreCase) && code.Contains("INavigationService", StringComparison.Ordinal) && code.Contains("SetMainNavigationHost", StringComparison.Ordinal))
        {
            diagnostics.Add(new("CC008", ReviewSeverity.Warning, "WPF.UI page navigation is being mixed with view-model-host APIs.", "Use INavigationService/IPageService for page navigation; use SetMainNavigationHost only for IViewModelRoutedViewHost flows.", "src/CrissCross.WPF.UI/INavigationService.cs"));
        }

        return diagnostics;
    }
}
