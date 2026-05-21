using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

public sealed class TemplateValidatorTests
{
    [Test]
    public async Task TemplateValidatorReportsInvalidRequestInputs()
    {
        var request = TestRequests.ValidWpf() with { AppName = "", RootNamespace = "9.Invalid", TargetFramework = "net10.0", IncludeControls = new[] { "NoSuchControl" } };
        var result = CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request);
        var ruleIds = result.Diagnostics.Select(diagnostic => diagnostic.RuleId).ToArray();

        await Assert.That(ruleIds).Contains("TPL001");
        await Assert.That(ruleIds).Contains("TPL002");
        await Assert.That(ruleIds).Contains("TPL003");
        await Assert.That(ruleIds).Contains("TPL004");
    }
}
