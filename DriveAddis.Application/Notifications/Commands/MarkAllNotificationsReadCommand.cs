using DriveAddis.Application.Common;
using MediatR;

namespace DriveAddis.Application.Notifications.Commands;

public record MarkAllNotificationsReadCommand(int InstructorId) : IRequest<Result<bool>>;
