using Ascendly.Application.DTOs.Auth;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;
using Ascendly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Ascendly.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(ApplicationDbContext context)
    {
        _context = context;
    }

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

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }
}