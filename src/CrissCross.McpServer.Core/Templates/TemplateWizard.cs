using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

/// <summary>
/// Default implementation of the preview-only CrissCross template wizard.
/// </summary>
public sealed class TemplateWizard : ITemplateWizard
{
    /// <inheritdoc />
    public TemplateGenerationResult Generate(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request);
}
