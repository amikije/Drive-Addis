using DriveAddis.Application.Common;
using MediatR;

namespace DriveAddis.Application.Notifications.Commands;

public record MarkNotificationReadCommand(int NotificationId) : IRequest<Result<bool>>;
