namespace DriveAddis.Application.Dtos;

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int InstructorId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}