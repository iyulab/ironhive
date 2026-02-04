using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Providers.GoogleAI.Payloads;
using IronHive.Providers.GoogleAI.Payloads.GenerateContent;
using System.Text.Json;
using System.Text.Json.Nodes;
using GoogleAIContent = IronHive.Providers.GoogleAI.Payloads.Content;

namespace IronHive.Abstractions.Messages;

internal static class MessageGenerationRequestExtensions
{
    /// <summary>
    /// GoogleAI의 GenerateContentRequest로 변환합니다.
    /// </summary>
    internal static GenerateContentRequest ToGoogleAI(this MessageGenerationRequest request)
    {
        var contents = new List<GoogleAIContent>();
        foreach (var msg in request.Messages)
        {
            // 사용자 메시지
            if (msg is UserMessage user)
            {
                var um = new GoogleAIContent(ContentRole.user);
                foreach (var item in user.Content)
                {
                    // 텍스트 메시지
                    if (item is TextMessageContent text)
                    {
                        um.Parts.Add(new ContentPart
                        {
                            Text = text.Value ?? string.Empty
                        });
                    }
                    // 파일 문서 메시지
                    else if (item is DocumentMessageContent document)
                    {
                        um.Parts.Add(new ContentPart
                        {
                            Text = $"document({document.ContentType}): \n{document.Data}\n",
                        });
                    }
                    // 이미지 메시지
                    else if (item is ImageMessageContent image)
                    {
                        um.Parts.Add(new ContentPart
                        {
                            InlineData = new BlobData
                            {
                                MimeType = image.Format switch
                                {
                                    ImageFormat.Png => "image/png",
                                    ImageFormat.Jpeg => "image/jpeg",
                                    ImageFormat.Webp => "image/webp",
                                    _ => throw new NotImplementedException("not supported yet")
                                },
                                Data = image.Base64
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }
                contents.Add(um);
            }
            // AI 메시지
            else if (msg is AssistantMessage assistant)
            {
                var am = new GoogleAIContent(ContentRole.model);
                var um = new GoogleAIContent(ContentRole.user);

                string? ts = null; // 사고 (Thinking) 메시지 시그니처 추적용
                foreach (var item in assistant.Content)
                {
                    // 사고 메시지
                    if (item is ThinkingMessageContent thinking)
                    {
                        am.Parts.Add(new ContentPart
                        {
                            Thought = true,
                            Text = thinking.Value ?? string.Empty,
                        });

                        // 시그니처 갱신
                        if (!string.IsNullOrWhiteSpace(thinking.Signature))
                        {
                            ts = thinking.Signature;
                        }
                    }
                    // 텍스트 메시지
                    else if (item is TextMessageContent text)
                    {
                        var part = new ContentPart
                        {
                            Text = text.Value ?? string.Empty,
                        };

                        // 이전 사고 시그니처가 있으면 연결 및 초기화
                        if (!string.IsNullOrWhiteSpace(ts))
                        {
                            part.ThoughtSignature = ts;
                            ts = null;
                        }

                        am.Parts.Add(part);
                    }
                    // 도구 메시지
                    else if (item is ToolMessageContent tool)
                    {
                        // 도구 호출 메시지
                        var part = new ContentPart
                        {
                            FunctionCall = new FunctionCall
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Args = JsonSerializer.Deserialize<JsonObject>(tool.Input ?? "{}") ?? new JsonObject()
                            }
                        };
                        // 이전 사고 시그니처가 있으면 연결 및 초기화
                        if (!string.IsNullOrWhiteSpace(ts))
                        {
                            part.ThoughtSignature = ts;
                            ts = null;
                        }
                        am.Parts.Add(part);

                        // 도구 결과 메시지
                        um.Parts.Add(new ContentPart
                        {
                            FunctionResponse = new FunctionResponse
                            {
                                Id = tool.Id,
                                Name = tool.Name,
                                Response = tool.Output != null ? tool.Output : new JsonObject()
                            }
                        });
                    }
                    else
                    {
                        throw new NotImplementedException("not supported yet");
                    }
                }

                contents.Add(am);
                if (um.Parts.Count > 0)
                    contents.Add(um);
            }
            else
            {
                throw new NotImplementedException("not supported yet");
            }
        }

        return new GenerateContentRequest
        {
            Model = request.Model,
            Contents = contents,
            SystemInstruction = string.IsNullOrWhiteSpace(request.System) ? null : new GoogleAIContent
            {
                Parts = [ new ContentPart { Text = request.System } ]
            },
            Tools = request.Tools?.Any() == true ? [new Tool
            {
                Functions = request.Tools.Select(t => new FunctionTool
                {
                    Name = t.UniqueName,
                    Description = t.Description ?? string.Empty,
                    ParametersJsonSchema = t.Parameters ?? new JsonObject()
                }).ToArray()
            }] : null,
            GenerationConfig = new GenerationConfig
            {
                CandidateCount = 1,
                MaxOutputTokens = request.MaxTokens,
                StopSequences = request.StopSequences,
                TopP = request.TopP,
                TopK = request.TopK,
                Temperature = request.Temperature,
                ThinkingConfig = request.ThinkingEffort is not null and not MessageThinkingEffort.None ? new ThinkingConfig
                {
                    IncludeThoughts = true,
                    ThinkingBudget = CalculateThinkingTokens(request)
                } : null,
            },
        };
    }

    /// <summary>
    /// 추론(Thinking) 사용 시 토큰 예산을 계산합니다.
    /// <see href="https://ai.google.dev/gemini-api/docs/thinking#set-budget">모델별 토큰 범위</see>
    /// </summary>
    internal static int CalculateThinkingTokens(MessageGenerationRequest request)
    {
        string model = request.Model.ToLowerInvariant();

        // Gemini 2.5 Pro (128 ~ 32768)
        if (model.Contains("gemini-2.5-pro"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 1024,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 32768,
                _ => -1 // dynamic
            };
        }
        // Gemini 2.5 Flash (0 ~ 24576)
        else if (model.Contains("gemini-2.5-flash"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 1024,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => -1 // dynamic
            };
        }
        // Gemini 2.5 Flash Lite(512 ~ 24576)
        else if (model.Contains("gemini-2.5-flash-lite"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => 0
            };
        }
        // Robotics-ER 1.5 Preview(512 ~ 24576)
        else if (model.Contains("robotics-er-1.5"))
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 8192,
                MessageThinkingEffort.High => 24576,
                _ => 0
            };
        }
        // 이외 모델(512 ~ 8192)
        else
        {
            return request.ThinkingEffort switch
            {
                MessageThinkingEffort.Low => 512,
                MessageThinkingEffort.Medium => 2048,
                MessageThinkingEffort.High => 8192,
                _ => 0
            };
        }
    }
}