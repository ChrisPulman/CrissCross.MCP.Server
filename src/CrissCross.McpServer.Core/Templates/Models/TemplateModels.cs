namespace CrissCross.McpServer.Core.Templates.Models;

/// <summary>
/// Describes the severity of a template validation diagnostic.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Informational diagnostic that does not block generation.
    /// </summary>
    Info,

    /// <summary>
    /// Warning diagnostic that allows previews but should be reviewed.
    /// </summary>
    Warning,

    /// <summary>
    /// Error diagnostic that prevents generated-file previews.
    /// </summary>
    Error
}

/// <summary>
/// Describes the available template targets and wizard modes.
/// </summary>
/// <param name="Targets">Supported platform targets.</param>
/// <param name="Modes">Supported wizard modes.</param>
public sealed record TemplateOptionSet(
    IReadOnlyList<FrameworkTarget> Targets,
    IReadOnlyList<WizardMode> Modes);

/// <summary>
/// Describes the file, package, startup, and validation manifest for a template target.
/// </summary>
/// <param name="Target">The platform target.</param>
/// <param name="DefaultTargetFramework">The default target framework.</param>
/// <param name="SupportedTargetFrameworks">Supported target frameworks.</param>
/// <param name="RequiredCrissCrossPackages">Required CrissCross packages.</param>
/// <param name="OptionalCrissCrossPackages">Optional CrissCross packages.</param>
/// <param name="Files">Template file specifications.</param>
/// <param name="StartupRequirements">Startup requirements for the target.</param>
/// <param name="ValidationRules">Validation rules that apply to the target.</param>
public sealed record TemplateManifest(
    FrameworkTarget Target,
    string DefaultTargetFramework,
    IReadOnlyList<string> SupportedTargetFrameworks,
    IReadOnlyList<string> RequiredCrissCrossPackages,
    IReadOnlyList<string> OptionalCrissCrossPackages,
    IReadOnlyList<TemplateFileSpec> Files,
    IReadOnlyList<string> StartupRequirements,
    IReadOnlyList<string> ValidationRules);

/// <summary>
/// Describes one generated file in a template manifest.
/// </summary>
/// <param name="RelativePath">The relative output path.</param>
/// <param name="TemplatePath">The source template path.</param>
/// <param name="IncludeInNavigationOnly">Whether the file is included in navigation-only mode.</param>
/// <param name="IncludeInNavigationAndUi">Whether the file is included in navigation-and-UI mode.</param>
/// <param name="RequiredControls">Controls required by the file.</param>
public sealed record TemplateFileSpec(
    string RelativePath,
    string TemplatePath,
    bool IncludeInNavigationOnly,
    bool IncludeInNavigationAndUi,
    IReadOnlyList<string> RequiredControls);

/// <summary>
/// Represents a preview-only generated file.
/// </summary>
/// <param name="RelativePath">The relative output path.</param>
/// <param name="Content">The generated file content.</param>
/// <param name="IsExecutable">A value indicating whether the file should be executable when written.</param>
/// <param name="SourceTemplate">The source template identifier.</param>
public sealed record GeneratedFile(
    string RelativePath,
    string Content,
    bool IsExecutable = false,
    string? SourceTemplate = null);

/// <summary>
/// Captures all inputs for preview-only project starter generation.
/// </summary>
/// <param name="AppName">The generated application/project name.</param>
/// <param name="RootNamespace">The generated root namespace.</param>
/// <param name="Target">The platform target.</param>
/// <param name="Mode">The wizard mode.</param>
/// <param name="TargetFramework">The target framework.</param>
/// <param name="HostName">The CrissCross navigation host name.</param>
/// <param name="Screens">Screen names to generate.</param>
/// <param name="IncludeControls">Control names to include in UI mode.</param>
/// <param name="IncludeTests">Whether to include generated smoke-test previews.</param>
/// <param name="IncludeReadme">Whether to include a generated README preview.</param>
/// <param name="UseCentralPackageManagement">Whether to include a central package management preview.</param>
/// <param name="OverwriteExistingFiles">Whether a caller intends to overwrite existing files.</param>
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

/// <summary>
/// Describes a validation issue found while preparing template previews.
/// </summary>
/// <param name="RuleId">The stable validation rule identifier.</param>
/// <param name="Severity">The diagnostic severity.</param>
/// <param name="Message">The diagnostic message.</param>
/// <param name="FilePath">Optional file path related to the diagnostic.</param>
/// <param name="SuggestedFix">Optional suggested fix.</param>
public sealed record ValidationDiagnostic(
    string RuleId,
    ValidationSeverity Severity,
    string Message,
    string? FilePath = null,
    string? SuggestedFix = null);

/// <summary>
/// Contains the result of preview-only project starter generation.
/// </summary>
/// <param name="Request">The request that produced the result.</param>
/// <param name="Files">Generated-file previews.</param>
/// <param name="Diagnostics">Validation diagnostics.</param>
/// <param name="NextSteps">Recommended next steps for the caller.</param>
public sealed record TemplateGenerationResult(
    TemplateGenerationRequest Request,
    IReadOnlyList<GeneratedFile> Files,
    IReadOnlyList<ValidationDiagnostic> Diagnostics,
    IReadOnlyList<string> NextSteps);
