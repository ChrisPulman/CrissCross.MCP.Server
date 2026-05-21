using CrissCross.McpServer.Core.Templates.Models;

namespace CrissCross.McpServer.Core.Templates;

public sealed class TemplateCatalog
{
    public IReadOnlyList<(FrameworkTarget Target, WizardMode Mode)> GetSupportedCombinations() =>
        Enum.GetValues<FrameworkTarget>().SelectMany(_ => Enum.GetValues<WizardMode>(), (target, mode) => (target, mode)).ToArray();
}
