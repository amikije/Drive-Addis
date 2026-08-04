using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DriveAddisDbContext _context;

    public UserRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct)
    {
        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Instructor)
            .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}