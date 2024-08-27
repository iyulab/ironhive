using System.Text;
using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using ChatHistory = Microsoft.SemanticKernel.ChatCompletion.ChatHistory;

namespace Raggle.Extensions.LLamaSharp;

public class LLamaSharpChatEngine : IDisposable
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

    public async IAsyncEnumerable<StreamingChatMessageContent> StreamingChatAsync(ChatHistory history, LLamaSharpPromptExecutionSettings? options)
    {
        if (Model == null)
        {
            throw new InvalidOperationException("Model is not loaded.");
        }

        //var tramsform = new HistoryTransform();
        //var text = tramsform.HistoryToText(history.ToLLamaSharpChatHistory());
        //Console.WriteLine($"[Original]\n{text}\n[End]\n");

        var executer = new StatelessExecutor(Model, _params);
        var chat = new LLamaSharpChatCompletion(executer);
        await foreach (var response in chat.GetStreamingChatMessageContentsAsync(history, options))
        {
            yield return response;
        }
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
}
