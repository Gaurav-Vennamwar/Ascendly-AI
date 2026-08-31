using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class InterviewAnswerDto
    {
        // Identifies the question being answered.
        public int QuestionOrder { get; set; }

        // The candidate's answer.
        public string Answer { get; set; } = string.Empty;
    }
}
