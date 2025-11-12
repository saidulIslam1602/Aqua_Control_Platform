using AquaControl.Domain.Common;

namespace AquaControl.Domain.Aggregates.UserAggregate;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? RevokedReason { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public string? ReplacedByToken { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    // Private constructor for EF
    private RefreshToken() { }

    public RefreshToken(string token, Guid userId, DateTime expiresAt, string createdByIp)
    {
        Id = Guid.NewGuid();
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIp = createdByIp;
        IsRevoked = false;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? revokedByIp = null, string? reason = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
    }
}
