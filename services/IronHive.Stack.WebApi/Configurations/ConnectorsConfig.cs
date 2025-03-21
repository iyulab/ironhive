using IronHive.Connectors.Anthropic;
using IronHive.Connectors.Ollama;
using IronHive.Connectors.OpenAI;

namespace IronHive.Stack.WebApi.Configurations;

public partial class ConnectorsConfig
{
    public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();

    public AnthropicConfig Anthropic { get; set; } = new AnthropicConfig();

    public OllamaConfig Ollama { get; set; } = new OllamaConfig();
}
