namespace CrissCross.McpServer.Core.Review;

public sealed record ReviewRule(
    string RuleId,
    ReviewSeverity Severity,
    string Description,
    string Fix,
    string? SourceReference = null);
