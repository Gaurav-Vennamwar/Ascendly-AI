    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using Ascendly.Application.Interfaces;
    using Ascendly.Domain.Entities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;

    namespace Ascendly.Infrastructure.Services
    {
        
            public class JwtService : IJwtService
            {
                private readonly IConfiguration _configuration;

                public JwtService(IConfiguration configuration)
                {
                    _configuration = configuration;
                }

            public string GenerateToken(User user)
            {
                return GenerateJwtToken(user);
            }

                public string GenerateJwtToken(User user)
                {
                    //c;aims users id email and roles as a digitally signed 
                    var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };
                    //the symmetric key from app settings .json
                    var key = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                    var credentials = new SigningCredentials(
                        key,
                        SecurityAlgorithms.HmacSha256);
                    //creating the jwt token
                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        audience: _configuration["Jwt:Audience"],
                        claims: claims,
                        expires: DateTime.UtcNow.AddMinutes(
                            Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
                        signingCredentials: credentials);

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
            }
        }
    
