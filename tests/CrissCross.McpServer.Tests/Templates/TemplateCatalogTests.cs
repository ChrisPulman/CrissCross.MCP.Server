using CrissCross.McpServer.Core.Catalog;
using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Tests.Templates;

public sealed class TemplateCatalogTests
{
    [Test]
    public async Task TemplateCatalogListsEightTargetModeCombinations()
    {
        var combinations = CrissCrossKnowledgeCatalog.CreateDefault().GetTemplateCombinations().ToArray();

        await Assert.That(combinations.Length).IsEqualTo(8);
        await Assert.That(combinations).Contains((FrameworkTarget.Wpf, WizardMode.NavigationOnly));
        await Assert.That(combinations).Contains((FrameworkTarget.Wpf, WizardMode.NavigationAndUi));
        await Assert.That(combinations).Contains((FrameworkTarget.Avalonia, WizardMode.NavigationOnly));
        await Assert.That(combinations).Contains((FrameworkTarget.Avalonia, WizardMode.NavigationAndUi));
        await Assert.That(combinations).Contains((FrameworkTarget.Maui, WizardMode.NavigationOnly));
        await Assert.That(combinations).Contains((FrameworkTarget.Maui, WizardMode.NavigationAndUi));
        await Assert.That(combinations).Contains((FrameworkTarget.WinForms, WizardMode.NavigationOnly));
        await Assert.That(combinations).Contains((FrameworkTarget.WinForms, WizardMode.NavigationAndUi));
    }
}
