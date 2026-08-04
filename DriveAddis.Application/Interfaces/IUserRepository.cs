using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}