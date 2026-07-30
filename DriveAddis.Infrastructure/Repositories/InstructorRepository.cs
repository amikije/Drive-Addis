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

    public async Task<Instructor?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Instructors
            .Include(i => i.Vehicles)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }
    public async Task UpdateAverageRatingAsync(int instructorId, CancellationToken ct)
    {
        var instructor = await _context.Instructors
            .Include(i => i.Reviews)
            .FirstOrDefaultAsync(i => i.Id == instructorId, ct);

        if (instructor is null || !instructor.Reviews.Any())
            return;

        instructor.AverageRating = instructor.Reviews.Average(r => r.Rating);
        await _context.SaveChangesAsync(ct);
    }
}