using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ascendly.Application.DTOs.Resume
{
    public class ResumeMatchResult
    {
        public List<string> ResumeSkills { get; set; } = [];
        public List<string> JobSkills { get; set; } = [];

        public List<string> MatchedSkills { get; set; } = [];
        public List<string> MissingSkills { get; set; } = [];

        public int KeywordMatchPercentage { get; set; }
    }
}
