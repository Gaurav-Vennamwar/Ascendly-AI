using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class FinalInterviewEvaluationDto
    {
        // Overall interview performance score.
        public int OverallScore { get; set; }

        // Score breakdown by important interview dimensions.
        public int CommunicationScore { get; set; }
        public int TechnicalScore { get; set; }
        public int ProblemSolvingScore { get; set; }
        public int BehavioralScore { get; set; }
        public int RoleAlignmentScore { get; set; }

        // Areas where the candidate performed strongest.
        public List<string> StrongestAreas { get; set; } = [];

        // Areas that need the most improvement.
        public List<string> AreasToImprove { get; set; } = [];

        // Mistakes or weaknesses repeated across multiple topics.
        public List<string> RepeatedMistakes { get; set; } = [];

        // What the candidate should prepare before a real interview.
        public List<string> PreparationPlan { get; set; } = [];

        // Overall assessment of interview readiness.
        public string InterviewReadiness { get; set; } = string.Empty;
    }
}
