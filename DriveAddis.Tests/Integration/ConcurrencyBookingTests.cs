using System.Net;
using System.Net.Http.Json;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DriveAddis.Tests.Integration;

public class ConcurrencyBookingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Register_TwoSimultaneousRequestsWithSamePhone_CreatesAtMostOneUser()
    {
        // Arrange: Prepare two simultaneous registration calls with the exact same phone number
        var client1 = factory.CreateClient();
        var client2 = factory.CreateClient();
        var uniquePhone = $"+2519{Random.Shared.Next(10000000, 99999999)}";

        var payload = new
        {
            phoneNumber = uniquePhone,
            password = "SecurePassword123!",
            fullName = "Concurrency Test User",
            role = "Student"
        };

        // Act: Fire both requests in parallel to simulate a double-click race condition
        var task1 = client1.PostAsJsonAsync("/api/auth/register", payload);
        var task2 = client2.PostAsJsonAsync("/api/auth/register", payload);

        var responses = await Task.WhenAll(task1, task2);

        // Assert: Exactly one request succeeds with 200 OK, and the race collision gets 400 BadRequest
        var successCount = responses.Count(r => r.IsSuccessStatusCode);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal(1, successCount);
        Assert.Equal(1, conflictCount);

        // Verify database state: Exactly one user exists with this phone number
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DriveAddisDbContext>();
        var userCount = db.Users.Count(u => u.PhoneNumber == uniquePhone);
        Assert.Equal(1, userCount);
    }
}
