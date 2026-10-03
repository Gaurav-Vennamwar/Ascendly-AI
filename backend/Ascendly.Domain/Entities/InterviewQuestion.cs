using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class InterviewQuestion
    {
        public Guid Id { get; set; }

        public Guid InterviewTopicId { get; set; }

        public int Order { get; set; }

        public string Topic { get; set; } = string.Empty;

        public string Question { get; set; } = string.Empty;

        public string Purpose { get; set; } = string.Empty;

        public InterviewTopic InterviewTopic { get; set; } = null!;

        public ICollection<InterviewAnswer> Answers { get; set; } = [];
        public ICollection<QuestionEvaluation> Evaluations { get; set; } = [];
    }
}
