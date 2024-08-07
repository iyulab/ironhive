using Microsoft.AspNetCore.Mvc;
using Raggle.Server.API.Models;
using Raggle.Server.API.Storages;
using System.IO;

namespace Raggle.Server.API.Controllers
{
    [ApiController]
    [Route("/file")]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly FileStorage _storage;

        public FileController(ILogger<FileController> logger, FileStorage fileStorage)
        {
            _logger = logger;
            _storage = fileStorage;
        }

        [HttpPost("{userId}/{sourceId}")]
        public async Task<IActionResult> UploadFile(Guid userId, Guid sourceId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var index = GetIndexPath(userId, sourceId);
            var fileName = Path.GetFileName(file.FileName);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("Invalid file name.");
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;
                    await _storage.UploadFile(index, fileName, stream);
                }

                return Ok($"Success to upload file {fileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/{sourceId}/{fileName}")]
        public async Task<IActionResult> DeleteFile(Guid userId, Guid sourceId, string fileName)
        {
            try
            {
                var index = GetIndexPath(userId, sourceId);
                await _storage.DeleteFile(index, fileName);
                return Ok($"Success to delete file {fileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        private string GetIndexPath(Guid userId, Guid sourceId)
        {
            return $"{userId}/{sourceId}";
        }
    }
}
