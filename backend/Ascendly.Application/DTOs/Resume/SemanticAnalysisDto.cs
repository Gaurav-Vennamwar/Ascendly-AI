using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
   
    public class SemanticAnalysisDto
    {
        // Gemini's contextual understanding of how the candidate
        // matches the job. This is NOT our final ATS score.
        public ScoreDetailDto ResumeMatchScore { get; set; } = new();
        public ScoreDetailDto AtsScore { get; set; } = new();

        public ScoreDetailDto FormattingScore { get; set; } = new();

        public KeywordMatchAnalysisDto KeywordMatch { get; set; } = new();

        // Direct matches between JD requirements and resume evidence.
        public List<MatchDto> DirectMatches { get; set; } = [];

        // Cases where the candidate has related/transferable experience.
        public List<TransferableMatchDto> TransferableMatches { get; set; } = [];

        // Requirements that are not sufficiently covered by the resume.
        public List<GapDto> Gaps { get; set; } = [];

        // Humanization suggestions for weak/generic resume wording.
        public HumanizationDto Humanization { get; set; } = new();
        //Ats Tailoring
        public List<AtsTailoringDto> AtsTailoring { get; set; } = [];

        // Suggestions for improving the resume itself.
        public List<ResumeImprovementDto> ResumeImprovements { get; set; } = [];

        // Final application recommendation.
        public ApplicationRecommendationDto ApplicationRecommendation { get; set; } = new();

        // Final plain-English recommendation for the candidate.
        public string FinalRecommendation { get; set; } = string.Empty;

    }
    public class KeywordMatchAnalysisDto
    {
        public ScoreDetailDto Score { get; set; } = new();

        public List<string> MatchedKeywords { get; set; } = [];

        public List<string> MissingKeywords { get; set; } = [];
    }

    public class ScoreDetailDto
    {
        // Gemini's contextual match score: 0-100.
        public int Value { get; set; }

        // Why Gemini gave this score.
        public string Explanation { get; set; } = string.Empty;
    }

    public class MatchDto
    {
        // Requirement found in the JD.
        public string JdRequirement { get; set; } = string.Empty;

        // Exact evidence found in the resume.
        public string ResumeEvidence { get; set; } = string.Empty;

        // Always DIRECT_MATCH for this collection.
        public string MatchType { get; set; } = "DIRECT_MATCH";
    }

    public class TransferableMatchDto
    {
        // Requirement from the JD.
        public string JdRequirement { get; set; } = string.Empty;

        // Related experience found in the resume.
        public string ResumeEvidence { get; set; } = string.Empty;

        // Why the experience is transferable.
        public string Reasoning { get; set; } = string.Empty;

        // Always TRANSFERABLE_MATCH for this collection.
        public string MatchType { get; set; } = "TRANSFERABLE_MATCH";
    }

    public class GapDto
    {
        // Missing or insufficiently supported requirement.
        public string Requirement { get; set; } = string.Empty;

        // REQUIRED or PREFERRED.
        public string RequirementType { get; set; } = string.Empty;

        // GENUINE_GAP or CRITICAL_BLOCKER.
        public string GapType { get; set; } = string.Empty;

        // HIGH / MEDIUM / LOW.
        public string Severity { get; set; } = string.Empty;

        // What the candidate should do about the gap.
        public string PreparationSuggestion { get; set; } = string.Empty;
    }

    public class HumanizationDto
    {
        public List<FlaggedSentenceDto> FlaggedSentences { get; set; } = [];
    }
    public class AtsTailoringDto
    {
        public string JdRequirement { get; set; } = string.Empty;

        // ALREADY_COVERED / SAFE_TO_ENHANCE / NOT_SUPPORTED / TRANSFERABLE
        public string Status { get; set; } = string.Empty;

        // Exact evidence from the resume.
        public string ResumeEvidence { get; set; } = string.Empty;

        // Summary / Experience / Skills / Projects etc.
        public string ResumeSection { get; set; } = string.Empty;

        // What Ascendly recommends changing.
        public string Recommendation { get; set; } = string.Empty;

        // JD terminology that can safely be incorporated.
        public List<string> SuggestedTerminology { get; set; } = [];
    }

    public class FlaggedSentenceDto
    {
        // Exact original resume text.
        public string Original { get; set; } = string.Empty;

        // KEEP / IMPROVE / REWRITE
        public string Action { get; set; } = string.Empty;

        // Improved version when applicable.
        public string Rewritten { get; set; } = string.Empty;

        // Why Ascendly classified this sentence this way.
        public string Reason { get; set; } = string.Empty;
    }

    public class ResumeImprovementDto
    {
        // Example: Summary, Experience, Skills.
        public string Section { get; set; } = string.Empty;

        // Problem identified in that section.
        public string Issue { get; set; } = string.Empty;

        // Concrete improvement.
        public string Suggestion { get; set; } = string.Empty;
    }

    public class ApplicationRecommendationDto
    {
        // APPLY / APPLY_WITH_PREPARATION / LOW_PRIORITY.
        public string Decision { get; set; } = string.Empty;

        // HIGH / MEDIUM / LOW.
        public string Confidence { get; set; } = string.Empty;

        // Why the recommendation was made.
        public string Reasoning { get; set; } = string.Empty;

        // Ordered preparation priorities.
        public List<string> PreparationPlan { get; set; } = [];
    }
}
