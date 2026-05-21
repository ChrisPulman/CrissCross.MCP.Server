using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class McpPromptAdapterTests
{
    [Test]
    public async Task PromptsReferenceRequiredToolsAndGuardrails()
    {
        await Assert.That(CrissCrossPrompts.GenerateCrissCrossApp()).Contains("crisscross_generate_project_starter");
        await Assert.That(CrissCrossPrompts.ReviewCrissCrossCode()).Contains("crisscross_review_code_snippet");
        await Assert.That(CrissCrossPrompts.GenerateCrissCrossViewModel()).Contains("RxObject");
        await Assert.That(CrissCrossPrompts.GenerateCrissCrossViewModel()).Contains("no RxApp");
    }
}
