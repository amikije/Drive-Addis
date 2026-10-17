using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public record ForgotPasswordCommand(string PhoneNumber) : IRequest<Result<ForgotPasswordResponseDto>>;
