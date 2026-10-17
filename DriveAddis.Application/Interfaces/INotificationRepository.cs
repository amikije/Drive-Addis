using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface INotificationRepository
{
    Task<List<Notification>> GetByInstructorIdAsync(int instructorId, CancellationToken ct);
    Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken ct);
    Task<Notification?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task MarkAsReadAsync(int id, CancellationToken ct);
    Task MarkAllAsReadForInstructorAsync(int instructorId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
