using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class ResumeJobAnalysisDto
    {
        // Requirements extracted from the job description.
        public List<JobRequirementDto> JobRequirements { get; set; } = [];

        // Evidence discovered from the candidate's resume.
        public List<ResumeEvidenceDto> ResumeEvidence { get; set; } = [];

        // Result of comparing JD requirements against resume evidence.
        public List<RequirementMatchDto> RequirementMatches { get; set; } = [];
    }
}
