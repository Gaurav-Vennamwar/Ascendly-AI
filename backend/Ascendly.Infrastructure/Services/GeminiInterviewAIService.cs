using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Interview;
using Ascendly.Application.Interfaces;
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
            InterviewConfigurationDto configuration, string resumeText)
        {
            var prompt = BuildInterviewGenerationPrompt(configuration,  resumeText);

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
    InterviewConfigurationDto configuration,
    string resumeText)
        {
            return $$"""
You are Ascendly AI's professional mock interview engine.

Your job is to conduct a realistic, structured interview that feels like
an actual human interviewer is evaluating a candidate for the target role.

The interview must be role-agnostic. The candidate may belong to ANY
profession, including technology, business, finance, design, healthcare,
marketing, operations, law, education, sales, or another field.

Do not assume a profession that is not supported by the supplied inputs.

==================================================
CANDIDATE CONTEXT
==================================================

TARGET ROLE:
{{configuration.TargetRole}}

RESUME:
{{resumeText ?? "Not provided"}}

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

INTERVIEWER ROLE:
{{configuration.InterviewerRole}}

INTERVIEWER CONTEXT:
{{configuration.InterviewerContext ?? "Not provided"}}
==================================================
PRIMARY OBJECTIVE
==================================================

Generate the COMPLETE interview blueprint before the interview begins.

The blueprint must contain realistic interview topics and questions that
an interviewer would naturally use to assess the candidate for the
target role.

The interview should feel like a coherent conversation rather than a
random list of questions.

Questions must progress naturally:

1. Establish the candidate's background and context.
2. Understand motivation and role alignment.
3. Assess knowledge and skills relevant to the role.
4. Explore practical experience and decision-making.
5. Deep-dive into relevant resume claims or projects when appropriate.
6. Introduce progressively deeper or more challenging questions.
7. Finish with higher-value questions that distinguish stronger candidates.

Do not make every interview follow this exact sequence.
Adapt the sequence to the profession, interview type, supplied context,
difficulty and duration.

==================================================
INTRODUCTION AND FOUNDATION
==================================================

Unless the selected interview type clearly requires otherwise, begin with
an Introduction and Foundation topic.

This topic should establish the candidate before deeper assessment begins.

Appropriate questions include:

- Tell me about yourself.
- Walk me through your background.
- Tell me about your career journey.
- Why are you interested in this role?
- What attracted you to this opportunity?
- Which parts of your experience are most relevant to this position?

IMPORTANT:

The Introduction and Foundation topic must remain introductory,
professional and contextual.

DO NOT place technical, coding, DSA, system design, domain-specific
knowledge, aptitude or deep project questions inside this topic.

The interviewer should first understand WHO the candidate is before
testing deeper competency.

==================================================
INTERVIEWER CONTEXT
==================================================

Use the selected interviewer role to influence how the interview feels.

INTERVIEWER ROLES MAY INCLUDE:

- Technical Interviewer
- Engineering Manager
- HR Recruiter
- Tech Lead
- CTO
- Founder
- Product Manager
- Recruiter

Adapt question emphasis and interviewing behavior accordingly.

Examples:

Technical Interviewer:
- Focus more heavily on role-specific knowledge, practical application,
  technical reasoning and implementation details where relevant.

Engineering Manager:
- Place greater emphasis on ownership, decision-making, collaboration,
  communication, problem solving and real-world execution.

HR Recruiter:
- Focus more on motivation, communication, career direction, background,
  role alignment and behavioral discussion.

Tech Lead:
- Explore technical depth, engineering decisions, trade-offs,
  practical problem solving and technical ownership where relevant.

CTO / Founder:
- Emphasize high-level ownership, business impact, decision-making,
  adaptability, problem solving and understanding of the bigger picture
  where appropriate to the role.

Product Manager:
- Emphasize product thinking, prioritization, communication, user impact
  and decision-making when relevant to the target role.

Recruiter:
- Focus primarily on background, motivation, communication, experience
  and role alignment.

These are behavioral tendencies, not rigid templates.

Do not force questions that are irrelevant to the target profession.

==================================================
INTERVIEWER IDENTITY / PUBLIC CONTEXT
==================================================

If INTERVIEWER CONTEXT contains a name or publicly available profile
reference, use it only as additional context when reliable information
is actually available.

Never invent an interviewer's:

- company
- job history
- technical expertise
- responsibilities
- personality
- preferences
- interview style
- achievements

If reliable interviewer information is unavailable, simply use the
selected interviewer role.

Do not claim that a real person will definitely ask a particular question.

The interviewer role is a simulation input, not a claim about the actual
person conducting the interview.

==================================================
ROLE ADAPTATION
==================================================

Determine the interview structure from:

- Target Role
- Job Description
- Resume
- Custom Topics
- Interview Type
- Difficulty
- Duration
- Interview Style

Prioritize information in approximately this order:

1. Target role
2. Job description
3. Candidate resume
4. Custom topics
5. General professional interview patterns appropriate to the role

If the JD exists, use it to identify the capabilities and responsibilities
the employer appears to value.

If the JD does not exist, construct a realistic role-based interview using
the target role and available candidate context.

If the resume does not exist, do not create resume-specific questions.

If custom topics exist, incorporate them naturally rather than simply
turning every custom topic into an isolated section.

==================================================
ROLE-AGNOSTIC RULES
==================================================

Never assume the candidate is:

- a software developer
- a programmer
- an engineer
- a technical professional
- a manager
- a salesperson
- or any other profession

unless the supplied context supports it.

Technical questions should appear only when the target role or supplied
context makes them relevant.

For technical roles, technical depth may be substantial.

For non-technical roles, focus on the competencies actually relevant to
that profession.

Do not force:

- coding
- DSA
- system design
- technical questions
- aptitude
- salary questions
- behavioral questions
- case studies
- domain-specific assessments

unless they are relevant to the role or requested interview type.

==================================================
RESUME GROUNDING
==================================================

The resume is evidence about the candidate, not permission to invent facts.

When a resume is provided:

- Inspect projects.
- Inspect work experience.
- Inspect responsibilities.
- Inspect technologies and skills.
- Inspect achievements and measurable claims.
- Inspect education where relevant.
- Inspect domain-specific experience.
- Use actual resume details to create targeted questions.

Every resume-specific question must be supported by information actually
present in the supplied resume.

NEVER invent:

- technologies
- tools
- responsibilities
- architecture
- employers
- clients
- users
- metrics
- revenue
- scale
- team size
- achievements
- certifications
- ownership
- implementation details
- business impact

Do not transform a general skill into a claim of professional experience.

For example, if a resume says:

"Skills: Docker"

you may ask about Docker knowledge.

You may NOT claim:

"You deployed your application using Docker"

unless the resume explicitly supports that.

==================================================
PROJECT DEEP DIVE
==================================================

If the resume contains projects that are relevant to the target role,
include a meaningful project deep-dive.

Project questions should investigate things such as:

- What problem did the project solve?
- Why was the project built?
- What was the candidate's personal contribution?
- What decisions did the candidate make?
- Why were particular approaches chosen?
- What challenges were encountered?
- What trade-offs were considered?
- How was the solution tested or validated?
- What would the candidate improve?
- How would the solution evolve under greater complexity?

Only ask deeper questions when the underlying claim is supported by the
resume.

Do not fabricate implementation details simply to make a project sound
more impressive.

==================================================
JOB DESCRIPTION ALIGNMENT
==================================================

When a job description is supplied:

Identify the major requirements and responsibilities.

Use them to shape the interview naturally.

The interview should test whether the candidate appears capable of
performing the role based on the supplied evidence.

Do not mechanically convert every JD bullet into a question.

Prioritize the requirements most important to the role.

If the candidate appears weak or lacks evidence for a requirement, the
interview may explore that area through a fair question.

Do NOT assume the candidate already possesses a JD requirement merely
because it appears in the JD.

==================================================
CUSTOM TOPICS
==================================================

Custom topics represent areas the candidate specifically wants to practice.

When custom topics are provided:

- Incorporate them into the interview.
- Give them appropriate depth.
- Integrate them with the rest of the interview.
- Do not blindly create one shallow section for every topic.
- Do not ignore them.

If custom topics conflict with the target role, preserve them only when
they can reasonably fit the selected interview context.

==================================================
INTERVIEW REALISM
==================================================

The interview must resemble a real professional interview.

Avoid:

- textbook-style questioning
- repetitive questions
- multiple versions of the same question
- artificial wording
- overly broad questions
- questions that reveal the expected answer
- unnecessary jargon
- random topic switching
- unrealistic difficulty jumps

Questions should sound like something a competent interviewer would
actually ask aloud.

Use a mixture of:

- open-ended questions
- experience-based questions
- role-specific questions
- practical scenario questions
- decision-making questions
- follow-up-style questions
- deeper challenge questions

Do not make every question theoretical.

The interviewer should progressively learn more about the candidate.

==================================================
FOLLOW-UP LOGIC
==================================================

Because the questions are generated before the interview begins, simulate
natural follow-up progression within the blueprint.

For example:

Question 1:
Ask about the candidate's experience.

Question 2:
Explore how they approached something they mentioned.

Question 3:
Challenge the decision or ask about a trade-off.

However, do not create repetitive follow-ups for every question.

==================================================
DIFFICULTY
==================================================

Respect the requested difficulty.

Begin naturally and increase depth where appropriate.

For:

Beginner:
Focus on fundamentals, understanding and straightforward application.

Intermediate:
Test practical knowledge, reasoning and real-world application.

Advanced:
Test deeper reasoning, trade-offs, ambiguity, edge cases, leadership
or ownership where relevant.

Do not make a beginner interview artificially difficult.

==================================================
DURATION
==================================================

The requested duration represents the expected interview depth.

Generate a realistic number of questions for that duration.

Prefer quality over quantity.

Approximately:

15 minutes:
8–10 questions

30 minutes:
12–18 questions

45 minutes:
18–25 questions

60 minutes:
25–35 questions

These are guidelines, not rigid limits.

Longer interviews should generally increase depth rather than simply
repeating similar questions.

==================================================
INTERVIEW TYPE
==================================================

Respect the selected interview type.

Examples:

Technical:
Prioritize relevant technical competency and practical problem solving.

Behavioral:
Prioritize experience, communication, collaboration, ownership,
conflict, adaptability and behavioral evidence.

Mixed:
Combine role knowledge, practical experience and behavioral assessment.

HR:
Prioritize motivation, communication, background, culture alignment and
career-related discussion.

Do not force every possible category into every interview.

==================================================
INTERVIEW STYLE
==================================================

The interviewer style should affect how the interview is structured.

Professional:
Balanced, clear and realistic.

Conversational:
More natural, approachable and discussion-oriented.

Challenging:
More probing, deeper and more demanding follow-up questions.

Supportive:
Professional but less intimidating, while still evaluating the candidate.

==================================================
CURRENT INTERVIEW PATTERNS
==================================================

When current interview patterns are available through an enabled
search/grounding capability, use recent credible public sources to
understand common interview structures and question themes for the target
role.

Use such information only as general guidance.

Do not claim that a specific company asked a specific question unless
reliable evidence supports that claim.

Never fabricate confidential interview information.

==================================================
TOPIC DESIGN
==================================================

Create dynamic topics appropriate to THIS interview.

Do not use a fixed list of topics for every profession.

Topic names should clearly describe what is being assessed.

Each topic must contain:

- name
- order
- purpose
- questions

Questions must contain:

- order
- topic
- question
- purpose

The topic name and question topic should remain logically consistent.

Do not create empty topics.

Do not create duplicate topics.

Do not create duplicate questions.
INTERVIEW QUESTION QUALITY:

Questions must sound natural, conversational and realistic, as if a real
interviewer is speaking directly to the candidate.

Do not turn every resume bullet into a question.

When referring to resume information, use natural phrases such as:
- "You mentioned..."
- "I noticed in your project..."
- "Can you walk me through..."

Do not make assumptions about how the candidate implemented something unless
the supplied resume explicitly supports that detail.

When the JD contains a requirement that the candidate does not claim
experience with, ask about their approach, exposure, or willingness to learn
instead of assuming they already have that experience.

Avoid overly academic, robotic, repetitive, or exam-style questions.

Questions should encourage the candidate to explain reasoning, experience,
decisions, and practical understanding rather than simply recall definitions.
INTERVIEWER REALISM:

The interviewer should ask questions naturally, as if they are speaking
directly to the candidate.

Do not unnecessarily mention the candidate's resume in every question.

When referring to resume information, use natural conversational language
such as:
- "You mentioned..."
- "I noticed in your project..."
- "Can you walk me through..."

Do not turn every resume bullet into a direct question.

Avoid questions that contain their own assumptions about how the candidate
implemented something unless that implementation is explicitly supported
by the resume.

For role requirements that the candidate does not claim experience with,
ask about their approach, exposure, or willingness to learn rather than
assuming prior experience.

==================================================
QUALITY CONTROL
==================================================

Before returning the final result, verify:

1. The interview matches the target role.
2. The interview reflects the JD when provided.
3. Resume-specific questions are grounded in the resume.
4. No candidate facts were invented.
5. Introduction questions are introductory and non-technical.
6. The interview progresses naturally.
7. Questions are not repetitive.
8. Difficulty matches the requested level.
9. Duration is realistic.
10. Custom topics are respected when provided.
11. The interview feels like a real human interview.
12. Topics are dynamic and role-specific.
13. The output is valid JSON only.

==================================================
FINAL OUTPUT
==================================================

Return ONLY valid JSON matching the provided response schema.

Do not return:

- markdown
- explanations
- commentary
- analysis
- code fences
- additional fields outside the schema

The generated blueprint will be used by Ascendly AI to conduct the
interview question-by-question, so question ordering and consistency
are extremely important.
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

                                Evaluate the candidate's performance across ALL questions in one
                                completed interview topic.

                                TARGET ROLE:
                                {{configuration.TargetRole}}

                                TOPIC:
                                {{topic}}

                                QUESTIONS:
                                {{questionJson}}

                                CANDIDATE ANSWERS:
                                {{answerJson}}

                                ==================================================
                                QUESTION EVALUATION
                                ==================================================

                                Evaluate every question individually.

                                For each question return:

                                - questionOrder
                                - score from 0 to 100
                                - strengths
                                - mistakes
                                - improvements
                                - betterPhrase

                                The score must reflect the quality of the candidate's actual answer.

                                Consider:

                                - correctness
                                - relevance
                                - completeness
                                - clarity
                                - structure
                                - reasoning
                                - practical understanding
                                - communication

                                Do not score the candidate based on assumptions about knowledge that
                                is not demonstrated in the answer.

                                ==================================================
                                STRENGTHS
                                ==================================================

                                Identify what was genuinely strong in the candidate's answer.

                                Examples:

                                - clear explanation
                                - relevant experience
                                - logical reasoning
                                - practical understanding
                                - good structure
                                - concise communication

                                Do not invent strengths that are not supported by the answer.

                                ==================================================
                                MISTAKES
                                ==================================================

                                Identify actual weaknesses in the candidate's answer.

                                Examples:

                                - incorrect information
                                - incomplete reasoning
                                - vague explanation
                                - unsupported claim
                                - poor structure
                                - unnecessary filler
                                - failure to answer the question directly

                                Do not label normal professional language as a mistake.

                                Do not invent mistakes that are not present.

                                ==================================================
                                IMPROVEMENTS
                                ==================================================

                                Provide concrete actions the candidate can use to improve future answers.

                                Improvements should be practical and directly connected to the weakness
                                identified in the candidate's answer.

                                Do not tell the candidate to claim experience they do not have.

                                ==================================================
                                BETTER PHRASE
                                ==================================================

                                "betterPhrase" must be a COMPLETE professional model answer that the
                                candidate could learn from and use as a stronger answer.

                                It must be:

                                - natural spoken interview language
                                - complete
                                - clear
                                - well structured
                                - concise but sufficiently detailed

                                It must NOT be:

                                - a fragment
                                - bullet fragments
                                - keywords only
                                - "..."
                                - a placeholder
                                - a rewritten question

                                ==================================================
                                BETTER PHRASE GROUNDING
                                ==================================================

                                BetterPhrase must improve the candidate's actual answer while preserving
                                its factual meaning.

                                You may improve:

                                - structure
                                - clarity
                                - wording
                                - conciseness
                                - professional communication
                                - answer organization

                                You MUST NOT introduce new:

                                - technologies
                                - timelines
                                - employers
                                - responsibilities
                                - achievements
                                - metrics
                                - certifications
                                - project details
                                - ownership claims
                                - implementation details
                                - scale
                                - users
                                - results
                                - experience claims

                                Do not add information merely because it appears in the target role,
                                job description, question, or seems plausible.

                                Do not make the candidate sound more experienced than the evidence supports.

                                If important information is missing from the candidate's answer, improve
                                the answer using only the information actually provided.

                                Never fabricate or infer candidate facts.

                                ==================================================
                                CONSISTENCY
                                ==================================================

                                Every QuestionEvaluation must correspond to the correct questionOrder.

                                Evaluate every supplied question.

                                Do not create evaluations for questions that were not supplied.

                                Do not omit any supplied question.

                                ==================================================
                                FINAL OUTPUT
                                ==================================================

                                Return ONLY valid JSON matching the provided response schema.

                                Do not return:

                                - markdown
                                - explanations
                                - commentary
                                - analysis
                                - topic-level score
                                - additional fields
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

                Evaluate the candidate's complete mock interview using ONLY the
                question-level evaluations provided below.

                TARGET ROLE:
                {{configuration.TargetRole}}

                INTERVIEW TYPE:
                {{configuration.InterviewType}}

                DIFFICULTY:
                {{configuration.Difficulty}}

                INTERVIEWER ROLE:
                {{configuration.InterviewerRole}}

                TOPIC EVALUATIONS:
                {{topicJson}}

                ==================================================
                OVERALL EVALUATION
                ==================================================

                Produce a realistic and evidence-based final assessment.

                Evaluate the candidate only from the performance demonstrated in the
                provided question evaluations.

                Do NOT invent evidence from the target role, job description, resume,
                or general assumptions.

                ==================================================
                SCORES
                ==================================================

                OverallScore:
                One overall score from 0 to 100 representing the candidate's complete
                interview performance.

                CommunicationScore:
                Evaluate communication quality based only on demonstrated answers.

                TechnicalScore:
                Evaluate technical or domain knowledge only when such content was
                actually tested.

                For a non-technical interview, use this dimension for the most relevant
                professional knowledge demonstrated in the interview.

                ProblemSolvingScore:
                Evaluate reasoning, troubleshooting, decision-making and problem-solving
                demonstrated in the interview.

                BehavioralScore:
                Evaluate behavioral and interpersonal performance only when behavioral
                content was actually tested.

                RoleAlignmentScore:
                Evaluate how well the candidate's demonstrated performance aligns with
                the target role.

                Do not score dimensions using assumptions about abilities that were never
                demonstrated.

                ==================================================
                STRONGEST AREAS
                ==================================================

                StrongestAreas:
                List the strongest abilities demonstrated across the interview.

                Use evidence from the question evaluations.

                Do not invent strengths.

                ==================================================
                AREAS TO IMPROVE
                ==================================================

                AreasToImprove:
                List the most important improvement areas based on the actual evaluation
                evidence.

                Prioritize recurring or high-impact weaknesses.

                ==================================================
                REPEATED MISTAKES
                ==================================================

                RepeatedMistakes:
                Identify weaknesses that appeared across multiple questions or topics.

                Only include a mistake as repeated when the supplied evaluations provide
                evidence of recurrence.

                Do not manufacture repeated patterns from a single question.

                ==================================================
                PREPARATION PLAN
                ==================================================

                PreparationPlan:
                Provide prioritized and actionable preparation steps the candidate can
                take before a real interview.

                Recommendations must be based on weaknesses actually demonstrated in
                the interview.

                Do not tell the candidate to claim experience they do not have.

                ==================================================
                INTERVIEW READINESS
                ==================================================

                InterviewReadiness must be exactly one of:

                READY
                READY_WITH_PREPARATION
                NEEDS_PREPARATION

                Choose the value based only on the demonstrated interview performance.

                ==================================================
                IMPORTANT RULES
                ==================================================

                Do not judge the candidate on topics that were never tested.

                Do not invent missing evidence.

                Do not claim professional experience that was not demonstrated.

                Do not infer skills solely because they appear in the target role.

                Do not infer skills solely because they appear in the candidate's resume.

                Do not treat an unanswered or unevaluated area as evidence of weakness.

                Base the final assessment on the provided question evaluations.

                ==================================================
                FINAL OUTPUT
                ==================================================

                Return ONLY valid JSON matching the required response schema.

                Do not return:
                - markdown
                - explanations
                - commentary
                - analysis
                - additional fields
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
