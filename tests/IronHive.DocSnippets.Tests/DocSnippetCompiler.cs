using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IronHive.DocSnippets.Tests;

/// <summary>
/// Compiles a doc snippet (preamble + fence content) as a top-level-statement program against the
/// real, currently-built assemblies — a semantic (type-checking) compile, not an execution. Catches
/// the class of defect #115/#116 were: a documented call site that no longer matches the shipped
/// public API.
/// </summary>
internal static class DocSnippetCompiler
{
    private static readonly string[] RequiredAssemblyNames =
    [
        "IronHive.Abstractions",
        "IronHive.Core",
        "Microsoft.Extensions.AI.Abstractions",
        "Microsoft.Extensions.AI.OpenAI",
        "OpenAI",
    ];

    public static ImmutableArray<Diagnostic> CompileTopLevelProgram(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create(
            assemblyName: "DocSnippetCheck",
            syntaxTrees: [tree],
            references: BuildReferences(),
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        return compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();
    }

    private static List<MetadataReference> BuildReferences()
    {
        // Force-load anything the snippets need that isn't already touched by this test assembly,
        // so it shows up in AppDomain.CurrentDomain.GetAssemblies() below.
        foreach (var name in RequiredAssemblyNames)
        {
            Assembly.Load(name);
        }

        var locations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // .NET runtime/BCL reference assemblies for this process.
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string trustedPlatformAssemblies)
        {
            foreach (var path in trustedPlatformAssemblies.Split(Path.PathSeparator))
            {
                if (!string.IsNullOrWhiteSpace(path))
                    locations.Add(path);
            }
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
                locations.Add(asm.Location);
        }

        return locations.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToList();
    }
}
