using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.DTOs.Resume;
using Ascendly.Application.Interfaces;

namespace Ascendly.Infrastructure.Services
{
    public class ResumeAnalyzerService : IResumeAnalyzerService
    {
        private readonly PdfResumeExtractorService _pdfResumeExtractor;

        public ResumeAnalyzerService(PdfResumeExtractorService pdfResumeExtractor)
        {
            _pdfResumeExtractor = pdfResumeExtractor;
        }

        public async Task<ResumeAnalysisResponse> AnalyzeAsync(
            Stream resumeStream,
            string jobDescription)
        {
            Console.WriteLine("STEP A: AnalyzeAsync started");
            // Extract readable text from the uploaded PDF.
            var resumeText = _pdfResumeExtractor.ExtractText(resumeStream);
            Console.WriteLine("STEP B: PDF extraction completed");

            // Temporary debug point.
            // Later this text will go through our actual analysis pipeline.
            //Console.WriteLine("===== RESUME TEXT =====");
            //Console.WriteLine(resumeText);
            //Console.WriteLine("=======================");
            Console.WriteLine($"Extracted characters: {resumeText.Length}");

            Console.WriteLine("STEP C: Returning temporary response");
            await Task.CompletedTask;

            return new ResumeAnalysisResponse
            {
                AtsScore = 0,
                ResumeMatch = 0,
                FormattingScore = 0,
                KeywordMatch = 0
            };
        }
    }
}
    
