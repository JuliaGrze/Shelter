using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Shelter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private static readonly HashSet<string> AllowedExt = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env) => _env = env;

    // POST /api/upload/animals/by-species-name/{speciesName}/{id}
    [HttpPost("animals/by-species-name/{speciesName}/{id:int}")]
    [Authorize(Roles = "Admin,Worker")]
    [RequestSizeLimit(MaxBytes)]
    public async Task<IActionResult> UploadBySpeciesName(
        [FromRoute] string speciesName,
        [FromRoute] int id,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Brak pliku." });

        if (file.Length > MaxBytes)
            return BadRequest(new { message = "Plik za duży (max 5 MB)." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExt.Contains(ext))
            return BadRequest(new { message = "Niedozwolony format. Użyj JPG/PNG/WEBP." });

        // Upewnijmy się, że speciesName jest „bezpieczne” dla ścieżki
        var safeName = string.Concat(speciesName
            .ToLowerInvariant()
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_'));

        if (string.IsNullOrWhiteSpace(safeName))
            return BadRequest(new { message = "Niepoprawna nazwa gatunku." });

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var dir = Path.Combine(webRoot, "images", "animals", safeName);
        Directory.CreateDirectory(dir);

        var fileName = $"{id}{ext}";
        var fullPath = Path.Combine(dir, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream, ct);

        var urlPath = $"/images/animals/{safeName}/{fileName}";
        var absoluteUrl = $"{Request.Scheme}://{Request.Host}{urlPath}";
        return Ok(new { url = absoluteUrl });
    }
}
