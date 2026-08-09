using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using MediatR;

namespace DriveAddis.Application.Auth.Commands;

public class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByPhoneNumberAsync(request.PhoneNumber, ct);

        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid phone number or password.");

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
            return Result<AuthResponseDto>.Failure("Invalid phone number or password.");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(new AuthResponseDto
        {
            Token = token,
            Role = user.Role.ToString(),
            UserId = user.Id
        });
    }
}