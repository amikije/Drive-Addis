using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, AdminDashboardDto>
{
    private readonly IAdminRepository _adminRepository;

    public GetDashboardHandler(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<AdminDashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        return await _adminRepository.GetDashboardStatsAsync(ct);
    }
}