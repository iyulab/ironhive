namespace IronHive.Providers.OpenAI.Compatible.GpuStack;

/// <summary>
/// GPUStack 서비스를 위한 메시지 생성기입니다.
/// </summary>
public class GpuStackMessageGenerator : OpenAIChatMessageGenerator
{
    public GpuStackMessageGenerator(GpuStackConfig config) : base(config.ToOpenAI())
    {
    }
}
