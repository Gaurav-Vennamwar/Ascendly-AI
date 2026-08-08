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
                            <div style="margin:0;padding:0;background:#070816;font-family:Arial,Helvetica,sans-serif;color:#eef2ff;">
                              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="background:#070816;padding:48px 16px;">
                                <tr>
                                  <td align="center">
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" border="0" style="max-width:560px;background:#111326;border:1px solid #2a2e52;border-radius:20px;overflow:hidden;">
                                      <tr>
                                        <td style="padding:34px 36px 28px;background:linear-gradient(135deg,#2e1065,#312e81 55%,#0e7490);">
                                          <div style="width:42px;height:42px;line-height:42px;text-align:center;border-radius:12px;background:rgba(255,255,255,0.14);border:1px solid rgba(255,255,255,0.24);font-size:22px;color:#ffffff;">
                                            A
                                          </div>

                                          <p style="margin:22px 0 7px;color:#c4b5fd;font-size:11px;font-weight:700;letter-spacing:1.8px;text-transform:uppercase;">
                                            Ascendly AI
                                          </p>

                                          <h1 style="margin:0;color:#ffffff;font-size:29px;line-height:1.25;font-weight:700;letter-spacing:-0.6px;">
                                            Verify your email address
                                          </h1>
                                        </td>
                                      </tr>

                                      <tr>
                                        <td style="padding:34px 36px 14px;">
                                          <p style="margin:0;color:#cbd5e1;font-size:16px;line-height:1.7;">
                                            Welcome to Ascendly AI. Please confirm your email address to activate your workspace and begin preparing for your next opportunity.
                                          </p>

                                          <table role="presentation" cellspacing="0" cellpadding="0" border="0" style="margin:30px 0;">
                                            <tr>
                                              <td style="border-radius:10px;background:#6366f1;">
                                                <a href="{verificationLink}"
                                                   style="display:inline-block;padding:14px 24px;border-radius:10px;background:linear-gradient(135deg,#8b5cf6,#4f46e5 55%,#0891b2);color:#ffffff;text-decoration:none;font-size:14px;font-weight:700;letter-spacing:0.1px;">
                                                  Verify Email Address →
                                                </a>
                                              </td>
                                            </tr>
                                          </table>

                                          <p style="margin:0;color:#94a3b8;font-size:13px;line-height:1.65;">
                                            This secure verification link expires in <strong style="color:#c4b5fd;">24 hours</strong>.
                                          </p>
                                        </td>
                                      </tr>

                                      <tr>
                                        <td style="padding:22px 36px 30px;">
                                          <div style="height:1px;background:#272b47;margin-bottom:18px;"></div>
                                          <p style="margin:0;color:#64748b;font-size:12px;line-height:1.6;">
                                            If you did not create an Ascendly AI account, you can safely ignore this email.
                                          </p>
                                        </td>
                                      </tr>
                                    </table>

                                    <p style="margin:20px 0 0;color:#475569;font-size:11px;">
                                      © 2026 Ascendly AI · Prepare · Practice · Ascend
                                    </p>
                                  </td>
                                </tr>
                              </table>
                            </div>
                            """
        };

        await _resend.EmailSendAsync(message);
    }
}