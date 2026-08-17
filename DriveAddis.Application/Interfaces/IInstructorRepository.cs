using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IInstructorRepository
{
    Task<List<Instructor>> GetAllVerifiedAsync(CancellationToken ct);
    Task<Instructor?> GetByIdAsync(int id, CancellationToken ct);
    Task UpdateAverageRatingAsync(int instructorId, CancellationToken ct);
    Task VerifyAsync(int instructorId, CancellationToken ct);
    Task RejectAsync(int instructorId, string reason, CancellationToken ct);
}