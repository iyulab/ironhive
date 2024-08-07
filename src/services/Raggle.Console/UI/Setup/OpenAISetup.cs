using Raggle.Core.Options.Platforms;
using Spectre.Console;

namespace Raggle.Console.UI.Setup;

public class OpenAISetup
{
    public OpenAIOption Setup()
    {
        AnsiConsole.MarkupLine(
"""
Welcome to the OpenAI Setup Wizard!
        
In this step, you will configure the necessary settings to connect to the OpenAI platform. 
You will need to provide your OpenAI API key and choose the models you wish to use for text generation and embedding.

Let's get started!

1. **API Key**: This is your unique key provided by OpenAI to access their services. 
    Make sure you have it ready, as you'll need to input it in the next step.

2. **Text Model**: OpenAI offers a variety of models for generating text. 
    The models range from the latest GPT-4 to the highly efficient GPT-3.5 Turbo.
    Each model has different capabilities and performance characteristics.

3. **Embedding Model**: These models are used for generating text embeddings, which are 
    useful for tasks like semantic search and text similarity. You can choose from several 
    options depending on your requirements.

Please follow the prompts to complete the setup.
""");

        var apiKey = AnsiConsole.Prompt(
            new TextPrompt<string>("Enter your OpenAI API key:")
                .PromptStyle("grey50")
                .Secret());

        var textModel = AnsiConsole.Prompt(
            new SelectionPrompt<OpenAITextModel>()
                .Title("Select the text model")
                .PageSize(10)
                .AddChoices(OpenAITextModel.GPT_4o, OpenAITextModel.GPT_4_Turbo, OpenAITextModel.GPT_4, OpenAITextModel.GPT_3_5_Turbo));
        
        var embeddingModel = AnsiConsole.Prompt(
            new SelectionPrompt<OpenAIEmbeddingModel>()
                .Title("Select the embedding model")
                .PageSize(10)
                .AddChoices(OpenAIEmbeddingModel.Text_Embedding_3_Large, OpenAIEmbeddingModel.Text_Embedding_3_Small, OpenAIEmbeddingModel.Text_Embedding_Ada_002));
        
        return new OpenAIOption
        {
            ApiKey = apiKey,
            TextModel = textModel,
            EmbeddingModel = embeddingModel
        };
    }
}