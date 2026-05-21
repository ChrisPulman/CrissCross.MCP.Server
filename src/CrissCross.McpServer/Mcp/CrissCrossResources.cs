using System.ComponentModel;
using CrissCross.McpServer.Core.Resources;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

[McpServerResourceType]
public static class CrissCrossResources
{
    [McpServerResource]
    [Description("Read a CrissCross knowledge resource by crisscross:// URI.")]
    public static string Read(string uri) => ResourceRouter.CreateDefault().ReadResource(uri);
}
