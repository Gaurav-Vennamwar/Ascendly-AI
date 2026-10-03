using Ascendly.Application.DTOs.Interview;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;
using Ascendly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ascendly.Infrastructure.Services
{
    public class InterviewService : IInterviewService
    {
        private readonly IInterviewAIService _interviewAIService;
        private readonly ApplicationDbContext _dbContext;

        public InterviewService(
            IInterviewAIService interviewAIService,
            ApplicationDbContext dbContext)
        {
            _interviewAIService = interviewAIService;
            _dbContext = dbContext;
        }

        public async Task<StartInterviewResponseDto> StartAsync(
            StartInterviewRequestDto request,
            string resumeText, Guid userId)
        {
            // Generate the complete interview blueprint.
            var topics = await _interviewAIService.GenerateInterviewAsync(
                request.Configuration,
                resumeText);

            // Create the interview session.
            var session = new InterviewSession
            {
                Id = Guid.NewGuid(),
                UserId = userId, // We will connect the authenticated user next.
                TargetRole = request.Configuration.TargetRole,
                JobDescription = request.Configuration.JobDescription,
                InterviewType = request.Configuration.InterviewType,
                Difficulty = request.Configuration.Difficulty,
                DurationMinutes = request.Configuration.DurationMinutes,
                InterviewStyle = request.Configuration.InterviewStyle,
                InterviewerRole = request.Configuration.InterviewerRole,
                InterviewerContext = request.Configuration.InterviewerContext,
                Status = "IN_PROGRESS",
                CurrentTopicIndex = 0,
                CurrentQuestionIndex = 0,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var topicDto in topics)
            {
                var topic = new InterviewTopic
                {
                    Id = Guid.NewGuid(),
                    InterviewSessionId = session.Id,
                    Name = topicDto.Name,
                    Order = topicDto.Order,
                    Purpose = topicDto.Purpose
                };

                foreach (var questionDto in topicDto.Questions)
                {
                    topic.Questions.Add(new InterviewQuestion
                    {
                        Id = Guid.NewGuid(),
                        InterviewTopicId = topic.Id,
                        Order = questionDto.Order,
                        Topic = questionDto.Topic,
                        Question = questionDto.Question,
                        Purpose = questionDto.Purpose
                    });
                }

                session.Topics.Add(topic);
            }

            _dbContext.InterviewSessions.Add(session);

            await _dbContext.SaveChangesAsync();

            return new StartInterviewResponseDto
            {
                SessionId = session.Id,
                Topics = topics,
                CurrentTopicIndex = 0,
                CurrentQuestionIndex = 0,
                Status = session.Status
            };
        }

        public Task<InterviewSessionDto?> GetSessionAsync(Guid sessionId)
        {
            throw new NotImplementedException();
        }

        public async Task SubmitAnswerAsync(
                Guid sessionId,
                InterviewAnswerDto answer)
        {
            // Find the session first.
            var session = await _dbContext.InterviewSessions
                .FirstOrDefaultAsync(x => x.Id == sessionId);

            if (session == null)
            {
                throw new InvalidOperationException(
                    "Interview session was not found.");
            }

            // Find the question inside this session.
            var question = await _dbContext.InterviewQuestions
            .Include(q => q.InterviewTopic)
            .FirstOrDefaultAsync(q =>
                q.Order == answer.QuestionOrder &&
                q.InterviewTopic.Order == answer.TopicOrder &&
                q.InterviewTopic.InterviewSessionId == sessionId);

            if (question == null)
            {
                throw new InvalidOperationException(
                    "Interview question was not found.");
            }

            // Prevent duplicate answers for the same question.
            var alreadyAnswered = await _dbContext.InterviewAnswers
                .AnyAsync(x => x.InterviewQuestionId == question.Id);

            if (alreadyAnswered)
            {
                throw new InvalidOperationException(
                    "This question has already been answered.");
            }

            // Save the candidate's answer.
            var interviewAnswer = new InterviewAnswer
            {
                Id = Guid.NewGuid(),
                InterviewQuestionId = question.Id,
                Answer = answer.Answer,
                SubmittedAt = DateTime.UtcNow
            };

            _dbContext.InterviewAnswers.Add(interviewAnswer);

            // Update the current question position.
            session.CurrentQuestionIndex = question.Order;

            await _dbContext.SaveChangesAsync();
        }

        public async Task<TopicEvaluationDto> EvaluateTopicAsync(
    Guid sessionId,
    int topicIndex)
        {
            // Find the requested topic inside this interview session.
            var topic = await _dbContext.InterviewTopics
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .Include(t => t.InterviewSession)
                .FirstOrDefaultAsync(t =>
                    t.InterviewSessionId == sessionId &&
                    t.Order == topicIndex + 1);

            if (topic == null)
            {
                throw new InvalidOperationException(
                    "Interview topic was not found.");
            }

            // Make sure every question has an answer.
            if (topic.Questions.Any(q => q.Answers.Count == 0))
            {
                throw new InvalidOperationException(
                    "All questions in this topic must be answered before evaluation.");
            }

            // Prevent evaluating the same questions twice.
            var questionIds = topic.Questions
                .Select(q => q.Id)
                .ToList();

            var alreadyEvaluated = await _dbContext.QuestionEvaluations
                .AnyAsync(e => questionIds.Contains(e.InterviewQuestionId));

            if (alreadyEvaluated)
            {
                throw new InvalidOperationException(
                    "This topic has already been evaluated.");
            }

            // Convert database questions into AI input DTOs.
            var questions = topic.Questions
                .OrderBy(q => q.Order)
                .Select(q => new InterviewQuestionDto
                {
                    Order = q.Order,
                    Topic = q.Topic,
                    Question = q.Question,
                    Purpose = q.Purpose
                })
                .ToList();

            // Convert database answers into AI input DTOs.
            var answers = topic.Questions
                .OrderBy(q => q.Order)
                .Select(q => new InterviewAnswerDto
                {
                    TopicOrder = topic.Order,
                    QuestionOrder = q.Order,
                    Answer = q.Answers
                        .OrderByDescending(a => a.SubmittedAt)
                        .First()
                        .Answer
                })
                .ToList();

            // Rebuild the interview configuration for Gemini.
            var configuration = new InterviewConfigurationDto
            {
                TargetRole = topic.InterviewSession.TargetRole,
                JobDescription = topic.InterviewSession.JobDescription,
                InterviewType = topic.InterviewSession.InterviewType,
                Difficulty = topic.InterviewSession.Difficulty,
                DurationMinutes = topic.InterviewSession.DurationMinutes,
                InterviewStyle = topic.InterviewSession.InterviewStyle,
                InterviewerRole = topic.InterviewSession.InterviewerRole,
                InterviewerContext = topic.InterviewSession.InterviewerContext
            };

            // Evaluate the entire topic in one Gemini call.
            var evaluation = await _interviewAIService.EvaluateTopicAsync(
                topic.Name,
                questions,
                answers,
                configuration);

            // Save every question evaluation to PostgreSQL.
            foreach (var questionEvaluationDto in evaluation.QuestionEvaluations)
            {
                var question = topic.Questions
                    .FirstOrDefault(q =>
                        q.Order == questionEvaluationDto.QuestionOrder);

                if (question == null)
                {
                    continue;
                }

                _dbContext.QuestionEvaluations.Add(
                    new QuestionEvaluation
                    {
                        Id = Guid.NewGuid(),
                        InterviewQuestionId = question.Id,
                        Score = questionEvaluationDto.Score,
                        Strengths = questionEvaluationDto.Strengths,
                        Mistakes = questionEvaluationDto.Mistakes,
                        Improvements = questionEvaluationDto.Improvements,
                        BetterPhrase = questionEvaluationDto.BetterPhrase
                    });
            }

            await _dbContext.SaveChangesAsync();

            return evaluation;
        }

        public async Task<FinalInterviewEvaluationDto> CompleteAsync(
    Guid sessionId)
        {
            // Load the interview session and all question evaluations.
            var session = await _dbContext.InterviewSessions
                .Include(s => s.Topics)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.Evaluations)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null)
            {
                throw new InvalidOperationException(
                    "Interview session was not found.");
            }

            // Prevent completing the same interview twice.
            var existingEvaluation = await _dbContext.FinalInterviewEvaluations
                .AnyAsync(x => x.InterviewSessionId == sessionId);

            if (existingEvaluation)
            {
                throw new InvalidOperationException(
                    "This interview has already been completed.");
            }

            // Make sure every question has been evaluated.
            var allQuestions = session.Topics
                .SelectMany(t => t.Questions)
                .ToList();

            if (allQuestions.Count == 0)
            {
                throw new InvalidOperationException(
                    "This interview does not contain any questions.");
            }

            if (allQuestions.Any(q => q.Evaluations.Count == 0))
            {
                throw new InvalidOperationException(
                    "All interview questions must be evaluated before completion.");
            }

            // Build topic-grouped DTOs for the final Gemini evaluation.
            var topicEvaluations = session.Topics
                .OrderBy(t => t.Order)
                .Select(topic => new TopicEvaluationDto
                {
                    Topic = topic.Name,
                    QuestionEvaluations = topic.Questions
                        .OrderBy(q => q.Order)
                        .Select(q => q.Evaluations
                            .OrderByDescending(e => e.Id)
                            .Select(e => new QuestionEvaluationDto
                            {
                                QuestionOrder = q.Order,
                                Score = e.Score,
                                Strengths = e.Strengths,
                                Mistakes = e.Mistakes,
                                Improvements = e.Improvements,
                                BetterPhrase = e.BetterPhrase
                            })
                            .First())
                        .ToList()
                })
                .ToList();

            // Rebuild configuration from the stored session.
            var configuration = new InterviewConfigurationDto
            {
                TargetRole = session.TargetRole,
                JobDescription = session.JobDescription,
                InterviewType = session.InterviewType,
                Difficulty = session.Difficulty,
                DurationMinutes = session.DurationMinutes,
                InterviewStyle = session.InterviewStyle,
                InterviewerRole = session.InterviewerRole,
                InterviewerContext = session.InterviewerContext
            };

            // Generate the final report using one Gemini call.
            var finalEvaluation =
                await _interviewAIService.GenerateFinalEvaluationAsync(
                    configuration,
                    topicEvaluations);

            // Save the final evaluation to PostgreSQL.
            var finalEntity = new FinalInterviewEvaluation
            {
                Id = Guid.NewGuid(),
                InterviewSessionId = session.Id,
                OverallScore = finalEvaluation.OverallScore,
                CommunicationScore = finalEvaluation.CommunicationScore,
                TechnicalScore = finalEvaluation.TechnicalScore,
                ProblemSolvingScore = finalEvaluation.ProblemSolvingScore,
                BehavioralScore = finalEvaluation.BehavioralScore,
                RoleAlignmentScore = finalEvaluation.RoleAlignmentScore,
                StrongestAreas = finalEvaluation.StrongestAreas,
                AreasToImprove = finalEvaluation.AreasToImprove,
                RepeatedMistakes = finalEvaluation.RepeatedMistakes,
                PreparationPlan = finalEvaluation.PreparationPlan,
                InterviewReadiness = finalEvaluation.InterviewReadiness,
                EvaluatedAt = DateTime.UtcNow
            };

            _dbContext.FinalInterviewEvaluations.Add(finalEntity);

            // Mark the interview as completed.
            session.Status = "COMPLETED";
            session.CompletedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return finalEvaluation;
        }
    }
}