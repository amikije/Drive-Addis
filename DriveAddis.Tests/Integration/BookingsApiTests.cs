using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DriveAddis.Application.Dtos;
using Xunit;

namespace DriveAddis.Tests.Integration;

public class BookingsApiTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetBookings_Anonymous_Returns401Unauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.Add("X-Test-Role", "none");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetBookings_AuthenticatedStudent_Returns200WithList()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.Add("X-Test-Role", "Student");
        request.Headers.Add("X-Test-StudentId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_Anonymous_Returns401Unauthorized()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new { instructorId = 1, scheduledAt = DateTime.UtcNow.AddDays(2) })
        };
        request.Headers.Add("X-Test-Role", "none");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_AsInstructor_Returns403Forbidden()
    {
        // Arrange: Instructors cannot book lessons, only students can
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new { instructorId = 1, scheduledAt = DateTime.UtcNow.AddDays(2) })
        };
        request.Headers.Add("X-Test-Role", "Instructor");
        // Remove studentId header to simulate instructor token without studentId claim
        request.Headers.Add("X-Test-StudentId", "");

        // Act
        var response = await _client.SendAsync(request);

        // Assert: 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateBooking_ValidStudentRequest_Returns200AndCreatedBooking()
    {
        // Arrange: Schedule daytime slot 10:00 AM EAT (07:00 UTC) tomorrow
        var tomorrow = DateTime.UtcNow.Date.AddDays(2);
        var scheduledAt = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, 7, 0, 0, DateTimeKind.Utc);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/bookings")
        {
            Content = JsonContent.Create(new
            {
                instructorId = 1,
                scheduledAt
            })
        };
        request.Headers.Add("X-Test-Role", "Student");
        request.Headers.Add("X-Test-StudentId", "1");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var booking = await response.Content.ReadFromJsonAsync<BookingResponseDto>();
        Assert.NotNull(booking);
        Assert.Equal(1, booking.StudentId);
        Assert.Equal(1, booking.InstructorId);
        Assert.Equal("Pending", booking.Status);
    }
}
