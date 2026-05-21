namespace CrissCross.McpServer.Core.Review;

/// <summary>
/// Describes one diagnostic emitted by the CrissCross snippet reviewer.
/// </summary>
/// <param name="RuleId">The stable review rule identifier.</param>
/// <param name="Severity">The diagnostic severity.</param>
/// <param name="Message">The diagnostic message.</param>
/// <param name="Fix">Optional suggested fix.</param>
/// <param name="SourceReference">Optional source reference backing the diagnostic.</param>
public sealed record ReviewDiagnostic(
    string RuleId,
    ReviewSeverity Severity,
    string Message,
    string? Fix = null,
    string? SourceReference = null);
