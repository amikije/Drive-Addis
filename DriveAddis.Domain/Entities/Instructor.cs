namespace DriveAddis.Domain.Entities;

public enum VerificationStatus
{
    Pending,
    Verified,
    Rejected
}

public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
    public string? RejectionReason { get; set; }
    public decimal HourlyPrice { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AverageRating { get; set; } = 0;
    public string? LicensePhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}