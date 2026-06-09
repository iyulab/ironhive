using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace IronHive.Tests;

// Enforces docs/LAYERING.md (layer rules). Two invariants:
//   1. IronHive.Abstractions references no implementation assembly (acyclic, downward-only deps).
//   2. Implementation assemblies (Core/Providers/Storages/Plugins) do not declare types in the
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
            "IronHive.Abstractions must not reference implementation assemblies (Core/Providers/Storages/Plugins):\n  "
            + string.Join("\n  ", offenders));
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
