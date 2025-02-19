using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.OpenAI.Configurations;

namespace Raggle.Server.Configurations;

public partial class AIServiceConfig
{
    public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();

    public AnthropicConfig Anthropic { get; set; } = new AnthropicConfig();

    public OllamaConfig Ollama { get; set; } = new OllamaConfig();
}
