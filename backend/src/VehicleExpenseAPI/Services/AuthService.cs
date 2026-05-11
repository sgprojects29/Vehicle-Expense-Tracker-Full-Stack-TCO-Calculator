using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using VehicleExpenseAPI.DTOs.Auth;
using VehicleExpenseAPI.Models;

namespace VehicleExpenseAPI.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    // Take the RegsiterDto obj and create a new user
    public async Task<(bool Success, string[] Errors)> RegisterAsync(RegisterDto registerDto)
    {
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        // .Net Core Identity handles hashing the password using PBKDF2
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        return (true, Array.Empty<string>());
    }

   // Take the LoginDto and log the user in, generate a JWT token if successful
    public async Task<(bool Success, AuthResponseDto? Response, string Error)> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            return (false, null, "Invalid email or password");
        }

        var token = GenerateJwtToken(user);
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60");

        var response = new AuthResponseDto
        {
            Token = token,
            Email = user.Email!,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
        };

        return (true, response, string.Empty);
    }

    // Take a user ID from the JWT token to get info abvoutr the user currently logged in
    public async Task<UserDto?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            CreatedAt = user.CreatedAt
        };
    }

  //Once user logfged in, generate toekn which will expire in 60 mins
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        // Support both JwtSettings (local) and Jwt__ (Fly.io env vars) - same as Program.cs
        var secretKey = Environment.GetEnvironmentVariable("Jwt__SecretKey")
            ?? jwtSettings["Secret"]
            ?? throw new InvalidOperationException("JWT Secret not configured");

        var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer")
            ?? jwtSettings["Issuer"]
            ?? "VehicleExpenseAPI";

        var audience = Environment.GetEnvironmentVariable("Jwt__Audience")
            ?? jwtSettings["Audience"]
            ?? "VehicleExpenseApp";

        var expirationMinutes = int.Parse(
            Environment.GetEnvironmentVariable("Jwt__ExpirationMinutes")
            ?? jwtSettings["ExpirationInMinutes"]
            ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
