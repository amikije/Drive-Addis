using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly DriveAddisDbContext _context;

    public StudentRepository(DriveAddisDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Students.FirstOrDefaultAsync(s => s.Id == id, ct);
    }
}