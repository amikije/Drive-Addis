using DriveAddis.Application.Common;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Notifications.Commands;

public class MarkAllNotificationsReadHandler : IRequestHandler<MarkAllNotificationsReadCommand, Result<bool>>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllNotificationsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        await _notificationRepository.MarkAllAsReadForInstructorAsync(request.InstructorId, ct);
        return Result<bool>.Success(true);
    }
}
