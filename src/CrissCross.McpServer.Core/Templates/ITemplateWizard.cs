using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

/// <summary>
/// Defines the preview-only CrissCross template wizard contract.
/// </summary>
public interface ITemplateWizard
{
    /// <summary>
    /// Generates preview-only template files, diagnostics, and next steps.
    /// </summary>
    /// <param name="request">The template generation request.</param>
    /// <returns>The template generation result.</returns>
    TemplateGenerationResult Generate(TemplateGenerationRequest request);
}
