using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

/// <summary>
/// Exposes the supported CrissCross template wizard combinations.
/// </summary>
public sealed class TemplateCatalog
{
    /// <summary>
    /// Gets every supported platform and wizard-mode combination.
    /// </summary>
    /// <returns>The supported template combinations.</returns>
    public IReadOnlyList<(FrameworkTarget Target, WizardMode Mode)> GetSupportedCombinations() =>
        Enum.GetValues<FrameworkTarget>().SelectMany(_ => Enum.GetValues<WizardMode>(), (target, mode) => (target, mode)).ToArray();
}
