using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public record GetInstructorsForAdminQuery(bool? UnverifiedOnly) : IRequest<List<InstructorAdminListItemDto>>;