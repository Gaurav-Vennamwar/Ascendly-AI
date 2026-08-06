using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;

namespace Ascendly.Infrastructure.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        public RefreshToken Generate(User user)
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };
        }
    }
}
