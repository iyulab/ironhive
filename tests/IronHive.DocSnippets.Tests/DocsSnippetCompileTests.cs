using System.Reflection;
using Iyu.Conventions.Testing.Snippets;
using Microsoft.CodeAnalysis;
using MarkdownSnippets = Iyu.Conventions.Testing.Snippets.DocSnippets;

namespace IronHive.DocSnippets.Tests;

/// <summary>
/// Compiles the C# blocks of the README, the guides under docs/ and the skill references against the current assemblies.
/// A reader copies these — and an agent that loads the skill writes code from them; a block that no longer matches the
/// shipped API fails here instead of in their build.
/// </summary>
/// <remarks>
/// Each guide compiles as a whole (<see cref="SnippetCompiler.CompileDocument"/>): a type one block declares is visible to
/// the next, each block's statements run in a method of their own, and a block that restates a library type compiles alone.
/// A block gets the usings below and the stand-ins below when it uses the name without declaring it — values a reader
/// already has from the surrounding text (a built <c>IHiveService</c>, a configured builder). A section that is
/// deliberately not a program is in <see cref="Fragments"/> with the reason. A document with a C# block is either in
/// <see cref="Checked"/> or in <see cref="NotYetChecked"/> with the reason — never silently in neither.
/// </remarks>
public class DocsSnippetCompileTests
{
    // Documents whose every C# block compiles (or is a listed fragment).
    private static readonly string[] Checked =
    [
        "README.md",
        "docs/AGENTS.md",
        "docs/ARCHITECTURE.md",
        "docs/MIDDLEWARE.md",
        "docs/ORCHESTRATION.md",
        "docs/PLUGINS.md",
        "docs/PROVIDERS.md",
        "docs/SERVICES.md",
        "docs/SETUP.md",
        "docs/TOOLS.md",
        "skills/ironhive/SKILL.md",
        "skills/ironhive/references/MIDDLEWARE.md",
        "skills/ironhive/references/ORCHESTRATION.md",
        "skills/ironhive/references/PROVIDERS.md",
        "skills/ironhive/references/SERVICES.md",
        "skills/ironhive/references/SETUP.md",
        "skills/ironhive/references/TOOLS.md",
        "src/IronHive.Providers.OpenAI.Compatible/GPUStack/README.md",
    ];

    // Documents with C# blocks that are not compiled yet, with the reason. Shrink this; a new document with a C# block
    // must land in one of the two lists (see EveryDocumentWithCSharp_IsCheckedOrListed).
    private static readonly Dictionary<string, string> NotYetChecked = new(StringComparer.Ordinal);

