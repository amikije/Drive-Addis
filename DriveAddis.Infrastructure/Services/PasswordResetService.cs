using System.Collections.Concurrent;
using System.Security.Cryptography;
using DriveAddis.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DriveAddis.Infrastructure.Services;

public class PasswordResetService : IPasswordResetService
{
    private class OtpSession
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string RawCode { get; set; } = string.Empty;
        public string CodeHash { get; set; } = string.Empty;
        public string DeepLinkKey { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int AttemptsCount { get; set; }
        public DateTime LastSentAt { get; set; }
    }

    private class TokenSession
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    private readonly ConcurrentDictionary<string, OtpSession> _otpSessions = new();
    private readonly ConcurrentDictionary<string, string> _deepLinkToPhone = new();
    private readonly ConcurrentDictionary<string, TokenSession> _resetTokens = new();
    private readonly ILogger<PasswordResetService> _logger;

    public PasswordResetService(ILogger<PasswordResetService> logger)
    {
        _logger = logger;
    }

    public Task<bool> CanRequestOtpAsync(string phoneNumber)
    {
        var normalizedPhone = NormalizePhone(phoneNumber);
        if (_otpSessions.TryGetValue(normalizedPhone, out var session))
        {
            if (DateTime.UtcNow - session.LastSentAt < TimeSpan.FromSeconds(30))
            {
                return Task.FromResult(false);
            }
        }
        return Task.FromResult(true);
    }

    public Task<(string Code, string DeepLinkKey)> GenerateAndSaveOtpAsync(string phoneNumber)
    {
        var normalizedPhone = NormalizePhone(phoneNumber);

        // 6-digit code
        var code = Random.Shared.Next(100000, 999999).ToString();
        var codeHash = HashCode(code);
        var deepLinkKey = "rst_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();

        // Clear any old session's deep link
        if (_otpSessions.TryGetValue(normalizedPhone, out var oldSession) && !string.IsNullOrEmpty(oldSession.DeepLinkKey))
        {
            _deepLinkToPhone.TryRemove(oldSession.DeepLinkKey, out _);
        }

        var session = new OtpSession
        {
            PhoneNumber = normalizedPhone,
            RawCode = code,
            CodeHash = codeHash,
            DeepLinkKey = deepLinkKey,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            AttemptsCount = 0,
            LastSentAt = DateTime.UtcNow
        };

        _otpSessions[normalizedPhone] = session;
        _deepLinkToPhone[deepLinkKey] = normalizedPhone;

        _logger.LogInformation("=================================================");
        _logger.LogInformation("[TELEGRAM OTP] Password Reset OTP for {PhoneNumber}: {Code} (DeepLinkKey: {DeepLinkKey})", normalizedPhone, code, deepLinkKey);
        _logger.LogInformation("=================================================");

        return Task.FromResult((code, deepLinkKey));
    }

    public bool TryGetOtpByDeepLinkKey(string deepLinkKey, out string phoneNumber, out string code)
    {
        phoneNumber = string.Empty;
        code = string.Empty;

        if (string.IsNullOrWhiteSpace(deepLinkKey))
            return false;

        if (_deepLinkToPhone.TryGetValue(deepLinkKey.Trim(), out var phone))
        {
            if (_otpSessions.TryGetValue(phone, out var session) && DateTime.UtcNow <= session.ExpiresAt)
            {
                phoneNumber = phone;
                code = session.RawCode;
                return true;
            }
        }

        return false;
    }

    public Task<bool> ValidateOtpAsync(string phoneNumber, string code)
    {
        var normalizedPhone = NormalizePhone(phoneNumber);
        if (!_otpSessions.TryGetValue(normalizedPhone, out var session))
        {
            return Task.FromResult(false);
        }

        if (DateTime.UtcNow > session.ExpiresAt)
        {
            _otpSessions.TryRemove(normalizedPhone, out _);
            if (!string.IsNullOrEmpty(session.DeepLinkKey)) _deepLinkToPhone.TryRemove(session.DeepLinkKey, out _);
            return Task.FromResult(false);
        }

        if (session.AttemptsCount >= 5)
        {
            _otpSessions.TryRemove(normalizedPhone, out _);
            if (!string.IsNullOrEmpty(session.DeepLinkKey)) _deepLinkToPhone.TryRemove(session.DeepLinkKey, out _);
            return Task.FromResult(false);
        }

        session.AttemptsCount++;

        bool isValid = VerifyCode(code, session.CodeHash);
        if (isValid)
        {
            // Successfully verified, remove OTP session so it cannot be reused
            _otpSessions.TryRemove(normalizedPhone, out _);
            if (!string.IsNullOrEmpty(session.DeepLinkKey)) _deepLinkToPhone.TryRemove(session.DeepLinkKey, out _);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    private static string NormalizePhone(string phone)
    {
        var clean = phone.Trim().Replace(" ", "").Replace("-", "");
        if (clean.StartsWith("0"))
        {
            return "+251" + clean.Substring(1);
        }
        if (clean.StartsWith("251"))
        {
            return "+" + clean;
        }
        return clean;
    }

    private static string HashCode(string code)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyCode(string code, string expectedHash)
    {
        var computed = HashCode(code);
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(computed),
            System.Text.Encoding.UTF8.GetBytes(expectedHash)
        );
    }

    public Task<string> IssueResetTokenAsync(string phoneNumber)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        _resetTokens[token] = new TokenSession
        {
            PhoneNumber = phoneNumber,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        return Task.FromResult(token);
    }

    public Task<string?> ValidateResetTokenAsync(string resetToken)
    {
        if (!_resetTokens.TryGetValue(resetToken, out var session))
        {
            return Task.FromResult<string?>(null);
        }

        if (DateTime.UtcNow > session.ExpiresAt)
        {
            _resetTokens.TryRemove(resetToken, out _);
            return Task.FromResult<string?>(null);
        }

        return Task.FromResult<string?>(session.PhoneNumber);
    }

    public Task InvalidateResetTokenAsync(string resetToken)
    {
        _resetTokens.TryRemove(resetToken, out _);
        return Task.CompletedTask;
    }
}
