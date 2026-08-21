    using System.Text.Json;
    using Ascendly.Application.DTOs.Resume;
    using Ascendly.Application.Interfaces;

    namespace Ascendly.Infrastructure.Services;

    public class ResumeAnalyzerService : IResumeAnalyzerService
    {
        private readonly PdfResumeExtractorService _pdfResumeExtractor;
        private readonly RoleAgnosticTextAnalyzer _textAnalyzer;
        private readonly IResumeAIService _resumeAIService;
       
        public ResumeAnalyzerService(
            PdfResumeExtractorService pdfResumeExtractor,
            RoleAgnosticTextAnalyzer textAnalyzer,
            IResumeAIService resumeAIService
           )
        {
            _pdfResumeExtractor = pdfResumeExtractor;
            _textAnalyzer = textAnalyzer;
            _resumeAIService = resumeAIService;
           
        }

        public async Task<ResumeAnalysisResponse> AnalyzeAsync(
            Stream resumeStream,
            string jobDescription)
        {
            // 1. Convert the uploaded PDF into clean resume text.
            var resumeText = _pdfResumeExtractor.ExtractText(resumeStream);

            // 2. Extract generic resume evidence.
            // This does not know anything about .NET, Python, finance, etc.
            var resumeEvidence =
                _textAnalyzer.ExtractResumeEvidence(resumeText);

            // 3. Extract generic requirements from the JD.
            var jobRequirements =
                _textAnalyzer.ExtractJobRequirements(jobDescription);

            // Calculate literal JD keyword coverage.
            

            // 4. Create the internal analysis model.
            var analysis = new ResumeJobAnalysisDto
            {
                JobRequirements = jobRequirements,
                ResumeEvidence = resumeEvidence
            };

            // 5. Convert our deterministic analysis into JSON.
            // Gemini will use this as factual backend context.
            var structuredAnalysisJson = JsonSerializer.Serialize(analysis);

            // 6. Ask Gemini to perform semantic reasoning.
            var aiResponse =
                  await _resumeAIService.AnalyzeSemanticallyAsync(
                          resumeText,
                          jobDescription,
                          structuredAnalysisJson);
        Console.WriteLine("===== SCORES =====");

        Console.WriteLine(
            $"ATS Score: {aiResponse.AtsScore.Value}");

        Console.WriteLine(
            $"Resume Match Score: {aiResponse.ResumeMatchScore.Value}");

        Console.WriteLine(
            $"Formatting Score: {aiResponse.FormattingScore.Value}");

        Console.WriteLine(
            $"Keyword Match Score: {aiResponse.KeywordMatch.Score.Value}");

        Console.WriteLine("==================");
        Console.WriteLine("===== KEYWORD MATCH =====");

        Console.WriteLine(
            $"Matched Keywords: {string.Join(", ", aiResponse.KeywordMatch.MatchedKeywords)}");

        Console.WriteLine(
            $"Missing Keywords: {string.Join(", ", aiResponse.KeywordMatch.MissingKeywords)}");

        Console.WriteLine("=========================");
        Console.WriteLine("============================");

            // Debug the structured AI result.
            Console.WriteLine("===== GEMINI ANALYSIS =====");

            Console.WriteLine(
                $"Resume Match Score: {aiResponse.ResumeMatchScore.Value}");

            Console.WriteLine(
                $"Direct Matches: {aiResponse.DirectMatches.Count}");

            Console.WriteLine(
                $"Transferable Matches: {aiResponse.TransferableMatches.Count}");

            Console.WriteLine(
                $"Gaps: {aiResponse.Gaps.Count}");

            Console.WriteLine(
                $"Application Recommendation: {aiResponse.ApplicationRecommendation.Decision}");

            Console.WriteLine(
                $"Final Recommendation: {aiResponse.FinalRecommendation}");

            Console.WriteLine("===========================");
            Console.WriteLine("===== DIRECT MATCHES =====");

            foreach (var match in aiResponse.DirectMatches)
            {
                Console.WriteLine(
                    $"JD: {match.JdRequirement}");

                Console.WriteLine(
                    $"Resume Evidence: {match.ResumeEvidence}");

                Console.WriteLine();
            }

            Console.WriteLine("===== TRANSFERABLE MATCHES =====");

            foreach (var match in aiResponse.TransferableMatches)
            {
                Console.WriteLine(
                    $"JD: {match.JdRequirement}");

                Console.WriteLine(
                    $"Resume Evidence: {match.ResumeEvidence}");

                Console.WriteLine(
                    $"Reasoning: {match.Reasoning}");

                Console.WriteLine();
            }

            Console.WriteLine("===== GAPS =====");

            foreach (var gap in aiResponse.Gaps)
            {
                Console.WriteLine(
                    $"{gap.Requirement} | " +
                    $"{gap.RequirementType} | " +
                    $"{gap.GapType} | " +
                    $"{gap.Severity}");

                Console.WriteLine(
                    $"Preparation: {gap.PreparationSuggestion}");

                Console.WriteLine();
            }
            Console.WriteLine("===== HUMANIZATION =====");

            Console.WriteLine(
                $"Generic writing detected: {aiResponse.Humanization}");

            foreach (var item in aiResponse.Humanization.FlaggedSentences)
            {
                Console.WriteLine($"Original: {item.Original}");
                Console.WriteLine($"Rewritten: {item.Rewritten}");
                Console.WriteLine($"Reason: {item.Reason}");
                Console.WriteLine();
            }

       

            Console.WriteLine("===== ATS TAILORING =====");

            Console.WriteLine(
                $"ATS Tailoring Items: {aiResponse.AtsTailoring.Count}");

            foreach (var item in aiResponse.AtsTailoring)
            {
                Console.WriteLine($"JD Requirement: {item.JdRequirement}");
                Console.WriteLine($"Status: {item.Status}");
                Console.WriteLine($"Resume Evidence: {item.ResumeEvidence}");
                Console.WriteLine($"Resume Section: {item.ResumeSection}");
                Console.WriteLine($"Recommendation: {item.Recommendation}");

                Console.WriteLine(
                    $"Suggested Terminology: {string.Join(", ", item.SuggestedTerminology)}");

                Console.WriteLine();
            }
        // Temporary response until we connect
        // SemanticAnalysisDto to our final API response.
        // Map Gemini's analysis into the API response used by the frontend.
        return new ResumeAnalysisResponse
        {
            AtsScore = aiResponse.AtsScore.Value,
            ResumeMatch = aiResponse.ResumeMatchScore.Value,
            FormattingScore = aiResponse.FormattingScore.Value,
            KeywordMatch = aiResponse.KeywordMatch.Score.Value,

            DirectMatches = aiResponse.DirectMatches,
            TransferableMatches = aiResponse.TransferableMatches,
            Gaps = aiResponse.Gaps,
            Humanization = aiResponse.Humanization,
            AtsTailoring = aiResponse.AtsTailoring,
            ApplicationRecommendation = aiResponse.ApplicationRecommendation,
            FinalRecommendation = aiResponse.FinalRecommendation
        };
    }
    }