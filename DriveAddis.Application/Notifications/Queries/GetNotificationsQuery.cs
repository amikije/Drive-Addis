using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Notifications.Queries;

public record GetNotificationsQuery(int? InstructorId, int? UserId) : IRequest<List<NotificationDto>>;
