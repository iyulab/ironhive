using System.ClientModel.Primitives;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// Sets the configured extra headers on every outgoing request. Registered at
/// <see cref="PipelinePosition.BeforeTransport"/>, after the SDK has assembled the request and applied
/// its own defaults, so a configured value replaces an SDK default of the same name; the credential
/// header is never among them (refused at resolution).
/// </summary>
internal sealed class ExtraRequestHeadersPolicy : PipelinePolicy
{
    private readonly IReadOnlyDictionary<string, string> _headers;

    public ExtraRequestHeadersPolicy(IReadOnlyDictionary<string, string> headers)
    {
        _headers = headers;
    }

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
        foreach (var (name, value) in _headers)
            message.Request.Headers.Set(name, value);
    }
}
