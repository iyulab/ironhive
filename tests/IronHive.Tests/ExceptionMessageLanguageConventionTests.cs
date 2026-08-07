using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace IronHive.Tests;

/// <summary>
/// Exception messages are operator-facing at runtime and are part of what the packages ship, so they are
/// ASCII like the log messages next to them (CLAUDE.md logging conventions, and the language policy for
/// public artefacts). A non-Korean-speaking operator cannot triage a failure whose message they cannot
/// read, and the same argument that keeps Korean out of log pipelines — Latin-boundary tokenizers index a
/// Korean phrase as one opaque token, and `grep` needs UTF-8-aware matching — applies to a message pasted
/// into an issue or a search.
/// <para>
/// <see cref="LogLanguageConventionTests"/> already covers <c>[LoggerMessage]</c> attributes by
/// reflection. Exception messages are string literals inside method bodies, which reflection cannot
/// reach, so this reads the sources. Sixty-six had accumulated across six assemblies before this check
/// existed — the convention was held by habit alone.
/// </para>
/// <para>
/// XML documentation is deliberately out of scope. It is Korean throughout this repository by
/// established practice, and it is read at development time rather than emitted at runtime.
/// </para>
/// </summary>
public class ExceptionMessageLanguageConventionTests
{
    private static readonly Regex Hangul = new(@"[가-힣ᄀ-ᇿ㄰-㆏]", RegexOptions.Compiled);

    // Any exception construction, whether thrown directly, assigned, or produced by an expression.
    private static readonly Regex ExceptionConstruction =
        new(@"new\s+[A-Za-z_][A-Za-z0-9_.]*Exception\s*\((?<args>[^;]*?)\)\s*[;,)\r\n]", RegexOptions.Compiled);

    private static DirectoryInfo RepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "IronHive.slnx")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        return dir!;
    }

    private static IEnumerable<string> SourceFiles()
    {
        var src = Path.Combine(RepositoryRoot().FullName, "src");
        Assert.True(Directory.Exists(src), "a guard that cannot find the sources is not a guard");

        foreach (var path in Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(src, path);
            if (relative.Contains($"obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)) continue;
            if (relative.Contains($"bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)) continue;
            yield return path;
        }
    }

    // Documentation comments carry Korean by established practice and are not emitted at runtime.
    private static string WithoutDocComments(string text)
        => string.Join('\n', text.Split('\n').Where(line => !line.TrimStart().StartsWith("///", StringComparison.Ordinal)));

    [Fact]
    public void NoExceptionMessage_ContainsHangul()
    {
        var root = RepositoryRoot().FullName;
        var failures = new List<string>();

        foreach (var path in SourceFiles())
        {
            var text = WithoutDocComments(File.ReadAllText(path));

            foreach (var construction in ExceptionConstruction.Matches(text).Cast<Match>())
            {
                var args = construction.Groups["args"].Value;
                if (!Hangul.IsMatch(args)) continue;

                var line = text.Take(construction.Index).Count(c => c == '\n') + 1;
                failures.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0}:{1} — {2}",
                    Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/'),
                    line,
                    args.Trim()));
            }
        }

        Assert.True(failures.Count == 0, Message(failures));
    }

    /// <summary>
    /// A source-scanning check passes silently when it finds nothing to scan or nothing to match, so both
    /// are asserted rather than assumed.
    /// </summary>
    [Fact]
    public void TheCheckActuallyHasSourcesAndExceptionsToInspect()
    {
        var files = SourceFiles().ToList();
        Assert.True(files.Count > 100, $"expected the sources to be discoverable, found {files.Count}");

        var constructions = files.Sum(p =>
            ExceptionConstruction.Count(WithoutDocComments(File.ReadAllText(p))));

        Assert.True(constructions > 100,
            $"expected the pattern to match exception constructions, matched {constructions}");
    }

    private static string Message(List<string> failures)
    {
        if (failures.Count == 0) return string.Empty;

        var builder = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture,
                $"{failures.Count} exception message(s) contain Hangul. Operational strings are ASCII.")
            .AppendLine();

        foreach (var failure in failures)
            builder.AppendLine(CultureInfo.InvariantCulture, $"  {failure}");

        return builder.ToString();
    }
}
