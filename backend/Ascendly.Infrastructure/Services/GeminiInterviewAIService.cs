using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Interview;
using Microsoft.Extensions.Configuration;

namespace Ascendly.Infrastructure.Services
{
    public class GeminiInterviewAIService : IInterviewAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeminiInterviewAIService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<List<InterviewTopicDto>> GenerateInterviewAsync(
            InterviewConfigurationDto configuration)
        {
            var prompt = BuildInterviewGenerationPrompt(configuration);

            var json = await SendToGeminiAsync(
                prompt,
                BuildInterviewGenerationSchema());

            return JsonSerializer.Deserialize<List<InterviewTopicDto>>(
                       json,
                       JsonOptions())
                   ?? [];
        }

        public async Task<TopicEvaluationDto> EvaluateTopicAsync(
            string topic,
            List<InterviewQuestionDto> questions,
            List<InterviewAnswerDto> answers,
            InterviewConfigurationDto configuration)
        {
            var prompt = BuildTopicEvaluationPrompt(
                topic,
                questions,
                answers,
                configuration);

            var json = await SendToGeminiAsync(
                prompt,
                BuildTopicEvaluationSchema());

            return JsonSerializer.Deserialize<TopicEvaluationDto>(
                       json,
                       JsonOptions())
                   ?? new TopicEvaluationDto
                   {
                       Topic = topic
                   };
        }

        public async Task<FinalInterviewEvaluationDto> GenerateFinalEvaluationAsync(
            InterviewConfigurationDto configuration,
            List<TopicEvaluationDto> topicEvaluations)
        {
            var prompt = BuildFinalEvaluationPrompt(
                configuration,
                topicEvaluations);

            var json = await SendToGeminiAsync(
                prompt,
                BuildFinalEvaluationSchema());

            return JsonSerializer.Deserialize<FinalInterviewEvaluationDto>(
                       json,
                       JsonOptions())
                   ?? new FinalInterviewEvaluationDto();
        }

