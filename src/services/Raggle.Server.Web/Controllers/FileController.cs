using Microsoft.AspNetCore.Mvc;
using Raggle.Server.API.Storages;

namespace Raggle.Server.API.Controllers
{
    [ApiController]
    [Route("/file")]
    public class FileController : ControllerBase
    {
        private readonly FileStorage _storage;

        public FileController(FileStorage fileStorage)
        {
            _storage = fileStorage;
        }

        [HttpPost("{sourceId:guid}")]
        public async Task<IActionResult> UploadFile(Guid sourceId, [FromForm] IFormFile file)
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
                await _storage.UploadAsync(sourceId.ToString(), fileName, stream);
            }
            return Ok($"Success to upload file {fileName}");
        }

        [HttpDelete("{sourceId:guid}/{fileName}")]
        public IActionResult DeleteFile(Guid sourceId, string fileName)
        {
            _storage.DeleteFile(sourceId.ToString(), fileName);
            return Ok($"Success to delete file {fileName}");
        }
    }
}
