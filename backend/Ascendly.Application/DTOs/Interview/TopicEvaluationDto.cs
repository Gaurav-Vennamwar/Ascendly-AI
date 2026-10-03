using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Interview
{
    public class TopicEvaluationDto
    {
        // Identifies the completed interview topic.
        public string Topic { get; set; } = string.Empty;

        //// Overall score for this topic.
        //public int Score { get; set; }

        // Evaluation for every question in this topic.
        public List<QuestionEvaluationDto> QuestionEvaluations { get; set; } = [];
    }
}
