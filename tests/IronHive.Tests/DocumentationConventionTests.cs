using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// Enforces that the guides describe the API that exists. Every <c>new T { P = … }</c> in a documented
/// example, and every property listed in a documented type declaration, must name a real member of a
/// real type.
/// <para>
/// The repository already requires documentation to be updated in the same commit as the public
/// surface (CLAUDE.md). That rule was held by convention alone and was not kept: a single sweep found
/// twenty-five documented properties that no longer existed, spread over five guides, every one of them
/// where a refactor had passed through — a context split into a source/target pair, a provider argument
/// moved from a request object to a method parameter, storage properties renamed, a collection response
/// reduced to a single value. Examples and interface declarations had drifted *together*, so they read
/// as consistent and corrected nothing. Prose cannot notice that; this can.
/// </para>
/// </summary>
public class DocumentationConventionTests
{
    // A string literal cannot contain a property assignment, but it can contain something that looks
    // exactly like one -- an Azure connection string carries "DefaultEndpointsProtocol=https;...".
    private static readonly Regex StringLiteral = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);

    // Object initialisers. Nested braces are not matched, so an example whose initialiser contains
    // another initialiser is skipped rather than mis-parsed.
    private static readonly Regex ObjectInitialiser =
        new(@"new\s+(?<type>[A-Z][A-Za-z0-9_]*)\s*\{(?<body>[^{}]*)\}", RegexOptions.Compiled);

    // A documented type declaration, e.g. "public class OpenAIConfig { ... }".
    private static readonly Regex TypeDeclaration =
        new(@"public\s+(?:sealed\s+)?(?:class|record)\s+(?<type>[A-Z][A-Za-z0-9_]*)[^\n]*\n\{(?<body>(?:[^\n]|\n(?!\}))*)\n\}",
            RegexOptions.Compiled);

    // "P =" but not "ex =>" (a lambda parameter) and not "a == b".
    private static readonly Regex Assignment =
        new(@"(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=(?![=>])", RegexOptions.Compiled);

    // "public TimeSpan? Timeout { get; set; }" inside a documented declaration.
    private static readonly Regex DeclaredMember =
        new(@"public\s+(?:required\s+|virtual\s+|override\s+|static\s+)*[^\s(){};]+(?:<[^>]*>)?[\s?\[\]]+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get",
            RegexOptions.Compiled);

    private static DirectoryInfo RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            dir = dir.Parent;

        Assert.NotNull(dir); // a guard that cannot find the documents is not a guard
        return dir!;
    }

    private static IEnumerable<string> DocumentPaths()
    {
        var root = RepositoryRoot();
        var readme = Path.Combine(root.FullName, "README.md");
        if (File.Exists(readme)) yield return readme;

        var docs = Path.Combine(root.FullName, "docs");
        if (!Directory.Exists(docs)) yield break;

        foreach (var path in Directory.EnumerateFiles(docs, "*.md").OrderBy(p => p, StringComparer.Ordinal))
            yield return path;
    }

    private static Dictionary<string, Type> PublicTypesByName()
    {
        var byName = new Dictionary<string, Type>(StringComparer.Ordinal);

        foreach (var dll in Directory.EnumerateFiles(AppContext.BaseDirectory, "IronHive.*.dll")
                                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            if (Path.GetFileNameWithoutExtension(dll) is "IronHive.Tests") continue;

            Type?[] types;
            try
            {
                types = Assembly.LoadFrom(dll).GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types; // an optional dependency may be absent; use what loaded
            }

            foreach (var type in types)
            {
                if (type is null || !type.IsPublic || type.IsGenericTypeDefinition) continue;
                // First assembly wins. A duplicated simple name would make the check ambiguous, and
                // treating it as unknown (skipped) is safer than asserting against the wrong type.
                byName.TryAdd(type.Name, type);
            }
        }

        return byName;
    }

    private static HashSet<string> WritableMemberNames(Type type)
    {
        // GetProperties/GetFields already include public inherited members.
        var names = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Select(p => p.Name)
                        .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                    .Select(f => f.Name));

        return new HashSet<string>(names, StringComparer.Ordinal);
    }

    private static string Relative(string path)
        => Path.GetRelativePath(RepositoryRoot().FullName, path).Replace('\\', '/');

    private static string Describe(Type type)
    {
        var names = WritableMemberNames(type).OrderBy(n => n, StringComparer.Ordinal);
        return string.Join(", ", names);
    }

    [Fact]
    public void EveryDocumentedObjectInitialiser_SetsOnlyMembersThatExist()
    {
        var types = PublicTypesByName();
        var failures = new List<string>();

        foreach (var path in DocumentPaths())
        {
            var text = StringLiteral.Replace(File.ReadAllText(path), "\"\"");

            foreach (var initialiser in ObjectInitialiser.Matches(text).Cast<Match>())
            {
                if (!types.TryGetValue(initialiser.Groups["type"].Value, out var type)) continue;

                var members = WritableMemberNames(type);
                if (members.Count == 0) continue;

                foreach (var assignment in Assignment.Matches(initialiser.Groups["body"].Value).Cast<Match>())
                {
                    var name = assignment.Groups["name"].Value;
                    if (members.Contains(name)) continue;

                    failures.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}: new {1} {{ {2} = … }} — no such member. Actual: {3}",
                        Relative(path), type.Name, name, Describe(type)));
                }
            }
        }

        Assert.True(failures.Count == 0, Message(failures));
    }

    [Fact]
    public void EveryDocumentedTypeDeclaration_ListsOnlyMembersThatExist()
    {
        var types = PublicTypesByName();
        var failures = new List<string>();

        foreach (var path in DocumentPaths())
        {
            var text = File.ReadAllText(path);

            foreach (var declaration in TypeDeclaration.Matches(text).Cast<Match>())
            {
                if (!types.TryGetValue(declaration.Groups["type"].Value, out var type)) continue;

                var members = WritableMemberNames(type);
                if (members.Count == 0) continue;

                foreach (var member in DeclaredMember.Matches(declaration.Groups["body"].Value).Cast<Match>())
                {
                    var name = member.Groups["name"].Value;
                    if (members.Contains(name)) continue;

                    failures.Add(string.Format(CultureInfo.InvariantCulture,
                        "{0}: documented {1}.{2} — no such member. Actual: {3}",
                        Relative(path), type.Name, name, Describe(type)));
                }
            }
        }

        Assert.True(failures.Count == 0, Message(failures));
    }

    /// <summary>
    /// The checks above are only meaningful if they see the documents and can resolve the types. Both
    /// failure modes are silent — no documents found, or no type names matched, produces a pass — so the
    /// preconditions are asserted rather than assumed.
    /// </summary>
    [Fact]
    public void TheCheckActuallyHasDocumentsAndTypesToInspect()
    {
        var paths = DocumentPaths().ToList();
        Assert.True(paths.Count >= 5, $"expected the guides to be discoverable, found {paths.Count}");

        var types = PublicTypesByName();
        Assert.True(types.Count > 100, $"expected the assemblies to be loaded, found {types.Count} public types");

        var resolvedInitialisers = 0;
        foreach (var path in paths)
        {
            var text = StringLiteral.Replace(File.ReadAllText(path), "\"\"");
            resolvedInitialisers += ObjectInitialiser.Matches(text)
                .Cast<Match>()
                .Count(m => types.ContainsKey(m.Groups["type"].Value));
        }

        Assert.True(resolvedInitialisers >= 20,
            $"expected the documents to contain resolvable initialisers, matched {resolvedInitialisers}");
    }

    private static string Message(List<string> failures)
    {
        if (failures.Count == 0) return string.Empty;

        var builder = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture, $"{failures.Count} documented member(s) do not exist.")
            .AppendLine("Update the document in the same commit as the surface change (CLAUDE.md).")
            .AppendLine();

        foreach (var failure in failures)
            builder.AppendLine(CultureInfo.InvariantCulture, $"  {failure}");

        return builder.ToString();
    }
}
