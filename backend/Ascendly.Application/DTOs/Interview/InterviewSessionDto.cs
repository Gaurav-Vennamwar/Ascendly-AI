using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class InterviewSessionDto
    {
        // Identifies the interview session.
        public Guid SessionId { get; set; }

        // Candidate's interview configuration.
        public InterviewConfigurationDto Configuration { get; set; } = new();

        // Topics and questions generated for this interview.
        public List<InterviewTopicDto> Topics { get; set; } = [];

        // Current topic being answered.
        public int CurrentTopicIndex { get; set; }

        // Current question within the topic.
        public int CurrentQuestionIndex { get; set; }

        // Tracks whether the interview is still running.
        public string Status { get; set; } = "IN_PROGRESS";
    }
}
