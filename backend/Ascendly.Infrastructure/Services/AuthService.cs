using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Ascendly.Application.DTOs.Auth;
using Ascendly.Application.Interfaces;
using Ascendly.Domain.Entities;
using Ascendly.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static Ascendly.Infrastructure.Services.JwtService;

namespace Ascendly.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IEmailService _emailService;

    public AuthService(ApplicationDbContext context, IConfiguration configuration, IJwtService jwtService, IRefreshTokenService refreshTokenService, IEmailVerificationService emailVerificationService, IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _emailVerificationService = emailVerificationService;
        _emailService = emailService;
    }
    //service to verify the email before creating the user in short a pending acount 
    //idr apn ne user ka name and email liya then send the user the verification link to thier email
    
    public async Task<bool> RequestEmailVerificationAsync(
    RequestEmailVerificationRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        // If an already verified account exists, don't send another verification.
        if (existingUser != null && existingUser.EmailVerified)
        {
            return false;
        }

        // Create a pending user if this email doesn't exist yet.
        var user = existingUser;

        if (user == null)
        {
            user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = string.Empty,
                Role = "User",
                EmailVerified = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Generate verification token
        var verificationToken = _emailVerificationService.Generate(user);

        _context.EmailVerificationTokens.Add(verificationToken);

        await _context.SaveChangesAsync();

        var verificationLink =
            $"https://ascendlyai.in/verify-email?token={verificationToken.Token}";

        await _emailService.SendVerificationEmailAsync(
            user.Email,
            verificationLink);

        return true;
    }
    //creating a register async service so we check if the email is verified first then register the user
    //idr apn ne check kiya if the the user email is verified or not then only create the user...
    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        // User doesn't exist
        if (user == null)
        {
            return false;
        }

        // Email must be verified first
        if (!user.EmailVerified)
        {
            return false;
        }

        // Don't allow registering an already completed account
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        if (request.Password != request.ConfirmPassword)
        {
            return false;
        }

        // Hash password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(
            request.Password);

        await _context.SaveChangesAsync();

        return true;
    }
    ////register service for the user to register the user 
    //public async Task<bool> RegisterAsync(RegisterRequest request)
    //{
    //    //checking if the email already exists or not 
    //    var existingUser = await _context.Users
    //.FirstOrDefaultAsync(x => x.Email == request.Email);

    //    if (existingUser != null)
    //    {
    //        return false;
    //    }
    //    //hashed the password
    //    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
    //    //creating the user entity 
    //    var user = new User
    //    {
    //        FullName = request.FullName,
    //        Email = request.Email,
    //        PasswordHash = passwordHash,
    //        Role = "User"
    //    };
    //    //saving it 
    //    _context.Users.Add(user);

    //    await _context.SaveChangesAsync();
    //    //generating the token for the email verification 
    //    var verificationToken = _emailVerificationService.Generate(user);

    //    _context.EmailVerificationTokens.Add(verificationToken);

    //    await _context.SaveChangesAsync();
    //    //sendiong the verification link
    //    var verificationLink =
    //         $"https://ascendlyai.in/verify-email?token={verificationToken.Token}";

    //    await _emailService.SendVerificationEmailAsync(
    //        user.Email,
    //        verificationLink);

    //    return true;

    //}
    //login service to login the users
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
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
        var accessToken = _jwtService.GenerateToken(user);

        var refreshToken = _refreshTokenService.Generate(user);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(
              Convert.ToDouble(15))
        };


    }
    //the main game of the refresh token rotaion
    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        //taking the user and the refresh token 
        var oldRefreshToken = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        //checking if the token is null or not
        if (oldRefreshToken == null)
        {
            return null;
        }
        //checking the token is active or not
        if (!oldRefreshToken.IsActive)
        {
            return null;
        }
        //generating the new acess token and the refresh token 
        var newAccessToken = _jwtService.GenerateToken(oldRefreshToken.User);

        var newRefreshToken = _refreshTokenService.Generate(oldRefreshToken.User);

        //revoking the old token and replacing it by the new one
        oldRefreshToken.IsRevoked = true;
        oldRefreshToken.RevokedAt = DateTime.UtcNow;
        oldRefreshToken.ReplacedByToken = newRefreshToken.Token;

        //saving the new refresh token 
        _context.RefreshTokens.Add(newRefreshToken);

        await _context.SaveChangesAsync();

        //returning the access token and the refresh token
        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

    }

    // the logout service to logout the user the only logic is to revoke the user's refresh token and returns a boolean indicating whether the operation succeeded
    public async Task<bool> LogoutAsync(LogoutRequest request)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

        if (refreshToken == null)
        {
            return false;
        }

        if (!refreshToken.IsActive)
        {
            return false;
        }
        //agar refreh token nulll and pahile se hie revoked ny hoga toh then make it :
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        //revoke karke save changes in the db 
        await _context.SaveChangesAsync();

        return true;
    }

    //the email verification service
    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var verificationToken = await _context.EmailVerificationTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == request.Token);

        if (verificationToken == null)
        {
            return false;
        }

        if (verificationToken.IsUsed)//to check already used or not 
        {
            return false;
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            return false;
        }

        verificationToken.User.EmailVerified = true;

        verificationToken.IsUsed = true;

        await _context.SaveChangesAsync();

        return true;
    }

    //shifted to the jwt service 

    //creating jwt token for the users
    //private string GenerateJwtToken(User user)
    //{
    //    //c;aims users id email and roles as a digitally signed 
    //    var claims = new[]
    //    {
    //    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    //    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    //    new Claim(ClaimTypes.Role, user.Role)
    //};
    //    //the symmetric key from app settings .json
    //    var key = new SymmetricSecurityKey(
    //        Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

    //    var credentials = new SigningCredentials(
    //        key,
    //        SecurityAlgorithms.HmacSha256);
    //    //creating the jwt token
    //    var token = new JwtSecurityToken(
    //        issuer: _configuration["Jwt:Issuer"],
    //        audience: _configuration["Jwt:Audience"],
    //        claims: claims,
    //        expires: DateTime.UtcNow.AddMinutes(
    //            Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"])),
    //        signingCredentials: credentials);

    //    return new JwtSecurityTokenHandler().WriteToken(token);
    //}


}