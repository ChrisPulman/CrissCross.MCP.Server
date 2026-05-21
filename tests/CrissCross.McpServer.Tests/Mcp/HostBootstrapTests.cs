using CrissCross.McpServer;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class HostBootstrapTests
{
    [Test]
    public async Task CreateHostRegistersCatalogRouterAndServerInfo()
    {
        using var host = Program.CreateHost([]);

        var catalog = host.Services.GetRequiredService<CrissCrossKnowledgeCatalog>();
        var router = host.Services.GetRequiredService<ResourceRouter>();
        var serverInfo = Program.BuildServerInfo();

        await Assert.That(catalog.GetPackageMatrix().Count).IsGreaterThanOrEqualTo(8);
        await Assert.That(router.ReadResource("crisscross://state-models")).Contains("State models");
        await Assert.That(serverInfo.Name).IsEqualTo("crisscross-mcp-server");
        await Assert.That(serverInfo.Version).IsNotNull();
        await Assert.That(serverInfo.Title).IsNull();
        await Assert.That(serverInfo.Description).IsNull();
        await Assert.That(serverInfo.WebsiteUrl).IsNull();
    }

    [Test]
    public async Task GetSuppressedClientMetadataKeysReturnsCompatibilityFields()
    {
        var keys = Program.GetSuppressedClientMetadataKeys();

        await Assert.That(keys).Contains("Title");
        await Assert.That(keys).Contains("Description");
        await Assert.That(keys).Contains("WebsiteUrl");
        await Assert.That(keys).Contains("Icons");
    }
}
