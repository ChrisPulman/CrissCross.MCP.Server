using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Review;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Review;

public sealed class CrissCrossSnippetReviewerTests
{
    [Test]
    public async Task ReviewerRejectsRxAppUsage()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("RxApp.MainThreadScheduler.Schedule(() => { });");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC001" && diagnostic.Severity == ReviewSeverity.Error)).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsWhenWpfStartupBuildAppIsMissing()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("public partial class App : Application { }", FrameworkTarget.Wpf);

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC002")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsWhenAvaloniaUseReactiveUiIsMissing()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("AppBuilder.Configure<App>().UsePlatformDetect();", FrameworkTarget.Avalonia);

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC003")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsWhenNavigationHostRegistrationIsMissing()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("new ViewModelRoutedViewHost();");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC004")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsOnEmptyHostNameInHostedNavigation()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("host.HostName = string.Empty; host.Setup();");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC005")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsOnPlatformTypesInCoreProject()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("public Window MainWindow { get; }", projectKind: "core");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC006")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsOnDeepStateMutation()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("State.Filter.Text = value;");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC007")).IsTrue();
    }

    [Test]
    public async Task ReviewerWarnsWhenWpfUiPageNavigationUsesViewModelHostApi()
    {
        var diagnostics = CrissCrossKnowledgeCatalog.CreateDefault().ReviewCodeSnippet("INavigationService nav; view.SetMainNavigationHost(host);", FrameworkTarget.Wpf, "wpf-ui-page");

        await Assert.That(diagnostics.Any(diagnostic => diagnostic.RuleId == "CC008")).IsTrue();
    }
}
