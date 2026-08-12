using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;

namespace Ascendly.Infrastructure.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        //generated the token for the email verification 
        public EmailVerificationToken Generate(User user)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return new EmailVerificationToken
            {
                Token = WebEncoders.Base64UrlEncode(randomBytes),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsUsed = false
            };
        }
    }
}
