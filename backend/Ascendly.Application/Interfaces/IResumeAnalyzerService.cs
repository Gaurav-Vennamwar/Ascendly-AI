using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Resume;

namespace Ascendly.Application.Interfaces
{
    public interface IResumeAnalyzerService
    {
        // Main entry point for resume analysis.
        // Controller will call this method,
        // while the actual processing stays inside the service.
        Task<ResumeAnalysisResponse> AnalyzeAsync (Stream resumeStream, string jobDescription);
    }
}
