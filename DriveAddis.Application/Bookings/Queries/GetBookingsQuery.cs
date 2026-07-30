using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Bookings.Queries;

public record GetBookingsQuery(
    int? StudentId,
    int? InstructorId
) : IRequest<List<BookingListItemDto>>;