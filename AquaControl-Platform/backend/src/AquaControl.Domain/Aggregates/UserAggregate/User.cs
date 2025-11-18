using AquaControl.Domain.Common;
using AquaControl.Domain.ValueObjects;

namespace AquaControl.Domain.Aggregates.UserAggregate;

public class User : AggregateRoot<Guid>
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Salt { get; private set; } = string.Empty;
    public string[] Roles { get; private set; } = Array.Empty<string>();
    public bool IsActive { get; private set; } = true;
    public new DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? LastPasswordChangeAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockedUntil { get; private set; }

    // Private constructor for EF
    private User() { }

    public User(
        string username,
        string email,
        string firstName,
        string lastName,
        string passwordHash,
        string salt,
        string[] roles)
    {
        Id = Guid.NewGuid();
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        Salt = salt;
        Roles = roles;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        FailedLoginAttempts = 0;
    }

    public bool VerifyPassword(string password, string providedHash)
    {
        // Compare the provided hash with the stored password hash
        // The providedHash should be computed using the same salt and hashing algorithm
        return PasswordHash == providedHash;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        
        // Lock account after 5 failed attempts for 15 minutes
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
    }

    public bool IsAccountLocked()
    {
        return LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
    }

    public void UnlockAccount()
    {
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void ChangePassword(string newPasswordHash, string newSalt)
    {
        PasswordHash = newPasswordHash;
        Salt = newSalt;
        LastPasswordChangeAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public void DeactivateAccount()
    {
        IsActive = false;
    }

    public void ActivateAccount()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }
}
