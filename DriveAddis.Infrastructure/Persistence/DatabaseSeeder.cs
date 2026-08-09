using DriveAddis.Domain.Entities;

namespace DriveAddis.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static void Seed(DriveAddisDbContext context)
    {
        SeedStudents(context);
        SeedInstructors(context);
    }

    private static void SeedStudents(DriveAddisDbContext context)
    {
        if (context.Students.Any())
            return;

        var students = new List<Student>
        {
            new() { FullName = "Selam Tadesse", PhoneNumber = "+251922000001", LicenseNumber = "LIC-001" },
            new() { FullName = "Dawit Girma", PhoneNumber = "+251922000002", LicenseNumber = "LIC-002" }
        };

        context.Students.AddRange(students);
        context.SaveChanges();
    }

    private static void SeedInstructors(DriveAddisDbContext context)
    {
        if (context.Instructors.Any())
            return;

        var instructors = new List<Instructor>
        {
            new()
            {
                FullName = "Abebe Kebede",
                PhoneNumber = "+251911000001",
                IsVerified = true,
                HourlyPrice = 300,
                Latitude = 9.0192,
                Longitude = 38.7525,
                AverageRating = 4.5,
                Vehicles = new List<Vehicle>
                {
                    new() { Type = VehicleType.Manual, Model = "Toyota Corolla", PlateNumber = "AA-12345" }
                }
            },
            new()
            {
                FullName = "Marta  Alemu",
                PhoneNumber = "+251911000002",
                IsVerified = true,
                HourlyPrice = 600,
                Latitude = 9.0350,
                Longitude = 38.7469,
                AverageRating = 4.8,
                Vehicles = new List<Vehicle>
                {
                    new() { Type = VehicleType.Automatic, Model = "Hyundai Accent", PlateNumber = "AA-54321" }
                }
            },
            new()
            {
                FullName = " Yonas ",
                PhoneNumber = "+251911000003",
                IsVerified = true,
                HourlyPrice = 250,
                Latitude = 9.0000,
                Longitude = 38.7600,
                AverageRating = 4.2,
                Vehicles = new List<Vehicle>
                {
                    new() { Type = VehicleType.Manual, Model = "Suzuki Swift", PlateNumber = "AA-67890" }
                }
            }
        };

        context.Instructors.AddRange(instructors);
        context.SaveChanges();
    }
}