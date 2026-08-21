using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class JobRequirementDto
    {
        public string Name { get; set; } = string.Empty;

        // Skill / Experience / Education / Certification / SoftSkill / etc.
        public string Category { get; set; } = string.Empty;

        // Required / Preferred
        public string Importance { get; set; } = string.Empty;

        // Evidence/context from the job description.
        public string Evidence { get; set; } = string.Empty;
    }
}
