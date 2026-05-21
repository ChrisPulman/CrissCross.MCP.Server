using CrissCross.McpServer.Mcp;

namespace CrissCross.McpServer.Tests.Mcp;

public sealed class McpToolAdapterTests
{
    [Test]
    public async Task ToolMethodsReturnDeterministicJsonAndDoNotWriteStdout()
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        string packageJson;
        string starterJson;
        try
        {
            packageJson = CrissCrossTools.crisscross_get_package_matrix("wpf", null);
            starterJson = CrissCrossTools.crisscross_generate_project_starter("wpf", "navigation-only", "SampleApp", "SampleApp", "Home", "");
        }
        finally
        {
            Console.SetOut(original);
        }

        await Assert.That(writer.ToString()).IsEmpty();
        await Assert.That(packageJson).Contains("CrissCross.WPF");
        await Assert.That(starterJson).Contains("GeneratedFile");
        await Assert.That(starterJson).Contains("App.xaml.cs");
    }
}
