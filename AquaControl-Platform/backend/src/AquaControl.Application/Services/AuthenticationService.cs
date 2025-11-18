using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using AquaControl.Domain.Aggregates.UserAggregate;
using AquaControl.Application.Common.Interfaces;
using AquaControl.Application.Common.Models;

namespace AquaControl.Application.Services;

public interface IAuthenticationService
{
    Task<(bool Success, User? User, string? Error)> AuthenticateAsync(string username, string password);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(Guid userId);
    string HashPassword(string password, string salt);
    string GenerateSalt();
    Task<bool> CreateDefaultAdminUserAsync();
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<(bool Success, User? User, string? Error)> AuthenticateAsync(string username, string password)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            
            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for username: {Username}", username);
                return (false, null, "Invalid username or password");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Authentication failed: User account is deactivated for username: {Username}", username);
                return (false, null, "Account is deactivated");
            }

            if (user.IsAccountLocked())
            {
                _logger.LogWarning("Authentication failed: Account is locked for username: {Username}", username);
                return (false, null, "Account is temporarily locked due to multiple failed login attempts");
            }

            var hashedPassword = HashPassword(password, user.Salt);
            _logger.LogInformation("Password verification - Stored hash: {StoredHash}, Computed hash: {ComputedHash}, Match: {Match}", 
                user.PasswordHash, hashedPassword, user.PasswordHash == hashedPassword);
            
            if (!user.VerifyPassword(password, hashedPassword))
            {
                user.RecordFailedLogin();
                await _userRepository.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogWarning("Authentication failed: Invalid password for username: {Username}", username);
                return (false, null, "Invalid username or password");
            }

            // Successful authentication
            user.UpdateLastLogin();
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Authentication successful for username: {Username}", username);
            return (true, user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for username: {Username}", username);
            return (false, null, "Authentication service error");
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes);
    }

    public string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    public async Task<bool> CreateDefaultAdminUserAsync()
    {
        _logger.LogInformation("Starting CreateDefaultAdminUserAsync");
        try
        {
            // Check if admin user already exists
            _logger.LogInformation("Checking if admin user already exists");
            var existingAdmin = await _userRepository.GetByUsernameAsync("admin");
            if (existingAdmin != null)
            {
                _logger.LogInformation("Default admin user already exists");
                return true;
            }

            _logger.LogInformation("Admin user not found, creating new admin user");
            // Create default admin user
            var salt = GenerateSalt();
            var passwordHash = HashPassword("Admin123", salt);

            var adminUser = new User(
                username: "admin",
                email: "admin@aquacontrol.com",
                firstName: "Admin",
                lastName: "User",
                passwordHash: passwordHash,
                salt: salt,
                roles: new[] { "Administrator", "User" }
            );

            _logger.LogInformation("Adding admin user to repository");
            await _userRepository.AddAsync(adminUser);
            
            _logger.LogInformation("Saving changes to database");
            var result = await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("SaveChangesAsync returned: {Result}", result);

            _logger.LogInformation("Default admin user created successfully with ID: {UserId}", adminUser.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default admin user: {Message}", ex.Message);
            _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
            return false;
        }
    }
}
