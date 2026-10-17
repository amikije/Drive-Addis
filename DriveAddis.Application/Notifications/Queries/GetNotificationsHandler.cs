using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Notifications.Queries;

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var notifications = request.InstructorId.HasValue
            ? await _notificationRepository.GetByInstructorIdAsync(request.InstructorId.Value, ct)
            : request.UserId.HasValue
                ? await _notificationRepository.GetByUserIdAsync(request.UserId.Value, ct)
                : new();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            InstructorId = n.InstructorId,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            RelatedEntityId = n.RelatedEntityId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }
}
