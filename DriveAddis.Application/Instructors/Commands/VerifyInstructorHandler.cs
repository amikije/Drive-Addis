using DriveAddis.Application.Common;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Instructors.Commands;

public class VerifyInstructorHandler : IRequestHandler<VerifyInstructorCommand, Result<bool>>
{
    private readonly IInstructorRepository _instructorRepository;
    private readonly INotificationRepository _notificationRepository;

    public VerifyInstructorHandler(
        IInstructorRepository instructorRepository,
        INotificationRepository notificationRepository)
    {
        _instructorRepository = instructorRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> Handle(VerifyInstructorCommand request, CancellationToken ct)
    {
        var instructor = await _instructorRepository.GetByIdAsync(request.InstructorId, ct);

        if (instructor is null)
            return Result<bool>.Failure("Instructor not found.");

        if (instructor.VerificationStatus == VerificationStatus.Verified)
            return Result<bool>.Failure("Instructor is already verified.");

        await _instructorRepository.VerifyAsync(request.InstructorId, ct);

        // Notify instructor of approval
        await _notificationRepository.AddAsync(new Notification
        {
            InstructorId = request.InstructorId,
            Title = "Application Approved! 🎉",
            Message = "Congratulations! Your instructor application has been approved by the Admin. You are now verified on DriveAddis and visible to students.",
            Type = "ApplicationApproved",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        }, ct);
        await _notificationRepository.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}