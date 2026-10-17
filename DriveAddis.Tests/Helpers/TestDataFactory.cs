using DriveAddis.Domain.Entities;

namespace DriveAddis.Tests.Helpers;

public static class TestDataFactory
{
    public static Student CreateStudent(int id = 1, string fullName = "Abeba Bikila", string phoneNumber = "+251911111111")
    {
        return new Student
        {
            Id = id,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            LicenseNumber = "LIC-ET-001",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Instructor CreateInstructor(
        int id = 1,
        string fullName = "Kassahun Desta",
        string phoneNumber = "+251922222222",
        VerificationStatus verificationStatus = VerificationStatus.Verified,
        decimal hourlyPrice = 450m,
        double lat = 9.0192,
        double lng = 38.7525,
        double rating = 4.8)
    {
        return new Instructor
        {
            Id = id,
            FullName = fullName,
            PhoneNumber = phoneNumber,
            VerificationStatus = verificationStatus,
            HourlyPrice = hourlyPrice,
            Latitude = lat,
            Longitude = lng,
            AverageRating = rating,
            CreatedAt = DateTime.UtcNow,
            Vehicles = new List<Vehicle>
            {
                new() { Id = 1, Type = VehicleType.Manual, Model = "Toyota Corolla", PlateNumber = "AA-10001" },
                new() { Id = 2, Type = VehicleType.Automatic, Model = "Hyundai Elantra", PlateNumber = "AA-20002" }
            }
        };
    }

    public static Booking CreateBooking(
        int id = 1,
        int studentId = 1,
        int instructorId = 1,
        DateTime? scheduledAt = null,
        BookingStatus status = BookingStatus.Pending)
    {
        return new Booking
        {
            Id = id,
            StudentId = studentId,
            InstructorId = instructorId,
            ScheduledAt = scheduledAt ?? DateTime.UtcNow.AddDays(2),
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }
}
