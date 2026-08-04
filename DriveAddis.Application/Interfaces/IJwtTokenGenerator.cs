using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}