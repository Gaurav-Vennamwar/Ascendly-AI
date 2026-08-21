//using System.Text.RegularExpressions;
//using Ascendly.Application.DTOs.Resume;

//namespace Ascendly.Infrastructure.Services;

//public class KeywordAnalysisService
//{
//    public KeywordAnalysisDto Analyze(
//        string resumeText,
//        List<JobRequirementDto> jobRequirements)
//    {
//        // Resume text came from PdfPig, so normalize it first.
//        var normalizedResume = Normalize(resumeText);

//        // Job requirements came from RoleAgnosticTextAnalyzer.
//        // Now we extract the actual terms we want to compare.
//        var jobKeywords = ExtractKeywords(jobRequirements);

//        var matchedKeywords = new List<string>();
//        var missingKeywords = new List<string>();

//        // Check every extracted JD keyword against the resume.
//        foreach (var keyword in jobKeywords)
//        {
//            var normalizedKeyword = Normalize(keyword);

//            if (normalizedResume.Contains(
//                    normalizedKeyword,
//                    StringComparison.OrdinalIgnoreCase))
//            {
//                matchedKeywords.Add(keyword);
//            }
//            else
//            {
//                missingKeywords.Add(keyword);
//            }
//        }

//        // Keyword score = matched keywords / total keywords.
//        var score = jobKeywords.Count == 0
//            ? 0
//            : (int)Math.Round(
//                matchedKeywords.Count * 100.0 / jobKeywords.Count);

//        return new KeywordAnalysisDto
//        {
//            JobKeywords = jobKeywords,
//            MatchedKeywords = matchedKeywords,
//            MissingKeywords = missingKeywords,
//            KeywordMatchScore = score
//        };
//    }

//    private static List<string> ExtractKeywords(
//    List<JobRequirementDto> jobRequirements)
//    {
//        var keywords = new HashSet<string>(
//            StringComparer.OrdinalIgnoreCase);

//        foreach (var requirement in jobRequirements)
//        {
//            // Break the requirement into meaningful words.
//            var tokens = Regex.Matches(
//                requirement.Name,
//                @"[A-Za-z][A-Za-z0-9+#.]*")
//                .Select(match => match.Value.Trim());

//            foreach (var token in tokens)
//            {
//                // Ignore common JD words that are not useful keywords.
//                if (IsNoiseWord(token))
//                {
//                    continue;
//                }

//                // Ignore very short words.
//                if (token.Length < 2)
//                {
//                    continue;
//                }

//                keywords.Add(token);
//            }
//        }

//        return keywords.ToList();
//    }

//    private static bool IsNoiseWord(string value)
//    {
//        var noiseWords = new HashSet<string>(
//            StringComparer.OrdinalIgnoreCase)
//    {
//        "about",
//        "the",
//        "job",
//        "we",
//        "are",
//        "looking",
//        "for",
//        "a",
//        "an",
//        "to",
//        "our",
//        "and",
//        "or",
//        "with",
//        "of",
//        "in",
//        "on",
//        "at",
//        "from",
//        "by",
//        "this",
//        "that",
//        "these",
//        "those",

//        "role",
//        "roles",
//        "position",
//        "candidate",
//        "candidates",
//        "team",
//        "teams",
//        "company",
//        "development",
//        "developer",
//        "developers",
//        "experience",
//        "experienced",
//        "knowledge",
//        "understanding",
//        "familiarity",
//        "exposure",
//        "basic",
//        "strong",
//        "good",
//        "working",
//        "skills",
//        "skill",
//        "required",
//        "requirements",
//        "qualifications",
//        "qualification",
//        "responsibilities",
//        "responsibility",

//        "should",
//        "will",
//        "would",
//        "can",
//        "could",
//        "must",
//        "has",
//        "have",
//        "had",
//        "be",
//        "being",
//        "been",

//        "build",
//        "building",
//        "develop",
//        "developing",
//        "maintain",
//        "maintaining",
//        "work",
//        "working",
//        "using",
//        "use",
//        "used",

//        "application",
//        "applications",
//        "software",
//        "systems",
//        "system",
//        "services",
//        "service",

//        "years",
//        "year",
//        "entry",
//        "level",
//        "degree",

//        "please",
//        "apply",
//        "linkedin",
//        "opportunity"
//    };

//        return noiseWords.Contains(value);
//    }

//    private static string Normalize(string text)
//    {
//        return Regex.Replace(
//            text
//                .Trim()
//                .ToLowerInvariant()
//                .Replace("-", " ")
//                .Replace("/", " "),
//            @"\s+",
//            " ");
//    }

//    private static string CleanKeyword(string value)
//    {
//        // Remove common descriptive words around the actual keyword.
//        var cleaned = Regex.Replace(
//            value,
//            @"\b(proficiency in|experience with|experience in|familiarity with|"
//            + @"understanding of|knowledge of|basic knowledge of|strong knowledge of|"
//            + @"exposure to|working knowledge of|hands[- ]on experience with)\b",
//            "",
//            RegexOptions.IgnoreCase);

//        return cleaned
//            .Trim()
//            .Trim('.', ':', ';', '(', ')');
//    }

//    private static bool IsNoisePhrase(string value)
//    {
//        var noisePhrases = new HashSet<string>(
//            StringComparer.OrdinalIgnoreCase)
//    {
//        "strong",
//        "basic",
//        "good",
//        "experience",
//        "knowledge",
//        "understanding",
//        "familiarity",
//        "exposure",
//        "working knowledge",
//        "hands-on",
//        "skills",
//        "applications",
//        "development",
//        "developer",
//        "team",
//        "teams"
//    };

//        return noisePhrases.Contains(value);
//    }

    
//}