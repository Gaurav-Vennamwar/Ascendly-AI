using System.ComponentModel;
using System.Text.RegularExpressions;
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
        return "Login Successful";
    }
}