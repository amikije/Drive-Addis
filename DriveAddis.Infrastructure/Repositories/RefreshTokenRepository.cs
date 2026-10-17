using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DriveAddisDbContext _context;

    public RefreshTokenRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, ct);
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken ct)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllUserTokensAsync(int userId, CancellationToken ct)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var t in tokens)
        {
            t.IsRevoked = true;
        }

        _context.RefreshTokens.UpdateRange(tokens);
        await _context.SaveChangesAsync(ct);
    }
}
