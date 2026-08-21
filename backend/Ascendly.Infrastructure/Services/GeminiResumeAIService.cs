using System.Net.Http.Json;
using Ascendly.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ascendly.Application.DTOs.Resume;

namespace Ascendly.Infrastructure.Services;

public class GeminiResumeAIService : IResumeAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GeminiResumeAIService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<SemanticAnalysisDto> AnalyzeSemanticallyAsync(
        string resumeText,
        string jobDescription,
        string structuredAnalysisJson)
    {
        // Read Gemini configuration from appsettings/environment variables.
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-3.1-flash-lite";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "Gemini API key is not configured.");
        }

        // Gemini REST API endpoint.
        var url =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        // Build the complete prompt using:
        // 1. Resume text
        // 2. Job description
        // 3. Our deterministic backend analysis
        var prompt = BuildAnalysisPrompt(
            resumeText,
            jobDescription,
            structuredAnalysisJson);

        // Request body expected by Gemini generateContent API.
        // Gemini request body.
        // responseMimeType tells Gemini:
        // "I want JSON, not normal prose."
        //
        // responseSchema tells Gemini:
        // "This is the exact structure your JSON must follow."
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
                responseSchema = BuildSemanticAnalysisSchema()
            }
        };

        // Send the API key securely through the HTTP header.
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent");

        request.Headers.Add("x-goog-api-key", apiKey);

        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(request);

        // Read the provider response first.
        // This lets us see the actual Gemini error instead of only "400".
        var responseJson = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Gemini API error ({(int)response.StatusCode}): {responseJson}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(
            responseJson,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (geminiResponse?.Candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "Gemini returned no candidates.");
        }

        var jsonText = geminiResponse?
            .Candidates[0]
            .Content
            .Parts[0]
            .Text;

        if (string.IsNullOrWhiteSpace(jsonText))
        {
            throw new InvalidOperationException(
                "Gemini returned an empty analysis.");
        }

        // Convert Gemini's JSON into our strongly typed C# model.
        var analysis = JsonSerializer.Deserialize<SemanticAnalysisDto>(
            jsonText,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (analysis == null)
        {
            throw new InvalidOperationException(
                "Gemini returned an invalid analysis.");
        }

        return analysis;
    }
    private static object BuildSemanticAnalysisSchema()
    {
        return new
        {
            type = "object",

            properties = new
            {
                resumeMatchScore = new
                {
                    type = "object",
                    properties = new
                    {
                        value = new { type = "integer", minimum = 0, maximum = 100
                        },
                        explanation = new { type = "string"
                        }
                    },
                    required = new[] { "value", "explanation" }
                },
                atsScore = new
                {
                    type = "object",
                    properties = new
                    {
                        value = new { type = "integer", minimum = 0, maximum = 100 },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "value", "explanation" }
                },

                formattingScore = new
                {
                    type = "object",
                    properties = new
                    {
                        value = new { type = "integer", minimum = 0, maximum = 100 },
                        explanation = new { type = "string" }
                    },
                    required = new[] { "value", "explanation" }
                },

                keywordMatch = new
                {
                    type = "object",
                    properties = new
                    {
                        score = new
                        {
                            type = "object",
                            properties = new
                            {
                                value = new { type = "integer", minimum = 0, maximum = 100 },
                                explanation = new { type = "string" }
                            },
                            required = new[] { "value", "explanation" }
                        },

                        matchedKeywords = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        },

                        missingKeywords = new
                        {
                            type = "array",
                            items = new { type = "string" }
                        }
                    },
                    required = new[]
                    {
                        "score",
                        "matchedKeywords",
                        "missingKeywords"
                    }
                },

                directMatches = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            jdRequirement = new { type = "string"
                            },
                            resumeEvidence = new { type = "string"
                            },
                            matchType = new
                            {
                                type = "string",
                                @enum = new[] { "DIRECT_MATCH" }
                            }
                        },
                        required = new[]
                        {
                            "jdRequirement",
                            "resumeEvidence",
                            "matchType"
                        }
                    }
                },

                transferableMatches = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            jdRequirement = new { type = "string"
                            },
                            resumeEvidence = new { type = "string"
                            },
                            reasoning = new { type = "string"
                            },
                            matchType = new
                            {
                                type = "string",
                                @enum = new[] { "TRANSFERABLE_MATCH" }
                            }
                        },
                        required = new[]
                        {
                            "jdRequirement",
                            "resumeEvidence",
                            "reasoning",
                            "matchType"
                        }
                    }
                },

                gaps = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            requirement = new { type = "string"
                            },
                            requirementType = new
                            {
                                type = "string",
                                @enum = new[] { "REQUIRED", "PREFERRED" }
                            },
                            gapType = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "GENUINE_GAP",
                                    "CRITICAL_BLOCKER"
                                }
                            },
                            severity = new
                            {
                                type = "string",
                                @enum = new[]
                                {
                                    "HIGH",
                                    "MEDIUM",
                                    "LOW"
                                }
                            },
                            preparationSuggestion = new { type = "string"
                            }
                        },
                        required = new[]
                        {
                            "requirement",
                            "requirementType",
                            "gapType",
                            "severity",
                            "preparationSuggestion"
                        }
                    }
                },

                humanization = new
                {
                    type = "object",
                    properties = new
                    {
                        flaggedSentences = new
                        {
                            type = "array",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    original = new { type = "string"
                                    },

                                    action = new
                                    {
                                        type = "string",
                                        @enum = new[]
                        {
                                            "KEEP",
                                            "IMPROVE",
                                            "REWRITE"
                                        }
                                    },

                                    rewritten = new { type = "string"
                                    },
                                    reason = new { type = "string"
                                    }
                                },
                                required = new[]
                                        {
                                    "original",
                                    "action",
                                    "rewritten",
                                    "reason"
                                }
                            }
                        }
                    },
                    required = new[]
                            {
                        "flaggedSentences"
                    }
                },

                atsTailoring = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            jdRequirement = new { type = "string"
                            },

                            status = new
                            {
                                type = "string",
                                @enum = new[]
                {
                                    "ALREADY_COVERED",
                                    "SAFE_TO_ENHANCE",
                                    "NOT_SUPPORTED",
                                    "TRANSFERABLE"
                                }
                            },

                            resumeEvidence = new { type = "string"
                            },

                            resumeSection = new { type = "string"
                            },

                            recommendation = new { type = "string"
                            },

                            suggestedTerminology = new
                            {
                                type = "array",
                                items = new { type = "string"
                                }
                            }
                        },

                        required = new[]
        {
                            "jdRequirement",
                            "status",
                            "resumeEvidence",
                            "resumeSection",
                            "recommendation",
                            "suggestedTerminology"
                        }
                    }
                },




                resumeImprovements = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            section = new { type = "string"
                            },
                            issue = new { type = "string"
                            },
                            suggestion = new { type = "string"
                            }
                        },
                        required = new[]
                        {
                            "section",
                            "issue",
                            "suggestion"
                        }
                    }
                },

                applicationRecommendation = new
                {
                    type = "object",
                    properties = new
                    {
                        decision = new
                        {
                            type = "string",
                            @enum = new[]
                            {
                                "APPLY",
                                "APPLY_WITH_PREPARATION",
                                "LOW_PRIORITY"
                            }
                        },

                        confidence = new
                        {
                            type = "string",
                            @enum = new[]
                            {
                                "HIGH",
                                "MEDIUM",
                                "LOW"
                            }
                        },

                        reasoning = new { type = "string"
                        },

                        preparationPlan = new
                        {
                            type = "array",
                            items = new { type = "string"
                            }
                        }
                    },
                    required = new[]
                    {
                        "decision",
                        "confidence",
                        "reasoning",
                        "preparationPlan"
                    }
                },

                finalRecommendation = new
                {
                    type = "string"
                }
            },

            required = new[]
            {
                "resumeMatchScore",
                "atsScore",
                "formattingScore",
                "keywordMatch",
                "directMatches",
                "transferableMatches",
                "gaps",
                "humanization",
                "atsTailoring",
                "resumeImprovements",
                "applicationRecommendation",
                "finalRecommendation"
            }
        };
    }

    private static string BuildAnalysisPrompt(
        string resumeText,
        string jobDescription,
        string structuredAnalysisJson)
    {
        return $$"""
        You are Ascendly AI's career intelligence engine.

        Your job is to analyze a candidate's resume against a target job
        description and provide evidence-based career intelligence.

        IMPORTANT:
        Ascendly AI is role-agnostic. The candidate may belong to any career
        domain including technology, finance, marketing, HR, design,
        engineering, healthcare, operations, sales, or other professions.

        You receive exactly three inputs:

        1. RESUME
        2. JOB_DESCRIPTION
        3. BACKEND_ANALYSIS

        ABSOLUTE RULES:

        - Use only evidence present in the supplied resume and job description.
        - Never invent candidate experience, skills, qualifications, employers,
          certifications, achievements, or responsibilities.
        - Never treat every missing keyword as a blocker.
        - Distinguish direct matches from transferable matches.
        - Distinguish genuine gaps from critical blockers.
        - Distinguish REQUIRED requirements from PREFERRED requirements.
        - If evidence is insufficient, say so instead of guessing.
        - Every recommendation must be explainable using supplied evidence.
        - Return ONLY valid JSON.
        - Do not return markdown.
        - Do not return explanations outside the JSON.

        MATCH DEFINITIONS:
                SCORE ANALYSIS:

        Return three additional scores from 0 to 100.

        ATS_SCORE:
        Estimate the resume's overall ATS compatibility for this specific JD.
        Consider:
        - machine-readable structure
        - section completeness
        - JD terminology alignment
        - keyword coverage
        - clarity of resume content
        - absence of obvious parsing risks

        KEYWORD_MATCH_SCORE:
        Measure explicit JD terminology coverage in the resume.
        Do NOT give keyword credit for transferable meaning.
        Return:
        - matchedKeywords
        - missingKeywords
        - score from 0 to 100

        FORMATTING_SCORE:
        Evaluate ATS-friendly resume structure and presentation based only on
        the supplied resume text.
        Consider:
        - standard section structure
        - consistent headings
        - readable bullet structure
        - clear dates/contact information
        - excessive formatting complexity
        - parsing risks

        Do not invent formatting problems that cannot be inferred from the resume.
                IMPORTANT SCORE RULE:

        Scores must be evidence-based and internally consistent.

        Do not choose a score merely because the candidate is a strong or weak
        candidate.

        A high Resume Match Score does not automatically mean a high ATS Score.

        A high Keyword Match Score does not automatically mean a high Resume Match.

        Return all four scores independently.

        DIRECT_MATCH:
        The resume explicitly demonstrates the requirement.

        TRANSFERABLE_MATCH:
        The exact requirement is not demonstrated, but the resume contains
        relevant experience that reasonably transfers to the requirement.
                ════════════════════════════════════════
        NO FALSE TRANSFERABILITY RULE
        ════════════════════════════════════════

        A TRANSFERABLE_MATCH requires evidence that the candidate has demonstrated
        a substantially similar underlying capability.

        A merely related tool, workflow, environment, or professional activity
        is NOT sufficient.

        Examples of potentially valid transferability:

        AWS cloud deployment ↔ Azure cloud deployment
        Django REST APIs ↔ FastAPI REST APIs
        Angular ↔ another modern frontend framework
        SQL Server ↔ PostgreSQL database development

        Examples that are NOT transferable by themselves:

        Git ↔ automated testing
        GitHub ↔ PyTest
        PostgreSQL ↔ Redis
        Django ↔ authorization
        Docker ↔ Kubernetes
        Python ↔ AWS
        Computer Science degree ↔ OOP

        If the underlying capability is not demonstrated, classify it as
        GENUINE_GAP instead of TRANSFERABLE_MATCH.

        When uncertain, prefer GENUINE_GAP.

        GENUINE_GAP:
        The resume does not provide sufficient direct or transferable evidence.

        CRITICAL_BLOCKER:
        A REQUIRED requirement that is a genuine gap and realistically prevents
        the candidate from performing the role.

        CRITICAL BLOCKER RULE:
        If you are uncertain whether something is a blocker, classify it as
        GENUINE_GAP instead.
                TECHNOLOGY-SPECIFIC TRANSFERABILITY:

        A related technology is NOT automatically transferable when the JD
        requires a specific platform, service, framework, or product.

        Examples:

        Azure deployment → Azure Functions
        NOT automatically transferable.

        AWS EC2 → Azure App Service
        Potentially transferable cloud deployment experience.

        Render/Vercel deployment → Azure DevOps
        Potentially transferable deployment workflow experience,
        but NOT evidence of Azure DevOps usage.

        ASP.NET Core → Azure Functions
        NOT automatically transferable unless the resume explicitly demonstrates
        serverless/function-based development.

        Graph API → REST APIs
        REST API experience may provide foundational transferability,
        but do NOT claim Microsoft Graph API experience without explicit evidence.

        When the required technology is specific and no equivalent underlying
        capability is demonstrated, use NOT_SUPPORTED rather than TRANSFERABLE.

        ════════════════════════════════════════
        CAPABILITY EVIDENCE RULE
        ════════════════════════════════════════

        Never infer a candidate capability solely from:

        - a programming language
        - a framework
        - a library
        - a degree
        - a job title
        - a company
        - a project title
        - general industry practice
        - assumptions about what someone "must have learned"

        A capability is supported only when the resume explicitly demonstrates it
        through a skill, responsibility, project, achievement, certification,
        coursework, or other concrete evidence.

        Examples:

        Django does NOT automatically prove:
        - Object-Oriented Programming
        - Testing
        - Authorization
        - CI/CD
        - Redis
        - Celery

        ASP.NET Core does NOT automatically prove:
        - Azure
        - Unit Testing
        - Microservices
        - DevOps
        - Docker

        A Computer Science degree does NOT automatically prove:
        - OOP
        - Data Structures
        - Algorithms
        - Testing
        - Cloud
        - DevOps

        If the capability is not explicitly supported, do not classify it as
        DIRECT_MATCH or TRANSFERABLE_MATCH.
                EXPLICIT EVIDENCE OVERRIDES IMPLIED KNOWLEDGE:

        Do not treat architectural patterns, frameworks, programming languages,
        degrees, or development experience as automatic proof of a separate
        fundamental capability.

        Examples:

        Repository Pattern ≠ explicit proof of OOP
        Dependency Injection ≠ explicit proof of OOP
        C# ≠ explicit proof of OOP
        Django ≠ explicit proof of OOP
        ASP.NET Core ≠ explicit proof of OOP

        Only classify OOP as a match when the resume explicitly demonstrates
        OOP principles or clearly documents object-oriented concepts.

        If the JD requires OOP and the resume does not explicitly demonstrate it,
        classify it as GENUINE_GAP or INSUFFICIENT_EVIDENCE.
        Do not recommend adding "OOP" to the resume merely because the candidate
        uses an object-oriented framework or architecture.


        GAP SEVERITY:

        HIGH:
        Critical blocker or important required gap with no meaningful coverage.

        MEDIUM:
        Required gap with some transferable foundation or a realistic path to close.

        LOW:
        Preferred or nice-to-have gap that is unlikely to independently prevent hiring.

        SEMANTIC MATCHING:

        Evaluate actual capabilities, not just literal keywords.
                AUTHENTICATION VS AUTHORIZATION:

        Authentication and authorization are different capabilities.

        Evidence of authentication does NOT automatically prove authorization.

        For example:

        "Implemented user authentication"
        → supports Authentication

        It does NOT automatically support:
        → Role-Based Authorization
        → Permission Management
        → Access Control

        Authorization should only be considered DIRECT_MATCH or TRANSFERABLE_MATCH
        when the resume explicitly demonstrates authorization, roles, permissions,
        access control, or equivalent capability.

        Example:
        If the JD asks for Azure and the resume demonstrates AWS deployment,
        do not automatically classify Azure as a critical blocker.
        Consider the transferable cloud/deployment experience.
                REQUIREMENT COVERAGE:

        Every meaningful REQUIRED or PREFERRED JD requirement must be accounted for
        in exactly one of these categories:

        DIRECT_MATCH
        TRANSFERABLE_MATCH
        GENUINE_GAP
        CRITICAL_BLOCKER

        Do not silently omit a meaningful JD requirement.

        If the resume does not provide sufficient explicit evidence,
        classify the requirement as GENUINE_GAP.

        Example:
        JD requires OOP principles.
        Resume demonstrates C# and .NET but never explicitly demonstrates OOP.

        Correct:
        OOP → GENUINE_GAP / INSUFFICIENT_EVIDENCE

        Incorrect:
        OOP → DIRECT_MATCH because C# is object-oriented.

        GAP GRANULARITY:

        Each distinct missing requirement MUST be returned as a separate gap object.

        Never combine multiple skills or requirements into one gap.

        Example:
        AWS, CI/CD, PyTest

        MUST become three separate gap objects:
        1. AWS
        2. CI/CD
        3. PyTest

        APPLICATION RECOMMENDATION:

        Choose one:

        APPLY
        APPLY_WITH_PREPARATION
        LOW_PRIORITY

        The recommendation must consider:
        - overall role alignment
        - required qualifications
        - critical blockers
        - transferable experience
        - seniority/experience alignment
        - major genuine gaps

        Do NOT determine the recommendation from a single score alone.
                REQUIREMENT COMPLETENESS:

        Every meaningful qualification or requirement in the JD must be accounted
        for exactly once as:

        DIRECT_MATCH
        TRANSFERABLE_MATCH
        GENUINE_GAP
        CRITICAL_BLOCKER

        Do not silently omit meaningful requirements.

        If there is insufficient resume evidence, use GENUINE_GAP /
        INSUFFICIENT_EVIDENCE rather than making an assumption.

        HUMANIZATION ANALYSIS:

        Analyze the ENTIRE resume before deciding whether any humanization
        improvements are needed.

        Review every meaningful sentence/bullet in:

        - Professional Summary
        - Experience
        - Projects
        - Skills
        - Education
        - Certifications
        - Achievements

        Evaluate each meaningful sentence for:

        1. Generic wording
        2. Vague wording
        3. Repetitive phrasing
        4. Buzzword-heavy language
        5. Weak action statements
        6. Unsupported claims
        7. Unnatural or overly polished phrasing
        8. Lack of clarity
        9. Poor readability
        10. Resume-specific wording that sounds templated

        CLASSIFICATION:

        KEEP:
        The sentence is already clear, specific, factual and natural.

        IMPROVE:
        The sentence contains useful information but can be made clearer,
        more concise or more natural without changing its factual meaning.

        REWRITE:
        The sentence is substantially weak, generic, vague, repetitive or
        unnatural and should be rewritten.

        CRITICAL OUTPUT RULE:

        ONLY return sentences classified as IMPROVE or REWRITE
        inside humanization.flaggedSentences.

        NEVER return KEEP sentences in flaggedSentences.

        If the entire resume is already natural and professional,
        return an EMPTY flaggedSentences array.

        Do NOT create a suggestion merely to produce output.

        For every IMPROVE or REWRITE item:

        - Include the exact original sentence.
        - Preserve its factual meaning exactly.
        - Never invent metrics.
        - Never invent performance improvements.
        - Never invent scale.
        - Never invent users/customers.
        - Never invent technologies.
        - Never invent responsibilities.
        - Never invent leadership.
        - Never invent business impact.
        - Never add unsupported skills.
        - Never strengthen a claim beyond the evidence.

        The rewritten text must be something the candidate could truthfully
        replace the original with immediately.

        Prefer minimal edits when the original sentence is already good.

        IMPORTANT:
        Do not confuse professional language with AI-generated language.

        Words such as:
        "implemented"
        "developed"
        "built"
        "designed"
        "managed"

        are normal professional language and are NOT sufficient evidence
        that text is AI-generated.

        Do NOT attempt to determine whether the resume was written by AI.

        The goal is not AI detection.

        The goal is to identify wording that could make the resume
        more natural, specific, credible and readable.
                STRICT HUMANIZATION THRESHOLD:

        Do not suggest a rewrite merely because a sentence can be shortened.

        Only return IMPROVE or REWRITE when there is a meaningful improvement
        in clarity, specificity, naturalness, readability, or factual precision.

        If the sentence is already strong and professional:
        KEEP it and DO NOT include it in flaggedSentences.

        Removing words such as "designed", "developed", "successful", or similar
        language is not automatically an improvement.
                ════════════════════════════════════════
        HUMANIZATION OUTPUT RULE
        ════════════════════════════════════════

        Only return actionable humanization suggestions.

        KEEP items MUST NOT appear in:
        humanization.flaggedSentences

        If a sentence is already:
        - clear
        - factual
        - concise
        - natural
        - professional

        then do not return it as a suggestion.

        If the entire resume is already well written:

        "flaggedSentences": []

        is the correct result.

        Do NOT create a suggestion merely to produce output.

        MINIMAL-EDIT RULE:

        Prefer the smallest useful improvement.

        Do not replace a strong factual sentence with a more impressive-sounding
        sentence unless the replacement is objectively clearer or more natural.

        Do not introduce stronger verbs when they change the implied meaning.

        For example:

        Original:
        "Designed PostgreSQL queries and improved backend data workflows."

        Do NOT automatically change this to:

        "Optimized backend data retrieval processes."

        unless the resume explicitly supports database optimization or retrieval
        performance work.

        Preserve the factual level of the original sentence.

        A successful humanization analysis may legitimately return zero suggestions.

         Only classify as IMPROVE/REWRITE when the change produces
        a meaningful improvement in clarity, concision, specificity,
        or credibility.

        If the improvement is merely stylistic or subjective:
        KEEP.

        ════════════════════════════════════════
        ATS TAILORING ANALYSIS
        ════════════════════════════════════════

        Analyze the resume specifically against the supplied JOB_DESCRIPTION.

        The goal is to improve the candidate's ATS compatibility and recruiter readability
        WITHOUT fabricating experience.

        For every important JD requirement that can be safely addressed:

        1. Identify the JD requirement.
        2. Determine whether the resume already covers it.
        3. Identify the exact resume evidence.
        4. Identify the resume section where an improvement could be made.
        5. Recommend a safe, natural tailoring action.
        6. Identify relevant JD terminology that could be naturally incorporated
           ONLY when the resume evidence supports it.

        Classify tailoring opportunities as:

        ALREADY_COVERED:
        The resume already contains sufficient evidence and terminology.

        SAFE_TO_ENHANCE:
        The resume contains relevant evidence, but the wording could better reflect
        the terminology or responsibility used in the JD.

        NOT_SUPPORTED:
        The JD requirement is not supported by the resume.
        Do NOT recommend adding the requirement as if the candidate had experience.

        TRANSFERABLE:
        The resume demonstrates related experience that can be emphasized,
        but the exact JD technology/tool/domain is not demonstrated.

        IMPORTANT ATS RULES:

        IMPORTANT ATS RULES:

        - Never keyword stuff.
        - Never repeat the same keyword unnaturally.
        - Never insert a JD keyword simply because it improves ATS matching.
        - Never add a technology, certification, responsibility, metric, achievement,
          or capability that is not supported by the resume.
        - Never convert a genuine gap into a fake match.
        - Prefer natural placement inside an existing relevant bullet or section.
        - Preserve factual accuracy over keyword coverage.
        - Exact JD terminology may be recommended ONLY when the underlying capability
          is explicitly supported by the resume.
        - If the exact JD term is absent but the underlying capability is explicitly
          demonstrated, classify the opportunity as SAFE_TO_ENHANCE or TRANSFERABLE
          as appropriate.
        - If neither the exact term nor the underlying capability is supported,
          classify it as NOT_SUPPORTED.

        For NOT_SUPPORTED requirements:

        - Do NOT recommend adding the requirement to the resume.
        - Do NOT recommend adding related keywords merely for ATS purposes.
        - Do NOT invent an experience statement.
        - Do NOT recommend implying experience the candidate does not have.
        - The recommendation should explicitly state:
          "Do not claim this experience."

        SUGGESTED TERMINOLOGY RULE:

        Suggested terminology may contain ONLY words or phrases whose underlying
        meaning is already supported by explicit resume evidence.

        Do not suggest terminology simply because it appears in the JD.

        Example:

        JD:
        "PyTest"

        Resume:
        No testing evidence.

        Correct:
        status = NOT_SUPPORTED
        suggestedTerminology = []

        Incorrect:
        status = NOT_SUPPORTED
        suggestedTerminology = ["Testing", "Quality Assurance"]

        The purpose of ATS tailoring is to maximize truthful alignment with the JD,
        not to manipulate the ATS or misrepresent the candidate.
                ATS TAILORING COVERAGE:

        Evaluate the JD requirement-by-requirement.

        Do not return ATS tailoring for only a small subset of requirements
        unless the remaining requirements are genuinely irrelevant to resume
        wording or cannot be safely addressed.

        For each meaningful JD requirement, determine one of:

        ALREADY_COVERED
        SAFE_TO_ENHANCE
        TRANSFERABLE
        NOT_SUPPORTED

        NOT_SUPPORTED requirements must still be included when they are important
        to the target role, so the candidate can clearly understand what must not
        be claimed and what remains a gap.
                CLAIM VS LEARNING DISTINCTION:

        If a JD requirement is missing, distinguish between:

        1. Safe resume tailoring:
           The resume already demonstrates the capability and the wording can be
           aligned more closely with the JD.

        2. Learning recommendation:
           The candidate should learn or practice the missing technology.

        Never convert a learning recommendation into a resume claim.

        Example:

        Azure Functions missing:
        Recommendation → Learn Azure Functions.
        Resume claim → Do NOT add Azure Functions.

        CI/CD missing but deployment/Git experience exists:
        Recommendation → Highlight deployment workflow and learn CI/CD.
        Resume claim → Do NOT claim automated CI/CD unless explicitly implemented.
                TRUTHFUL KEYWORD RULE:

        Do not recommend adding a JD keyword merely because the candidate's existing
        technology or architecture implies familiarity with it.

        Example:

        JD: Object-Oriented Programming
        Resume: C#, Repository Pattern, Dependency Injection

        Do NOT recommend adding "Object-Oriented Programming" unless the resume
        explicitly supports that claim.

        In such a case:
        status = NOT_SUPPORTED
        suggestedTerminology = []
                ════════════════════════════════════════
        APPLICATION RECOMMENDATION
        ════════════════════════════════════════

        Determine whether the candidate should apply to this specific job.

        Choose exactly one:

        APPLY
        APPLY_WITH_PREPARATION
        LOW_PRIORITY

        APPLY:
        Use when the candidate demonstrates the majority of core requirements,
        has no critical blockers, and the role is realistically aligned with the
        candidate's experience level.

        APPLY_WITH_PREPARATION:
        Use when the candidate has meaningful alignment but has one or more
        important gaps that can realistically be addressed through focused
        preparation.

        LOW_PRIORITY:
        Use when there is substantial mismatch, multiple critical blockers,
        a major seniority gap, or the candidate lacks the core capabilities
        required to perform the role.

        IMPORTANT:

        Do NOT determine the decision from Resume Match Score alone.

        Consider together:
        - core required capabilities
        - required qualifications
        - direct matches
        - transferable matches
        - genuine gaps
        - critical blockers
        - seniority and experience level
        - domain alignment
        - evidence quality

        A high Resume Match Score must NOT override a clearly mandatory
        qualification that the candidate does not possess.

        The confidence field must reflect how strong the evidence is:

        HIGH:
        The resume contains clear evidence for the major decision factors.

        MEDIUM:
        The decision is reasonable but some important information is limited.

        LOW:
        The available resume/JD evidence is insufficient for a confident decision.

        The reasoning must explicitly explain:
        - why the candidate should or should not apply
        - the strongest evidence supporting the decision
        - the most important gaps or risks

        PREPARATION PLAN:

        Only include preparation items that are relevant to meaningful gaps.

        Order preparation items by priority.

        Do NOT create a preparation item for a requirement that is already
        strongly demonstrated.

        Do NOT recommend learning something solely because it appears as a
        preferred requirement when it has little impact on the application.

        Do NOT recommend fabricated resume claims as preparation.
                ════════════════════════════════════════
        FINAL RECOMMENDATION
        ════════════════════════════════════════

        Produce a concise, actionable final recommendation for the candidate.

        The final recommendation must synthesize:
        - Resume Match
        - strongest direct matches
        - strongest transferable matches
        - important genuine gaps
        - critical blockers
        - ATS tailoring opportunities
        - humanization issues when relevant
        - preparation priorities
        - application recommendation

        The final recommendation must answer:

        1. Should the candidate apply?
        2. Why is the candidate a fit or mismatch?
        3. What are the most important gaps?
        4. What should the candidate prepare or improve first?
        5. What should the candidate NOT falsely add to the resume?

        FACTUAL ACCURACY:

        Never claim that the candidate:
        - meets all requirements
        - is an excellent fit
        - is fully qualified
        - has a skill
        - has experience
        - has achieved an outcome

        unless the supplied resume provides sufficient evidence.

        If meaningful gaps exist, acknowledge them.

        If a requirement is NOT_SUPPORTED, do not describe it as satisfied.

        If the candidate has transferable experience, explain the transferability
        instead of presenting it as direct experience.

        The final recommendation should be concise enough for a candidate to
        understand immediately, but specific enough to support an application
        decision.

        Do not repeat the entire analysis.

        Do not mention internal model reasoning.

        Return the recommendation inside:
        applicationRecommendation
        and
        finalRecommendation
        according to the response schema.
                OUTPUT REQUIREMENTS:

        The response MUST include:

        - resumeMatchScore
        - directMatches
        - transferableMatches
        - gaps
        - humanization
        - atsTailoring
        - resumeImprovements
        - applicationRecommendation
        - finalRecommendation

        applicationRecommendation must contain:
        - decision
        - confidence
        - reasoning
        - preparationPlan

        finalRecommendation must contain the final concise candidate-facing summary.
        ════════════════════════════════════════
        APPLICATION SAFETY PRINCIPLE
        ════════════════════════════════════════

        Ascendly must optimize for:

        TRUTHFUL ATS COMPATIBILITY
        +
        RECRUITER READABILITY
        +
        REAL ROLE ALIGNMENT

        Never optimize the ATS score at the expense of factual accuracy.

        A candidate should never be advised to claim experience they do not have.

         CRITICAL FACT-PRESERVATION RULE:

        A rewrite must preserve exactly the factual meaning of the original text.

        Do NOT add or imply:
        - metrics
        - performance improvements
        - efficiency gains
        - scale
        - users/customers
        - business impact
        - leadership
        - ownership
        - architectural decisions

        unless that information is explicitly present in the original resume evidence.

        If the original sentence is already factual and concise, prefer a minimal
        rewrite or return no suggestion.

        Never make a resume claim stronger simply to make it sound impressive.

        SCORE RESPONSIBILITY:

        ATS score, formatting score, and keyword match score are primarily
        determined by our backend deterministic analysis.

        Do not invent arbitrary values for those scores.

        Resume match is the semantic fit between the candidate and the role.

        The backend analysis provided below contains objective signals.
        Use them as evidence rather than overriding them without justification.

        RESUME:
        {{resumeText}}

        JOB_DESCRIPTION:
        {{jobDescription}}

        BACKEND_ANALYSIS:
        {{structuredAnalysisJson}}

        Return JSON matching the required response structure exactly.
        """;
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