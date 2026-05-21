namespace CrissCross.McpServer.Core.Review;

/// <summary>
/// Describes a deterministic CrissCross review rule.
/// </summary>
/// <param name="RuleId">The stable review rule identifier.</param>
/// <param name="Severity">The default severity for the rule.</param>
/// <param name="Description">The rule description.</param>
/// <param name="Fix">The recommended fix.</param>
/// <param name="SourceReference">Optional source reference backing the rule.</param>
public sealed record ReviewRule(
    string RuleId,
    ReviewSeverity Severity,
    string Description,
    string Fix,
    string? SourceReference = null);
