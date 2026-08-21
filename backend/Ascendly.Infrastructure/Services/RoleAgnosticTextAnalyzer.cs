using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Resume;

namespace Ascendly.Infrastructure.Services
{
    public class RoleAgnosticTextAnalyzer
    {
        // These are NOT skills like C#, Python, AWS, etc.
        // These are only common HEADINGS that appear in resumes/JDs.
        // This keeps our analyzer role-agnostic.
        // We don't care whether the resume is for a developer,
        // accountant, designer, HR person, engineer, etc.
        private static readonly HashSet<string> SectionHeadings =
            new(StringComparer.OrdinalIgnoreCase)
            {
            "skills",
            "technical skills",
            "core skills",

            "experience",
            "work experience",
            "professional experience",
            "employment",

            "projects",
            "education",
            "certifications",
            "achievements",

            "summary",
            "professional summary",
            "profile",

            "responsibilities",
            "requirements",
            "qualifications",
            "preferred qualifications",
            "nice to have",

            "what you'll do",
            "what you will do"
            };

       
        // RESUME SIDE

        public List<ResumeEvidenceDto> ExtractResumeEvidence(string resumeText)
        {
            // First we clean the raw text and break it into lines.
            //
            // Example:
            //
            // "Skills\nC#\nAngular\nSQL"
            //
            // becomes:
            //
            // ["Skills", "C#", "Angular", "SQL"]
            var lines = CleanLines(resumeText);

            // This list will eventually contain the things
            // the candidate has shown evidence for.
            var evidence = new List<ResumeEvidenceDto>();

            // Before we find a heading, we don't know which section
            // a line belongs to.
            //
            // So we start with "General".
            string currentSection = "General";

            // Go through the resume one line at a time.
            foreach (var line in lines)
            {
                // Is this line a section heading?
                //
                // Example:
                // "Skills"
                // "Experience"
                // "Education"
                if (IsSectionHeading(line))
                {
                    // If it is a heading, remember that heading.
                    //
                    // Example:
                    // currentSection = "Skills"
                    currentSection = NormalizeHeading(line);

                    // We don't want to treat "Skills" itself
                    // as a candidate skill/evidence item.
                    continue;
                }

                // Ignore tiny/noisy lines.
                //
                // Example:
                // "-"
                // "•"
                // ""
                if (line.Length < 3)
                {
                    continue;
                }

                // This is an actual piece of candidate information.
                //
                // Example:
                // "Built ASP.NET Core REST APIs"
                //
                // We store:
                // Name = the actual line
                // Evidence = which section it came from + line
                // EvidenceStrength = our initial estimate
                evidence.Add(new ResumeEvidenceDto
                {
                    Name = line,

                    // Example:
                    // "Experience: Built ASP.NET Core REST APIs"
                    Evidence = $"{currentSection}: {line}",

                    // Decide whether this looks like strong evidence.
                    EvidenceStrength = DetermineEvidenceStrength(line)
                });
            }

            // Return everything we discovered from the resume.
            return evidence;
        }

        // JOB DESCRIPTION SIDE
       
        public List<JobRequirementDto> ExtractJobRequirements(string jobDescription)
        {
            // Clean the JD and turn it into individual lines.
            var lines = CleanLines(jobDescription);

            // This will contain requirements discovered in the JD.
            var requirements = new List<JobRequirementDto>();

            // Before finding a section heading,
            // treat the section as "General".
            string currentSection = "General";

            // Read every JD line.
            foreach (var line in lines)
            {
                // Check if this line is a section heading.
                if (IsSectionHeading(line))
                {
                    // Remember the current section.
                    currentSection = NormalizeHeading(line);

                    // Don't add the heading itself as a requirement.
                    continue;
                }

                // Ignore empty/tiny lines.
                if (line.Length < 3)
                {
                    continue;
                }

                // Convert this JD line into a generic requirement.
                requirements.Add(new JobRequirementDto
                {
                    // The requirement text itself.
                    //
                    // Example:
                    // "Strong knowledge of C#"
                    Name = line,

                    // Try to understand what kind of requirement it is.
                    //
                    // Example:
                    // Qualification
                    // Responsibility
                    // General
                    Category = DetermineCategory(currentSection, line),

                    // Decide whether it is Required or Preferred.
                    Importance = DetermineImportance(currentSection, line),

                    // Keep the original context.
                    Evidence = $"{currentSection}: {line}"
                });
            }

            // Return all discovered JD requirements.
            return requirements;
        }

       
        // CLEAN RAW TEXT
        

