using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class InterviewConfigurationDto
    {
        // Role the candidate wants to practice for.
        public string TargetRole { get; set; } = string.Empty;

        // Optional resume used for personalized questions.
        public string? ResumeText { get; set; }

        // Optional JD used for role-specific questions.
        public string? JobDescription { get; set; }

        // Optional topics chosen by the candidate.
        public List<string> CustomTopics { get; set; } = [];

        // Controls the type of interview Gemini should generate.
        public string InterviewType { get; set; } = "Mixed";

        // Controls question difficulty.
        public string Difficulty { get; set; } = "Intermediate";

        // Requested interview duration in minutes.
        public int DurationMinutes { get; set; }

        // Controls the interviewer's communication style.
        public string InterviewStyle { get; set; } = "Professional";
    }
}
