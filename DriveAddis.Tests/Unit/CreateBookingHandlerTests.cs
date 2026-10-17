using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class CreateBookingHandlerTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IInstructorRepository _instructorRepository = Substitute.For<IInstructorRepository>();
    private readonly IStudentRepository _studentRepository = Substitute.For<IStudentRepository>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();

    private CreateBookingHandler CreateSut() =>
        new(_bookingRepository, _instructorRepository, _studentRepository, _notificationRepository);

    [Fact]
    public async Task Handle_StudentNotFound_ReturnsFailureAndNeverWrites()
    {
        // Arrange
        var sut = CreateSut();
        _studentRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        var command = new CreateBookingCommand(InstructorId: 1, ScheduledAt: DateTime.UtcNow.AddDays(2))
        {
            StudentId = 999
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Student not found.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InstructorNotFound_ReturnsFailureAndNeverWrites()
    {
        // Arrange
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(888, Arg.Any<CancellationToken>()).Returns((Instructor?)null);

        var command = new CreateBookingCommand(InstructorId: 888, ScheduledAt: DateTime.UtcNow.AddDays(2))
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Instructor not found.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(VerificationStatus.Pending)]
    [InlineData(VerificationStatus.Rejected)]
    public async Task Handle_InstructorNotVerified_ReturnsFailureAndNeverWrites(VerificationStatus status)
    {
        // Arrange
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        var instructor = TestDataFactory.CreateInstructor(id: 2, verificationStatus: status);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: DateTime.UtcNow.AddDays(2))
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Instructor is not verified.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BookingLessThanTwoHoursInFuture_ReturnsValidationError()
    {
        // Arrange: 1 hour in future (boundary: must be > 2 hours)
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        var instructor = TestDataFactory.CreateInstructor(id: 2, verificationStatus: VerificationStatus.Verified);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: DateTime.UtcNow.AddHours(1))
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Booking must be at least 2 hours in the future.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BookingMoreThanThirtyDaysAhead_ReturnsValidationError()
    {
        // Arrange: 31 days in future (boundary: must be <= 30 days)
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        var instructor = TestDataFactory.CreateInstructor(id: 2, verificationStatus: VerificationStatus.Verified);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: DateTime.UtcNow.AddDays(31))
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot book more than 30 days in advance.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConflictingBookingExists_ReturnsConflictErrorAndNeverWrites()
    {
        // Arrange: valid daytime slot, but instructor already has a scheduled lesson
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        var instructor = TestDataFactory.CreateInstructor(id: 2, verificationStatus: VerificationStatus.Verified);

        // Schedule at 10:00 AM EAT (07:00 UTC) tomorrow
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var targetTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7, 0, 0, DateTimeKind.Utc);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);
        _bookingRepository.HasConflictingBookingAsync(2, targetTime, 90, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: targetTime)
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("This instructor already has a lesson scheduled at that time.", result.Error);
        await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        await _notificationRepository.DidNotReceive().AddAsync(Arg.Any<Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesBookingAndSendsInstructorNotification()
    {
        // Arrange: valid daytime slot (10:00 AM EAT = 07:00 UTC)
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1, fullName: "Almaz Ayana");
        var instructor = TestDataFactory.CreateInstructor(id: 2, fullName: "Haile Gebrselassie", verificationStatus: VerificationStatus.Verified);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var targetTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7, 0, 0, DateTimeKind.Utc);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);
        _bookingRepository.HasConflictingBookingAsync(2, targetTime, 90, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: targetTime)
        {
            StudentId = 1
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Almaz Ayana", result.Value.StudentName);
        Assert.Equal("Haile Gebrselassie", result.Value.InstructorName);
        Assert.Equal("Pending", result.Value.Status);

        // Verify boundary writes
        await _bookingRepository.Received(1).AddAsync(
            Arg.Is<Booking>(b => b.StudentId == 1 && b.InstructorId == 2 && b.Status == BookingStatus.Pending),
            Arg.Any<CancellationToken>());
        await _bookingRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        // Verify notification sent to instructor
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<Notification>(n => n.InstructorId == 2 && n.Type == "NewBooking" && n.IsRead == false),
            Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_BookingRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 1);
        var instructor = TestDataFactory.CreateInstructor(id: 2, verificationStatus: VerificationStatus.Verified);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var targetTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7, 0, 0, DateTimeKind.Utc);

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(instructor);
        _bookingRepository.HasConflictingBookingAsync(2, targetTime, 90, Arg.Any<CancellationToken>())
            .Returns(false);

        _bookingRepository.AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("Database write failure"));

        var command = new CreateBookingCommand(InstructorId: 2, ScheduledAt: targetTime)
        {
            StudentId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(command, CancellationToken.None));
    }
}
