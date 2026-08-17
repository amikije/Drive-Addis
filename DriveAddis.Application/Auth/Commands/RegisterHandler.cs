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

    public RegisterHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, ct);
        if (existingUser is not null)
            return Result<AuthResponseDto>.Failure("Phone number is already registered.");

        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role
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
                IsVerified = false, // must be verified by an Admin before receiving bookings
                LicensePhotoUrl = request.LicensePhotoUrl

            };
        }
        else
        {
            return Result<AuthResponseDto>.Failure("Cannot self-register as Admin.");
        }

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Role = user.Role.ToString(),
            UserId = user.Id
        });
    }
}