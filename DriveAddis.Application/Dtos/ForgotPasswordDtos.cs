namespace DriveAddis.Application.Dtos;

public class ForgotPasswordRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string? DevCode { get; set; }
    public string? TelegramUrl { get; set; }
    public string? TelegramBotUsername { get; set; }
}

public class VerifyResetOtpRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class VerifyResetOtpResponseDto
{
    public string Message { get; set; } = string.Empty;
    public string ResetToken { get; set; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ResetPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
}
