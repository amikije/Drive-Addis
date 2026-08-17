using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public record GetInstructorsForAdminQuery(
    bool? UnverifiedOnly,
    string? Search
) : IRequest<List<InstructorAdminListItemDto>>;