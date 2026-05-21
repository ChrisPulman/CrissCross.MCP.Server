using CrissCross.McpServer.Core.Resources;
using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class McpResourceAdapterTests
{
    [Test]
    public async Task ResourceAdapterRoutesCrissCrossUris()
    {
        var router = ResourceRouter.CreateDefault();

        await Assert.That(CrissCrossResources.Read(router, "crisscross://startup/wpf")).Contains(".WithWpf().BuildApp()");
        await Assert.That(CrissCrossResources.Read(router, "crisscross://navigation/core")).Contains("NavigationRegistry");
        await Assert.That(CrissCrossResources.Read(router, "crisscross://templates/wpf/navigation-only")).Contains("NavigationOnly");
    }

    [Test]
    public async Task ResourceTemplatesCoverDocumentedResourceFamilies()
    {
        var router = ResourceRouter.CreateDefault();

        await Assert.That(CrissCrossResources.GetPackageMatrix(router)).Contains("CrissCross.WPF");
        await Assert.That(CrissCrossResources.GetStartupRecipe(router, "avalonia")).Contains("UseReactiveUI");
        await Assert.That(CrissCrossResources.GetCoreNavigation(router)).Contains("NavigationRegistry");
        await Assert.That(CrissCrossResources.GetNavigationRecipe(router, "navigation-view")).Contains("NavigationView");
        await Assert.That(CrissCrossResources.GetControls(router, "maui")).Contains("SearchBox");
        await Assert.That(CrissCrossResources.GetControl(router, "wpf", "CommandButton")).Contains("CommandButton");
        await Assert.That(CrissCrossResources.GetStateModels(router)).Contains("replace immutable");
        await Assert.That(CrissCrossResources.GetTemplate(router, "wpf", "navigation-and-ui")).Contains("Views/ControlsGalleryView.xaml");
        await Assert.That(CrissCrossResources.GetAntiPatterns(router)).Contains("RxApp");
        await Assert.That(CrissCrossResources.GetTestingGuidance(router)).Contains("TUnit");
    }

    [Test]
    public async Task ResourceRouterReportsUnknownAndInvalidUris()
    {
        var router = ResourceRouter.CreateDefault();

        await Assert.That(() => router.ReadResource("crisscross://missing/resource")).Throws<InvalidOperationException>();
        await Assert.That(() => router.ReadResource("crisscross://startup/unknown")).Throws<ArgumentOutOfRangeException>();
    }
}
