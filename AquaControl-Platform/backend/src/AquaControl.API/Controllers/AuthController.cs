using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AquaControl.Application.Common.Models;
using AquaControl.Application.Services;

namespace AquaControl.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        try
        {
            var ipAddress = GetIpAddress();
            var (success, user, error) = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

            if (!success || user == null)
            {
                _logger.LogWarning("Login failed for username: {Username} - {Error}", request.Username, error);
                return Unauthorized(new { message = error ?? "Invalid username or password" });
            }

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user, ipAddress);

            var userDto = new UserDto
            {
                Id = user.Id.ToString(),
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = user.Roles,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };

            var response = new LoginResponse(
                accessToken,
                refreshToken,
                userDto,
                DateTime.UtcNow.AddHours(1)
            );

            _logger.LogInformation("Login successful for username: {Username}", request.Username);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return BadRequest(new { message = "Login failed" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogInformation("Token refresh attempt");

        try
        {
            var ipAddress = GetIpAddress();
            var (success, accessToken, refreshToken, error) = await _tokenService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            if (!success)
            {
                _logger.LogWarning("Token refresh failed: {Error}", error);
                return Unauthorized(new { message = error ?? "Token refresh failed" });
            }

            var response = new TokenResponse(
                accessToken!,
                refreshToken!,
                DateTime.UtcNow.AddHours(1)
            );

            _logger.LogInformation("Token refresh successful");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return Unauthorized(new { message = "Token refresh failed" });
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
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest? request = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Logout for user: {UserId}", userId);

        try
        {
            var ipAddress = GetIpAddress();

            // Revoke the specific refresh token if provided
            if (request?.RefreshToken != null)
            {
                await _tokenService.RevokeTokenAsync(request.RefreshToken, ipAddress, "User logout");
            }
            // If no specific token provided, revoke all user tokens for security
            else if (Guid.TryParse(userId, out var userGuid))
            {
                await _tokenService.RevokeAllUserTokensAsync(userGuid, "User logout - all sessions");
            }

            _logger.LogInformation("Logout successful for user: {UserId}", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            // Still return success to avoid revealing internal errors
            return Ok(new { message = "Logged out successfully" });
        }
    }

    private string GetIpAddress()
    {
        // Get IP address from various headers (for proxy scenarios)
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
        {
            ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        }
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        
        return ipAddress ?? "Unknown";
    }
}

