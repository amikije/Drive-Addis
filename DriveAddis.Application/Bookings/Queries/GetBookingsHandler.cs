using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Bookings.Queries;

public class GetBookingsHandler : IRequestHandler<GetBookingsQuery, List<BookingListItemDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public GetBookingsHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<List<BookingListItemDto>> Handle(GetBookingsQuery request, CancellationToken ct)
    {
        var bookings = await _bookingRepository.GetBookingsAsync(request.StudentId, request.InstructorId, ct);

        return bookings.Select(b => new BookingListItemDto
        {
            Id = b.Id,
            StudentId = b.StudentId,
            StudentName = b.Student.FullName,
            InstructorId = b.InstructorId,
            InstructorName = b.Instructor.FullName,
            ScheduledAt = b.ScheduledAt,
            Status = b.Status.ToString()
        }).ToList();
    }
}