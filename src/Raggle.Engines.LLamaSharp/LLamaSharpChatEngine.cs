using System.Text;
using LLama;
using LLama.Common;
using Raggle.Abstractions.Engines;
using Raggle.Abstractions.Models;
using ChatSession = Raggle.Abstractions.Models.ChatSession;

namespace Raggle.Engines.LLamaSharp;

public class LLamaSharpChatEngine : IChatEngine, IDisposable
{
    private readonly IDictionary<string, LLamaContext> _models;

    public LLamaSharpChatEngine(LLamaSharpConfig config)
    {
        _models = LoadModels(config);
    }

    public void Dispose()
    {
        foreach(var model in _models)
        {
            model.Value.Dispose();
        }
        _models.Clear();
        GC.SuppressFinalize(this);
    }

    public Dictionary<string, LLamaContext> LoadModels(LLamaSharpConfig config)
    {
        var models = new Dictionary<string, LLamaContext>();
        var modelPaths = config.ModelPaths;
        foreach (var modelPath in modelPaths)
        {
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 2048,                          // 모델이 한 번에 처리할 수 있는 최대 토큰 수
                MainGpu = 0,                                 // 주로 사용할 GPU의 ID
                GpuLayerCount = 1,                           // 모델이 GPU에서 처리할 레이어의 수
                Encoding = Encoding.UTF8,                    // 텍스트 데이터를 인코딩할 방식
                Seed = 0,                                    // 랜덤 시드
            };
            var model = LLamaWeights.LoadFromFile(parameters);
            var context = new LLamaContext(model, parameters);
            models.Add(modelPath, context);
        }
        return models;
    }

    public Task<IEnumerable<ChatModel>> GetChatCompletionModelsAsync()
    {
        var models = new List<ChatModel>();
        foreach (var model in _models)
        {
            models.Add(new ChatModel
            {
                ModelId = model.Key,
                CreatedAt = null,
                Owner = null,
            });
        }
        return Task.FromResult(models.AsEnumerable());
    }

    public Task<ChatResponse> ChatCompletionAsync(ChatSession session, ChatOptions options)
    {
        if (!_models.ContainsKey(options.ModelId))
        {
            throw new ArgumentException("Model not found.");
        }
        else
        {
            
        }
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<StreamingChatResponse> StreamingChatCompletionAsync(ChatSession session, ChatOptions options)
    {
        throw new NotImplementedException();
    }
}
