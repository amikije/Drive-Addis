using DriveAddis.Application.Auth.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class PasswordStrengthTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

    private RegisterHandler CreateSut() =>
        new(_userRepository, _passwordHasher, _jwtTokenGenerator, _refreshTokenRepository);

    [Theory]
    [InlineData("Pass1!")] // length 6 (< 8)
    [InlineData("Pass12!")] // length 7 (just below boundary 8)
    [InlineData("pass1234!")] // missing uppercase
    [InlineData("PASS1234!")] // missing lowercase
    [InlineData("Password!")] // missing digit
    [InlineData("Password123")] // missing special character
    [InlineData("")] // empty string
    [InlineData("   ")] // whitespace
    public async Task Register_WeakPasswordBoundary_ReturnsValidationErrorAndDoesNotSave(string password)
    {
        // Arrange
        var sut = CreateSut();
        var command = new RegisterCommand(
            PhoneNumber: "+251911999999",
            Password: password,
            FullName: "Test Student",
            Role: UserRole.Student,
            HourlyPrice: null,
            Latitude: null,
            Longitude: null,
            LicensePhotoUrl: null);

        _userRepository.GetByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Password must be at least 8 characters long", result.Error);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("P@ssw0rd")] // exactly 8 characters (boundary)
    [InlineData("P@ssw0rd9")] // 9 characters (boundary + 1)
    [InlineData("SuperSecure#2026!Addis")] // complex long password
    public async Task Register_ValidPasswordStrength_PassesValidationAndSavesUser(string password)
    {
        // Arrange
        var sut = CreateSut();
        var command = new RegisterCommand(
            PhoneNumber: "+251911888888",
            Password: password,
            FullName: "Valid Student",
            Role: UserRole.Student,
            HourlyPrice: null,
            Latitude: null,
            Longitude: null,
            LicensePhotoUrl: null);

        _userRepository.GetByPhoneNumberAsync(command.PhoneNumber, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.Hash(password).Returns("hashed_password_123");
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>())
            .Returns(new AuthTokens { AccessToken = "access_token", RefreshToken = "refresh_token", JwtId = "jwt_id_1" });

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PhoneNumber == command.PhoneNumber && u.Role == UserRole.Student),
            Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
