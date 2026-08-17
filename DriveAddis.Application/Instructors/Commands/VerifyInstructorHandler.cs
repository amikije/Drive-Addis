using DriveAddis.Application.Common;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Instructors.Commands;

public class VerifyInstructorHandler : IRequestHandler<VerifyInstructorCommand, Result<bool>>
{
    private readonly IInstructorRepository _instructorRepository;

    public VerifyInstructorHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<Result<bool>> Handle(VerifyInstructorCommand request, CancellationToken ct)
    {
        var instructor = await _instructorRepository.GetByIdAsync(request.InstructorId, ct);

        if (instructor is null)
            return Result<bool>.Failure("Instructor not found.");

        if (instructor.VerificationStatus == VerificationStatus.Verified)
            return Result<bool>.Failure("Instructor is already verified.");

        await _instructorRepository.VerifyAsync(request.InstructorId, ct);

        return Result<bool>.Success(true);
    }
}