using Microsoft.AspNetCore.StaticFiles;

namespace Raggle.Abstractions.Utils;

public class MimeTypeDetector
{
    private readonly FileExtensionContentTypeProvider _provider = new();

    public bool TryGetContentType(string filePath, out string contentType)
    {
        return _provider.TryGetContentType(filePath, out contentType);
    }

    public void SetContentType(string extension, string contentType)
    {
        _provider.Mappings[extension] = contentType;
    }

    public void RemoveContentType(string extension)
    {
        _provider.Mappings.Remove(extension);
    }

    public void ClearContentTypes()
    {
        _provider.Mappings.Clear();
    }
}
