using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Instructors.Queries;

public record SearchInstructorsQuery(
    double? StudentLatitude,
    double? StudentLongitude,
    decimal? MaxPrice,
    double? MinRating,
    string? VehicleType,
    string? Name,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResultDto<InstructorSearchResultDto>>;