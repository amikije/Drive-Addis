using DriveAddis.Application.Auth.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class LoginHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

    private LoginHandler CreateSut() =>
        new(_userRepository, _passwordHasher, _jwtTokenGenerator, _refreshTokenRepository);

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailureAndNeverGeneratesToken()
    {
        // Arrange
        var sut = CreateSut();
        _userRepository.GetByPhoneNumberAsync("+251900000000", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginCommand("+251900000000", "AnyPassword123!");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid phone number or password.", result.Error);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>());
        await _refreshTokenRepository.DidNotReceive().AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FirstFailedAttempt_IncrementsFailedCountWithoutLockout()
    {
        // Arrange
        var sut = CreateSut();
        var user = new User
        {
            Id = 1,
            PhoneNumber = "+251911111111",
            PasswordHash = "correct_hash",
            AccessFailedCount = 0
        };

        _userRepository.GetByPhoneNumberAsync(user.PhoneNumber, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong_pass", "correct_hash").Returns(false);

        var command = new LoginCommand(user.PhoneNumber, "wrong_pass");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid phone number or password.", result.Error);
        Assert.Equal(1, user.AccessFailedCount);
        Assert.Null(user.LockoutEnd);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1, 3)] // was 1 failed attempt, becomes 2 -> reveals 3 attempts remaining
    [InlineData(2, 2)] // was 2 failed attempts, becomes 3 -> reveals 2 attempts remaining
    [InlineData(3, 1)] // was 3 failed attempts, becomes 4 -> reveals 1 attempt remaining
    public async Task Handle_SubsequentFailedAttempt_ShowsRemainingAttemptsWarning(int currentFailed, int expectedRemaining)
    {
        // Arrange
        var sut = CreateSut();
        var user = new User
        {
            Id = 2,
            PhoneNumber = "+251911222333",
            PasswordHash = "correct_hash",
            AccessFailedCount = currentFailed
        };

        _userRepository.GetByPhoneNumberAsync(user.PhoneNumber, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong_pass", "correct_hash").Returns(false);

        var command = new LoginCommand(user.PhoneNumber, "wrong_pass");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains($"You have {expectedRemaining} attempt(s) remaining", result.Error);
    }

    [Fact]
    public async Task Handle_FifthFailedAttempt_LocksAccountForFifteenMinutes()
    {
        // Arrange: 4 failed attempts previously (boundary before lockout)
        var sut = CreateSut();
        var user = new User
        {
            Id = 3,
            PhoneNumber = "+251911444555",
            PasswordHash = "correct_hash",
            AccessFailedCount = 4
        };

        _userRepository.GetByPhoneNumberAsync(user.PhoneNumber, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong_pass", "correct_hash").Returns(false);

        var command = new LoginCommand(user.PhoneNumber, "wrong_pass");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(5, user.AccessFailedCount);
        Assert.NotNull(user.LockoutEnd);
        Assert.True(user.LockoutEnd.Value > DateTimeOffset.UtcNow);
        Assert.Contains("temporarily locked because of too many failed login attempts", result.Error);
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ActiveLockout_BlocksLoginImmediatelyWithoutCheckingPassword()
    {
        // Arrange
        var sut = CreateSut();
        var user = new User
        {
            Id = 4,
            PhoneNumber = "+251911666777",
            PasswordHash = "correct_hash",
            AccessFailedCount = 5,
            LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(10)
        };

        _userRepository.GetByPhoneNumberAsync(user.PhoneNumber, Arg.Any<CancellationToken>()).Returns(user);

        var command = new LoginCommand(user.PhoneNumber, "AnyPassword123!");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("temporarily locked", result.Error);
        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_ResetsLockoutGeneratesTokenAndCreatesRefreshToken()
    {
        // Arrange: User previously had 3 failed attempts
        var sut = CreateSut();
        var user = new User
        {
            Id = 5,
            PhoneNumber = "+251911777888",
            PasswordHash = "hashed_correct",
            Role = UserRole.Student,
            StudentId = 10,
            AccessFailedCount = 3,
            Student = TestDataFactory.CreateStudent(id: 10, fullName: "Tirunesh Dibaba")
        };

        _userRepository.GetByPhoneNumberAsync(user.PhoneNumber, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("CorrectPass123!", "hashed_correct").Returns(true);
        _jwtTokenGenerator.GenerateToken(user)
            .Returns(new AuthTokens { AccessToken = "jwt_access_token", RefreshToken = "jwt_refresh_token", JwtId = "jwt_id_99" });

        var command = new LoginCommand(user.PhoneNumber, "CorrectPass123!");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Tirunesh Dibaba", result.Value.FullName);
        Assert.Equal("Student", result.Value.Role);
        Assert.Equal(0, user.AccessFailedCount); // reset on success
        Assert.Null(user.LockoutEnd);

        // Assert refresh token persisted
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshToken>(r => r.UserId == 5 && r.Token == "jwt_refresh_token" && r.JwtId == "jwt_id_99"),
            Arg.Any<CancellationToken>());
    }
}
