using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class ResumeEvidenceDto
    {
        // Skill, experience, project, certification, education, etc.
        public string Name { get; set; } = string.Empty;

        // Where/how the candidate demonstrates this.
        public string Evidence { get; set; } = string.Empty;

        // Strong / Partial
        public string EvidenceStrength { get; set; } = string.Empty;
    }
}
