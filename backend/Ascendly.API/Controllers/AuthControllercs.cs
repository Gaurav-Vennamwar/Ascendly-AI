using System.Security.Claims;
using Ascendly.Application.DTOs.Auth;
using Ascendly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ascendly.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    //endpoint to verify the email 
    [HttpPost("request-email-verification")]
    public async Task<IActionResult> RequestEmailVerification(
    RequestEmailVerificationRequest request)
    {
        var result =
            await _authService.RequestEmailVerificationAsync(request);

        if (!result)
        {
            return BadRequest(
                "Email is already verified or could not be processed.");
        }

        return Ok("Verification email sent successfully.");
    }
    //endpoint to register the user service will handle it for us
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result)
        {
            return BadRequest("Email must be verified before creating your account.");
        }

        return Ok("User registered successfully.");
    }
    //endpoint to login the user 
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        // Authentication and token generation happen inside AuthService.
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // HTTP-specific responsibility:
        // Store refresh token in an HttpOnly cookie.
        // JavaScript cannot directly read this cookie.
        Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // Only send the access token to Angular.
        return Ok(new
        {
            result.AccessToken,
            result.ExpiresAt
        });
    }
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
    //refreh token endpoint to refreh rotate the refreh token 
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        //The controller simply delegates the request to the authentication service. The service validates the refresh token, checks whether it's still active, revokes it, generates a new access token and a new refresh token, stores the new refresh token, and returns the new token pair. This is called refresh token rotation because every refresh invalidates the previous refresh token.
        if (result == null)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        return Ok(result);
    }
    //endpoint to logout the user
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Read the refresh token from the HttpOnly cookie.
        // Angular/JavaScript never gets direct access to it.
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("No active refresh token found.");
        }

        // Pass the token to the service.
        // The service remains responsible for DB/revocation logic.
        var result = await _authService.LogoutAsync(
            new LogoutRequest
            {
                RefreshToken = refreshToken
            });

        if (!result)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }

        // Remove the refresh-token cookie from the browser.
        Response.Cookies.Delete("refreshToken");

        return Ok("Logged out successfully.");
    }
    //endpoint to verify the email
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request);

        if (!result)
        {
            return BadRequest("Invalid or expired verification token.");
        }

        return Ok("Email verified successfully.");
    }
}