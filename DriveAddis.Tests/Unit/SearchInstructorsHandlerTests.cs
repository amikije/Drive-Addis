using DriveAddis.Application.Dtos;
using DriveAddis.Application.Instructors.Queries;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class SearchInstructorsHandlerTests
{
    private readonly IInstructorRepository _repository = Substitute.For<IInstructorRepository>();

    private SearchInstructorsHandler CreateSut() => new(_repository);

    private static List<Instructor> SeedInstructors() => new()
    {
        new Instructor
        {
            Id = 1,
            FullName = "Abebe Kebede",
            VerificationStatus = VerificationStatus.Verified,
            HourlyPrice = 300,
            AverageRating = 4.5,
            Latitude = 9.0192,
            Longitude = 38.7525,
            Vehicles = new List<Vehicle> { new() { Type = VehicleType.Manual } }
        },
        new Instructor
        {
            Id = 2,
            FullName = "Marta Alemu",
            VerificationStatus = VerificationStatus.Verified,
            HourlyPrice = 600,
            AverageRating = 4.9,
            Latitude = 9.0350,
            Longitude = 38.7469,
            Vehicles = new List<Vehicle> { new() { Type = VehicleType.Automatic } }
        },
        new Instructor
        {
            Id = 3,
            FullName = "Yonas Desta",
            VerificationStatus = VerificationStatus.Verified,
            HourlyPrice = 250,
            AverageRating = 4.2,
            Latitude = 9.0000,
            Longitude = 38.7600,
            Vehicles = new List<Vehicle> { new() { Type = VehicleType.Manual } }
        }
    };

    [Theory]
    [InlineData("Abebe", 1)]
    [InlineData("marta", 1)] // case-insensitive
    [InlineData("NonExistent", 0)] // zero match
    [InlineData(null, 3)] // all match
    [InlineData("   ", 3)] // whitespace ignored
    public async Task Handle_NameFiltering_ReturnsMatchingInstructors(string? nameFilter, int expectedCount)
    {
        // Arrange
        var sut = CreateSut();
        _repository.GetAllVerifiedAsync(Arg.Any<CancellationToken>()).Returns(SeedInstructors());

        var query = new SearchInstructorsQuery(
            StudentLatitude: null,
            StudentLongitude: null,
            MaxPrice: null,
            MinRating: null,
            VehicleType: null,
            Name: nameFilter,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedCount, result.TotalCount);
        Assert.Equal(expectedCount, result.Items.Count);
    }

    [Theory]
    [InlineData(250, 1)] // boundary: exactly matches Yonas
    [InlineData(300, 2)] // matches Yonas (250) and Abebe (300)
    [InlineData(249.99, 0)] // just below lowest
    [InlineData(1000, 3)] // matches all
    public async Task Handle_MaxPriceBoundary_FiltersAccurately(decimal maxPrice, int expectedCount)
    {
        // Arrange
        var sut = CreateSut();
        _repository.GetAllVerifiedAsync(Arg.Any<CancellationToken>()).Returns(SeedInstructors());

        var query = new SearchInstructorsQuery(
            StudentLatitude: null,
            StudentLongitude: null,
            MaxPrice: maxPrice,
            MinRating: null,
            VehicleType: null,
            Name: null,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedCount, result.TotalCount);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSliceAndTotalCount()
    {
        // Arrange: 3 instructors, requested pageSize = 2, pageNumber = 1
        var sut = CreateSut();
        _repository.GetAllVerifiedAsync(Arg.Any<CancellationToken>()).Returns(SeedInstructors());

        var query = new SearchInstructorsQuery(
            StudentLatitude: null,
            StudentLongitude: null,
            MaxPrice: null,
            MinRating: null,
            VehicleType: null,
            Name: null,
            PageNumber: 1,
            PageSize: 2
        );

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }
}
