using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public record GetStudentsForAdminQuery(string? Search) : IRequest<List<StudentAdminListItemDto>>;
