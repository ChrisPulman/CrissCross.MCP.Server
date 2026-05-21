using CrissCross.McpServer.Core.Catalog;

namespace CrissCross.McpServer.Tests;

public sealed class SmokeTests
{
    [Test]
    public async Task CoreCatalogCanBeCreated()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        await Assert.That(catalog).IsNotNull();
    }
}
