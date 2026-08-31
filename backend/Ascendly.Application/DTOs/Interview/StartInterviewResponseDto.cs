using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class StartInterviewResponseDto
    {
        // Newly created interview session.
        public Guid SessionId { get; set; }

        // Generated interview topics and questions.
        public List<InterviewTopicDto> Topics { get; set; } = [];

        // First question the frontend should display.
        public int CurrentTopicIndex { get; set; }

        public int CurrentQuestionIndex { get; set; }

        public string Status { get; set; } = "IN_PROGRESS";
    }
}
