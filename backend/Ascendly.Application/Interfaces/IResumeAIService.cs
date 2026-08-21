using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Resume;

namespace Ascendly.Application.Interfaces
{
    public interface IResumeAIService
    {
        // Gemini will receive the structured resume/JD analysis
        // and perform semantic reasoning.
        Task<SemanticAnalysisDto> AnalyzeSemanticallyAsync(
            string resumeText,
            string jobDescription,
            string structuredAnalysisJson);
    }
}
