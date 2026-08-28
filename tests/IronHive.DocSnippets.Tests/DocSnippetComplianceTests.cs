using System.Runtime.CompilerServices;
using AwesomeAssertions;

namespace IronHive.DocSnippets.Tests;

/// <summary>
/// Compiles specific, known-important doc/skill code examples against the current public API,
/// catching drift like #115/#116 (a worked example that no longer matches the shipped signature)
/// before a reader hits it. Deliberately scoped to the sections that have actually broken, not
/// every ```csharp fence in the docs tree — many fences elsewhere are intentionally elided
/// pseudocode (e.g. `cfg => { ... }`) rather than standalone-compilable examples, and deciding how
/// (or whether) to make those checkable too is a separate design question, not a mechanical
/// extension of this harness. See the doc-snippet compile guard issue draft for that follow-up.
/// </summary>
public class DocSnippetComplianceTests
{
    // Shared by both targets below: they use IHiveService and AITool as stand-ins for values a
    // reader would already have in scope from earlier in the same document (a built IHiveService,
    // an AITool from their MCP client) — the snippet itself is what's under test, not how to obtain
    // those values.
    private const string MeaiPreamble = """
        using IronHive.Abstractions;
        using IronHive.Core.Extensions;
        using IronHive.Core.Microsoft;
        using Microsoft.Extensions.AI;

        IHiveService hive = null!;
        AITool mcpClientTool = null!;
        """;

    public static IEnumerable<object[]> MeaiCompatibilitySnippets()
    {
        yield return ["docs/ARCHITECTURE.md", "## Microsoft.Extensions.AI 호환", MeaiPreamble];
        yield return ["skills/ironhive/references/SERVICES.md", "## M.E.AI Compatibility", MeaiPreamble];
    }

    [Theory]
    [MemberData(nameof(MeaiCompatibilitySnippets))]
    public void MeaiCompatibilitySnippet_ShouldCompile(string relativeDocPath, string heading, string preamble)
    {
        var docPath = Path.Combine(RepoRoot(), relativeDocPath.Replace('/', Path.DirectorySeparatorChar));
        File.Exists(docPath).Should().BeTrue("the doc file the example lives in must exist at " + docPath);

        var markdown = File.ReadAllText(docPath);
        var fence = MarkdownCSharpExtractor.ExtractFirstFenceAfterHeading(markdown, heading);

        var source = preamble + Environment.NewLine + fence;
        var errors = DocSnippetCompiler.CompileTopLevelProgram(source);

        var diagnosticText = string.Join(Environment.NewLine, errors.Select(e => e.ToString()))
            .Replace("{", "{{", StringComparison.Ordinal)
            .Replace("}", "}}", StringComparison.Ordinal);

        errors.Should().BeEmpty(
            $"the '{heading}' example in {relativeDocPath} should compile against the current API " +
            "— if this fails, either the doc drifted from the API or the API changed and the doc " +
            $"needs updating. Diagnostics:{Environment.NewLine}{diagnosticText}");
    }

    private static string RepoRoot([CallerFilePath] string here = "")
        => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
}
