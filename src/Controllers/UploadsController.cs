using Microsoft.AspNetCore.Mvc;

namespace artgallery_server.Controllers
{
    [ApiController]
    [Route("api/v1/uploads")]
    public class UploadsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadsController(IWebHostEnvironment env) => _env = env;

        [HttpPost("arts")]
        [RequestSizeLimit(10_000_000)]
        public async Task<ActionResult<string>> UploadArtwork(IFormFile file)
        {
            if (file is null || file.Length == 0) return BadRequest("Empty file.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avif" };
            if (!allowed.Contains(ext)) return BadRequest("Unsupported type.");

            var name = $"{Guid.NewGuid():N}{ext}";
            var relPath = $"/images/artworks/{name}";
            var absPath = Path.Combine(_env.WebRootPath!, "images", "artworks", name);

            Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);

            await using var stream = System.IO.File.Create(absPath);
            await file.CopyToAsync(stream);

            return Ok(relPath);
        }
    }

}
