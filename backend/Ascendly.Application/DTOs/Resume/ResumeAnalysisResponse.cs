namespace Ascendly.Application.DTOs.Resume;

public class ResumeAnalysisResponse
{
    // Main scores shown on the dashboard.
    public int AtsScore { get; set; }
    public int ResumeMatch { get; set; }
    public int FormattingScore { get; set; }
    public int KeywordMatch { get; set; }

    // Semantic matching details.
    public List<MatchDto> DirectMatches { get; set; } = [];
    public List<TransferableMatchDto> TransferableMatches { get; set; } = [];

    // Missing requirements and blockers.
    public List<GapDto> Gaps { get; set; } = [];

    // Resume writing improvements.
    public HumanizationDto Humanization { get; set; } = new();

    // JD-specific ATS tailoring recommendations.
    public List<AtsTailoringDto> AtsTailoring { get; set; } = [];

    // Final decision and preparation guidance.
    public ApplicationRecommendationDto ApplicationRecommendation { get; set; } = new();

    // Candidate-facing final summary.
    public string FinalRecommendation { get; set; } = string.Empty;
}