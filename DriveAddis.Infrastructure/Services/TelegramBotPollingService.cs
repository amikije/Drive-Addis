using System.Text;
using System.Text.Json;
using DriveAddis.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveAddis.Infrastructure.Services;

public class TelegramBotPollingService : BackgroundService
{
    private readonly IPasswordResetService _passwordResetService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotPollingService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly bool _enabled;

    public TelegramBotPollingService(
        IPasswordResetService passwordResetService,
        IConfiguration configuration,
        ILogger<TelegramBotPollingService> logger)
    {
        _passwordResetService = passwordResetService;
        _configuration = configuration;
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(35) };

        _botToken = _configuration["TelegramBot:BotToken"] 
                    ?? "8874712111:AAFlEGxWN7bsHDVVbqTI_SIaShyTt5FfK0w";
        _enabled = _configuration.GetValue<bool>("TelegramBot:Enabled", true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(_botToken))
        {
            _logger.LogInformation("TelegramBotPollingService is disabled or bot token is missing.");
            return;
        }

        _logger.LogInformation("🚀 TelegramBotPollingService started. Listening for Telegram OTP requests...");

        long offset = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{_botToken}/getUpdates?offset={offset}&timeout=20";
                var response = await _httpClient.GetAsync(url, stoppingToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Telegram getUpdates returned {StatusCode}. Retrying in 5 seconds...", response.StatusCode);
                    await Task.Delay(5000, stoppingToken);
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync(stoppingToken);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("result", out var results) && results.ValueKind == JsonValueKind.Array)
                {
                    foreach (var update in results.EnumerateArray())
                    {
                        if (update.TryGetProperty("update_id", out var updateIdElem))
                        {
                            offset = updateIdElem.GetInt64() + 1;
                        }

                        if (update.TryGetProperty("message", out var messageElem))
                        {
                            await HandleIncomingMessageAsync(messageElem, stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TelegramBotPollingService loop. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _logger.LogInformation("TelegramBotPollingService is stopping.");
    }

    private async Task HandleIncomingMessageAsync(JsonElement messageElem, CancellationToken ct)
    {
        try
        {
            if (!messageElem.TryGetProperty("chat", out var chatElem) ||
                !chatElem.TryGetProperty("id", out var chatIdElem))
            {
                return;
            }

            long chatId = chatIdElem.GetInt64();

            if (!messageElem.TryGetProperty("text", out var textElem))
            {
                return;
            }

            string text = textElem.GetString()?.Trim() ?? string.Empty;

            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var deepLinkKey = parts[1].Trim();

                    if (_passwordResetService.TryGetOtpByDeepLinkKey(deepLinkKey, out var phone, out var code))
                    {
                        _logger.LogInformation("Delivering Telegram OTP for {Phone} to chat {ChatId}", phone, chatId);

                        var msg = $"🚗 *DriveAddis Security Verification*\n\n" +
                                  $"Hello! Your password reset verification code is:\n\n" +
                                  $"*`{code}`*\n\n" +
                                  $"_(Tap or click the code to copy)_\n\n" +
                                  $"⏱️ *Valid for 10 minutes.*\n" +
                                  $"⚠️ *Never share this code with anyone.*";

                        await SendTelegramMessageAsync(chatId, msg, ct);
                        return;
                    }
                    else
                    {
                        var expiredMsg = "⚠️ *Verification Link Expired or Invalid*\n\n" +
                                         "This verification code link has already been used or has expired.\n\n" +
                                         "Please go back to the DriveAddis website and request a new code.";
                        await SendTelegramMessageAsync(chatId, expiredMsg, ct);
                        return;
                    }
                }

                // Default /start greeting
                var welcomeMsg = "👋 *Welcome to DriveAddis Security Bot!*\n\n" +
                                 "This official bot delivers instant, real-time verification codes and notifications for your *DriveAddis* account.\n\n" +
                                 "To reset your password, enter your registered phone number on the DriveAddis website and tap *Open Telegram*.\n\n" +
                                 "🚗 *DriveAddis — Driving with Confidence.*";

                await SendTelegramMessageAsync(chatId, welcomeMsg, ct);
            }
            else if (text.StartsWith("/help", StringComparison.OrdinalIgnoreCase))
            {
                var helpMsg = "ℹ️ *DriveAddis Bot Help*\n\n" +
                              "• Need a verification code? Request a password reset from the DriveAddis website.\n" +
                              "• For further support, contact the DriveAddis administrator.";
                await SendTelegramMessageAsync(chatId, helpMsg, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle incoming Telegram message.");
        }
    }

    private async Task SendTelegramMessageAsync(long chatId, string text, CancellationToken ct)
    {
        try
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = text,
                parse_mode = "Markdown"
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Telegram sendMessage returned {StatusCode}: {Error}", response.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Telegram message to chat {ChatId}", chatId);
        }
    }

    public override void Dispose()
    {
        _httpClient.Dispose();
        base.Dispose();
    }
}
