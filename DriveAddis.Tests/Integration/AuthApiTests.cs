using System.Net;
using System.Net.Http.Json;
using DriveAddis.Domain.Entities;
using Xunit;

namespace DriveAddis.Tests.Integration;

public class AuthApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WeakPassword_Returns400BadRequest()
    {
        // Arrange: Short password (< 8 chars)
        var payload = new
        {
            phoneNumber = "+251911990011",
            password = "short",
            fullName = "Insecure User",
            role = "Student"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_AttemptAdminSelfRegistration_Returns400BadRequest()
    {
        // Arrange: Attempting to register as Admin
        var payload = new
        {
            phoneNumber = "+251900000099",
            password = "ValidAdminPassword123!",
            fullName = "Fake Admin",
            role = "Admin"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401Unauthorized()
    {
        // Arrange
        var payload = new
        {
            phoneNumber = "+251999999999",
            password = "NonExistentPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", payload);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
