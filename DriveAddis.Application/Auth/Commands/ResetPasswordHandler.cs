using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService,
        IPasswordHasher passwordHasher,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _passwordHasher = passwordHasher;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<ResetPasswordResponseDto>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ResetToken))
            return Result<ResetPasswordResponseDto>.Failure("Reset session token is missing.");

        var phoneNumber = await _passwordResetService.ValidateResetTokenAsync(request.ResetToken);
        if (phoneNumber is null)
            return Result<ResetPasswordResponseDto>.Failure("Password reset session has expired. Please request a new code.");

        if (!IsPasswordStrong(request.NewPassword))
        {
            return Result<ResetPasswordResponseDto>.Failure(
                "Password must be at least 8 characters long, and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, ct);
        if (user is null)
            return Result<ResetPasswordResponseDto>.Failure("User account not found.");

        // Hash and update password
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _userRepository.SaveChangesAsync(ct);

        // Invalidate reset token and any existing refresh tokens
        await _passwordResetService.InvalidateResetTokenAsync(request.ResetToken);
        await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, ct);

        return Result<ResetPasswordResponseDto>.Success(new ResetPasswordResponseDto
        {
            Message = "Password has been successfully updated. You can now sign in with your new password."
        });
    }

    private bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
