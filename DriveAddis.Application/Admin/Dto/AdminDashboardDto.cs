namespace DriveAddis.Application.Dtos;

public class AdminDashboardDto
{
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
    public int VerifiedInstructors { get; set; }
    public int UnverifiedInstructors { get; set; }
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int TotalReviews { get; set; }
    public double PlatformAverageRating { get; set; }
}