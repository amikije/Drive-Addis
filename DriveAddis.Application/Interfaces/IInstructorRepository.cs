using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IInstructorRepository
{
    Task<List<Instructor>> GetAllVerifiedAsync(CancellationToken ct);
}