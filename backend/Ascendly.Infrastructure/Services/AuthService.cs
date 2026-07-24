using System.ComponentModel;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Ascendly.Application.DTOs.Auth;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;
using Ascendly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;



using BCrypt.Net;

using Microsoft.IdentityModel.Tokens;

namespace Ascendly.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    //register service for the user to register the user 
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        //checking if the email already exists or not 
        var existingUser = await _context.Users
    .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (existingUser != null)
        {
            return false;
        }
        //hashed the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        //creating the user entity 
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = "User"
        };
        //saving it 
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return true;

    }
    //login service to login the users
    public async Task<string?> LoginAsync(LoginRequest request)
    {
        //checking if the email exist in our db or not
        var user = await _context.Users
    .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
        {
            return null;
        }
        //checking the password is correct or not
        //BCrypt:
        //Reads the salt embedded inside the stored hash.
        //Uses that same salt to hash the password the user just entered.
        //Compares the result with the stored hash.

        //If they match → true.

        //If not → false.
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(
         request.Password,
        user.PasswordHash);

        if (!isPasswordValid)
        {
            return null;
        }
        return GenerateJwtToken(user);
    }
    //creating jwt token for the users
    private string GenerateJwtToken(User user)
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