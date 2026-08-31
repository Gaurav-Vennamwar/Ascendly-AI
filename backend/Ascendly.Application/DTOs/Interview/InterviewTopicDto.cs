using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class InterviewTopicDto
    {
        // Identifies the topic.
        public string Name { get; set; } = string.Empty;

        // Controls the order of topics in the interview.
        public int Order { get; set; }

        // Questions generated for this topic.
        public List<InterviewQuestionDto> Questions { get; set; } = [];

        // Explains what this topic is intended to evaluate.
        public string Purpose { get; set; } = string.Empty;
    }
}

