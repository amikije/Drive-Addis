using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);
}