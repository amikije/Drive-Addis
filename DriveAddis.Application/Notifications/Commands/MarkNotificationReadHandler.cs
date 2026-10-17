using DriveAddis.Application.Common;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Notifications.Commands;

public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, ct);
        if (notification is null)
            return Result<bool>.Failure("Notification not found.");

        await _notificationRepository.MarkAsReadAsync(request.NotificationId, ct);
        return Result<bool>.Success(true);
    }
}
