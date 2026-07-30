using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Instructors.Queries;

public class SearchInstructorsHandler
    : IRequestHandler<SearchInstructorsQuery, List<InstructorSearchResultDto>>
{
    private readonly IInstructorRepository _repository;

    public SearchInstructorsHandler(IInstructorRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<InstructorSearchResultDto>> Handle(
        SearchInstructorsQuery request, CancellationToken ct)
    {
        var instructors = await _repository.GetAllVerifiedAsync(ct);

        var results = instructors
            .Select(i => new InstructorSearchResultDto
            {
                Id = i.Id,
                FullName = i.FullName,
                HourlyPrice = i.HourlyPrice,
                AverageRating = i.AverageRating,
                DistanceKm = CalculateDistanceKm(
                    request.StudentLatitude, request.StudentLongitude,
                    i.Latitude, i.Longitude),
                VehicleTypes = i.Vehicles.Select(v => v.Type.ToString()).ToList()
            })
            .Where(r => string.IsNullOrWhiteSpace(request.Name)
    || r.FullName.Contains(request.Name, StringComparison.OrdinalIgnoreCase))
            .Where(r => request.MaxPrice == null || r.HourlyPrice <= request.MaxPrice)
            .Where(r => request.MinRating == null || r.AverageRating >= request.MinRating)
            .Where(r => request.VehicleType == null || r.VehicleTypes.Contains(request.VehicleType))
            .OrderBy(r => r.DistanceKm)
            .ToList();

        return results;
    }

    // Haversine formula — calculates great-circle distance between two lat/lng points in km
    private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadiusKm = 6371;

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}