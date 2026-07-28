using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class InstructorRepository : IInstructorRepository
{
    private readonly DriveAddisDbContext _context;

    public InstructorRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<List<Instructor>> GetAllVerifiedAsync(CancellationToken ct)
    {
        return await _context.Instructors
            .Include(i => i.Vehicles)
            .Where(i => i.IsVerified)
            .ToListAsync(ct);
    }
}