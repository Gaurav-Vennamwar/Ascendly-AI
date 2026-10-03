using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Interview;

namespace Ascendly.Application.Interfaces
{
    public interface IInterviewAIService
    {
        // Generates the complete interview blueprint.
        Task<List<InterviewTopicDto>> GenerateInterviewAsync(
            InterviewConfigurationDto configuration, string resumeText);

        // Evaluates all answers from one completed topic.
        Task<TopicEvaluationDto> EvaluateTopicAsync(
            string topic,
            List<InterviewQuestionDto> questions,
            List<InterviewAnswerDto> answers,
            InterviewConfigurationDto configuration);

        // Generates the final interview report.
        Task<FinalInterviewEvaluationDto> GenerateFinalEvaluationAsync(
            InterviewConfigurationDto configuration,
            List<TopicEvaluationDto> topicEvaluations);
    }
}
