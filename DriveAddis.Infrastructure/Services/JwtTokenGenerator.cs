using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DriveAddis.Infrastructure.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.MobilePhone, user.PhoneNumber),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Attach whichever profile ID is relevant, so endpoints can identify "which student/instructor is this?"
        if (user.StudentId.HasValue)
            claims.Add(new Claim("studentId", user.StudentId.Value.ToString()));

        if (user.InstructorId.HasValue)
            claims.Add(new Claim("instructorId", user.InstructorId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}