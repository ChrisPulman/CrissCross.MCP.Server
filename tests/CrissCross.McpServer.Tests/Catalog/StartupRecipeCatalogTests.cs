using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Catalog;

public sealed class StartupRecipeCatalogTests
{
    [Test]
    public async Task StartupRecipeWpfContainsBuilderAndDictionary()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetStartupRecipe(FrameworkTarget.Wpf, WizardMode.NavigationAndUi);

        await Assert.That(recipe.CodeSnippet).Contains(".WithWpf().BuildApp()");
        await Assert.That(string.Join("\n", recipe.Gotchas.Concat(recipe.RequiredFiles))).Contains("CrissCrossWpfDictionary");
    }

    [Test]
    public async Task StartupRecipeAvaloniaContainsUseReactiveUiAndStyles()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetStartupRecipe(FrameworkTarget.Avalonia, WizardMode.NavigationAndUi);

        await Assert.That(recipe.CodeSnippet).Contains(".UseReactiveUI");
        await Assert.That(string.Join("\n", recipe.RequiredFiles.Concat(recipe.Gotchas))).Contains("avares://CrissCross.Avalonia/Themes/Index.axaml");
    }

    [Test]
    public async Task StartupRecipeMauiUiContainsBuilderAndUiResources()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetStartupRecipe(FrameworkTarget.Maui, WizardMode.NavigationAndUi);

        await Assert.That(recipe.CodeSnippet).Contains(".WithMaui().BuildApp()");
        await Assert.That(recipe.CodeSnippet).Contains("UseCrissCrossMauiUiResources");
    }

    [Test]
    public async Task StartupRecipeWinFormsContainsInitializationAndBuilder()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetStartupRecipe(FrameworkTarget.WinForms, WizardMode.NavigationOnly);

        await Assert.That(recipe.CodeSnippet).Contains("ApplicationConfiguration.Initialize()");
        await Assert.That(recipe.CodeSnippet).Contains(".WithWinForms().BuildApp()");
    }
}
