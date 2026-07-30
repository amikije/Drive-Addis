using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IReviewRepository
{
    Task<Review?> GetByBookingIdAsync(int bookingId, CancellationToken ct);
    Task AddAsync(Review review, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}