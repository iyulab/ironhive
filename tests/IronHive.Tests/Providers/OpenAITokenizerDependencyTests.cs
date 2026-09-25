using System.Xml.Linq;
using AwesomeAssertions;
using IronHive.Providers.OpenAI;

namespace IronHive.Tests.Providers;

/// <summary>
/// <c>IronHive.Providers.OpenAI</c> ships only the tokenizer encodings it counts with. It referenced the <c>Tiktoken</c>
/// metapackage, which carries every encoding (o200k, p50k, r50k — about 8 MB) while the provider uses cl100k alone, so
/// every consumer shipped assemblies it never loads. A package reference is justified here only by an assembly the
/// provider actually references; a metapackage has none.
/// </summary>
public class OpenAITokenizerDependencyTests
{
    [Fact]
    public void Every_Tiktoken_package_reference_is_an_assembly_the_provider_references()
    {
        var csproj = Path.Combine(RepositoryRoot(), "src", "IronHive.Providers.OpenAI", "IronHive.Providers.OpenAI.csproj");
        var packages = XDocument.Load(csproj).Descendants("PackageReference")
            .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
            .Where(id => id.StartsWith("Tiktoken", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var referenced = typeof(OpenAIEmbeddingGenerator).Assembly.GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        packages.Should().NotBeEmpty("the scan must read the real project file");
        referenced.Should().Contain("Tiktoken.Encodings.cl100k", "positive control: the scan sees the encoding in use");
        packages.Where(id => !referenced.Contains(id)).Should().BeEmpty(
            "a tokenizer package whose assembly the provider never references ships to every consumer unused");
    }

    private static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException($"IronHive.slnx not found above {AppContext.BaseDirectory}.");
    }
}
