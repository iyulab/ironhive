using Raggle.Abstractions.ChatCompletion.Messages;
using Raggle.Connectors.Ollama.ChatCompletion;
using System.Text.Json;
using OllamaMessage = Raggle.Connectors.Ollama.ChatCompletion.Message;

namespace Raggle.Connectors.Ollama.Extensions;

internal static class MessageCollectionExtensions
{
    internal static IEnumerable<OllamaMessage> ToOllama(this MessageCollection messages, string? system = null)
    {
        var _messages = new List<OllamaMessage>();

        if (!string.IsNullOrWhiteSpace(system))
        {
            _messages.Add(new OllamaMessage
            {
                Role = MessageRole.System,
                Content = system
            });
        }

        foreach (var message in messages)
        {
            if (message.Content == null || message.Content.Count == 0)
                continue;

            if (message.GetType() == typeof(UserMessage))
            {
                var um = new OllamaMessage { Role = MessageRole.User };
                foreach (var item in message.Content)
                {
                    if (item is TextContent text)
                    {
                        um.Content ??= string.Empty;
                        um.Content += text.Text;
                    }
                    else if (item is ImageContent image)
                    {
                        um.Images ??= [];
                        um.Images.Add(image.Data!);
                    }
                }
                _messages.Add(um);
            }
            else if (message.GetType() == typeof(AssistantMessage))
            {
                foreach (var item in message.Content)
                {
                    var am = new OllamaMessage { Role = MessageRole.Assistant };
                    var tm = new OllamaMessage { Role = MessageRole.Tool };

                    if (item is TextContent text)
                    {
                        am.Content ??= string.Empty;
                        am.Content += text.Text;
                    }
                    else if (item is ToolContent tool)
                    {
                        am.ToolCalls ??= new List<ToolCall>();
                        am.ToolCalls.Add(new ToolCall
                        {
                            Function = new FunctionCall
                            {
                                Name = tool.Name,
                                // Argument = tool.Arguments // This is not implemented in the Ollama API
                            }
                        });
                        tm.Content ??= string.Empty;
                        tm.Content += JsonSerializer.Serialize(tool.Result);
                    }

                    _messages.Add(am);
                    if (tm.Content != null)
                        _messages.Add(tm);
                }
            }
        }

        return _messages;
    }
}
