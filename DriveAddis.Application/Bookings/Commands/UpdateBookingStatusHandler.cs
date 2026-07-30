using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Bookings.Commands;

public class UpdateBookingStatusHandler : IRequestHandler<UpdateBookingStatusCommand, Result<BookingResponseDto>>
{
    private readonly IBookingRepository _bookingRepository;

    public UpdateBookingStatusHandler(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<BookingResponseDto>> Handle(UpdateBookingStatusCommand request, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, ct);

        if (booking is null)
            return Result<BookingResponseDto>.Failure("Booking not found.");

        if (!IsValidTransition(booking.Status, request.NewStatus))
            return Result<BookingResponseDto>.Failure(
                $"Cannot change booking from '{booking.Status}' to '{request.NewStatus}'.");

        booking.Status = request.NewStatus;
        await _bookingRepository.SaveChangesAsync(ct);

        return Result<BookingResponseDto>.Success(new BookingResponseDto
        {
            Id = booking.Id,
            StudentId = booking.StudentId,
            InstructorId = booking.InstructorId,
            ScheduledAt = booking.ScheduledAt,
            Status = booking.Status.ToString()
        });
    }

    private static bool IsValidTransition(BookingStatus current, BookingStatus next)
    {
        return (current, next) switch
        {
            (BookingStatus.Pending, BookingStatus.Confirmed) => true,
            (BookingStatus.Pending, BookingStatus.Cancelled) => true,
            (BookingStatus.Confirmed, BookingStatus.Completed) => true,
            (BookingStatus.Confirmed, BookingStatus.Cancelled) => true,
            _ => false
        };
    }
}