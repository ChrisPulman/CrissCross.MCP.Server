using CrissCross.McpServer.Core.Templates;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

public sealed class TemplateFacadeTests
{
    [Test]
    public async Task PublicTemplateFacadesDelegateToWizardEngine()
    {
        var request = TestRequests.ValidWpf();
        ITemplateWizard wizard = new TemplateWizard();
        var renderer = new TemplateRenderer();
        var validator = new TemplateValidator();
        var catalog = new TemplateCatalog();

        var wizardResult = wizard.Generate(request);
        var files = renderer.Render(request);
        var diagnostics = validator.Validate(request);
        var combinations = catalog.GetSupportedCombinations();

        await Assert.That(wizardResult.Files.Select(file => file.RelativePath)).Contains("App.xaml.cs");
        await Assert.That(files.Select(file => file.RelativePath)).Contains("MainWindow.xaml");
        await Assert.That(diagnostics.Where(diagnostic => diagnostic.Severity == ValidationSeverity.Error)).IsEmpty();
        await Assert.That(combinations).Contains((FrameworkTarget.Wpf, WizardMode.NavigationOnly));
        await Assert.That(combinations).Contains((FrameworkTarget.Maui, WizardMode.NavigationAndUi));
    }

    [Test]
    public async Task TemplateOptionsHonorOptionalFileFlags()
    {
        var request = TestRequests.ValidWpf() with
        {
            IncludeTests = false,
            IncludeReadme = false,
            UseCentralPackageManagement = false
        };

        var result = new TemplateWizard().Generate(request);
        var paths = result.Files.Select(file => file.RelativePath).ToArray();

        await Assert.That(paths).DoesNotContain("Tests/GeneratedTemplateSmokeTests.cs");
        await Assert.That(paths).DoesNotContain("README.md");
        await Assert.That(paths).DoesNotContain("Directory.Packages.props");
        await Assert.That(paths).Contains($"{request.AppName}.csproj");
    }
}
