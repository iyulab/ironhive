using Microsoft.AspNetCore.StaticFiles;

namespace IronHive.Core.Memory;

public class MimeTypeDetector
{
    private readonly FileExtensionContentTypeProvider _provider = new();

    public MimeTypeDetector()
    {
    }

    public bool TryGetContentType(string fileName, out string contentType)
    {
        return _provider.TryGetContentType(fileName, out contentType);
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
