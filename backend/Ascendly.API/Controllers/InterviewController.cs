using Ascendly.Application.DTOs.Interview;
using Ascendly.Application.Interfaces;
using Ascendly.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascendly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _interviewService;
    private readonly PdfResumeExtractorService _pdfResumeExtractor;

    public InterviewController(IInterviewService interviewService, PdfResumeExtractorService pdfResumeExtractor)
    {
        _interviewService = interviewService;
        _pdfResumeExtractor = pdfResumeExtractor;
    }
    [HttpPost("start")]
    public async Task<ActionResult<StartInterviewResponseDto>> Start(
     [FromForm] InterviewConfigurationDto configuration,
     IFormFile? resume)
    {
        // Get the authenticated user's ID from the JWT.
        var userIdValue = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized("User ID was not found.");
        }

        // Extract resume text before starting the interview.
        var resumeText = string.Empty;

        if (resume != null)
        {
            resumeText = _pdfResumeExtractor.ExtractText(
                resume.OpenReadStream());
        }

        var request = new StartInterviewRequestDto
        {
            Configuration = configuration
        };

        var result = await _interviewService.StartAsync(
            request,
            resumeText,
            userId);

        return Ok(result);
    }
    [HttpPost("{sessionId:guid}/answer")]
    public async Task<IActionResult> SubmitAnswer(
    Guid sessionId,
    [FromBody] InterviewAnswerDto answer)
    {
        await _interviewService.SubmitAnswerAsync(
            sessionId,
            answer);

        return Ok(new
        {
            message = "Answer saved successfully."
        });
    }
    [HttpPost("{sessionId:guid}/evaluate-topic/{topicIndex:int}")]
    public async Task<ActionResult<TopicEvaluationDto>> EvaluateTopic(
    Guid sessionId,
    int topicIndex)
    {
        var result = await _interviewService.EvaluateTopicAsync(
            sessionId,
            topicIndex);

        return Ok(result);
    }
    [HttpPost("{sessionId:guid}/complete")]
    public async Task<ActionResult<FinalInterviewEvaluationDto>> Complete(
    Guid sessionId)
    {
        var result = await _interviewService.CompleteAsync(sessionId);

        return Ok(result);
    }
}