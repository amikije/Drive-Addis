using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class BookingStatusTransitionTests
{
    private readonly IBookingRepository _bookingRepository = Substitute.For<IBookingRepository>();

    private UpdateBookingStatusHandler CreateSut() => new(_bookingRepository);

    [Theory]
    [InlineData(BookingStatus.Pending, BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Pending, BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Completed)]
    [InlineData(BookingStatus.Confirmed, BookingStatus.Cancelled)]
    public async Task UpdateStatus_ValidStateTransitions_SucceedsAndPersists(
        BookingStatus currentStatus,
        BookingStatus targetStatus)
    {
        // Arrange
        var booking = TestDataFactory.CreateBooking(id: 10, status: currentStatus);
        booking.Student = TestDataFactory.CreateStudent(id: 1);
        booking.Instructor = TestDataFactory.CreateInstructor(id: 2);

        _bookingRepository.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(booking);

        var sut = CreateSut();
        var command = new UpdateBookingStatusCommand(BookingId: 10, NewStatus: targetStatus);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(targetStatus.ToString(), result.Value!.Status);
        Assert.Equal(targetStatus, booking.Status);
        await _bookingRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(BookingStatus.Pending, BookingStatus.Completed)] // Cannot jump Pending directly to Completed
    [InlineData(BookingStatus.Completed, BookingStatus.Pending)] // Cannot reopen Completed
    [InlineData(BookingStatus.Completed, BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Completed, BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Cancelled, BookingStatus.Pending)] // Cannot reopen Cancelled
    [InlineData(BookingStatus.Cancelled, BookingStatus.Confirmed)]
    [InlineData(BookingStatus.Cancelled, BookingStatus.Completed)]
    public async Task UpdateStatus_InvalidStateTransitions_ReturnsFailureAndNeverSaves(
        BookingStatus currentStatus,
        BookingStatus targetStatus)
    {
        // Arrange
        var booking = TestDataFactory.CreateBooking(id: 11, status: currentStatus);
        _bookingRepository.GetByIdAsync(11, Arg.Any<CancellationToken>()).Returns(booking);

        var sut = CreateSut();
        var command = new UpdateBookingStatusCommand(BookingId: 11, NewStatus: targetStatus);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"Cannot change booking from '{currentStatus}' to '{targetStatus}'", result.Error);
        Assert.Equal(currentStatus, booking.Status); // status must remain untouched
        await _bookingRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateStatus_ConfirmedOrCompleted_ExposesStudentPhoneNumber()
    {
        // Arrange
        var booking = TestDataFactory.CreateBooking(id: 12, status: BookingStatus.Pending);
        booking.Student = TestDataFactory.CreateStudent(id: 5, phoneNumber: "+251911333444");
        _bookingRepository.GetByIdAsync(12, Arg.Any<CancellationToken>()).Returns(booking);

        var sut = CreateSut();
        var command = new UpdateBookingStatusCommand(12, BookingStatus.Confirmed);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert: When confirmed, phone number must be revealed to instructor
        Assert.True(result.IsSuccess);
        Assert.Equal("+251911333444", result.Value!.StudentPhoneNumber);
    }

    [Fact]
    public async Task UpdateStatus_Cancelled_RedactsStudentPhoneNumber()
    {
        // Arrange
        var booking = TestDataFactory.CreateBooking(id: 13, status: BookingStatus.Pending);
        booking.Student = TestDataFactory.CreateStudent(id: 5, phoneNumber: "+251911333444");
        _bookingRepository.GetByIdAsync(13, Arg.Any<CancellationToken>()).Returns(booking);

        var sut = CreateSut();
        var command = new UpdateBookingStatusCommand(13, BookingStatus.Cancelled);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert: When cancelled, phone number must remain null for privacy
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.StudentPhoneNumber);
    }
}
