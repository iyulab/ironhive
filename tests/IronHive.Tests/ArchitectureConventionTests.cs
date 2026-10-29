using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace IronHive.Tests;

// Enforces docs/LAYERING.md (layer rules). Two invariants:
//   1. IronHive.Abstractions references no implementation assembly (acyclic, downward-only deps).
//   2. Implementation assemblies (Core/Providers/Plugins) do not declare types in the
//      IronHive.Abstractions namespace tree, so the abstraction namespace stays free of
//      provider/implementation surface and layer boundaries remain legible.
// Carve-out: DI registration entrypoints (HiveServiceBuilderExtensions) may declare
// IronHive.Abstractions for discoverability — same rationale as ServiceCollectionExtensions
// staying in Microsoft.Extensions.DependencyInjection (commit 783f114, LAYERING.md §5).
public class ArchitectureConventionTests
{
    // DI-discoverability carve-out: HiveServiceBuilder registration entrypoints (e.g.
    // HiveServiceBuilderExtensions, CompatibleHiveServiceBuilderExtensions) may declare the
    // IronHive.Abstractions namespace so `using IronHive.Abstractions;` surfaces the AddX methods.
    private static bool IsDiCarveOut(Type type)
        => type.Name.EndsWith("HiveServiceBuilderExtensions", StringComparison.Ordinal);

    // Scan every implementation assembly present in the test output, not just the ones the
    // compiler retained as metadata references — unused ProjectReferences get pruned from
    // GetReferencedAssemblies(), which would silently skip provider assemblies the tests
    // don't directly touch (e.g. Anthropic/GoogleAI). The DLLs are still copied to bin.
    private static IEnumerable<Assembly> ImplementationAssemblies()
    {
        var baseDir = AppContext.BaseDirectory;
        foreach (var dll in Directory.EnumerateFiles(baseDir, "IronHive.*.dll").OrderBy(p => p))
        {
            var name = Path.GetFileNameWithoutExtension(dll);
            if (name is "IronHive.Abstractions" or "IronHive.Tests") continue;
            yield return Assembly.LoadFrom(dll);
        }
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        // GetTypes can throw ReflectionTypeLoadException when an optional dependency is absent
        // (e.g. cross-platform). Fall back to the types that did load. (See commit 3403b39.)
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    public static IEnumerable<object[]> ImplementationAssemblyNames()
        => ImplementationAssemblies().Select(a => new object[] { a.GetName().Name! });

    [Fact]
    public void Abstractions_DoesNotReferenceImplementationAssemblies()
    {
        var abstractions = typeof(IronHive.Abstractions.IHiveService).Assembly;
        var offenders = abstractions.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal) && n != "IronHive.Abstractions")
            .ToList();

        Assert.True(offenders.Count == 0,
            "IronHive.Abstractions must not reference implementation assemblies (Core/Providers/Plugins):\n  "
            + string.Join("\n  ", offenders));
    }

    // IronHive.Extensions.AI is the lean bridge a host takes to use a provider as an M.E.AI IChatClient without
    // IronHive.Core: referencing any other IronHive assembly would bring Core's stack back.
    [Fact]
    public void ExtensionsAI_ReferencesOnlyAbstractions()
    {
        var bridge = typeof(IronHive.Extensions.AI.ChatClientAdapter).Assembly;
        var offenders = bridge.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal) && n != "IronHive.Abstractions")
            .ToList();

        Assert.True(offenders.Count == 0,
            "IronHive.Extensions.AI must reference no IronHive assembly but IronHive.Abstractions:\n  "
            + string.Join("\n  ", offenders));
    }

    // IronHive.Providers.Declarations turns a declared provider into a client, so it sits above the four wire adapters. It
    // may take the adapters, the M.E.AI bridge and Abstractions - never Core or a plugin - and nothing below it may take it.
    [Fact]
    public void Declarations_ReferencesOnlyTheAdaptersTheBridgeAndAbstractions()
    {
        string[] allowed =
        [
            "IronHive.Abstractions", "IronHive.Extensions.AI", "IronHive.Providers.OpenAI", "IronHive.Providers.OpenAI.Compatible",
            "IronHive.Providers.Anthropic", "IronHive.Providers.GoogleAI",
        ];
        var declarations = typeof(IronHive.Providers.Declarations.ProviderClientFactory).Assembly;
        var offenders = declarations.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal) && !allowed.Contains(n))
            .ToList();

        Assert.True(offenders.Count == 0,
            "IronHive.Providers.Declarations may reference only Abstractions, Extensions.AI and the four provider adapters:\n  "
            + string.Join("\n  ", offenders));
    }

    [Fact]
    public void NothingReferencesDeclarations_ItIsTheTopOfTheProviderStack()
    {
        var offenders = ImplementationAssemblies()
            .Where(a => a.GetName().Name != "IronHive.Providers.Declarations")
            .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == "IronHive.Providers.Declarations"))
            .Select(a => a.GetName().Name!)
            .ToList();

        Assert.True(offenders.Count == 0,
            "No IronHive assembly may reference IronHive.Providers.Declarations (it is the layer above the providers):\n  "
            + string.Join("\n  ", offenders));
    }

    // IronHive.Providers.Endpoints serves a chat client as the HTTP endpoints a provider-shaped client calls. It speaks the wires
    // with its own types, so it needs no IronHive assembly at all - not Core, not a provider adapter - and nothing may take it:
    // it is a leaf a host mounts, the inbound mirror of the provider stack.
    [Fact]
    public void Endpoints_ReferencesNoIronHiveAssembly_AndNothingReferencesIt()
    {
        var endpoints = typeof(IronHive.Providers.Endpoints.ProviderEndpointOptions).Assembly;
        var references = endpoints.GetReferencedAssemblies()
            .Select(a => a.Name!)
            .Where(n => n.StartsWith("IronHive.", StringComparison.Ordinal))
            .ToList();
        var referencedBy = ImplementationAssemblies()
            .Where(a => a.GetName().Name != "IronHive.Providers.Endpoints")
            .Where(a => a.GetReferencedAssemblies().Any(r => r.Name == "IronHive.Providers.Endpoints"))
            .Select(a => a.GetName().Name!)
            .ToList();

        Assert.True(references.Count == 0,
            "IronHive.Providers.Endpoints must reference no other IronHive assembly:\n  " + string.Join("\n  ", references));
        Assert.True(referencedBy.Count == 0,
            "No IronHive assembly may reference IronHive.Providers.Endpoints (a host mounts it):\n  " + string.Join("\n  ", referencedBy));
    }

    [Theory]
    [MemberData(nameof(ImplementationAssemblyNames))]
    public void ImplementationAssembly_DoesNotDeclareAbstractionsNamespace(string assemblyName)
    {
        var assembly = ImplementationAssemblies().First(a => a.GetName().Name == assemblyName);

        var offenders = SafeGetTypes(assembly)
            .Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null)
            .Where(t => t.Namespace is not null
                && (t.Namespace == "IronHive.Abstractions"
                    || t.Namespace.StartsWith("IronHive.Abstractions.", StringComparison.Ordinal)))
            .Where(t => !IsDiCarveOut(t))
            .Select(t => $"  {t.FullName}")
            .ToList();

        Assert.True(offenders.Count == 0,
            $"Types in '{assemblyName}' must declare their own assembly namespace, not IronHive.Abstractions* "
            + "(converter/registry extensions). DI HiveServiceBuilderExtensions is the only carve-out:\n"
            + string.Join("\n", offenders));
    }
}
