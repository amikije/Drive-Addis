using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RegisterHandler(
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

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, ct);
        if (existingUser is not null)
            return Result<AuthResponseDto>.Failure("Phone number is already registered.");

        if (!IsPasswordStrong(request.Password))
        {
            return Result<AuthResponseDto>.Failure("Password must be at least 8 characters long, and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        var user = new User
        {
            UserName = request.PhoneNumber,
            NormalizedUserName = request.PhoneNumber,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        if (request.Role == UserRole.Student)
        {
            user.Student = new Student
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber
            };
        }
        else if (request.Role == UserRole.Instructor)
        {
            if (string.IsNullOrWhiteSpace(request.LicensePhotoUrl))
                return Result<AuthResponseDto>.Failure("License photo is required for instructor registration.");
            user.Instructor = new Instructor
            {
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                HourlyPrice = request.HourlyPrice ?? 0,
                Latitude = request.Latitude ?? 0,
                Longitude = request.Longitude ?? 0,
             
                LicensePhotoUrl = request.LicensePhotoUrl

            };
        }
        else
        {
            return Result<AuthResponseDto>.Failure("Cannot self-register as Admin.");
        }

        try
        {
            await _userRepository.AddAsync(user, ct);
            await _userRepository.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex.GetType().Name == "DbUpdateException" ||
                                  ex.Message.Contains("SQLite Error 1") ||
                                  ex.Message.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
                                  (ex.InnerException != null && ex.InnerException.Message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)))
        {
            return Result<AuthResponseDto>.Failure("Phone number is already registered.");
        }

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
            FullName = request.FullName,
            PhoneNumber = user.PhoneNumber
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