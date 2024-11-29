namespace Raggle.Core.Utils;

public static partial class DocumentFileHelper
{
    public static string PipelineFileExtension { get; private set; } = ".pipeline.json";
    public static string DecodedFileExtension { get; private set; } = ".decode.json";
    public static string ChunkedFileExtension { get; private set; } = ".chunk.json";

    public static string GetPipelineFileName(string fileName)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}{PipelineFileExtension}";
    }

    public static void SetPipelineFileExtension(string extension)
    {
        PipelineFileExtension = extension;
    }

    public static string GetDecodedFileName(string fileName)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}{DecodedFileExtension}";
    }

    public static void SetDecodedFileExtension(string extension)
    {
        DecodedFileExtension = extension;
    }

    public static string GetChunkedFileName(string fileName, int number)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}_{number:D3}{ChunkedFileExtension}";
    }

    public static void SetChunkedFileExtension(string extension)
    {
        ChunkedFileExtension = extension;
    }
}
