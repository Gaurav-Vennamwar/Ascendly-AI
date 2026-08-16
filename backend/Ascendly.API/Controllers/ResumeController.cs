using Ascendly.Application.DTOs.Resume;
using Ascendly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascendly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResumeController : ControllerBase
{
    private readonly IResumeAnalyzerService _resumeAnalyzerService;

    public ResumeController(IResumeAnalyzerService resumeAnalyzerService)
    {
        _resumeAnalyzerService = resumeAnalyzerService;
    }

    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Analyze(
    IFormFile resume,
    [FromForm] string jobDescription)
    {
        // Resume file is mandatory.
        if (resume == null || resume.Length == 0)
        {
            return BadRequest("Resume PDF is required.");
        }

        // Maximum allowed resume size: 5 MB.
        const long maxFileSize = 5 * 1024 * 1024;

        if (resume.Length > maxFileSize)
        {
            return BadRequest("Resume file must be 5 MB or smaller.");
        }

        // Only PDF files are accepted.
        if (!string.Equals(resume.ContentType, "application/pdf",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF resumes are supported.");
        }

        try
        {
            // Convert the uploaded file into a Stream
            // before passing it to the service layer.
            await using var resumeStream = resume.OpenReadStream();

            var result = await _resumeAnalyzerService.AnalyzeAsync(
                resumeStream,
                jobDescription);

            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            // Return a clean 400 response for invalid/corrupt PDFs.
            return BadRequest(ex.Message);
        }
    }
}