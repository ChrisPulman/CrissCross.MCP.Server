using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CrissCross.McpServer.Mcp;

/// <summary>
/// MCP prompts that guide agents toward source-backed CrissCross code generation and review workflows.
/// </summary>
[McpServerPromptType]
public sealed class CrissCrossPrompts
{
    /// <summary>
    /// Builds a prompt for generating a CrissCross app from the project starter tool.
    /// </summary>
    /// <returns>A prompt instructing the agent to call the project starter before emitting files.</returns>
    [McpServerPrompt(Name = "generate_crisscross_app")]
    [Description("Guide an agent to generate a CrissCross app using the project starter first.")]
    public static string GenerateCrissCrossApp() =>
        "Call crisscross_generate_project_starter first, inspect diagnostics, then emit only files returned by the manifest. Validate startup builder calls and platform packages.";

    /// <summary>
    /// Builds a prompt for generating a CrissCross view model.
    /// </summary>
    /// <returns>A prompt containing CrissCross view-model guardrails.</returns>
    [McpServerPrompt(Name = "generate_crisscross_viewmodel")]
    [Description("Guide an agent to generate a CrissCross view model.")]
    public static string GenerateCrissCrossViewModel() =>
        "Generate a CrissCross view model with RxObject, RaiseAndSetIfChanged, ReactiveCommand.CreateFromTask, ObservableAsPropertyHelper, RxSchedulers, immutable replacement state, and no RxApp.";

    /// <summary>
    /// Builds a prompt for wiring CrissCross navigation.
    /// </summary>
    /// <returns>A prompt describing the available CrissCross navigation patterns.</returns>
    [McpServerPrompt(Name = "wire_crisscross_navigation")]
    [Description("Guide an agent to wire CrissCross navigation.")]
    public static string WireCrissCrossNavigation() =>
        "Choose NavigationRegistry/IBidirectionalNavigator for navigation-only, IViewModelRoutedViewHost + SetMainNavigationHost for hosted VM navigation, or INavigationService/IPageService for page navigation.";

    /// <summary>
    /// Builds a prompt for reviewing CrissCross code.
    /// </summary>
    /// <returns>A prompt instructing the agent to use the review tool and fix diagnostics.</returns>
    [McpServerPrompt(Name = "review_crisscross_code")]
    [Description("Guide an agent to review CrissCross code.")]
    public static string ReviewCrissCrossCode() =>
        "Call crisscross_review_code_snippet and fix every Error or Warning diagnostic before finalizing.";

    /// <summary>
    /// Builds a prompt for implementing source-backed CrissCross controls.
    /// </summary>
    /// <returns>A prompt instructing the agent to use control lookup and replacement state semantics.</returns>
    [McpServerPrompt(Name = "implement_crisscross_control_usage")]
    [Description("Guide an agent to use CrissCross controls and state models.")]
    public static string ImplementCrissCrossControlUsage() =>
        "Call crisscross_find_control for the target platform, use source-backed controls only, and replace state-model snapshots rather than deep-mutating nested values.";
}
