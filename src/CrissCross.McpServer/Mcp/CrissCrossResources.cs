using System.ComponentModel;
using CrissCross.McpServer.Core.Resources;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

/// <summary>
/// MCP resources that expose read-only CrissCross knowledge by URI template.
/// </summary>
[McpServerResourceType]
public sealed class CrissCrossResources
{
    /// <summary>
    /// Reads a CrissCross resource by its full <c>crisscross://</c> URI.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="uri">The full resource URI.</param>
    /// <returns>The resource content.</returns>
    public static string Read(ResourceRouter router, string uri)
    {
        ArgumentNullException.ThrowIfNull(router);

        return router.ReadResource(uri);
    }

    /// <summary>
    /// Gets the CrissCross package matrix.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <returns>A JSON package matrix.</returns>
    [McpServerResource(UriTemplate = "crisscross://packages/matrix", Name = "CrissCross Package Matrix", MimeType = "application/json")]
    [Description("Read-only package, dependency, framework, and source-path matrix for CrissCross.")]
    public static string GetPackageMatrix(ResourceRouter router) => Read(router, "crisscross://packages/matrix");

    /// <summary>
    /// Gets a platform startup recipe.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <returns>A Markdown startup recipe.</returns>
    [McpServerResource(UriTemplate = "crisscross://startup/{platform}", Name = "CrissCross Startup Recipe", MimeType = "text/markdown")]
    [Description("Read-only startup guidance for a CrissCross platform.")]
    public static string GetStartupRecipe(ResourceRouter router, string platform) => Read(router, $"crisscross://startup/{platform}");

    /// <summary>
    /// Gets the core navigation recipe.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <returns>A Markdown navigation recipe.</returns>
    [McpServerResource(UriTemplate = "crisscross://navigation/core", Name = "CrissCross Core Navigation", MimeType = "text/markdown")]
    [Description("Read-only NavigationRegistry guidance for CrissCross.")]
    public static string GetCoreNavigation(ResourceRouter router) => Read(router, "crisscross://navigation/core");

    /// <summary>
    /// Gets a navigation recipe by kind.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="kind">The navigation recipe kind.</param>
    /// <returns>A Markdown navigation recipe.</returns>
    [McpServerResource(UriTemplate = "crisscross://navigation/{kind}", Name = "CrissCross Navigation Recipe", MimeType = "text/markdown")]
    [Description("Read-only navigation guidance for a CrissCross navigation pattern.")]
    public static string GetNavigationRecipe(ResourceRouter router, string kind) => Read(router, $"crisscross://navigation/{kind}");

    /// <summary>
    /// Gets platform control guidance.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <returns>Control guidance for the platform.</returns>
    [McpServerResource(UriTemplate = "crisscross://controls/{platform}", Name = "CrissCross Controls", MimeType = "text/plain")]
    [Description("Read-only source-backed CrissCross control list for a platform.")]
    public static string GetControls(ResourceRouter router, string platform) => Read(router, $"crisscross://controls/{platform}");

    /// <summary>
    /// Gets one platform control manifest.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <param name="controlName">The control name.</param>
    /// <returns>A JSON control manifest.</returns>
    [McpServerResource(UriTemplate = "crisscross://controls/{platform}/{controlName}", Name = "CrissCross Control", MimeType = "application/json")]
    [Description("Read-only source-backed CrissCross control manifest.")]
    public static string GetControl(ResourceRouter router, string platform, string controlName) => Read(router, $"crisscross://controls/{platform}/{controlName}");

    /// <summary>
    /// Gets CrissCross state-model guidance.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <returns>State-model guidance text.</returns>
    [McpServerResource(UriTemplate = "crisscross://state-models", Name = "CrissCross State Models", MimeType = "text/plain")]
    [Description("Read-only guidance for CrissCross replacement state-model semantics.")]
    public static string GetStateModels(ResourceRouter router) => Read(router, "crisscross://state-models");

    /// <summary>
    /// Gets a template manifest for a platform and wizard mode.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <param name="platform">The platform identifier.</param>
    /// <param name="mode">The wizard mode.</param>
    /// <returns>A Markdown template manifest.</returns>
    [McpServerResource(UriTemplate = "crisscross://templates/{platform}/{mode}", Name = "CrissCross Template Preview", MimeType = "text/markdown")]
    [Description("Read-only template starter guidance for a CrissCross platform and mode.")]
    public static string GetTemplate(ResourceRouter router, string platform, string mode) => Read(router, $"crisscross://templates/{platform}/{mode}");

    /// <summary>
    /// Gets CrissCross anti-pattern guidance.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <returns>Anti-pattern guidance.</returns>
    [McpServerResource(UriTemplate = "crisscross://quality/anti-patterns", Name = "CrissCross Anti-patterns", MimeType = "text/markdown")]
    [Description("Read-only CrissCross anti-pattern checklist.")]
    public static string GetAntiPatterns(ResourceRouter router) => Read(router, "crisscross://quality/anti-patterns");

    /// <summary>
    /// Gets CrissCross testing guidance.
    /// </summary>
    /// <param name="router">The resource router service.</param>
    /// <returns>Testing guidance.</returns>
    [McpServerResource(UriTemplate = "crisscross://quality/testing", Name = "CrissCross Testing", MimeType = "text/markdown")]
    [Description("Read-only testing guidance for CrissCross generated code and platform starters.")]
    public static string GetTestingGuidance(ResourceRouter router) => Read(router, "crisscross://quality/testing");
}
