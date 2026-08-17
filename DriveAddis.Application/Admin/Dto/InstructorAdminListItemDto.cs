namespace DriveAddis.Application.Dtos;

public class InstructorAdminListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal HourlyPrice { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}