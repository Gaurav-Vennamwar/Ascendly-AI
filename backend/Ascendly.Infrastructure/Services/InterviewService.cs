using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Interview;
using Ascendly.Application.Interfaces;

namespace Ascendly.Infrastructure.Services
{
    public class InterviewService : IInterviewService
    {
        // Temporary session storage for the first working version.
        private readonly ConcurrentDictionary<Guid, InterviewSessionDto> _sessions = [];

        public Task<StartInterviewResponseDto> StartAsync(
            StartInterviewRequestDto request)
        {
            var sessionId = Guid.NewGuid();

            var session = new InterviewSessionDto
            {
                SessionId = sessionId,
                Configuration = request.Configuration,
                Status = "IN_PROGRESS"
            };

            _sessions[sessionId] = session;

            // Questions will be added through Gemini generation next.
            return Task.FromResult(new StartInterviewResponseDto
            {
                SessionId = sessionId,
                Topics = session.Topics,
                CurrentTopicIndex = session.CurrentTopicIndex,
                CurrentQuestionIndex = session.CurrentQuestionIndex,
                Status = session.Status
            });
        }

        public Task<InterviewSessionDto?> GetSessionAsync(Guid sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);

            return Task.FromResult(session);
        }

        public Task SubmitAnswerAsync(
            Guid sessionId,
            InterviewAnswerDto answer)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                throw new InvalidOperationException(
                    "Interview session was not found.");
            }

            // Answer storage will be added with the answer model.
            return Task.CompletedTask;
        }

        public Task<TopicEvaluationDto> EvaluateTopicAsync(
            Guid sessionId,
            int topicIndex)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                throw new InvalidOperationException(
                    "Interview session was not found.");
            }

            // Gemini topic evaluation will be connected next.
            throw new NotImplementedException(
                "Topic evaluation is not implemented yet.");
        }

        public Task<FinalInterviewEvaluationDto> CompleteAsync(
            Guid sessionId)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                throw new InvalidOperationException(
                    "Interview session was not found.");
            }

            // Gemini final evaluation will be connected next.
            throw new NotImplementedException(
                "Final interview evaluation is not implemented yet.");
        }
    }
}
