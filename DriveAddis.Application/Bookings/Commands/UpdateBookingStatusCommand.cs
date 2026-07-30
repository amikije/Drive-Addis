using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Bookings.Commands;

public record UpdateBookingStatusCommand(
    int BookingId,
    BookingStatus NewStatus
) : IRequest<Result<BookingResponseDto>>;