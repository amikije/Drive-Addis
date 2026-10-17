using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct);
    Task UpdateAsync(RefreshToken token, CancellationToken ct);
    Task RevokeAllUserTokensAsync(int userId, CancellationToken ct);
}
