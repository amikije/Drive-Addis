using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Instructors.Queries;

public class SearchInstructorsHandler
    : IRequestHandler<SearchInstructorsQuery, PagedResultDto<InstructorSearchResultDto>>
{
    private readonly IInstructorRepository _repository;

    public SearchInstructorsHandler(IInstructorRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<InstructorSearchResultDto>> Handle(
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
                DistanceKm = (request.StudentLatitude.HasValue && request.StudentLongitude.HasValue)
                    ? CalculateDistanceKm(request.StudentLatitude.Value, request.StudentLongitude.Value, i.Latitude, i.Longitude)
                    : -1,
                VehicleTypes = i.Vehicles.Select(v => v.Type.ToString()).ToList()
            })
            .Where(r => string.IsNullOrWhiteSpace(request.Name)
                || r.FullName.Contains(request.Name, StringComparison.OrdinalIgnoreCase))
            .Where(r => request.MaxPrice == null || r.HourlyPrice <= request.MaxPrice)
            .Where(r => request.MinRating == null || r.AverageRating >= request.MinRating)
            .Where(r => request.VehicleType == null || r.VehicleTypes.Contains(request.VehicleType))
            .ToList();

        // Sort by distance if location was given, otherwise fall back to best-rated first
        results = (request.StudentLatitude.HasValue && request.StudentLongitude.HasValue)
            ? results.OrderBy(r => r.DistanceKm).ToList()
            : results.OrderByDescending(r => r.AverageRating).ToList();

        var totalCount = results.Count;

        var pagedItems = results
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResultDto<InstructorSearchResultDto>
        {
            Items = pagedItems,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
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