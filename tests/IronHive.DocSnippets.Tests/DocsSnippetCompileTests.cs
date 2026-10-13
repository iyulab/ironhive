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
/// Each block is compiled as a top-level program by <see cref="SnippetCompiler"/>: its own usings, the usings below, and the
/// stand-ins below when the block uses the name without declaring it — values a reader already has from the surrounding
/// text (a built <c>IHiveService</c>, a configured builder), not part of what the block shows. A section that is
/// deliberately not a program is in <see cref="Fragments"/> with the reason. A document is either in
/// <see cref="Checked"/> or in <see cref="NotYetChecked"/> with the reason — never silently in neither.
/// </remarks>
public class DocsSnippetCompileTests
{
    // Documents whose every C# block compiles (or is a listed fragment).
    private static readonly string[] Checked =
    [
        "README.md",
        "docs/ARCHITECTURE.md",
        "src/IronHive.Providers.OpenAI.Compatible/GPUStack/README.md",
        "docs/MEMORY.md",
        "docs/STORAGES.md",
        "skills/ironhive/references/MEMORY.md",
        "skills/ironhive/references/MIDDLEWARE.md",
    ];

    private const string Pending = "migration to the kit compiler (run 146) - blocks not yet repaired";

    // Documents with C# blocks that are not compiled yet, with the reason. Shrink this; a new document with a C# block
    // must land in one of the two lists (see EveryDocumentWithCSharp_IsCheckedOrListed).
    private static readonly Dictionary<string, string> NotYetChecked = new(StringComparer.Ordinal)
    {
        ["docs/AGENTS.md"] = Pending,
        ["docs/MIDDLEWARE.md"] = Pending,
        ["docs/ORCHESTRATION.md"] = Pending,
        ["docs/PLUGINS.md"] = Pending,
        ["docs/PROVIDERS.md"] = Pending,
        ["docs/SERVICES.md"] = Pending,
        ["docs/SETUP.md"] = Pending,
        ["docs/TOOLS.md"] = Pending,
        ["docs/WORKFLOW.md"] = Pending,
        ["skills/ironhive/SKILL.md"] = Pending,
        ["skills/ironhive/references/ORCHESTRATION.md"] = Pending,
        ["skills/ironhive/references/PROVIDERS.md"] = Pending,
        ["skills/ironhive/references/SERVICES.md"] = Pending,
        ["skills/ironhive/references/SETUP.md"] = Pending,
        ["skills/ironhive/references/STORAGES.md"] = Pending,
        ["skills/ironhive/references/TOOLS.md"] = Pending,
    };

    // A block that is deliberately not a program (a signature sketch, pseudocode), by the heading it sits under, with the
    // reason. Shrink this, never grow it silently.
    private static readonly SnippetRoster Fragments = new(new Dictionary<string, string>(StringComparer.Ordinal));

    private static readonly string[] AssembliesToLoad =
    [
        "IronHive.Abstractions", "IronHive.Core", "IronHive.Extensions.AI",
        "IronHive.Plugins.MCP", "IronHive.Plugins.OpenAPI",
        "IronHive.Providers.Anthropic", "IronHive.Providers.GoogleAI", "IronHive.Providers.OpenAI",
        "IronHive.Providers.OpenAI.Compatible",
        "IronHive.Storages.Amazon", "IronHive.Storages.Azure", "IronHive.Storages.Qdrant", "IronHive.Storages.RabbitMQ",
        "Microsoft.Extensions.AI.Abstractions", "Microsoft.Extensions.Configuration.Abstractions",
        "Microsoft.Extensions.DependencyInjection", "Microsoft.Extensions.DependencyInjection.Abstractions",
        "Microsoft.Extensions.Hosting.Abstractions", "Microsoft.Extensions.Logging.Abstractions",
    ];

