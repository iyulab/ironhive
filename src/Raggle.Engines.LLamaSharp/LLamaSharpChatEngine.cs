using System.Text;
using LLama;
using LLama.Common;
using Raggle.Abstractions.Engines;

namespace Raggle.Engines.LLamaSharp;

public class LLamaSharpChatEngine : IChatCompletionEngine, IDisposable
{
    private readonly ModelParams _params;
    public LLamaWeights? Model { get; private set; }

    public LLamaSharpChatEngine(string modelPath)
    {
        _params = GetParams(modelPath);
        LoadModel();
    }

    public void Dispose()
    {
        Model?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void LoadModel()
    {
        Model = LLamaWeights.LoadFromFile(_params);
    }

    public long CountTokens(string text)
    {
        if (Model == null)
        {
            throw new InvalidOperationException("Model is not loaded.");
        }

        var tokens = Model.Tokenize(text, false, false, Encoding.UTF8);
        return tokens.Length;
    }

    private static ModelParams GetParams(string modelPath)
    {
        return new ModelParams(modelPath)
        {
            ContextSize = 2500,                          // 모델이 한 번에 처리할 수 있는 최대 토큰 수
            MainGpu = 0,                                 // 주로 사용할 GPU의 ID
            GpuLayerCount = 1,                           // 모델이 GPU에서 처리할 레이어의 수
            Encoding = Encoding.UTF8,                    // 텍스트 데이터를 인코딩할 방식
            Seed = 0,                                    // 랜덤 시드
        };
    }

    public Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ChatCompletionResponse>> ChatCompletionAsync()
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<StreamingChatCompletionResponse> StreamingChatCompletionAsync()
    {
        throw new NotImplementedException();
    }
}
