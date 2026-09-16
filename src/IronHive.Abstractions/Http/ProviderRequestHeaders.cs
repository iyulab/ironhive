namespace IronHive.Abstractions.Http;

/// <summary>
/// Resolves the extra request headers a provider configuration declares into the one set the
/// provider sends on every request — the same rules for every provider, so a consumer configures a
/// gateway header without knowing which vendor SDK sits behind the config.
/// </summary>
/// <remarks>
/// <para>
/// Two rules, both enforced here rather than left to whichever pipeline position a header happens
/// to be applied at:
/// </para>
/// <list type="bullet">
/// <item><description><b>The credential is not a header.</b> A configured header may not name the
/// provider's credential header (<c>Authorization</c>, <c>x-api-key</c>, <c>x-goog-api-key</c> — each
/// provider passes its own list). Whether such a header would override the credential or be
/// overridden by it depended on the vendor SDK's pipeline order, and a header that loses silently is
/// the same class of defect as a setting that loses silently. The credential belongs to the config's
/// credential slot; a gateway that replaces the bearer token gets its key through that slot.</description></item>
/// <item><description><b>Two sources of the same header must agree.</b> A provider may expose the
/// vendor's own header slot beside the uniform one (Anthropic's <c>ExtraHeaders</c>, Google's
/// <c>HttpOptions.Headers</c>). Their union is sent; the same name with two different values is
/// refused at construction, naming the header, instead of one value winning by position.</description></item>
/// </list>
/// <para>Header names compare case-insensitively, as HTTP does. An empty result is <c>null</c>.</para>
/// </remarks>
public static class ProviderRequestHeaders
{
    /// <summary>
    /// Merges the given header sources under the rules above.
    /// </summary>
    /// <param name="configName">The configuration type the headers came from, for the error text.</param>
    /// <param name="credentialSlot">The config member that carries the credential, for the error text (e.g. <c>ApiKey</c>).</param>
    /// <param name="reservedNames">Header names the provider's credential occupies; a configured header with one of these names is refused.</param>
    /// <param name="sources">The header dictionaries to merge, in declaration order. Null entries are skipped.</param>
    /// <returns>The merged headers, or <c>null</c> when nothing was configured.</returns>
    /// <exception cref="ArgumentException">A header names the credential, or two sources give one header different values.</exception>
    public static IReadOnlyDictionary<string, string>? Resolve(
        string configName,
        string credentialSlot,
        IEnumerable<string> reservedNames,
        params IDictionary<string, string>?[] sources)
    {
        var reserved = new HashSet<string>(reservedNames, StringComparer.OrdinalIgnoreCase);
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in sources)
        {
            if (source is null)
                continue;

            foreach (var (name, value) in source)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException($"{configName}.Headers contains an empty header name.");

                if (reserved.Contains(name))
                {
                    throw new ArgumentException(
                        $"{configName}.Headers must not set '{name}': that header carries the credential, " +
                        $"which is configured through {configName}.{credentialSlot}. A gateway that replaces the " +
                        $"credential gets its key through that slot; other gateway headers go in Headers.");
                }

                if (merged.TryGetValue(name, out var existing))
                {
                    if (!string.Equals(existing, value, StringComparison.Ordinal))
                    {
                        throw new ArgumentException(
                            $"{configName} configures the header '{name}' twice with different values " +
                            $"('{existing}' and '{value}'). Set it in one place.");
                    }
                    continue;
                }

                merged[name] = value;
            }
        }

        return merged.Count == 0 ? null : merged;
    }
}
