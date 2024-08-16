using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Raggle.Abstractions.Prompts;

namespace Raggle.Abstractions;

public interface IRaggleServiceBuilder
{
    IKernelBuilder KernelBuilder { get; set; }
    IKernelMemoryBuilder MemoryBuilder { get; set; }
    IPromptProvider? PromptProvider { get; set; }

    IRaggleService Build();
}
