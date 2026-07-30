using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Instructors.Queries;

public record SearchInstructorsQuery(
      
    string? Name,
    decimal? MaxPrice,
    double? MinRating,
    string? VehicleType,
    double StudentLatitude,
    double StudentLongitude
) : IRequest<List<InstructorSearchResultDto>>;