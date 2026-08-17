using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public class GetInstructorsForAdminHandler : IRequestHandler<GetInstructorsForAdminQuery, List<InstructorAdminListItemDto>>
{
    private readonly IAdminRepository _adminRepository;

    public GetInstructorsForAdminHandler(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<List<InstructorAdminListItemDto>> Handle(GetInstructorsForAdminQuery request, CancellationToken ct)
    {
        return await _adminRepository.GetInstructorsAsync(request.UnverifiedOnly, request.Search, ct);
    }
}