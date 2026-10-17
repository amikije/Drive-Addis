using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class VerifyResetOtpHandler : IRequestHandler<VerifyResetOtpCommand, Result<VerifyResetOtpResponseDto>>
{
    private readonly IPasswordResetService _passwordResetService;

    public VerifyResetOtpHandler(IPasswordResetService passwordResetService)
    {
        _passwordResetService = passwordResetService;
    }

    public async Task<Result<VerifyResetOtpResponseDto>> Handle(VerifyResetOtpCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Code))
            return Result<VerifyResetOtpResponseDto>.Failure("Phone number and verification code are required.");

        var isValid = await _passwordResetService.ValidateOtpAsync(request.PhoneNumber.Trim(), request.Code.Trim());
        if (!isValid)
            return Result<VerifyResetOtpResponseDto>.Failure("Invalid or expired verification code. Please check the code or request a new one.");

        var resetToken = await _passwordResetService.IssueResetTokenAsync(request.PhoneNumber.Trim());

        return Result<VerifyResetOtpResponseDto>.Success(new VerifyResetOtpResponseDto
        {
            Message = "Verification successful. You can now reset your password.",
            ResetToken = resetToken
        });
    }
}
