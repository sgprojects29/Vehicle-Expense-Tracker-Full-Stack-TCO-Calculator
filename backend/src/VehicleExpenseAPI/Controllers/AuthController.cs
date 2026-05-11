using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VehicleExpenseAPI.DTOs.Auth;
using VehicleExpenseAPI.Services;

namespace VehicleExpenseAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", registerDto.Email);

        var (success, errors) = await _authService.RegisterAsync(registerDto);

        if (!success)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Errors: {Errors}",
                registerDto.Email, string.Join(", ", errors));
            return BadRequest(new { errors });
        }

        _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);

        // Automatically log in the user after registration
        var loginDto = new LoginDto { Email = registerDto.Email, Password = registerDto.Password };
        var (loginSuccess, response, error) = await _authService.LoginAsync(loginDto);

        if (!loginSuccess)
        {
            _logger.LogWarning("Auto-login failed after registration for email: {Email}", registerDto.Email);
            return Ok(new { message = "User registered successfully but auto-login failed. Please log in manually." });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

        var (success, response, error) = await _authService.LoginAsync(loginDto);

        if (!success)
        {
            _logger.LogWarning("Login failed for email: {Email}", loginDto.Email);
            return Unauthorized(new { error });
        }

        _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetCurrentUser called but no user ID found in token");
            return Unauthorized(new { error = "Invalid token" });
        }

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            return NotFound(new { error = "User not found" });
        }

        return Ok(user);
    }
}
