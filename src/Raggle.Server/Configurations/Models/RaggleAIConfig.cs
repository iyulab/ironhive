using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.OpenAI.Configurations;

namespace Raggle.Server.Configurations.Models;

public partial class RaggleAIConfig
{
    public OpenAIConfig OpenAI { get; set; } = new OpenAIConfig();

    public AnthropicConfig Anthropic { get; set; } = new AnthropicConfig();

    public OllamaConfig Ollama { get; set; } = new OllamaConfig();
}
