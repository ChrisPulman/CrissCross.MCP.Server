using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Catalog;

public sealed class ControlCatalogTests
{
    [Test]
    public async Task ControlCatalogFindsSourceBackedUiControls()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        await Assert.That(catalog.FindControl(FrameworkTarget.Wpf, "SearchBox")?.PackageId).IsEqualTo("CrissCross.WPF.UI");
        await Assert.That(catalog.FindControl(FrameworkTarget.Avalonia, "DataPager")?.SourcePaths.Single()).Contains("CrissCross.Avalonia.UI/Controls/DataPager/DataPager.cs");
        await Assert.That(catalog.FindControl(FrameworkTarget.Maui, "CommandButton")?.PackageId).IsEqualTo("CrissCross.Maui.UI");
    }

    [Test]
    public async Task WpfUiControlsDoNotAppearForWinFormsWithoutSourceBacking()
    {
        var result = CrissCrossKnowledgeCatalog.CreateDefault().FindControl(FrameworkTarget.WinForms, "SearchBox");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task StateModelGuidanceRequiresReplacementSemantics()
    {
        var guidance = CrissCrossKnowledgeCatalog.CreateDefault().GetStateModelGuidance();

        await Assert.That(guidance).Contains("replace immutable/snapshot state values");
        await Assert.That(guidance).Contains("do not deep-mutate nested values");
    }
}
