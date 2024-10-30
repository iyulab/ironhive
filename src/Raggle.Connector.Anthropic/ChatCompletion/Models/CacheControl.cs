namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class CacheControl
{
    /// <summary>
    /// "ephemeral" only
    /// </summary>
    internal string Type { get; } = "ephemeral";
}
