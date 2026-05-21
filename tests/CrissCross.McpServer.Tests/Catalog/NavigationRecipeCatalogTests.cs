using CrissCross.McpServer.Core.Catalog;

namespace CrissCross.McpServer.Tests.Catalog;

public sealed class NavigationRecipeCatalogTests
{
    [Test]
    public async Task NavigationOnlyRecipeUsesNavigationRegistryAndBidirectionalNavigator()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetNavigationRecipe("navigation-only");

        await Assert.That(recipe.CodeSnippet).Contains("NavigationRegistry");
        await Assert.That(recipe.CodeSnippet).Contains("IBidirectionalNavigator");
    }

    [Test]
    public async Task ViewModelHostRecipeIncludesHostRegistrationGuidance()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetNavigationRecipe("viewmodel-host");

        await Assert.That(recipe.CodeSnippet).Contains("IViewModelRoutedViewHost");
        await Assert.That(recipe.CodeSnippet).Contains("SetMainNavigationHost");
        await Assert.That(recipe.Summary).Contains("host name");
    }

    [Test]
    public async Task PageNavigationRecipeKeepsWpfUiServicesSeparateFromViewModelHost()
    {
        var recipe = CrissCrossKnowledgeCatalog.CreateDefault().GetNavigationRecipe("page-navigation");

        await Assert.That(recipe.CodeSnippet).Contains("INavigationService");
        await Assert.That(recipe.CodeSnippet).Contains("IPageService");
        await Assert.That(recipe.Summary).Contains("not the same as VM-host navigation");
    }
}
