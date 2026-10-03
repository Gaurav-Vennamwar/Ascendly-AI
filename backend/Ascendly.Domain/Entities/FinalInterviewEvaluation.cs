using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class FinalInterviewEvaluation
    {
        public Guid Id { get; set; }

        public Guid InterviewSessionId { get; set; }

        public int OverallScore { get; set; }

        public int CommunicationScore { get; set; }

        public int TechnicalScore { get; set; }

        public int ProblemSolvingScore { get; set; }

        public int BehavioralScore { get; set; }

        public int RoleAlignmentScore { get; set; }

        public List<string> StrongestAreas { get; set; } = [];

        public List<string> AreasToImprove { get; set; } = [];

        public List<string> RepeatedMistakes { get; set; } = [];

        public List<string> PreparationPlan { get; set; } = [];

        public string InterviewReadiness { get; set; } = string.Empty;

        public DateTime EvaluatedAt { get; set; }

        public InterviewSession InterviewSession { get; set; } = null!;
    }
}
