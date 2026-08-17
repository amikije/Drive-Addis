using DriveAddis.Application.Common;
using MediatR;

namespace DriveAddis.Application.Instructors.Commands;

public record RejectInstructorCommand(int InstructorId, string Reason) : IRequest<Result<bool>>;