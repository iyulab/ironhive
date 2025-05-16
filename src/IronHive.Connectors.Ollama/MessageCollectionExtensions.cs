using IronHive.Connectors.Ollama.ChatCompletion;
using System.Text.Json;
using OllamaMessage = IronHive.Connectors.Ollama.ChatCompletion.Message;
using IronHive.Abstractions.Messages;

namespace IronHive.Connectors.Ollama;

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
            if (message is UserMessage user)
            {
                var um = new OllamaMessage 
                { 
                    Role = MessageRole.User 
                };
                foreach (var item in user.Content)
                {
                    if (item is UserTextContent text)
                    {
                        um.Content ??= string.Empty;
                        um.Content += text.Value;
                    }
                    else if (item is UserFileContent file)
                    {
                        //um.Images ??= [];
                        //um.Images.Add(image.Data!);
                        throw new NotImplementedException("not supported file yet");
                    }
                }
                _messages.Add(um);
            }
            else if (message is AssistantMessage assistant)
            {
                foreach (var item in assistant.Content)
                {
                    var am = new OllamaMessage 
                    { 
                        Role = MessageRole.Assistant 
                    };
                    var tm = new OllamaMessage 
                    {
                        Role = MessageRole.Tool 
                    };

                    if (item is AssistantTextContent text)
                    {
                        am.Content ??= string.Empty;
                        am.Content += text.Value;
                    }
                    else if (item is AssistantToolContent tool)
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
