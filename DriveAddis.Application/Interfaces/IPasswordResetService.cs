namespace DriveAddis.Application.Interfaces;

public interface IPasswordResetService
{
    Task<(string Code, string DeepLinkKey)> GenerateAndSaveOtpAsync(string phoneNumber);
    Task<bool> CanRequestOtpAsync(string phoneNumber);
    Task<bool> ValidateOtpAsync(string phoneNumber, string code);
    Task<string> IssueResetTokenAsync(string phoneNumber);
    Task<string?> ValidateResetTokenAsync(string resetToken);
    Task InvalidateResetTokenAsync(string resetToken);
    bool TryGetOtpByDeepLinkKey(string deepLinkKey, out string phoneNumber, out string code);
}
