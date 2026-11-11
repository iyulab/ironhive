using System.Security.Cryptography;
using System.Text;

namespace IronHive.Abstractions.Tools;

/// <summary>
/// Rules for the name a model is offered for a tool.
/// </summary>
/// <remarks>
/// Function-calling providers (OpenAI and the OpenAI-compatible endpoints, Anthropic) accept <c>[a-zA-Z0-9_-]{1,64}</c> as a
/// function name and answer HTTP 400 to the whole request otherwise. A tool whose name an outside party writes - an MCP
/// server's tool, an OpenAPI <c>operationId</c> - can hold dots, slashes or spaces, or run past 64 characters.
/// </remarks>
public static class ToolNames
{
    private const int MaxLength = 64;
    private const int HashLength = 10;

    /// <summary>
    /// The form of <paramref name="name"/> a provider accepts as a function name: the name itself when it already matches
    /// <c>[a-zA-Z0-9_-]{1,64}</c>, otherwise its safe characters (each other character becomes <c>_</c>) followed by <c>_</c> and
    /// ten hex characters of the SHA-256 of the whole name, cut to 64 characters. A pure function of the name: the same input
    /// always gives the same result, and two different names share one only by a 40-bit hash collision.
    /// </summary>
    /// <remarks>
    /// The same rule as the agent layer's <c>ToolNameAliasChatClient.AliasFor</c>, so a name made safe here is not changed
    /// again further up.
    /// </remarks>
    /// <param name="name">The tool's name as its owner wrote it.</param>
    public static string ProviderSafe(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (IsProviderSafe(name))
        {
            return name;
        }

        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(name)))[..HashLength];
        var readable = new StringBuilder(MaxLength);
        foreach (var c in name)
        {
            if (readable.Length == MaxLength - HashLength - 1)
            {
                break;
            }

            readable.Append(IsSafeChar(c) ? c : '_');
        }

        if (readable.Length == 0)
        {
            readable.Append("tool");
        }

        return $"{readable}_{hash}";
    }

    private static bool IsSafeChar(char c) => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-';

    private static bool IsProviderSafe(string name) => name.Length is >= 1 and <= MaxLength && name.All(IsSafeChar);
}
