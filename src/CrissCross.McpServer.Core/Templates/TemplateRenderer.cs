using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

public sealed class TemplateRenderer
{
    public IReadOnlyList<GeneratedFile> Render(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request).Files;
}
