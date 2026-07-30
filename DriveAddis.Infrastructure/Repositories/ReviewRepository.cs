using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly DriveAddisDbContext _context;

    public ReviewRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken ct)
    {
        return await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == bookingId, ct);
    }

    public async Task AddAsync(Review review, CancellationToken ct)
    {
        await _context.Reviews.AddAsync(review, ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}