using Ascendly.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Resend;

namespace Ascendly.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IResend _resend;

    public EmailService(IConfiguration configuration, IResend resend)
    {
        _configuration = configuration;
        _resend = resend;
    }

    public async Task SendVerificationEmailAsync(
        string email,
        string verificationLink)
    {
        var message = new EmailMessage
        {
            From = _configuration["Resend:FromEmail"]!,
            To = email,
            Subject = "Verify your Ascendly AI account",
            HtmlBody = $"""
                <h2>Welcome to Ascendly AI 🚀</h2>

                <p>Click the button below to verify your email.</p>

                <a href="{verificationLink}"
                   style="
                        background:#4f46e5;
                        color:white;
                        padding:12px 20px;
                        text-decoration:none;
                        border-radius:8px;">
                    Verify Email
                </a>

                <p>This link expires in 24 hours.</p>
                """
        };

        await _resend.EmailSendAsync(message);
    }
}