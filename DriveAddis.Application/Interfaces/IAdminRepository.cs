using DriveAddis.Application.Dtos;

namespace DriveAddis.Application.Interfaces;

public interface IAdminRepository
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken ct);
    Task<List<InstructorAdminListItemDto>> GetInstructorsAsync(bool? unverifiedOnly, string? search, CancellationToken ct);
}