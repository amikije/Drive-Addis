using MediatR;
using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;

namespace DriveAddis.Application.Bookings.Commands;

public record CreateBookingCommand(
    int InstructorId,
    DateTime ScheduledAt
) : IRequest<Result<BookingResponseDto>>
{
    public int StudentId { get; init; } // set by the controller, not the client
}