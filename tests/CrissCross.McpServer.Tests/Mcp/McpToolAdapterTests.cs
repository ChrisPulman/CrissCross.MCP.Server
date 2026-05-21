using System.Text.Json;
using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class McpToolAdapterTests
{
    [Test]
    public async Task ToolMethodsReturnDeterministicJsonAndDoNotWriteStdout()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        string packageJson;
        string starterJson;
        try
        {
            packageJson = CrissCrossTools.crisscross_get_package_matrix(catalog, "wpf", null);
            starterJson = CrissCrossTools.crisscross_generate_project_starter(catalog, "wpf", "navigation-only", "SampleApp", "SampleApp", "Home", "");
        }
        finally
        {
            Console.SetOut(original);
        }

        await Assert.That(writer.ToString()).IsEmpty();
        await Assert.That(packageJson).Contains("CrissCross.WPF");
        await Assert.That(starterJson).Contains("GeneratedFile");
        await Assert.That(starterJson).Contains("App.xaml.cs");
    }

    [Test]
    public async Task ToolMethodsReturnParsedJsonForPrimarySurface()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        using var packages = JsonDocument.Parse(CrissCrossTools.crisscross_get_package_matrix(catalog, "wpf"));
        using var startup = JsonDocument.Parse(CrissCrossTools.crisscross_get_startup_recipe(catalog, "wpf"));
        using var navigation = JsonDocument.Parse(CrissCrossTools.crisscross_get_navigation_recipe(catalog, "viewmodel-host", "wpf", "MainHost", "Home"));
        using var control = JsonDocument.Parse(CrissCrossTools.crisscross_find_control(catalog, "wpf", "SearchBox"));
        using var diagnostics = JsonDocument.Parse(CrissCrossTools.crisscross_review_code_snippet(catalog, "RxApp.MainThreadScheduler;", "wpf"));

        await Assert.That(packages.RootElement.EnumerateArray().Any(package => package.GetProperty("id").GetString() == "CrissCross.WPF")).IsTrue();
        await Assert.That(startup.RootElement.GetProperty("title").GetString()).IsEqualTo("WPF startup");
        await Assert.That(navigation.RootElement.GetProperty("summary").GetString()).Contains("MainHost");
        await Assert.That(control.RootElement.GetProperty("name").GetString()).IsEqualTo("SearchBox");
        await Assert.That(diagnostics.RootElement.EnumerateArray().Any(diagnostic => diagnostic.GetProperty("ruleId").GetString() == "CC001")).IsTrue();
    }

    [Test]
    public async Task ToolMethodsCoverSnippetGenerationAndErrorExplanation()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        var viewModel = CrissCrossTools.crisscross_generate_viewmodel(catalog, "Search", "SearchViewModel", "SampleApp.ViewModels");
        var registry = CrissCrossTools.crisscross_generate_navigation_registry(catalog, "HomeViewModel -> HomeView");
        var explanation = CrissCrossTools.crisscross_explain_error(catalog, "Missing view registration", "wpf");

        await Assert.That(viewModel).Contains("public sealed class SearchViewModel");
        await Assert.That(viewModel).Contains("ReactiveCommand.CreateFromTask");
        await Assert.That(registry).Contains("NavigationRegistry");
        await Assert.That(registry).Contains("HomeViewModel -> HomeView");
        await Assert.That(explanation).Contains("Missing view registration");
    }

    [Test]
    public async Task ToolMethodsRejectInvalidPlatformAndMode()
    {
        var catalog = CrissCrossKnowledgeCatalog.CreateDefault();

        await Assert.That(() => CrissCrossTools.crisscross_get_startup_recipe(catalog, "silverlight")).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => CrissCrossTools.crisscross_generate_project_starter(catalog, "wpf", "bad-mode", "SampleApp", "SampleApp")).Throws<ArgumentOutOfRangeException>();
    }
}
