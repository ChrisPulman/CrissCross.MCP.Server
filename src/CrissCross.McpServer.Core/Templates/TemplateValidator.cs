using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

public sealed class TemplateValidator
{
    public IReadOnlyList<ValidationDiagnostic> Validate(TemplateGenerationRequest request) =>
        CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request).Diagnostics;
}
