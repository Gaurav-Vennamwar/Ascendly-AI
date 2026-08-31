using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class StartInterviewRequestDto
    {
        // Interview setup selected by the candidate.
        public InterviewConfigurationDto Configuration { get; set; } = new();
    }
}
