namespace CrissCross.McpServer.Core.Templates.Models;

/// <summary>
/// Identifies a CrissCross-supported UI platform.
/// </summary>
public enum FrameworkTarget
{
    /// <summary>
    /// Avalonia target.
    /// </summary>
    Avalonia,

    /// <summary>
    /// .NET MAUI target.
    /// </summary>
    Maui,

    /// <summary>
    /// Windows Forms target.
    /// </summary>
    WinForms,

    /// <summary>
    /// Windows Presentation Foundation target.
    /// </summary>
    Wpf
}
