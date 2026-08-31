using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class QuestionEvaluationDto
    {
        // Identifies which question was evaluated.
        public int QuestionOrder { get; set; }

        // Score for the candidate's answer.
        public int Score { get; set; }

        // What the candidate did well.
        public List<string> Strengths { get; set; } = [];

        // Mistakes or weak points in the answer.
        public List<string> Mistakes { get; set; } = [];

        // What the candidate should improve.
        public List<string> Improvements { get; set; } = [];

        // A more professional way to phrase the candidate's answer.
        public string BetterPhrase { get; set; } = string.Empty;
    }
}
