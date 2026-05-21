namespace CrissCross.McpServer.Core.Templates.Models;

/// <summary>
/// Identifies the template wizard mode used for generated previews.
/// </summary>
public enum WizardMode
{
    /// <summary>
    /// Generates navigation setup without CrissCross UI controls.
    /// </summary>
    NavigationOnly,

    /// <summary>
    /// Generates navigation setup plus CrissCross UI control previews.
    /// </summary>
    NavigationAndUi
}
