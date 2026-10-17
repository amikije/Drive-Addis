using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Admin.Queries;

public class GetStudentsForAdminHandler : IRequestHandler<GetStudentsForAdminQuery, List<StudentAdminListItemDto>>
{
    private readonly IAdminRepository _adminRepository;

    public GetStudentsForAdminHandler(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<List<StudentAdminListItemDto>> Handle(GetStudentsForAdminQuery request, CancellationToken ct)
    {
        return await _adminRepository.GetStudentsAsync(request.Search, ct);
    }
}
