using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

[McpServerPromptType]
public static class CrissCrossPrompts
{
    [McpServerPrompt]
    [Description("Guide an agent to generate a CrissCross app using the project starter first.")]
    public static string GenerateCrissCrossApp() =>
        "Call crisscross_generate_project_starter first, inspect diagnostics, then emit only files returned by the manifest. Validate startup builder calls and platform packages.";

    [McpServerPrompt]
    [Description("Guide an agent to generate a CrissCross view model.")]
    public static string GenerateCrissCrossViewModel() =>
        "Generate a CrissCross view model with RxObject, RaiseAndSetIfChanged, ReactiveCommand.CreateFromTask, ObservableAsPropertyHelper, RxSchedulers, immutable replacement state, and no RxApp.";

    [McpServerPrompt]
    [Description("Guide an agent to wire CrissCross navigation.")]
    public static string WireCrissCrossNavigation() =>
        "Choose NavigationRegistry/IBidirectionalNavigator for navigation-only, IViewModelRoutedViewHost + SetMainNavigationHost for hosted VM navigation, or INavigationService/IPageService for page navigation.";

    [McpServerPrompt]
    [Description("Guide an agent to review CrissCross code.")]
    public static string ReviewCrissCrossCode() =>
        "Call crisscross_review_code_snippet and fix every Error or Warning diagnostic before finalizing.";

    [McpServerPrompt]
    [Description("Guide an agent to use CrissCross controls and state models.")]
    public static string ImplementCrissCrossControlUsage() =>
        "Call crisscross_find_control for the target platform, use source-backed controls only, and replace state-model snapshots rather than deep-mutating nested values.";
}
