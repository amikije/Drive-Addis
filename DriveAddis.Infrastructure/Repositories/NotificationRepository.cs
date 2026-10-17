using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly DriveAddisDbContext _context;

    public NotificationRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<List<Notification>> GetByInstructorIdAsync(int instructorId, CancellationToken ct)
    {
        return await _context.Notifications
            .Where(n => n.InstructorId == instructorId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, CancellationToken ct)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Notifications.FindAsync([id], ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }

    public async Task MarkAsReadAsync(int id, CancellationToken ct)
    {
        var notification = await _context.Notifications.FindAsync([id], ct);
        if (notification is not null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadForInstructorAsync(int instructorId, CancellationToken ct)
    {
        var notifications = await _context.Notifications
            .Where(n => n.InstructorId == instructorId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in notifications)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }
}
