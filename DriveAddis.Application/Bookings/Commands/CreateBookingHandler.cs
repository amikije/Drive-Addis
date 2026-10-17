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
    private readonly INotificationRepository _notificationRepository;

    // Lesson duration in minutes — single source of truth for conflict checks
    private const int LessonDurationMinutes = 90;

    public CreateBookingHandler(
        IBookingRepository bookingRepository,
        IInstructorRepository instructorRepository,
        IStudentRepository studentRepository,
        INotificationRepository notificationRepository)
    {
        _bookingRepository = bookingRepository;
        _instructorRepository = instructorRepository;
        _studentRepository = studentRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<BookingResponseDto>> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        // 1. Validate student exists
        var student = await _studentRepository.GetByIdAsync(request.StudentId, ct);
        if (student is null)
            return Result<BookingResponseDto>.Failure("Student not found.");

        // 2. Validate instructor exists
        var instructor = await _instructorRepository.GetByIdAsync(request.InstructorId, ct);
        if (instructor is null)
            return Result<BookingResponseDto>.Failure("Instructor not found.");

        // 3. Instructor must be verified
        if (instructor.VerificationStatus != VerificationStatus.Verified)
            return Result<BookingResponseDto>.Failure("Instructor is not verified.");

        // 4. Booking must be at least 2 hours in the future
        if (request.ScheduledAt <= DateTime.UtcNow.AddHours(2))
            return Result<BookingResponseDto>.Failure("Booking must be at least 2 hours in the future.");

        // 5. Booking must not be more than 30 days ahead
        if (request.ScheduledAt > DateTime.UtcNow.AddDays(30))
            return Result<BookingResponseDto>.Failure("Cannot book more than 30 days in advance.");

        // 6. Validate business hours (6 AM — 7 PM East Africa Time, UTC+3)
        var localHour = request.ScheduledAt.AddHours(3).Hour;
        if (localHour < 6 || localHour >= 19)
            return Result<BookingResponseDto>.Failure("Lessons are only available between 6:00 AM and 7:00 PM (EAT).");

        // 7. Check student active bookings limit (Max 3 active bookings: Pending or Confirmed)
        var activeBookings = await _bookingRepository.CountActiveBookingsForStudentAsync(request.StudentId, ct);
        if (activeBookings >= 3)
            return Result<BookingResponseDto>.Failure("You have reached the maximum limit of 3 active bookings. Please complete or cancel existing bookings first.");

        // 8. Check for instructor time conflicts
        var hasConflict = await _bookingRepository.HasConflictingBookingAsync(
            request.InstructorId, request.ScheduledAt, LessonDurationMinutes, ct);

        if (hasConflict)
            return Result<BookingResponseDto>.Failure("This instructor already has a lesson scheduled at that time.");

        // 8. Create the booking
        var booking = new Booking
        {
            StudentId = request.StudentId,
            InstructorId = request.InstructorId,
            ScheduledAt = request.ScheduledAt,
            Status = BookingStatus.Pending
        };

        await _bookingRepository.AddAsync(booking, ct);
        await _bookingRepository.SaveChangesAsync(ct);

        // Notify instructor of new booking request
        await _notificationRepository.AddAsync(new Notification
        {
            InstructorId = request.InstructorId,
            Title = "New Lesson Booking! 📅",
            Message = $"Student {student.FullName} has booked a driving lesson for {booking.ScheduledAt:MMM dd, yyyy 'at' hh:mm tt}. Please review and confirm.",
            Type = "NewBooking",
            RelatedEntityId = booking.Id,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        }, ct);
        await _notificationRepository.SaveChangesAsync(ct);

        return Result<BookingResponseDto>.Success(new BookingResponseDto
        {
            Id = booking.Id,
            StudentId = booking.StudentId,
            StudentName = student.FullName,
            InstructorId = booking.InstructorId,
            InstructorName = instructor.FullName,
            ScheduledAt = booking.ScheduledAt,
            Status = booking.Status.ToString()
        });
    }
}