using System.Text.Json;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class PackageInstallMetadataTests
{
    [Test]
    public async Task ReadmeContainsMcpNameAndQuickInstallLinks()
    {
        var root = FindRepositoryRoot();
        var readme = await File.ReadAllTextAsync(Path.Combine(root, "README.md"));

        await Assert.That(readme).Contains("<!-- mcp-name: io.github.chrispulman/crisscross-mcp-server -->");
        await Assert.That(readme).Contains("vscode.dev/redirect/mcp/install?name=crisscross-mcp-server");
        await Assert.That(readme).Contains("insiders.vscode.dev/redirect/mcp/install?name=crisscross-mcp-server");
        await Assert.That(readme).Contains("vs-open.link/mcp-install");
        await Assert.That(readme).Contains("CP.CrissCross.Mcp.Server@0.*");
    }

    [Test]
    public async Task McpManifestMatchesPackageInstallIdentity()
    {
        var root = FindRepositoryRoot();
        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, ".mcp", "server.json")));
        var package = manifest.RootElement.GetProperty("packages")[0];

        await Assert.That(manifest.RootElement.GetProperty("name").GetString()).IsEqualTo("io.github.chrispulman/crisscross-mcp-server");
        await Assert.That(package.GetProperty("identifier").GetString()).IsEqualTo("CP.CrissCross.Mcp.Server");
        await Assert.That(package.GetProperty("runtimeHint").GetString()).IsEqualTo("dnx");
        await Assert.That(package.GetProperty("transport").GetProperty("type").GetString()).IsEqualTo("stdio");
    }

    [Test]
    public async Task RepoLocalSkillIsPackagedWithServerProject()
    {
        var root = FindRepositoryRoot();
        var skill = await File.ReadAllTextAsync(Path.Combine(root, "skills", "crisscross", "SKILL.md"));
        var project = await File.ReadAllTextAsync(Path.Combine(root, "src", "CrissCross.McpServer", "CrissCross.McpServer.csproj"));

        await Assert.That(skill).Contains("name: crisscross");
        await Assert.That(skill).Contains("crisscross_generate_project_starter");
        await Assert.That(project).Contains("PackageType>McpServer");
        await Assert.That(project).Contains("ToolCommandName>crisscross-mcp-server");
        await Assert.That(project).Contains("skills\\crisscross\\SKILL.md");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "CrissCross.MCP.Server.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the CrissCross.MCP.Server repository root.");
    }
}
