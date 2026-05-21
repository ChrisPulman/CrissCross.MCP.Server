using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

/// <summary>
/// Validates CrissCross template generation requests without requiring callers to inspect generated files.
/// </summary>
public sealed class TemplateValidator
{
    /// <summary>
    /// Validates a template generation request.
    /// </summary>
    /// <param name="request">The template generation request.</param>
    /// <returns>The validation diagnostics.</returns>
    public IReadOnlyList<ValidationDiagnostic> Validate(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request).Diagnostics;
}
