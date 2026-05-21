using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

public interface ITemplateWizard
{
    TemplateGenerationResult Generate(TemplateGenerationRequest request);
}
