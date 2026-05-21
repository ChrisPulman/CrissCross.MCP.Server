using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Resources;
using CrissCross.McpServer.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    // MCP stdio reserves stdout for JSON-RPC. Diagnostics must always go to stderr.
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(CrissCrossKnowledgeCatalog.CreateDefault());
builder.Services.AddSingleton<ResourceRouter>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithResourcesFromAssembly()
    .WithPromptsFromAssembly();

await builder.Build().RunAsync().ConfigureAwait(false);
