namespace DriveAddis.Application.Dtos;

public class BookingResponseDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int InstructorId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
}