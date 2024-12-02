using Raggle.Abstractions.Memory;

namespace Raggle.Core.Extensions;

public static class DataPipelineExtensions
{
    public static string GetCurrentStepFileName(
        this DataPipeline pipeline,
        int? index = null,
        string extension = "json")
    {
        var baseFile = Path.GetFileNameWithoutExtension(pipeline.Source.FileName);
        return index.HasValue
            ? $"{baseFile}.{index.Value:D3}.{pipeline.CurrentStep}.{extension}"
            : $"{baseFile}.{pipeline.CurrentStep}.{extension}";
    }

    public static string GetPreviousStepFileName(
        this DataPipeline pipeline,
        int? index = null,
        string extension = "json")
    {
        var baseFile = Path.GetFileNameWithoutExtension(pipeline.Source.FileName);
        var previousStep = pipeline.GetPreviousStep()
            ?? throw new InvalidOperationException("Cannot get previous step when current step is Queued.");
        return index.HasValue
            ? $"{baseFile}.{index.Value:D3}.{previousStep}.{extension}"
            : $"{baseFile}.{previousStep}.{extension}";
    }

    public static bool IsCurrentStepFileName(
        this DataPipeline pipeline,
        string filePath,
        string extension = "json")
    {
        var currentStep = pipeline.CurrentStep;
        return filePath.EndsWith($"{currentStep}.{extension}");
    }

    public static bool IsPreviousStepFileName(
        this DataPipeline pipeline,
        string filePath,
        string extension = "json")
    {
        var previousStep = pipeline.GetPreviousStep()
            ?? throw new InvalidOperationException("Cannot get previous step when current step is Queued.");
        return filePath.EndsWith($"{previousStep}.{extension}");
    }
}
