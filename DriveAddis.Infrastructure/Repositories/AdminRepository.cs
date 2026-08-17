using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly DriveAddisDbContext _context;

    public AdminRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct)
    {
        var totalStudents = await _context.Students.CountAsync(ct);
        var totalInstructors = await _context.Instructors.CountAsync(ct);
        var verifiedInstructors = await _context.Instructors.CountAsync(i => i.IsVerified, ct);

        var totalBookings = await _context.Bookings.CountAsync(ct);
        var pending = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending, ct);
        var confirmed = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed, ct);
        var completed = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Completed, ct);
        var cancelled = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled, ct);

        var totalReviews = await _context.Reviews.CountAsync(ct);
        var avgRating = totalReviews > 0
            ? await _context.Reviews.AverageAsync(r => r.Rating, ct)
            : 0;

        return new AdminDashboardDto
        {
            TotalStudents = totalStudents,
            TotalInstructors = totalInstructors,
            VerifiedInstructors = verifiedInstructors,
            UnverifiedInstructors = totalInstructors - verifiedInstructors,
            TotalBookings = totalBookings,
            PendingBookings = pending,
            ConfirmedBookings = confirmed,
            CompletedBookings = completed,
            CancelledBookings = cancelled,
            TotalReviews = totalReviews,
            PlatformAverageRating = avgRating
        };
    }
}