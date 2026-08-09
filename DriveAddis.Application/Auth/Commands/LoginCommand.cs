using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public record LoginCommand(
    string PhoneNumber,
    string Password
) : IRequest<Result<AuthResponseDto>>;