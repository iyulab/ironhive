using Microsoft.AspNetCore.Mvc;
using Microsoft.KernelMemory.Pipeline;
using Raggle.Server.Web.Storages;

namespace Raggle.Server.Web.Controllers;

[ApiController]
[Route("/api/file")]
public class FileController : ControllerBase
{
    private readonly FileStorage _storage;

    public FileController(FileStorage fileStorage)
    {
        _storage = fileStorage;
    }

    [HttpGet("check")]
    public IActionResult CheckFileType([FromQuery] string filename)
    {
        try
        {
            var detector = new MimeTypesDetection();
            var type = detector.GetFileType(filename);
            return Ok(type);
        }
        catch (NotSupportedException)
        {
            return StatusCode(415, $"{filename}은 지원되지 않는 파일 타입입니다.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("{knowledgeId:guid}")]
    public async Task<IActionResult> UploadFileAsync(Guid knowledgeId, [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }
        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return BadRequest("Invalid file name.");
        }

        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            await _storage.UploadAsync(knowledgeId.ToString(), fileName, stream);
        }
        return Ok($"Success to upload file {fileName}");
    }

    [HttpDelete("{knowledgeId:guid}")]
    public IActionResult DeleteFile(
        [FromRoute] Guid knowledgeId, 
        [FromQuery] string fileName)
    {
        _storage.DeleteFile(knowledgeId.ToString(), fileName);
        return Ok($"Success to delete file {fileName}");
    }
}
