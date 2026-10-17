using DriveAddis.Domain.Entities;
using DriveAddis.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DriveAddis.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly SqliteConnection _keepAliveConnection;

    public CustomWebApplicationFactory()
    {
        // Shared-cache in-memory SQLite enables independent connections per scope/thread
        var dbName = "driveaddis_mem_" + Guid.NewGuid().ToString("N");
        _connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "SuperSecretDriveAddisIntegrationTestKey2026!MustBeVeryLongForHmac",
                ["Jwt:Issuer"] = "DriveAddisTestIssuer",
                ["Jwt:Audience"] = "DriveAddisTestAudience"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext configurations
            services.RemoveAll<DbContextOptions<DriveAddisDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DriveAddisDbContext>();

            // Add SQLite DbContext using shared in-memory connection string
            services.AddDbContext<DriveAddisDbContext>(options =>
            {
                options.UseSqlite(_connectionString);
            });

            // Replace JWT authentication with TestAuthHandler
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

            // Initialize schema and seed deterministic test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DriveAddisDbContext>();

            db.Database.EnsureCreated();

            // Seed student
            if (!db.Students.Any())
            {
                db.Students.Add(new Student
                {
                    Id = 1,
                    FullName = "Test Student Abeba",
                    PhoneNumber = "+251911001122",
                    LicenseNumber = "LIC-TEST-001"
                });
            }

            // Seed verified instructor
            if (!db.Instructors.Any())
            {
                var instructor = new Instructor
                {
                    Id = 1,
                    FullName = "Test Instructor Haile",
                    PhoneNumber = "+251922334455",
                    VerificationStatus = VerificationStatus.Verified,
                    HourlyPrice = 350m,
                    Latitude = 9.0192,
                    Longitude = 38.7525,
                    AverageRating = 4.8,
                    Vehicles = new List<Vehicle>
                    {
                        new() { Type = VehicleType.Manual, Model = "Toyota Yaris", PlateNumber = "AA-99999" }
                    }
                };
                db.Instructors.Add(instructor);
            }

            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAliveConnection.Dispose();
        }
    }
}
