using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Instructors.Queries;

public record SearchInstructorsQuery(
    double StudentLatitude,
    double StudentLongitude,
    decimal? MaxPrice,
    double? MinRating,
    string? VehicleType
) : IRequest<List<InstructorSearchResultDto>>;