        private static List<string> CleanLines(string text)
        {
            return text

                // Windows PDFs/text can contain \r.
                // Remove it so our lines are cleaner.
                .Replace("\r", "")

                // Break the whole text into separate lines.
                .Split('\n')

                // Remove common bullet characters from the beginning.
                //
                // Example:
                // "• Python"
                //
                // becomes:
                // "Python"
                .Select(line =>
                    Regex.Replace(
                        line,
                        @"^[\s•▪◦\-*]+",
                        "" //remove
                    ).Trim())

                // Remove completely empty lines.
                .Where(line => !string.IsNullOrWhiteSpace(line))

                // Convert everything into a List<string>.
                .ToList();
        }

        // CHECK SECTION HEADING
      
        private static bool IsSectionHeading(string line)
        {
            // Check whether the current line exists
            // inside our generic heading dictionary.
            //
            // StringComparer.OrdinalIgnoreCase means:
            //
            // "Skills"
            // "skills"
            // "SKILLS"
            //
            // are all treated as the same.
            return SectionHeadings.Contains(
                line.Trim().Trim(':'));
        }

        // CLEAN HEADING
        private static string NormalizeHeading(string line)
        {
            // Remove unnecessary spaces and ":".
            //
            // "Skills:"
            //
            // becomes:
            //
            // "Skills"
            return line.Trim().Trim(':');
        }

        // RESUME EVIDENCE STRENGTH

        private static string DetermineEvidenceStrength(string line)
        {
            // These words usually indicate that the resume
            // contains an ACTION rather than just mentioning a word.
            //
            // Example:
            //
            // "ASP.NET Core"
            // → just a mention
            //
            // "Built ASP.NET Core APIs"
            // → actual action/evidence
            var actionWords = new[]
            {
            "built",
            "developed",
            "created",
            "managed",
            "designed",
            "implemented",
            "led",
            "improved",
            "deployed",
            "delivered",
            "worked",
            "achieved"
        };

            // Check whether any action word exists in the line.
            var hasActionWord = actionWords.Any(word =>
                line.Contains(
                    word,
                    StringComparison.OrdinalIgnoreCase));

            // If an action word exists,
            // we consider this stronger evidence.
            if (hasActionWord)
            {
                return "Strong";
            }

            // Otherwise it's just normal evidence for now.
            return "Standard";
        }

        // JD REQUIREMENT CATEGORY


        private static string DetermineCategory(
            string section,
            string line)
        {
            // If the JD section is about requirements
            // or qualifications, classify it as Qualification.
            if (section.Contains(
                    "requirement",
                    StringComparison.OrdinalIgnoreCase) ||
                section.Contains(
                    "qualification",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Qualification";
            }

            // If the JD section talks about responsibilities,
            // classify it as Responsibility.
            if (section.Contains(
                    "responsibil",
                    StringComparison.OrdinalIgnoreCase) ||
                section.Contains(
                    "what you",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Responsibility";
            }

            // If the JD section is "Preferred" or "Nice to Have",
            // classify it as Preferred.
            if (section.Contains(
                    "preferred",
                    StringComparison.OrdinalIgnoreCase) ||
                section.Contains(
                    "nice to have",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Preferred";
            }

            // If we cannot classify it yet,
            // don't guess.
            return "General";
        }

        // JD REQUIREMENT IMPORTANCE

        private static string DetermineImportance(
            string section,
            string line)
        {
            // If it came from Preferred/Nice to Have,
            // it is not a must-have requirement.
            if (section.Contains(
                    "preferred",
                    StringComparison.OrdinalIgnoreCase) ||
                section.Contains(
                    "nice to have",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Preferred";
            }

            // If it came from Requirements/Qualifications,
            // treat it as Required.
            if (section.Contains(
                    "requirement",
                    StringComparison.OrdinalIgnoreCase) ||
                section.Contains(
                    "qualification",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Required";
            }

            // We don't have enough information.
            return "Unclassified";
        }
    }
}
