using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class ResumeAnalysisResponse
    {
        public int AtsScore { get; set; }
        public int ResumeMatch { get; set; }
        public int FormattingScore { get; set; }
        public int KeywordMatch { get; set; }
    }
}
