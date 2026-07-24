using Ascendly.Application.DTOs.Auth;
using Ascendly.Application.Interfaces;
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
    //endpoint to register the user service will handle it for us
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result)
        {
            return BadRequest("Email already exists.");
        }

        return Ok("User registered successfully.");
    }
    //endpoint to login the user 
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        //all the work is done by the auth service
        var result = await _authService.LoginAsync(request);

        if (result == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        return Ok(result);
    }
}