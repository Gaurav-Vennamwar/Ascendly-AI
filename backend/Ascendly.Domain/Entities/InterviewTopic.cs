using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class InterviewTopic
    {
        public Guid Id { get; set; }

        public Guid InterviewSessionId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public string Purpose { get; set; } = string.Empty;

        public InterviewSession InterviewSession { get; set; } = null!;

        public ICollection<InterviewQuestion> Questions { get; set; } = [];
    }
}
