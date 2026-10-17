using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public class AuthTokens
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string JwtId { get; set; } = string.Empty;
}

public interface IJwtTokenGenerator
{
    AuthTokens GenerateToken(User user);
}