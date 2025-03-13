using IronHive.Connectors.Anthropic.Configurations;
using IronHive.Connectors.Ollama.Configurations;
using IronHive.Connectors.OpenAI.Configurations;

namespace IronHive.Stack.WebApi.Configurations;

public partial class ConnectorsConfig
{
    public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();

    public AnthropicConfig Anthropic { get; set; } = new AnthropicConfig();

    public OllamaConfig Ollama { get; set; } = new OllamaConfig();
}
