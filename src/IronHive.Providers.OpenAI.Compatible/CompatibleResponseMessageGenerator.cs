namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Generates response messages that are compatible with the specified configuration.
/// </summary>
internal class CompatibleResponseMessageGenerator : OpenAIResponseMessageGenerator
{
    protected readonly CompatibleConfig _config;

    public CompatibleResponseMessageGenerator(CompatibleConfig config)
        : base(config.ToOpenAI())
    {
        _config = config;
    }
}
