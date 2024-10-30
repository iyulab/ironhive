namespace Raggle.Core.Utils;

public static class DocumentFileHelper
{
    public static string PipelineFileExtension { get; private set; } = ".pipeline.json";
    public static string ParsedFileExtension { get; private set; } = ".parsed.json";
    public static string ChunkedFileExtension { get; private set; } = ".chunked.json";

    public static string GetPipelineFileName(string fileName)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}{PipelineFileExtension}";
    }

    public static void SetPipelineFileExtension(string extension)
    {
        PipelineFileExtension = extension;
    }

    public static string GetParsedFileName(string fileName)
    {
        return $"{Path.GetFileNameWithoutExtension(fileName)}{ParsedFileExtension}";
    }

    public static void SetParsedFileExtension(string extension)
    {
        ParsedFileExtension = extension;
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
