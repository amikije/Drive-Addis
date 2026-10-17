using DriveAddis.Application.Instructors.Queries;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class DistanceCalculationTests
{
    private readonly IInstructorRepository _instructorRepository = Substitute.For<IInstructorRepository>();

    [Fact]
    public async Task CalculateDistance_IdenticalCoordinates_ReturnsZeroDistance()
    {
        // Arrange: Student and instructor at the exact same location in Bole (9.0000, 38.7800)
        var instructor = new Instructor
        {
            Id = 1,
            FullName = "Local Instructor",
            VerificationStatus = VerificationStatus.Verified,
            Latitude = 9.0000,
            Longitude = 38.7800,
            HourlyPrice = 300,
            AverageRating = 4.5
        };
        _instructorRepository.GetAllVerifiedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Instructor> { instructor });

        var sut = new SearchInstructorsHandler(_instructorRepository);
        var query = new SearchInstructorsQuery(
            StudentLatitude: 9.0000,
            StudentLongitude: 38.7800,
            MaxPrice: null,
            MinRating: null,
            VehicleType: null,
            Name: null,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert: Distance to same point must be 0 km
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(0, result.Items[0].DistanceKm, precision: 2);
    }

    [Fact]
    public async Task CalculateDistance_KnownAddisCoordinates_CalculatesExpectedKilometers()
    {
        // Arrange: Edna Mall (9.0000, 38.7800) to Arat Kilo / University (9.0350, 38.7500)
        // Straight-line distance is approx 5.0 - 5.5 km
        var instructor = new Instructor
        {
            Id = 2,
            FullName = "Arat Kilo Instructor",
            VerificationStatus = VerificationStatus.Verified,
            Latitude = 9.0350,
            Longitude = 38.7500,
            HourlyPrice = 400,
            AverageRating = 4.7
        };
        _instructorRepository.GetAllVerifiedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Instructor> { instructor });

        var sut = new SearchInstructorsHandler(_instructorRepository);
        var query = new SearchInstructorsQuery(
            StudentLatitude: 9.0000,
            StudentLongitude: 38.7800,
            MaxPrice: null,
            MinRating: null,
            VehicleType: null,
            Name: null,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Items[0].DistanceKm > 4.5 && result.Items[0].DistanceKm < 6.0,
            $"Expected distance ~5.1 km, actual was {result.Items[0].DistanceKm}");
    }

    [Fact]
    public async Task Search_WithoutStudentCoordinates_ReturnsNegativeOneDistance()
    {
        // Arrange: When coordinates are not provided, DistanceKm should default to -1
        var instructor = new Instructor
        {
            Id = 3,
            FullName = "General Instructor",
            VerificationStatus = VerificationStatus.Verified,
            Latitude = 9.0000,
            Longitude = 38.7800,
            HourlyPrice = 350,
            AverageRating = 4.2
        };
        _instructorRepository.GetAllVerifiedAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Instructor> { instructor });

        var sut = new SearchInstructorsHandler(_instructorRepository);
        var query = new SearchInstructorsQuery(
            StudentLatitude: null,
            StudentLongitude: null,
            MaxPrice: null,
            MinRating: null,
            VehicleType: null,
            Name: null,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(-1, result.Items[0].DistanceKm);
    }
}
