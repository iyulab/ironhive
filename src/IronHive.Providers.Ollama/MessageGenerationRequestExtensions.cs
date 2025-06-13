using System.Text.Json;
using IronHive.Abstractions.Message;
using IronHive.Abstractions.Message.Content;
using IronHive.Abstractions.Message.Roles;
using IronHive.Providers.Ollama.Chat;
using OllamaMessage = IronHive.Providers.Ollama.Chat.Message;

namespace IronHive.Providers.Ollama;

internal static class MessageGenerationRequestExtensions
{
    internal static ChatRequest ToOllama(this MessageGenerationRequest request)
    {
        var _messages = new List<OllamaMessage>();

        if (!string.IsNullOrWhiteSpace(request.System))
        {
            _messages.Add(new OllamaMessage
            {
                Role = MessageRole.System,
                Content = request.System
            });
        }

        foreach (var message in request.Messages)
        {
            if (message is UserMessage user)
            {
                var um = new OllamaMessage 
                { 
                    Role = MessageRole.User 
                };
                foreach (var item in user.Content)
                {
                    if (item is TextMessageContent text)
                    {
                        um.Content ??= string.Empty;
                        um.Content += text.Value;
                    }
                    else if (item is DocumentMessageContent document)
                    {
                        um.Content ??= string.Empty;
                        um.Content += $"\nDocument Content:\n{document.Data}\n";
                    }
                    else if (item is ImageMessageContent image)
                    {
                        um.Images ??= [];
                        um.Images.Add(image.Base64);
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

                    if (item is TextMessageContent text)
                    {
                        am.Content ??= string.Empty;
                        am.Content += text.Value;
                    }
                    else if (item is ToolMessageContent tool)
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
                        tm.Content += JsonSerializer.Serialize(tool.Output);
                    }

                    _messages.Add(am);
                    if (tm.Content != null)
                        _messages.Add(tm);
                }
            }
        }

        return new ChatRequest
        {
            Model = request.Model,
            Messages = _messages,
            // 동작하지 않는 모델 존재
            Tools = request?.Tools.Select(t =>
            {
                return new Tool
                {
                    Function = new FunctionTool
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = t.Parameters
                    }
                };
            }),
            Options = new OllamaModelOptions
            {
                NumPredict = request?.Parameters?.MaxTokens,
                Temperature = request?.Parameters?.Temperature,
                TopP = request?.Parameters?.TopP,
                TopK = request?.Parameters?.TopK,
                Stop = request?.Parameters?.StopSequences != null
                    ? string.Join(" ", request.Parameters.StopSequences)
                    : null,
            },
        };
    }
}
