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
        var clean = phoneNumber.Trim().Replace(" ", "").Replace("-", "");
        var alt = clean;
        if (clean.StartsWith("+251"))
        {
            alt = "0" + clean.Substring(4);
        }
        else if (clean.StartsWith("251"))
        {
            alt = "0" + clean.Substring(3);
        }
        else if (clean.StartsWith("0"))
        {
            alt = "+251" + clean.Substring(1);
        }

        return await _context.Users
            .Include(u => u.Student)
            .Include(u => u.Instructor)
            .FirstOrDefaultAsync(u => u.PhoneNumber == clean || u.PhoneNumber == alt || u.PhoneNumber == phoneNumber, ct);
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