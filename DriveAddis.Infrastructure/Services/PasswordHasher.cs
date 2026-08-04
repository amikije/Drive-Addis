using DriveAddis.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DriveAddis.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password)
    {
        return _hasher.HashPassword(new object(), password);
    }

    public bool Verify(string password, string hash)
    {
        var result = _hasher.VerifyHashedPassword(new object(), hash, password);
        return result == PasswordVerificationResult.Success;
    }
}