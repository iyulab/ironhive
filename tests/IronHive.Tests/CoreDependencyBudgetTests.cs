using System.Xml.Linq;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// IronHive.Core stays a thin implementation layer: heavy third-party dependencies (document parsers,
/// template engines, stores) belong in satellite packages, or nowhere when no consumer needs them.
/// This test makes that a rule instead of a hope -- the packages `IronHive.Core.csproj` may reference
/// are the named list below, so adding one (or swapping one for another) requires editing this list in
/// the same commit that explains why. Removing a package never fails.
/// </summary>
public class CoreDependencyBudgetTests
{
    // The allowed third-party PackageReference set of src/IronHive.Core/IronHive.Core.csproj. A count alone would let a
    // heavy package replace a light one unnoticed, so the names are the budget.
    private static readonly string[] AllowedPackages =
    [
        "Microsoft.Extensions.DependencyInjection",
        "Microsoft.Extensions.Resilience",
        "Tomlyn",
        "YamlDotNet",
    ];

    private static DirectoryInfo RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            dir = dir.Parent;

        Assert.NotNull(dir); // a guard that cannot find the project is not a guard
        return dir!;
    }

    private static List<string> CorePackageReferences()
    {
        var csproj = Path.Combine(RepositoryRoot().FullName, "src", "IronHive.Core", "IronHive.Core.csproj");
        Assert.True(File.Exists(csproj), $"expected to find {csproj}");

        var doc = XDocument.Load(csproj);
        return doc.Descendants("PackageReference")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .Select(v => v!)
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToList();
    }

    [Fact]
    public void Core_ReferencesOnlyAllowedPackages()
    {
        var added = CorePackageReferences()
            .Except(AllowedPackages, StringComparer.Ordinal)
            .ToList();

        Assert.True(added.Count == 0,
            "IronHive.Core references package(s) outside the allowed list above:\n  "
            + string.Join("\n  ", added)
            + "\nMove the dependency to a satellite package (or drop the feature if no consumer needs it); "
            + "add it to AllowedPackages in the same commit only when it truly belongs in the core.");
    }

    [Fact]
    public void TheCheckActuallyFoundPackages()
    {
        // Both failure modes of the check above are silent -- a wrong path or an empty ItemGroup
        // parse always satisfies "<= Baseline". Assert the precondition instead of assuming it.
        Assert.True(CorePackageReferences().Count > 0,
            "expected IronHive.Core.csproj to declare at least one PackageReference");
    }
}
