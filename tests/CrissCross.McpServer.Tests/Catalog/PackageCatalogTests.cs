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
}
