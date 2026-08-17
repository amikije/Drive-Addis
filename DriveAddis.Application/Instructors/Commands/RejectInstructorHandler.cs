using DriveAddis.Application.Common;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Instructors.Commands;

public class RejectInstructorHandler : IRequestHandler<RejectInstructorCommand, Result<bool>>
{
    private readonly IInstructorRepository _instructorRepository;

    public RejectInstructorHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<Result<bool>> Handle(RejectInstructorCommand request, CancellationToken ct)
    {
        var instructor = await _instructorRepository.GetByIdAsync(request.InstructorId, ct);

        if (instructor is null)
            return Result<bool>.Failure("Instructor not found.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            return Result<bool>.Failure("A rejection reason is required.");

        await _instructorRepository.RejectAsync(request.InstructorId, request.Reason, ct);

        return Result<bool>.Success(true);
    }
}