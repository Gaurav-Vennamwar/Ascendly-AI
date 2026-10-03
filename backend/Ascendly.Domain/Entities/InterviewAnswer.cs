using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class InterviewAnswer
    {
        public Guid Id { get; set; }

        public Guid InterviewQuestionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        public InterviewQuestion InterviewQuestion { get; set; } = null!;
    }
}
