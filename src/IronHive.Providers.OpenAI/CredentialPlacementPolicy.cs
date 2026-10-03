using System.ClientModel.Primitives;
using IronHive.Abstractions.Http;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Writes the API key where <see cref="OpenAIConfig.ApiKeyPlacement"/> says, in place of the SDK's
/// <c>Authorization: Bearer</c>. Registered at <see cref="PipelinePosition.BeforeTransport"/>, after the SDK's own
/// credential policy, so the SDK's header is removed before the request leaves and the key is sent once, in the
/// configured form. The key is read per request (<see cref="OpenAIConfig.ApiKeyResolver"/>, else
/// <see cref="OpenAIConfig.ApiKey"/>); with neither, no credential header is sent.
/// </summary>
internal sealed class CredentialPlacementPolicy(CredentialPlacement placement, Func<string?>? resolve, string? fallback) : PipelinePolicy
{
    public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        Apply(message);
        ProcessNext(message, pipeline, currentIndex);
    }

    public override ValueTask ProcessAsync(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
    {
        Apply(message);
        return ProcessNextAsync(message, pipeline, currentIndex);
    }

    private void Apply(PipelineMessage message)
    {
        var resolved = resolve?.Invoke();
        var key = string.IsNullOrWhiteSpace(resolved) ? fallback : resolved;

        var headers = message.Request.Headers;
        headers.Remove(CredentialPlacement.AuthorizationHeader);
        headers.Remove(placement.Header);
        if (!string.IsNullOrWhiteSpace(key))
            headers.Set(placement.Header, placement.FormatValue(key));
    }
}
