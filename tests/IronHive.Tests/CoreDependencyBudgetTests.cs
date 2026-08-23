using System.Xml.Linq;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// IronHive.Core is meant to stay a thin implementation layer, but it currently carries a heavy
/// third-party footprint (a document format library, a PDF parser, a templating engine, among
/// others) that a slim core ideally would not need directly. Shrinking that footprint is a
/// separate, deliberate effort; this test does not attempt it. What it does do is stop further
/// growth from landing silently -- adding a `PackageReference` to `IronHive.Core.csproj` now
/// requires a conscious bump of the baseline below, in the same commit that explains why.
/// </summary>
public class CoreDependencyBudgetTests
{
    // Baseline: 13 PackageReference in src/IronHive.Core/IronHive.Core.csproj (DocumentFormat.
    // OpenXml, HtmlAgilityPack, MessagePack, Microsoft.Data.Sqlite,
    // Microsoft.Extensions.AI.Abstractions, Microsoft.Extensions.DependencyInjection,
    // Microsoft.Extensions.Resilience, OpenTelemetry.Api, PdfPig, Scriban,
    // SQLitePCLRaw.bundle_e_sqlite3, Tomlyn, YamlDotNet). Lowering this number is the point of
    // the debt this test tracks; raising it silently defeats that point.
    private const int Baseline = 13;

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
    public void Core_DoesNotExceedTheDocumentedDependencyBaseline()
    {
        var packages = CorePackageReferences();

        Assert.True(packages.Count <= Baseline,
            $"IronHive.Core references {packages.Count} package(s), exceeding the baseline of "
            + $"{Baseline} recorded above. Update Baseline in the same commit if this growth is "
            + "intentional, or move the dependency to a satellite package instead.\n  "
            + string.Join("\n  ", packages));
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
