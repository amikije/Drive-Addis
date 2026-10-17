using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class StudentActiveBookingsRuleTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();
    private readonly IInstructorRepository _instructorRepository = Substitute.For<IInstructorRepository>();
    private readonly IStudentRepository _studentRepository = Substitute.For<IStudentRepository>();
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();

    private CreateBookingHandler CreateSut() =>
        new(_bookingRepository, _instructorRepository, _studentRepository, _notificationRepository);

    [Theory]
    [InlineData(0, true)] // 0 active: allowed
    [InlineData(2, true)] // 2 active (limit - 1): allowed
    [InlineData(3, false)] // 3 active (limit boundary): REJECTED
    [InlineData(4, false)] // 4 active (data drift / exceeded): REJECTED
    public async Task Handle_ActiveBookingsCount_EnforcesMaximumThreeActiveLimit(int currentActive, bool shouldAllow)
    {
        // Arrange
        var sut = CreateSut();
        var student = TestDataFactory.CreateStudent(id: 10);
        var instructor = TestDataFactory.CreateInstructor(id: 20, verificationStatus: VerificationStatus.Verified);

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var targetTime = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7, 0, 0, DateTimeKind.Utc);

        _studentRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(student);
        _instructorRepository.GetByIdAsync(20, Arg.Any<CancellationToken>()).Returns(instructor);
        _bookingRepository.HasConflictingBookingAsync(20, targetTime, 90, Arg.Any<CancellationToken>())
            .Returns(false);

        // Mock current active bookings count for the student
        _bookingRepository.CountActiveBookingsForStudentAsync(10, Arg.Any<CancellationToken>())
            .Returns(currentActive);

        var command = new CreateBookingCommand(InstructorId: 20, ScheduledAt: targetTime)
        {
            StudentId = 10
        };

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(shouldAllow, result.IsSuccess);

        if (!shouldAllow)
        {
            Assert.Contains("maximum limit of 3 active bookings", result.Error);
            await _bookingRepository.DidNotReceive().AddAsync(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
        }
    }
}
