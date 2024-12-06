using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.Ollama.Configurations;
using Raggle.Driver.OpenAI.Configurations;
using Raggle.Server.ToolKits;
using Raggle.Server.Utils;

namespace Raggle.Server.Configurations;

public class RaggleKeyedServiceConfig
{
    public AIProviderConfig AIProviders { get; set; } = new AIProviderConfig();

    //public ToolKitConfig ToolKits { get; set; } = new ToolKitConfig();

    //public HandlerKeyConfig StepNames { get; set; } = new HandlerKeyConfig();

    public class AIProviderConfig
    {
        public ServiceKeyValue<OpenAIConfig> OpenAI { get; set; } = new ServiceKeyValue<OpenAIConfig>();
        public ServiceKeyValue<AnthropicConfig> Anthropic { get; set; } = new ServiceKeyValue<AnthropicConfig>();
        public ServiceKeyValue<OllamaConfig> Ollama { get; set; } = new ServiceKeyValue<OllamaConfig>();
    }

    //public class HandlerKeyConfig
    //{

    //}
}
