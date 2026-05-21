using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

public sealed class TemplateWizardTests
{
    [Test]
    public async Task TemplateWizardGeneratesWpfNavigationOnlyFiles()
    {
        var result = CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(TestRequests.ValidWpf());
        var paths = result.Files.Select(file => file.RelativePath).ToArray();
        var combined = string.Join("\n", result.Files.Select(file => file.Content));

        await Assert.That(paths).Contains("App.xaml.cs");
        await Assert.That(paths).Contains("ViewModels/HomeViewModel.cs");
        await Assert.That(combined).Contains(".WithWpf().BuildApp()");
        await Assert.That(combined).Contains("SetMainNavigationHost");
        await Assert.That(combined).DoesNotContain("UseCrissCrossMauiUiResources");
    }

    [Test]
    public async Task TemplateWizardGeneratesMauiUiFilesWithMauiResources()
    {
        var request = TestRequests.ValidMauiUi();
        var result = CrissCrossKnowledgeCatalog.CreateDefault().GenerateProjectStarter(request);
        var combined = string.Join("\n", result.Files.Select(file => file.Content));

        await Assert.That(result.Files.Select(file => file.RelativePath).ToArray()).Contains("MauiProgram.cs");
        await Assert.That(combined).Contains(".WithMaui().BuildApp()");
        await Assert.That(combined).Contains("UseCrissCrossMauiUiResources");
    }
}