    // Every namespace the IronHive packages export, as a reader who followed the guides' setup has them; plus the BCL
    // namespaces a project has implicitly and the ones the guides use without saying so.
    private static readonly string[] Usings =
    [
        "System", "System.Collections.Generic", "System.ComponentModel", "System.IO", "System.Linq", "System.Net.Http",
        "System.Text.Json", "System.Threading", "System.Threading.Tasks", "Microsoft.Extensions.DependencyInjection",
        .. AssembliesToLoad.Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal))
            .SelectMany(n => Assembly.Load(n).GetExportedTypes())
            .Select(t => t.Namespace)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal),
    ];

    // Values the guides' running text gives the reader, typed as the API takes them.
    private static readonly Dictionary<string, string> StandIns = new(StringComparer.Ordinal)
    {
        ["hive"] = "IHiveService hive = null!;",
        ["builder"] = "IHiveServiceBuilder builder = null!;",
        ["agent"] = "IAgent agent = null!;",
        ["baseAgent"] = "IAgent baseAgent = null!;",
        ["tools"] = "ToolCollection tools = new();",
        ["serviceProvider"] = "IServiceProvider serviceProvider = null!;",
        ["messages"] = "List<Message> messages = [];",
        ["cancellationToken"] = "CancellationToken cancellationToken = default;",
        ["openAIGenerator"] = "IMessageGenerator openAIGenerator = null!;",
        ["anthropicGenerator"] = "IMessageGenerator anthropicGenerator = null!;",
        ["qdrantStorage"] = "IVectorStorage qdrantStorage = null!;",
        ["localStorage"] = "IVectorStorage localStorage = null!;",
        ["mcpClientTool"] = "Microsoft.Extensions.AI.AITool mcpClientTool = null!;",
        ["backupAgent"] = "IAgent backupAgent = null!;",
        ["collection"] = "IMemoryCollection collection = null!;",
        ["context"] = "MemoryContext context = null!;",
        ["stoppingToken"] = "CancellationToken stoppingToken = default;",
        ["queueStorage"] = "IQueueStorage queueStorage = null!;",
        ["rabbitMQStorage"] = "IQueueStorage rabbitMQStorage = null!;",
        ["s3Config"] = "AmazonS3Config s3Config = null!;",
        ["localVecConfig"] = "LocalVectorConfig localVecConfig = null!;",
        ["localQueueConfig"] = "LocalQueueConfig localQueueConfig = null!;",
    };

    private static readonly IReadOnlyList<MetadataReference> References =
        SnippetCompiler.ReferencesFor([.. AssembliesToLoad.Select(Assembly.Load)]);

    private static readonly SnippetCompiler Compiler = new(References) { Usings = Usings, StandIns = StandIns };

    // An ASP.NET Core block's `builder` is the host's application builder (builder.Services, builder.Configuration), not
    // the hive builder the other blocks configure.
    private static readonly SnippetCompiler HostCompiler = new(References)
    {
        Usings = [.. Usings, "Microsoft.Extensions.Hosting"],
        StandIns = new Dictionary<string, string>(StandIns, StringComparer.Ordinal)
        {
            ["builder"] = "IHostApplicationBuilder builder = null!;",
        },
    };

    private static readonly IReadOnlyList<DocSnippet> Snippets =
        [.. Checked.SelectMany(d => MarkdownSnippets.Read(Path.Combine(RepositoryRoot(), d), d))];

    public static TheoryData<string> Blocks() => new(Snippets.Select(s => s.Key));

    [Theory]
    [MemberData(nameof(Blocks))]
    public void DocsBlock_Compiles(string key)
    {
        var snippet = Snippets.Single(s => s.Key == key);
        if (Fragments.IsFragment(snippet))
            return;
        var compiler = snippet.Code.Contains("builder.Services", StringComparison.Ordinal) ? HostCompiler : Compiler;
        compiler.ShouldCompile(snippet);
    }

    [Fact]
    public void EveryCheckedBlock_IsFoundAndFragmentsNameRealHeadings()
    {
        Assert.True(Snippets.Count(s => s.File == "README.md") >= 4, "expected the README's C# blocks");
        Fragments.ShouldMatch([.. Snippets], atLeast: 56);
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
