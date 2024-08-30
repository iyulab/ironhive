using Raggle.Engines.Anthropic;

var ci = new AnthropicClient(new AnthropicConfig
{
    ApiKey = ""
});

var response = await ci.PostMessagesAsync(new MessagesRequest
{
    Model = "claude-3-5-sonnet-20240620",
    MaxTokens = 100,
    Messages = [
        new Message {
            Role = MessageRole.user,
            Content = "Hello how are you"
        },
    ],
});
