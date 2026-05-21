using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

public sealed class TemplateWizard : ITemplateWizard
{
    public TemplateGenerationResult Generate(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request);
}
