using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class InterviewQuestionDto
        //questions ka dto hai 
    {
        // Keeps questions in the correct order.
        public int Order { get; set; }

        // Groups the question into an interview topic.
        public string Topic { get; set; } = string.Empty;

        // The actual question shown to the candidate.
        public string Question { get; set; } = string.Empty;

        // Helps the evaluator understand what this question tests.
        public string Purpose { get; set; } = string.Empty;
    }
}
