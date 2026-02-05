namespace IronHive.Providers.OpenAI.Compatible;

/// <summary>
/// Generates response messages that are compatible with the specified configuration.
/// </summary>
internal class CompatibleResponseMessageGenerator : OpenAIResponseMessageGenerator
{
    public CompatibleResponseMessageGenerator(CompatibleConfig config) 
        : base(config.ToOpenAI())
    { }
}
