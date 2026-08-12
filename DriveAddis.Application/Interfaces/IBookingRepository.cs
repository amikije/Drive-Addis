using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Booking booking, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task<List<Booking>> GetBookingsAsync(int? studentId, int? instructorId, CancellationToken ct);
    Task<bool> HasConflictingBookingAsync(int instructorId, DateTime scheduledAt, CancellationToken ct);
}