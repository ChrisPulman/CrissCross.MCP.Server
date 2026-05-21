namespace CrissCross.McpServer.Core.Review;

public sealed record ReviewDiagnostic(
    string RuleId,
    ReviewSeverity Severity,
    string Message,
    string? Fix = null,
    string? SourceReference = null);
