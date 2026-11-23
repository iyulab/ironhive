using Anthropic.Exceptions;
using Anthropic.Models.Models;

namespace IronHive.Providers.Anthropic;

/// <summary>
/// Reads what the Models API reports about a model, member by member. The SDK declares every member of
/// <c>capabilities</c> as required and throws <see cref="AnthropicInvalidDataException"/> when the answer leaves one out or gives
/// it another shape, so a gateway or an older upstream that passes on only some of the object would cost the whole card (or the
/// whole list) if the members were read directly. A member that cannot be read counts as «not reported».
/// </summary>
internal static class ReportedSupport
{
    /// <summary>
    /// The value of one reported <c>supported</c> flag: <see langword="null"/> when the answer does not report it (the member is
    /// absent, or not shaped like the documented object), otherwise what it says.
    /// </summary>
    public static bool? Read(Func<bool?> supported)
    {
        try
        {
            return supported();
        }
        catch (AnthropicInvalidDataException)
        {
            return null;
        }
    }

    /// <summary>Whether the answer reports the capability as supported — a capability it does not report is not supported.</summary>
    public static bool IsSupported(Func<bool?> supported) => Read(supported) == true;

    /// <summary>
    /// The model's <c>capabilities</c> object, or <see langword="null"/> when the answer carries none or carries something that is
    /// not that object (a gateway that echoes only the id) — the model's limits stay readable either way.
    /// </summary>
    public static ModelCapabilities? CapabilitiesOf(ModelInfo model)
    {
        try
        {
            return model.Capabilities;
        }
        catch (AnthropicInvalidDataException)
        {
            return null;
        }
    }
}
