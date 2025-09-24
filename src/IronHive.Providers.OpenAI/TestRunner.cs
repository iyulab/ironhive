using IronHive.Providers.OpenAI.Responses;
using System.Text.Json;

namespace IronHive.Providers.OpenAI;

public static class TestRunner
{
    private static ResponsesRequest _request = new()
    {
        Model = "gpt-5-nano",
        Input =
        [
            new ResponsesMessageItem
            {
                Role = ResponsesMessageRole.User,
                Content =
                [
                    new ResponsesInputTextContent
                    {
                        Text = "Hello, world!"
                    }
                ]
            }
        ]
    };

    public static async Task RunAsync(string apiKey)
    {
        var client = new OpenAIResponsesClient(apiKey);

        var result = await client.PostResponsesAsync(_request);

        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }

    public static async Task RunStreamAsync(string apiKey)
    {
        var client = new OpenAIResponsesClient(apiKey);

        await foreach (var res in client.PostStreamingResponsesAsync(_request))
        {
            var json = JsonSerializer.Serialize(res, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }
}
