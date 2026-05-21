namespace CrissCross.McpServer.Core.Templates.Models;

public enum ValidationSeverity { Info, Warning, Error }

public sealed record TemplateOptionSet(
    IReadOnlyList<FrameworkTarget> Targets,
    IReadOnlyList<WizardMode> Modes);

public sealed record TemplateManifest(
    FrameworkTarget Target,
    string DefaultTargetFramework,
    IReadOnlyList<string> SupportedTargetFrameworks,
    IReadOnlyList<string> RequiredCrissCrossPackages,
    IReadOnlyList<string> OptionalCrissCrossPackages,
    IReadOnlyList<TemplateFileSpec> Files,
    IReadOnlyList<string> StartupRequirements,
    IReadOnlyList<string> ValidationRules);

public sealed record TemplateFileSpec(
    string RelativePath,
    string TemplatePath,
    bool IncludeInNavigationOnly,
    bool IncludeInNavigationAndUi,
    IReadOnlyList<string> RequiredControls);

public sealed record GeneratedFile(
    string RelativePath,
    string Content,
    bool IsExecutable = false,
    string? SourceTemplate = null);

public sealed record TemplateGenerationRequest(
    string AppName,
    string RootNamespace,
    FrameworkTarget Target,
    WizardMode Mode,
    string TargetFramework,
    string? HostName,
    IReadOnlyList<string> Screens,
    IReadOnlyList<string> IncludeControls,
    bool IncludeTests,
    bool IncludeReadme,
    bool UseCentralPackageManagement,
    bool OverwriteExistingFiles);

public sealed record ValidationDiagnostic(
    string RuleId,
    ValidationSeverity Severity,
    string Message,
    string? FilePath = null,
    string? SuggestedFix = null);

public sealed record TemplateGenerationResult(
    TemplateGenerationRequest Request,
    IReadOnlyList<GeneratedFile> Files,
    IReadOnlyList<ValidationDiagnostic> Diagnostics,
    IReadOnlyList<string> NextSteps);
