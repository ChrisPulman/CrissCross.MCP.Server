using CrissCross.McpServer.Core.Resources;
using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class TemplateWizardMcpSurfaceTests
{
    [Test]
    public async Task ProjectStarterToolReturnsDiagnosticsNextStepsAndFilePreviews()
    {
        var json = CrissCrossTools.crisscross_generate_project_starter(
            "avalonia",
            "navigation-and-ui",
            "SampleApp",
            "SampleApp",
            "Home,Settings",
            "CommandButton,SearchBox");

        await Assert.That(json).Contains("TemplateGenerationResult<GeneratedFile>");
        await Assert.That(json).Contains("Views/ControlsGalleryView.axaml");
        await Assert.That(json).Contains("ViewModels/ControlsGalleryViewModel.cs");
        await Assert.That(json).Contains("diagnostics");
        await Assert.That(json).Contains("nextSteps");
        await Assert.That(json).Contains("sourceTemplate");
        await Assert.That(json).Contains("Use crisscross_review_code_snippet");
    }

    [Test]
    public async Task TemplateResourceReturnsFrameworkModeManifestWithPreviewPaths()
    {
        var resource = ResourceRouter.CreateDefault().ReadResource("crisscross://templates/wpf/navigation-and-ui");

        await Assert.That(resource).Contains("# Template wpf/navigation-and-ui");
        await Assert.That(resource).Contains("crisscross_generate_project_starter");
        await Assert.That(resource).Contains("Sample generated preview paths");
        await Assert.That(resource).Contains("Views/ControlsGalleryView.xaml");
        await Assert.That(resource).Contains("CrissCross.WPF.UI");
    }
}
