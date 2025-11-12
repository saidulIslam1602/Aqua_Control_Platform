using AquaControl.Domain.Aggregates.UserAggregate;

namespace AquaControl.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
    Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync();
    Task AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task DeleteAsync(RefreshToken refreshToken);
    Task DeleteExpiredTokensAsync();
    Task DeleteAllUserTokensAsync(Guid userId);
}
