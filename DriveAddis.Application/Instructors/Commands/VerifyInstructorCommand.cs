using DriveAddis.Application.Common;
using MediatR;

namespace DriveAddis.Application.Instructors.Commands;

public record VerifyInstructorCommand(int InstructorId) : IRequest<Result<bool>>;