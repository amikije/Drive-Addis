using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Bookings.Commands;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Result<BookingResponseDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IStudentRepository _studentRepository;

    public CreateBookingHandler(
        IBookingRepository bookingRepository,
        IInstructorRepository instructorRepository,
        IStudentRepository studentRepository)
    {
        _bookingRepository = bookingRepository;
        _instructorRepository = instructorRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<BookingResponseDto>> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result<BookingResponseDto>.Failure("Student not found.");

        var instructor = await _instructorRepository.GetByIdAsync(request.InstructorId, ct);
        if (instructor is null)
            return Result<BookingResponseDto>.Failure("Instructor not found.");

        if (!instructor.IsVerified)
            return Result<BookingResponseDto>.Failure("Instructor is not verified.");

        if (request.ScheduledAt <= DateTime.UtcNow)
            return Result<BookingResponseDto>.Failure("Booking time must be in the future.");
        var hasConflict = await _bookingRepository.HasConflictingBookingAsync(
            request.InstructorId, request.ScheduledAt, ct);

        if (hasConflict)
            return Result<BookingResponseDto>.Failure("This instructor is already booked around that time.");
        var booking = new Booking
        {
            StudentId = request.StudentId,
            InstructorId = request.InstructorId,
            ScheduledAt = request.ScheduledAt,
            Status = BookingStatus.Pending
        };

        await _bookingRepository.AddAsync(booking, ct);
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
}