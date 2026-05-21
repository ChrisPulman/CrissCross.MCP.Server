using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class McpResourceAdapterTests
{
    [Test]
    public async Task ResourceAdapterRoutesCrissCrossUris()
    {
        await Assert.That(CrissCrossResources.Read("crisscross://startup/wpf")).Contains(".WithWpf().BuildApp()");
        await Assert.That(CrissCrossResources.Read("crisscross://navigation/core")).Contains("NavigationRegistry");
        await Assert.That(CrissCrossResources.Read("crisscross://templates/wpf/navigation-only")).Contains("NavigationOnly");
    }
}
