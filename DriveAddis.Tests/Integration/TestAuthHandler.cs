using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveAddis.Tests.Integration;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public new const string Scheme = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Support header: "X-Test-Role: Admin|Instructor|Student|none"
        var role = Request.Headers["X-Test-Role"].FirstOrDefault() ?? "Student";

        if (string.Equals(role, "none", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "test-user"),
            new(ClaimTypes.Role, role)
        };

        if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            var studentId = Request.Headers["X-Test-StudentId"].FirstOrDefault() ?? "1";
            if (!string.IsNullOrEmpty(studentId))
                claims.Add(new("studentId", studentId));
        }
        else if (string.Equals(role, "Instructor", StringComparison.OrdinalIgnoreCase))
        {
            var instructorId = Request.Headers["X-Test-InstructorId"].FirstOrDefault() ?? "1";
            if (!string.IsNullOrEmpty(instructorId))
                claims.Add(new("instructorId", instructorId));
        }

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
