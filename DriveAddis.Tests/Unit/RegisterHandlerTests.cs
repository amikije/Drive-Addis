using DriveAddis.Application.Auth.Commands;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using DriveAddis.Tests.Helpers;
using NSubstitute;
using Xunit;

namespace DriveAddis.Tests.Unit;

public class RegisterHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

    private RegisterHandler CreateSut() =>
        new(_userRepository, _passwordHasher, _jwtTokenGenerator, _refreshTokenRepository);

    [Fact]
    public async Task Handle_DuplicatePhoneNumber_ReturnsFailureAndNeverSaves()
    {
        // Arrange
        var sut = CreateSut();
        var existingUser = new User { Id = 1, PhoneNumber = "+251911223344" };
        _userRepository.GetByPhoneNumberAsync("+251911223344", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var command = new RegisterCommand(
            PhoneNumber: "+251911223344",
            Password: "SecurePassword123!",
            FullName: "Existing Person",
            Role: UserRole.Student,
            HourlyPrice: null,
            Latitude: null,
            Longitude: null,
            LicensePhotoUrl: null);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Phone number is already registered.", result.Error);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AdminSelfRegistration_ReturnsSecurityRejection()
    {
        // Arrange
        var sut = CreateSut();
        _userRepository.GetByPhoneNumberAsync("+251900112233", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new RegisterCommand(
            PhoneNumber: "+251900112233",
            Password: "SuperAdminPassword123!",
            FullName: "Malicious Attacker",
            Role: UserRole.Admin,
            HourlyPrice: null,
            Latitude: null,
            Longitude: null,
            LicensePhotoUrl: null);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Cannot self-register as Admin.", result.Error);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InstructorWithoutLicensePhoto_ReturnsValidationError()
    {
        // Arrange
        var sut = CreateSut();
        _userRepository.GetByPhoneNumberAsync("+251933445566", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new RegisterCommand(
            PhoneNumber: "+251933445566",
            Password: "InstructorPass123!",
            FullName: "New Instructor",
            Role: UserRole.Instructor,
            HourlyPrice: 500,
            Latitude: 9.0,
            Longitude: 38.7,
            LicensePhotoUrl: null); // Missing license photo

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("License photo is required for instructor registration.", result.Error);
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidStudentRegistration_CreatesStudentProfileAndPersistsUser()
    {
        // Arrange
        var sut = CreateSut();
        _userRepository.GetByPhoneNumberAsync("+251911556677", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _passwordHasher.Hash("ValidStudentPass123!").Returns("hashed_pass_xyz");
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>())
            .Returns(new AuthTokens { AccessToken = "access_token", RefreshToken = "refresh_token", JwtId = "jwt_id_10" });

        var command = new RegisterCommand(
            PhoneNumber: "+251911556677",
            Password: "ValidStudentPass123!",
            FullName: "Kenenisa Bekele",
            Role: UserRole.Student,
            HourlyPrice: null,
            Latitude: null,
            Longitude: null,
            LicensePhotoUrl: null);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Kenenisa Bekele", result.Value.FullName);
        Assert.Equal("Student", result.Value.Role);

        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PhoneNumber == "+251911556677" && u.Student != null && u.Student.FullName == "Kenenisa Bekele"),
            Arg.Any<CancellationToken>());
        await _userRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
