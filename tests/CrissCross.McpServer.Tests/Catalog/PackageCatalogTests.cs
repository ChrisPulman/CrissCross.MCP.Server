using CrissCross.McpServer.Core.Catalog;

namespace CrissCross.McpServer.Tests.Catalog;

public sealed class PackageCatalogTests
{
    [Test]
    public async Task PackageCatalogListsKnownCrissCrossPackages()
    {
        var packages = CrissCrossKnowledgeCatalog.CreateDefault().GetPackageMatrix();
        var ids = packages.Select(package => package.Id).ToArray();

        foreach (var expected in new[] { "CrissCross", "CrissCross.WPF", "CrissCross.WPF.UI", "CrissCross.Avalonia", "CrissCross.Avalonia.UI", "CrissCross.MAUI", "CrissCross.Maui.UI", "CrissCross.WinForms" })
        {
            await Assert.That(ids).Contains(expected);
        }

        await Assert.That(packages.Single(package => package.Id == "CrissCross.WPF").SourcePaths).Contains("src/CrissCross.WPF/CrissCross.WPF.csproj");
    }

    [Test]
    public async Task PackageCatalogFiltersByPlatformAndTargetFramework()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        var avalonia = catalog.GetPackageMatrix("avalonia", "net10.0");
        var windows = catalog.GetPackageMatrix("wpf", "net10.0-windows10.0.19041.0");

        await Assert.That(avalonia.Select(package => package.Id)).Contains("CrissCross.Avalonia");
        await Assert.That(avalonia.Select(package => package.Id)).Contains("CrissCross");
        await Assert.That(avalonia.Select(package => package.Id)).DoesNotContain("CrissCross.WPF");
        await Assert.That(windows.Select(package => package.Id)).Contains("CrissCross.WPF");
    }

    [Test]
    public async Task CatalogStringOverloadsParsePlatformModesAndNavigationContext()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        var startup = catalog.GetStartupRecipe("maui", "navigation-and-ui");
        var navigation = catalog.GetNavigationRecipe("viewmodel-host", hostName: "MainHost", contract: "Home");
        var control = catalog.FindControl("wpf", "validation");

        await Assert.That(startup.RequiredPackages).Contains("CrissCross.Maui.UI");
        await Assert.That(navigation.Summary).Contains("MainHost");
        await Assert.That(navigation.Summary).Contains("Home");
        await Assert.That(control?.Name).IsEqualTo("ValidationSummary");
    }
}
