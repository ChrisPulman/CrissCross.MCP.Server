namespace CrissCross.McpServer.Core.Review;

/// <summary>
/// Describes the severity of a CrissCross review diagnostic.
/// </summary>
public enum ReviewSeverity
{
    /// <summary>
    /// Informational review feedback.
    /// </summary>
    Info,

    /// <summary>
    /// Review feedback that should be fixed before finalizing code.
    /// </summary>
    Warning,

    /// <summary>
    /// Review feedback that marks code as incompatible with CrissCross guidance.
    /// </summary>
    Error
}
