namespace DriveAddis.Application.Dtos;

public class StudentAdminListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? LicenseNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalBookings { get; set; }
}