        private async Task<string> SendToGeminiAsync(
            string prompt,
            object responseSchema)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:Model"]
                        ?? "gemini-3.1-flash-lite";

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = prompt
                        }
                    }
                }
            },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    responseSchema
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent");

            request.Headers.Add("x-goog-api-key", apiKey);
            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(request);

            var responseJson =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Gemini API error ({(int)response.StatusCode}): {responseJson}");
            }

            var geminiResponse =
                JsonSerializer.Deserialize<GeminiResponse>(
                    responseJson,
                    JsonOptions());

            var text = geminiResponse?
                .Candidates
                .FirstOrDefault()?
                .Content?
                .Parts
                .FirstOrDefault()?
                .Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException(
                    "Gemini returned an empty interview response.");
            }

            return text;
        }

        private static string BuildInterviewGenerationPrompt(
            InterviewConfigurationDto configuration)
        {
            return $$"""
        You are Ascendly AI's professional mock interview engine.

        Generate a realistic, role-agnostic interview based ONLY on the
        candidate's supplied context.

        INPUTS:

        TARGET ROLE:
        {{configuration.TargetRole}}

        RESUME:
        {{configuration.ResumeText ?? "Not provided"}}

        JOB DESCRIPTION:
        {{configuration.JobDescription ?? "Not provided"}}

        CUSTOM TOPICS:
        {{string.Join(", ", configuration.CustomTopics)}}

        INTERVIEW TYPE:
        {{configuration.InterviewType}}

        DIFFICULTY:
        {{configuration.Difficulty}}

        DURATION:
        {{configuration.DurationMinutes}} minutes

        INTERVIEW STYLE:
        {{configuration.InterviewStyle}}

        CORE RULES:

        1. The interview must be role-agnostic.
        2. Never assume the candidate is a software developer or belongs
           to a technology role.
        3. Generate topics based on the target role, supplied JD, resume,
           interview type, difficulty and custom topics.
        4. JD is optional.
        5. Resume is optional.
        6. Custom topics are optional.
        7. If a resume is provided, deeply inspect projects, experience,
           responsibilities, skills and claims.
        8. If projects are present, create meaningful project deep-dive
           questions where relevant.
        9. Do not invent candidate experience.
        10. Do not state that the candidate used a technology unless the
            supplied resume explicitly supports it.
        11. Do not force technical, DSA, aptitude, salary, or behavioral
            sections when they are not relevant to the role.
        12. Adapt the interview naturally to the target profession.
        13. Match the requested duration with a realistic number of questions.
        14. Questions should progress from easier/contextual questions
            toward deeper and more challenging questions.
        15. Avoid repetitive questions.

        PROJECT DEEP-DIVE RULE:

        When the resume contains a project relevant to the role:

        - Ask what problem it solved.
        - Ask why the candidate built it.
        - Ask what they personally implemented.
        - Ask about important technical or professional decisions.
        - Ask about challenges and trade-offs.
        - Ask how they would improve the project.
        - Ask deeper follow-ups based on actual claims.

        Never invent architecture, metrics, technologies, users, scale,
        responsibilities or achievements.

        CURRENT INTERVIEW PATTERN:

        When current interview patterns are available through an enabled
        search/grounding capability, use recent credible public sources
        to understand common interview structure and question themes.

        Prefer recent and credible sources.

        Do NOT claim:
        "Company X asked this exact question"

        unless reliable source evidence supports that claim.

        Public interview patterns are guidance, not confidential company data.

        QUESTION DESIGN:

        Every question must have:
        - order
        - topic
        - question
        - purpose

        The purpose should describe what the question is testing.

        The final output must contain only valid JSON matching the schema.
        """;
        }

        private static string BuildTopicEvaluationPrompt(
            string topic,
            List<InterviewQuestionDto> questions,
            List<InterviewAnswerDto> answers,
            InterviewConfigurationDto configuration)
        {
            var questionJson =
                JsonSerializer.Serialize(questions);

            var answerJson =
                JsonSerializer.Serialize(answers);

            return $$"""
        You are Ascendly AI's interview evaluator.

        Evaluate the candidate's COMPLETE performance in one interview topic.

        TARGET ROLE:
        {{configuration.TargetRole}}

        TOPIC:
        {{topic}}

        QUESTIONS:
        {{questionJson}}

        CANDIDATE ANSWERS:
        {{answerJson}}

        IMPORTANT:

        Evaluate every question individually.

        For each question return:

        - questionOrder
        - score from 0 to 100
        - strengths
        - mistakes
        - improvements
        - betterPhrase

        BETTER PHRASE RULE:

        "betterPhrase" must be the COMPLETE professional model answer
        that the candidate could learn from and use as a stronger answer.

        It must NOT be:
        - a fragment
        - a few words
        - bullet fragments
        - "..."
        - a shortened placeholder

        It should be a natural spoken interview answer.

        FACTUAL ACCURACY:

        The betterPhrase must remain grounded in the candidate's actual
        answer, supplied resume and interview context.

        Never invent:
        - technologies
        - responsibilities
        - metrics
        - achievements
        - employers
        - certifications
        - implementation details
        - scale
        - users
        - results

        Never make the candidate sound more experienced than the evidence supports.

        MISTAKES:

        Identify actual weaknesses such as:
        - incorrect information
        - incomplete reasoning
        - vague answers
        - poor structure
        - unsupported claims
        - unnecessary filler
        - failure to answer the question

        Do not label normal professional language as a mistake.

        STRENGTHS:

        Mention what was genuinely good in the answer.

        IMPROVEMENTS:

        Give concrete actions the candidate can use in the next interview.

        TOPIC SCORE:

        Calculate one overall topic score from the individual answers.
        The topic score should reflect the candidate's actual performance,
        not the target role alone.

        Return only JSON matching the required schema.
        """;
        }

        private static string BuildFinalEvaluationPrompt(
            InterviewConfigurationDto configuration,
            List<TopicEvaluationDto> topicEvaluations)
        {
            var topicJson =
                JsonSerializer.Serialize(topicEvaluations);

            return $$"""
        You are Ascendly AI's final interview readiness evaluator.

        Evaluate the candidate's complete mock interview using the topic
        evaluations below.

        TARGET ROLE:
        {{configuration.TargetRole}}

        TOPIC EVALUATIONS:
        {{topicJson}}

        Produce a realistic overall assessment.

        Return:

        OverallScore:
        One overall score from 0 to 100.

        CommunicationScore:
        Evaluate communication performance.

        TechnicalScore:
        Evaluate technical/domain performance only when technical/domain
        content was actually part of the interview.
        For non-technical roles, evaluate the most relevant professional
        knowledge dimension instead.

        ProblemSolvingScore:
        Evaluate reasoning and problem-solving demonstrated in the interview.

        BehavioralScore:
        Evaluate behavioral/interpersonal performance when behavioral content
        was part of the interview.

        RoleAlignmentScore:
        Evaluate how well the candidate's demonstrated answers align with
        the target role.

        StrongestAreas:
        List the strongest demonstrated areas.

        AreasToImprove:
        List the most important weaknesses.

        RepeatedMistakes:
        Identify mistakes that appeared across multiple questions or topics.

        PreparationPlan:
        Give prioritized preparation actions before a real interview.

        InterviewReadiness:
        Return a concise practical assessment such as:
        READY
        READY_WITH_PREPARATION
        NEEDS_PREPARATION

        IMPORTANT:

        Do not judge the candidate on topics that were never tested.

        Do not invent missing evidence.

        Do not claim professional experience that the candidate did not
        demonstrate.

        Return only JSON matching the required schema.
        """;
        }

        private static object BuildInterviewGenerationSchema()
        {
            return new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        name = new { type = "string" },
                        order = new { type = "integer" },
                        purpose = new { type = "string" },
                        questions = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    order = new { type = "integer" },
                                    topic = new { type = "string" },
                                    question = new { type = "string" },
                                    purpose = new { type = "string" }
                                },
                                required = new[]
                                {
                                "order",
                                "topic",
                                "question",
                                "purpose"
                            }
                            }
                        }
                    },
                    required = new[]
                    {
                    "name",
                    "order",
                    "purpose",
                    "questions"
                }
                }
            };
        }

        private static object BuildTopicEvaluationSchema()
        {
            return new
            {
                type = "object",
                properties = new
                {
                    topic = new { type = "string" },
                    score = new { type = "integer", minimum = 0, maximum = 100 },

                    questionEvaluations = new
                    {
                        type = "array",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                questionOrder = new { type = "integer" },
                                score = new
                                {
                                    type = "integer",
                                    minimum = 0,
                                    maximum = 100
                                },
                                strengths = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                mistakes = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                improvements = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                betterPhrase = new { type = "string" }
                            },
                            required = new[]
                            {
                            "questionOrder",
                            "score",
                            "strengths",
                            "mistakes",
                            "improvements",
                            "betterPhrase"
                        }
                        }
                    }
                },
                required = new[]
                {
                "topic",
                "score",
                "questionEvaluations"
            }
            };
        }

        private static object BuildFinalEvaluationSchema()
        {
            return new
            {
                type = "object",
                properties = new
                {
                    overallScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    communicationScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    technicalScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    problemSolvingScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    behavioralScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    roleAlignmentScore = new
                    {
                        type = "integer",
                        minimum = 0,
                        maximum = 100
                    },
                    strongestAreas = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    areasToImprove = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    repeatedMistakes = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    preparationPlan = new
                    {
                        type = "array",
                        items = new { type = "string" }
                    },
                    interviewReadiness = new
                    {
                        type = "string"
                    }
                },
                required = new[]
                {
                "overallScore",
                "communicationScore",
                "technicalScore",
                "problemSolvingScore",
                "behavioralScore",
                "roleAlignmentScore",
                "strongestAreas",
                "areasToImprove",
                "repeatedMistakes",
                "preparationPlan",
                "interviewReadiness"
            }
            };
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private class GeminiResponse
        {
            public List<GeminiCandidate> Candidates { get; set; } = [];
        }

        private class GeminiCandidate
        {
            public GeminiContent Content { get; set; } = new();
        }

        private class GeminiContent
        {
            public List<GeminiPart> Parts { get; set; } = [];
        }

        private class GeminiPart
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}
