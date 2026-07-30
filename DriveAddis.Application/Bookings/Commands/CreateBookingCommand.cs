using MediatR;
using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;

namespace DriveAddis.Application.Bookings.Commands;

public record CreateBookingCommand(
    int StudentId,
    int InstructorId,
    DateTime ScheduledAt
) : IRequest<Result<BookingResponseDto>>;