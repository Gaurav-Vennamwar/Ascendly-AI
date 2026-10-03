using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class QuestionEvaluation
    {
        public Guid Id { get; set; }


        public Guid InterviewQuestionId { get; set; }

        public int Score { get; set; }

        public List<string> Strengths { get; set; } = [];

        public List<string> Mistakes { get; set; } = [];

        public List<string> Improvements { get; set; } = [];

        public string BetterPhrase { get; set; } = string.Empty;


        public InterviewQuestion InterviewQuestion { get; set; } = null!;
    }
}
