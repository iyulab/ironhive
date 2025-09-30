using IronHive.Providers.GoogleAI.GenerateContent;
using IronHive.Providers.GoogleAI.GenerateContent.Models;
using IronHive.Providers.GoogleAI.Share.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace IronHive.Providers.GoogleAI;

public static class TestRunner
{
    public static async Task RunAsync(string key)
    {
        var client = new GoogleAIGenerateContentClient(key);
        var res = client.GenerateStreamContentAsync(
            "gemini-2.5-flash-lite",
            request: new GenerateContentRequest
            {
                Contents =
                [
                    new Content
                    {
                        Role = "user",
                        Parts = 
                        [
                            new ContentPart { Text = "how is the weather in Seoul, South Korea?" }

                        ]
                    }
                ],
                GenerationConfig = new GenerationConfig
                {
                    CandidateCount = 1,
                    MaxOutputTokens = 1024,
                    Temperature = 0.2f,
                    StopSequences = ["###"],
                    TopK = 40,
                    TopP = 0.95f,
                    ThinkingConfig = new ThinkingConfig
                    {
                        IncludeThoughts = true,
                        ThinkingBudget = 512,
                    },
                },
                SafetySettings = 
                [
                    new SafetySetting
                    {
                        Category = HarmCategory.HARM_CATEGORY_HARASSMENT,
                        Threshold = HarmThreshold.BLOCK_MEDIUM_AND_ABOVE
                    },
                ],
                SystemInstruction = new Content
                {
                    Parts = 
                    [
                        new ContentPart { Text = "You are a helpful assistant that helps people find information." }
                    ]
                },
                Tools = 
                [
                    new Tool
                    {
                        Functions = 
                        [
                            new FunctionTool
                            {
                                Name = "get_current_weather",
                                Description = "Get the current weather in a given location",
                                Parameters = new JsonObject
                                {
                                    ["type"] = "object",
                                    ["properties"] = new JsonObject
                                    {
                                        ["location"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            ["description"] = "The city and state, e.g. San Francisco, CA",
                                        },
                                        ["unit"] = new JsonObject
                                        {
                                            ["type"] = "string",
                                            ["enum"] = new JsonArray("celsius", "fahrenheit"),
                                            ["description"] = "The unit of temperature",
                                        },
                                    },
                                    ["required"] = new JsonArray("location"),
                                },
                            }
                        ],
                    }
                ],
                ToolConfig = new ToolConfig
                {
                    FunctionConfig = new ToolConfig.Config
                    {
                        Mode = ToolConfig.FunctionMode.AUTO,
                    },
                },
            });

        await foreach (var item in res)
        {

        }
        //var json = JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true });
        //Console.WriteLine(json);

    }
}
