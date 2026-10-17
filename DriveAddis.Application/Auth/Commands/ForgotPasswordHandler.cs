using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
    }

    public async Task<Result<ForgotPasswordResponseDto>> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result<ForgotPasswordResponseDto>.Failure("Phone number is required.");

        var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber.Trim(), ct);
        if (user is null)
            return Result<ForgotPasswordResponseDto>.Failure("No account registered with this phone number.");

        if (!await _passwordResetService.CanRequestOtpAsync(request.PhoneNumber.Trim()))
            return Result<ForgotPasswordResponseDto>.Failure("Please wait a moment before requesting another verification code.");

        var (code, deepLinkKey) = await _passwordResetService.GenerateAndSaveOtpAsync(request.PhoneNumber.Trim());
        var botUsername = "Drive_AddisBot";
        var telegramUrl = $"https://t.me/{botUsername}?start={deepLinkKey}";

        return Result<ForgotPasswordResponseDto>.Success(new ForgotPasswordResponseDto
        {
            Message = "Verification code generated. Open Telegram to receive it, or enter the code below.",
            DevCode = code,
            TelegramUrl = telegramUrl,
            TelegramBotUsername = botUsername
        });
    }
}
