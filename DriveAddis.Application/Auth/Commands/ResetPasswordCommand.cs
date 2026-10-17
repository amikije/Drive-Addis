using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public record ResetPasswordCommand(
    string ResetToken,
    string NewPassword
) : IRequest<Result<ResetPasswordResponseDto>>;
