namespace DriveAddis.Domain.Entities;

public enum BookingStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
}

public class Booking
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public int InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Review? Review { get; set; }
}