    // A block that is deliberately not a program (a signature sketch, pseudocode), by the heading it sits under, with the
    // reason. Shrink this, never grow it silently.
    private static readonly SnippetRoster Fragments = new(new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["HiveServiceBuilder Full Signature (key methods)"] =
            "a signature summary of the builder's extension methods across packages, declared without bodies",
        ["Telemetry"] = "registers OpenTelemetry (OpenTelemetry.Extensions.Hosting), a package the library does not reference",
    });

    private static readonly string[] AssembliesToLoad =
    [
        "IronHive.Abstractions", "IronHive.Core", "IronHive.Extensions.AI",
        "IronHive.Plugins.MCP",
        "IronHive.Providers.Anthropic", "IronHive.Providers.GoogleAI", "IronHive.Providers.OpenAI",
        "IronHive.Providers.OpenAI.Compatible", "IronHive.Providers.Declarations",
        "Microsoft.Extensions.AI.Abstractions", "Microsoft.Extensions.Configuration.Abstractions",
        "Microsoft.Extensions.DependencyInjection", "Microsoft.Extensions.DependencyInjection.Abstractions",
        "Microsoft.Extensions.Hosting.Abstractions", "Microsoft.Extensions.Logging.Abstractions",
        "Google.Apis.Auth",
    ];

    // Every namespace the IronHive packages export, as a reader who followed the guides' setup has them; plus the BCL
    // namespaces a project has implicitly and the ones the guides use without saying so.
    private static readonly string[] Usings =
    [
        "System", "System.Collections.Generic", "System.ComponentModel", "System.Diagnostics.CodeAnalysis", "System.IO", "System.Linq", "System.Net.Http",
        "System.Runtime.CompilerServices", "System.Text.Json", "System.Threading", "System.Threading.Tasks", "Microsoft.Extensions.DependencyInjection",
        .. AssembliesToLoad.Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal))
            .SelectMany(n => Assembly.Load(n).GetExportedTypes())
            .Select(t => t.Namespace)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal),
    ];

    // The agents the guides' text has already built (a researcher, a reviewer, a triage agent...): any IAgent.
    private static readonly string[] AgentNames =
    [
        "agent", "agentA", "agentB", "agentC", "analystA", "analystB", "analystC", "backupAgent", "baseAgent",
        "billingAgent", "classifyAgent", "coordinatorAgent", "expertA", "expertB", "fallbackAgent", "finalizer",
        "formatterAgent", "generalAgent", "hubAgent", "managerAgent", "mergeAgent", "moderator", "primaryAgent",
        "researchAgent", "reviewAgent", "reviewer1", "reviewer2", "reviewerAgent", "specialistA", "specialistB",
        "specialistC", "startAgent", "summarizerAgent", "summaryAgent", "supportAgent", "technicalAgent",
        "translatorAgent", "triageAgent", "writer", "writerAgent",
    ];

    // Values the guides' running text gives the reader, typed as the API takes them.
    private static readonly Dictionary<string, string> StandIns = new(
        AgentNames.Select(n => KeyValuePair.Create(n, $"IAgent {n} = null!;")), StringComparer.Ordinal)
    {
        ["hive"] = "IHiveService hive = null!;",
        ["builder"] = "IHiveServiceBuilder builder = null!;",
        ["tools"] = "ToolCollection tools = new();",
        ["serviceProvider"] = "IServiceProvider serviceProvider = null!;",
        ["messages"] = "List<Message> messages = [];",
        ["cancellationToken"] = "CancellationToken cancellationToken = default;",
        ["openAIGenerator"] = "IMessageGenerator openAIGenerator = null!;",
        ["anthropicGenerator"] = "IMessageGenerator anthropicGenerator = null!;",
        ["mcpClientTool"] = "Microsoft.Extensions.AI.AITool mcpClientTool = null!;",
        ["stoppingToken"] = "CancellationToken stoppingToken = default;",
        ["services"] = "IServiceCollection services = new ServiceCollection();",
        ["existingHiveInstance"] = "IHiveService existingHiveInstance = null!;",
        ["yamlString"] = "string yamlString = \"\";",
        ["yamlConfig"] = "string yamlConfig = \"\";",
        ["tomlString"] = "string tomlString = \"\";",
        ["json"] = "string json = \"{}\";",
        ["mcpManager"] = "McpClientManager mcpManager = null!;",
        ["messageService"] = "IMessageService messageService = null!;",
        ["input"] = "ToolInput input = new();",
        ["output"] = "ToolOutput output = ToolOutput.Success(\"\");",
        ["request"] = "MessageRequest request = null!;",
        ["delegateFn"] = "Func<string, string> delegateFn = text => text;",
        ["config"] = "OpenAIConfig config = null!;",
        ["secretStore"] = "ISecretStore secretStore = null!;",
        ["chatClient"] = "Microsoft.Extensions.AI.IChatClient chatClient = null!;",
        ["text"] = "string text = \"\";",
        ["texts"] = "IEnumerable<string> texts = [];",
        ["stream"] = "Stream stream = Stream.Null;",
        ["pdfStream"] = "Stream pdfStream = Stream.Null;",
        ["bytes"] = "byte[] bytes = [];",
        ["imageBytes"] = "byte[] imageBytes = [];",
        ["ISecretStore"] = "public interface ISecretStore { string? Read(string name); }",
        ["CompactingMiddleware"] = "public sealed class CompactingMiddleware : IMessageMiddleware { }",
        ["orchestrator"] = "IAgentOrchestrator orchestrator = null!;",
        ["orch"] = "IAgentOrchestrator orch = null!;",
        ["options"] = "OrchestratorOptions options = null!;",
        ["groupChat"] = "GroupChatOrchestratorBuilder groupChat = new();",
        ["analysisExecutor"] = "ITypedExecutor<string, Analysis> analysisExecutor = null!;",
        ["summaryExecutor"] = "ITypedExecutor<Analysis, Summary> summaryExecutor = null!;",
        ["Analysis"] = "public sealed record Analysis(string Text);",
        ["Summary"] = "public sealed record Summary(string Text);",
        // The reader's own services and types that a tool asks the container for, or a tool argument deserializes to.
        ["ICodeRunner"] = "public interface ICodeRunner { Task<string> RunAsync(string code); }",
        ["ISearchService"] = "public interface ISearchService { Task<string> SearchAsync(string query); }",
        ["IMyService"] = "public interface IMyService { }",
        ["MyOptions"] = "public sealed class MyOptions { public int Limit { get; set; } }",
    };

    private static readonly IReadOnlyList<MetadataReference> References =
        SnippetCompiler.ReferencesFor([.. AssembliesToLoad.Select(Assembly.Load)]);

    private static readonly SnippetCompiler Compiler = new(References) { Usings = Usings, StandIns = StandIns, CheckListings = true };

    // An ASP.NET Core block's `builder` is the host's application builder (builder.Services, builder.Configuration), not
    // the hive builder the other blocks configure.
    private static readonly SnippetCompiler HostCompiler = new(References)
    {
        CheckListings = true,
        Usings = [.. Usings, "Microsoft.Extensions.Hosting"],
        StandIns = new Dictionary<string, string>(StandIns, StringComparer.Ordinal)
        {
            ["builder"] = "IHostApplicationBuilder builder = null!;",
        },
    };

    private static readonly IReadOnlyList<DocSnippet> Snippets =
        [.. Checked.SelectMany(d => MarkdownSnippets.Read(Path.Combine(RepositoryRoot(), d), d))];

    // An ASP.NET Core block's `builder` is the host's: it is read from the same document compiled by HostCompiler, so it
    // still sees the document's types. The other blocks read the document compiled by Compiler.
    private static bool IsHostBlock(DocSnippet snippet) => snippet.Code.Contains("builder.Services", StringComparison.Ordinal);

    private static readonly Dictionary<string, Lazy<DocumentCompilation>> HostDocuments = Checked.ToDictionary(
        d => d,
        d => new Lazy<DocumentCompilation>(() => HostCompiler.CompileDocument(
            [.. Snippets.Where(s => s.File == d && !Fragments.IsFragment(s))])),
        StringComparer.Ordinal);

    // Each document's examples compile together: a guide declares a type (a tool class, a workflow step) in one example
    // and uses it in the next.
    private static readonly Dictionary<string, Lazy<DocumentCompilation>> Documents = Checked.ToDictionary(
        d => d,
        d => new Lazy<DocumentCompilation>(() => Compiler.CompileDocument(
            [.. Snippets.Where(s => s.File == d && !IsHostBlock(s) && !Fragments.IsFragment(s))])),
        StringComparer.Ordinal);

    public static TheoryData<string> Blocks() => new(Snippets.Select(s => s.Key));

    public static TheoryData<string> CheckedDocuments() => new(Checked);

    [Theory]
    [MemberData(nameof(Blocks))]
    public void DocsBlock_Compiles(string key)
    {
        var snippet = Snippets.Single(s => s.Key == key);
        if (Fragments.IsFragment(snippet))
            return;
        if (IsHostBlock(snippet))
            HostDocuments[snippet.File].Value.ShouldCompile(snippet);
        else
            Documents[snippet.File].Value.ShouldCompile(snippet);
    }

    /// <summary>A document's type stand-ins compile against the current API (they belong to no single example).</summary>
    [Theory]
    [MemberData(nameof(CheckedDocuments))]
    public void Document_HasNoErrorsOutsideItsExamples(string document) => Documents[document].Value.ShouldHaveNoSharedErrors();

    [Fact]
    public void EveryCheckedBlock_IsFoundAndFragmentsNameRealHeadings()
    {
        Assert.True(Snippets.Count(s => s.File == "README.md") >= 4, "expected the README's C# blocks");
        Fragments.ShouldMatch([.. Snippets], atLeast: 196);
    }

    /// <summary>
    /// A document with a C# block is either compiled or listed as not yet compiled with the reason, and the two lists
    /// name only documents that exist and have C# blocks — so a new guide cannot be skipped silently and a repaired one
    /// cannot stay on the waiting list.
    /// </summary>
    [Fact]
    public void EveryDocumentWithCSharp_IsCheckedOrListed()
    {
        var withCSharp = DocumentsWithCSharp().ToList();

        Assert.All(withCSharp, d => Assert.True(Checked.Contains(d) || NotYetChecked.ContainsKey(d), $"{d} has C# blocks but is in neither list"));
        Assert.All(Checked, d => Assert.False(NotYetChecked.ContainsKey(d), $"{d} is both checked and waiting"));
        Assert.All(Checked.Concat(NotYetChecked.Keys), d => Assert.Contains(d, withCSharp));
    }

    /// <summary>
    /// The guides' type listings (an interface or enum shown with its members) are checked against the real types
    /// (<see cref="SnippetCompiler.CheckListings"/>), not compiled as a second type of the same name. A floor, so a listing
    /// the kit stops recognising shows here instead of passing unchecked. Turning it on found 11 drifted members in three
    /// guides (an enum's members, a storage interface's signatures, the planning interfaces).
    /// </summary>
    [Fact]
    public void TypeListings_AreCheckedAgainstTheRealTypes()
    {
        // Documents, not HostDocuments: the host compilation takes every block of a document again, and a listing is never a host block.
        var listed = Checked.Sum(d => Documents[d].Value.ListedTypes);
        Assert.True(listed >= 43, $"expected the guides' type listings to be checked, found {listed}");
    }

    /// <summary>Positive control: the compiler rejects the GPUStack README's old message example.</summary>
    [Fact]
    public void Compile_RejectsTheRemovedMessageApi()
    {
        var errors = Compiler.Compile("""
            var generator = hive.GetMessageGenerator("gpustack");
            var response = await generator.GenerateMessageAsync(new MessageGenerationRequest
            {
                Model = "m",
                Messages = [new UserMessage { Content = [new TextMessageContent { Value = "hi" }] }],
            });
            """);

        Assert.NotEmpty(errors);
    }

    /// <summary>Every Markdown document of the repository with a C# block, except changelogs and the tests' own.</summary>
    private static IEnumerable<string> DocumentsWithCSharp()
    {
        var root = RepositoryRoot();
        return Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(root, p).Replace('\\', '/'))
            .Where(p => !p.StartsWith("tests/", StringComparison.Ordinal)
                        && !p.Contains("/bin/", StringComparison.Ordinal) && !p.Contains("/obj/", StringComparison.Ordinal)
                        && !p.StartsWith('.')
                        && !Path.GetFileName(p).Equals("CHANGELOG.md", StringComparison.OrdinalIgnoreCase))
            .Where(p => MarkdownSnippets.Read(Path.Combine(root, p), p).Count > 0)
            .Order(StringComparer.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("IronHive.slnx not found above the test output directory");
    }
}
