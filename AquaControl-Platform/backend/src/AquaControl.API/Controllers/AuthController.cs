using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AquaControl.Application.Common.Models;

namespace AquaControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        try
        {
            // TODO: Replace with actual user authentication
            // For now, use demo credentials
            if (request.Username == "admin" && request.Password == "admin123")
            {
                var user = new UserDto
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "admin",
                    Email = "admin@aquacontrol.com",
                    FirstName = "Admin",
                    LastName = "User",
                    Roles = new[] { "Administrator", "User" }
                };

                var (accessToken, refreshToken) = GenerateTokens(user);

                var response = new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 3600, // 1 hour
                    User = user
                };

                _logger.LogInformation("Login successful for username: {Username}", request.Username);
                return Task.FromResult<ActionResult<LoginResponse>>(Ok(response));
            }

            _logger.LogWarning("Login failed for username: {Username}", request.Username);
            return Task.FromResult<ActionResult<LoginResponse>>(Unauthorized(new { message = "Invalid username or password" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return Task.FromResult<ActionResult<LoginResponse>>(BadRequest(new { message = "Login failed" }));
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        try
        {
            // TODO: Implement proper refresh token validation
            // For now, generate new tokens
            var user = new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Username = "admin",
                Email = "admin@aquacontrol.com",
                FirstName = "Admin",
                LastName = "User",
                Roles = new[] { "Administrator", "User" }
            };

            var (accessToken, refreshToken) = GenerateTokens(user);

            var response = new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600
            };

            _logger.LogInformation("Token refresh successful");
            return Task.FromResult<ActionResult<TokenResponse>>(Ok(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Task.FromResult<ActionResult<TokenResponse>>(Unauthorized(new { message = "Token refresh failed" }));
        }
    }

    /// <summary>
    /// Validate current token
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult ValidateToken()
    {
        _logger.LogInformation("Token validation for user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return Ok(new { valid = true });
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserDto> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogInformation("Profile request for user: {UserId}", userId);

        var user = new UserDto
        {
            Id = userId ?? Guid.NewGuid().ToString(),
            Username = username ?? "admin",
            Email = email ?? "admin@aquacontrol.com",
            FirstName = "Admin",
            LastName = "User",
            Roles = new[] { "Administrator", "User" }
        };

        return Ok(user);
    }

    /// <summary>
    /// Logout user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Logout for user: {UserId}", userId);

        // TODO: Implement token blacklisting or refresh token revocation
        return Ok(new { message = "Logged out successfully" });
    }

    private (string accessToken, string refreshToken) GenerateTokens(UserDto user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "AquaControl";
        var audience = jwtSettings["Audience"] ?? "AquaControl";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim("jti", Guid.NewGuid().ToString())
        };

        // Add role claims
        var roleClaims = user.Roles.Select(role => new Claim(ClaimTypes.Role, role));
        var allClaims = claims.Concat(roleClaims).ToArray();

        var accessToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: allClaims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var refreshToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, user.Id) },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return (
            new JwtSecurityTokenHandler().WriteToken(accessToken),
            new JwtSecurityTokenHandler().WriteToken(refreshToken)
        );
    }
}

// DTOs
public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public UserDto User { get; init; } = new();
}

public record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
}

public record UserDto
{
    public string Id { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string[] Roles { get; init; } = Array.Empty<string>();
}
