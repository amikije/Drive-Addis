using DriveAddis.Application.Interfaces;
using System.Text.Json.Serialization;

namespace DriveAddis.Application.Dtos;

public class AuthResponseDto
{
    [JsonIgnore] // Don't send this in the JSON response body (managed via HttpOnly cookies)
    public AuthTokens Tokens { get; set; } = null!;
    
    public string Role { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int? StudentId { get; set; }
    public int? InstructorId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}