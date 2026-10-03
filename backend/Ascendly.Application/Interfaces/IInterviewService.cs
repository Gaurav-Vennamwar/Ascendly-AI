using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Interview;

namespace Ascendly.Application.Interfaces
{
    public interface IInterviewService
    {
        // Creates a session and generates the interview blueprint.
        Task<StartInterviewResponseDto> StartAsync(
        StartInterviewRequestDto request, string resumeText, Guid userId);
        // Returns the current interview session.
        Task<InterviewSessionDto?> GetSessionAsync(Guid sessionId);

        // Stores the candidate's answer.
        Task SubmitAnswerAsync( Guid sessionId,InterviewAnswerDto answer);

        // Evaluates all answers from the completed topic.
        Task<TopicEvaluationDto> EvaluateTopicAsync(Guid sessionId, int topicIndex);

        // Generates the final interview report.
        Task<FinalInterviewEvaluationDto> CompleteAsync(Guid sessionId);
    }
}
