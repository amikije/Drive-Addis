using DriveAddis.Application.Dtos;

namespace DriveAddis.Application.Interfaces;

public interface IAdminRepository
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct);
}