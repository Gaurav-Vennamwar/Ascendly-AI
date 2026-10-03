using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Domain.Entities
{
    public class InterviewSession
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string TargetRole { get; set; } = string.Empty;

        public string? JobDescription { get; set; }

        public string InterviewType { get; set; } = string.Empty;

        public string Difficulty { get; set; } = string.Empty;

        public int DurationMinutes { get; set; }

        public string InterviewStyle { get; set; } = string.Empty;

        public string InterviewerRole { get; set; } = string.Empty;

        public string? InterviewerContext { get; set; }

        public string Status { get; set; } = "IN_PROGRESS";

        public int CurrentTopicIndex { get; set; }

        public int CurrentQuestionIndex { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public User User { get; set; } = null!;

        public ICollection<InterviewTopic> Topics { get; set; } = [];
    }
}
