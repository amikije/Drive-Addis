using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public record GetDashboardQuery() : IRequest<AdminDashboardDto>;