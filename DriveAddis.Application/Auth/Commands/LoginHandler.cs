using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, ct);

        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid phone number or password.");

        if (user.LockoutEnd.HasValue)
        {
            if (user.LockoutEnd.Value > DateTimeOffset.UtcNow)
            {
                var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes);
                return Result<AuthResponseDto>.Failure($"Your account has been temporarily locked because of too many failed login attempts. Please try again in {remainingMinutes} minute(s).");
            }
            else
            {
                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
            }
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                await _userRepository.SaveChangesAsync(ct);
                return Result<AuthResponseDto>.Failure("Your account has been temporarily locked because of too many failed login attempts. Please try again in 15 minute(s).");
            }
            
            await _userRepository.SaveChangesAsync(ct);

            // User must see remaining try count on the login page after trying 2 times
            if (user.AccessFailedCount >= 2)
            {
                var remainingAttempts = 5 - user.AccessFailedCount;
                return Result<AuthResponseDto>.Failure($"Invalid phone number or password. You have {remainingAttempts} attempt(s) remaining.");
            }

            return Result<AuthResponseDto>.Failure("Invalid phone number or password.");
        }

        // Reset lockout on successful login
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await _userRepository.SaveChangesAsync(ct);

        var tokens = _jwtTokenGenerator.GenerateToken(user);

        var refreshToken = new RefreshToken
        {
            Token = tokens.RefreshToken,
            JwtId = tokens.JwtId,
            UserId = user.Id,
            AddedDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshToken, ct);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Tokens = tokens,
            Role = user.Role.ToString(),
            UserId = user.Id,
            StudentId = user.StudentId,
            InstructorId = user.InstructorId,
            FullName = user.Student?.FullName ?? user.Instructor?.FullName ?? (user.Role == UserRole.Admin ? "System Administrator" : user.PhoneNumber),
            PhoneNumber = user.PhoneNumber
        });
    }
}