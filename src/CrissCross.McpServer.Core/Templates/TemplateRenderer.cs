using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

/// <summary>
/// Renders preview-only CrissCross template files from a generation request.
/// </summary>
public sealed class TemplateRenderer
{
    /// <summary>
    /// Renders generated-file previews for the request.
    /// </summary>
    /// <param name="request">The template generation request.</param>
    /// <returns>The generated-file previews.</returns>
    public IReadOnlyList<GeneratedFile> Render(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request).Files;
}
