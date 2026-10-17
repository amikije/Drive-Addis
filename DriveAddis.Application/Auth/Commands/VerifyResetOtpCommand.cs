using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public record VerifyResetOtpCommand(
    string PhoneNumber,
    string Code
) : IRequest<Result<VerifyResetOtpResponseDto>>;
