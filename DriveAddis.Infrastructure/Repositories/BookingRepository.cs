using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly DriveAddisDbContext _context;

    public BookingRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Bookings
            .Include(b => b.Student)
            .Include(b => b.Instructor)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct)
    {
        await _context.Bookings.AddAsync(booking, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
    public async Task<List<Booking>> GetBookingsAsync(int? studentId, int? instructorId, CancellationToken ct)
    {
        var query = _context.Bookings
            .Include(b => b.Student)
            .Include(b => b.Instructor)
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(b => b.StudentId == studentId.Value);

        if (instructorId.HasValue)
            query = query.Where(b => b.InstructorId == instructorId.Value);

        return await query
            .OrderByDescending(b => b.ScheduledAt)
            .ToListAsync(ct);
    }
}