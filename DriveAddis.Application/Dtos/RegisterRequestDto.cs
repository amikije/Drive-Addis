using DriveAddis.Domain.Entities;

namespace DriveAddis.Application.Dtos;

public class RegisterRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    // Only used if Role == Instructor
    public decimal? HourlyPrice { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}