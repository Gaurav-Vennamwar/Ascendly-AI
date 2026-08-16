using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Ascendly.Infrastructure.Services
{
    public class PdfResumeExtractorService
    {
        public string ExtractText(Stream resumeStream)
        {
            try
            {
                // PdfPig attempts to open the actual PDF structure.
                // If the file is corrupt or not a real PDF, this will fail.
                using var document = PdfDocument.Open(resumeStream);

                var text = new StringBuilder();

                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }

                // Raw text extracted from the PDF.
                var rawText = text.ToString();

                // Clean common PDF extraction noise.
                var cleanedText = string.Join(
                    Environment.NewLine,
                    rawText
                        .Split('\n')
                        .Select(line => line.Trim())
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                );

                return cleanedText.Trim();
            }
            catch (Exception ex) when (
                ex is InvalidOperationException ||
                ex is ArgumentException)
            {
                throw new InvalidDataException(
                    "The uploaded file is not a valid readable PDF.", ex);
            }
        }
    }
}