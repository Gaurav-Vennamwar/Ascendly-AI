using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class RequirementMatchDto
    {
        public string Requirement { get; set; } = string.Empty;

        // StrongMatch / PartialMatch / Missing
        public string MatchStatus { get; set; } = string.Empty;

        // Evidence found in the candidate's resume.
        public string ResumeEvidence { get; set; } = string.Empty;

        // Critical / Significant / Minor / None
        public string Severity { get; set; } = string.Empty;
    }
}
