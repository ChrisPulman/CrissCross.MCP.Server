using System.Reflection;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Resources;
using CrissCross.McpServer.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

namespace CrissCross.McpServer;

/// <summary>
/// Entry point and host factory for the CrissCross MCP server.
/// </summary>
public static class Program
{
    private static readonly string[] RiskyClientMetadataKeys =
    [
        "Title",
        "Description",
        "WebsiteUrl",
        "Icons"
    ];

    /// <summary>
    /// Builds the configured MCP server host.
    /// </summary>
    /// <param name="args">Command-line arguments supplied to the server process.</param>
    /// <returns>The configured host.</returns>
    public static IHost CreateHost(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        ConfigureLogging(builder);

        builder.Services.AddSingleton(CrissCrossKnowledgeCatalog.CreateDefault());
        builder.Services.AddSingleton<ResourceRouter>();

        builder.Services
            .AddMcpServer(options => options.ServerInfo = BuildServerInfo())
            .WithStdioServerTransport()
            .WithTools<CrissCrossTools>()
            .WithResources<CrissCrossResources>()
            .WithPrompts<CrissCrossPrompts>();

        return builder.Build();
    }

    /// <summary>
    /// Builds the MCP server metadata sent during client initialization.
    /// </summary>
    /// <returns>The implementation metadata advertised to MCP clients.</returns>
    public static Implementation BuildServerInfo()
    {
        var assembly = typeof(Program).Assembly;
        var serverInfo = new Implementation
        {
            Name = "crisscross-mcp-server",
            Version = assembly
                .GetCustomAttributes<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0"
        };

        if (!ShouldAdvertiseRichClientMetadata())
        {
            return serverInfo;
        }

        serverInfo.Title = "CrissCross MCP Server";
        serverInfo.Description = "CrissCross developer guidance for AI-assisted code generation, review, startup, navigation, controls, and preview-only project scaffolding.";
        serverInfo.WebsiteUrl = "https://github.com/ChrisPulman/CrissCross.MCP.Server";

        return serverInfo;
    }

    /// <summary>
    /// Gets the optional metadata fields suppressed for editor compatibility.
    /// </summary>
    /// <returns>The names of metadata fields omitted from the default MCP initialization payload.</returns>
    public static IReadOnlyList<string> GetSuppressedClientMetadataKeys() => RiskyClientMetadataKeys;

    /// <summary>
    /// Starts the MCP server process.
    /// </summary>
    /// <param name="args">Command-line arguments supplied to the server process.</param>
    /// <returns>A task representing the asynchronous host lifetime.</returns>
    public static async Task Main(string[] args) => await CreateHost(args).RunAsync().ConfigureAwait(false);

    private static bool ShouldAdvertiseRichClientMetadata() => false;

    private static void ConfigureLogging(HostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            // MCP stdio reserves stdout for JSON-RPC. Diagnostics must always go to stderr.
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });
    }
}
