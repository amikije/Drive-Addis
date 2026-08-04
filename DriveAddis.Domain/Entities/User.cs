namespace DriveAddis.Domain.Entities;

public enum UserRole
{
    Student,
    Instructor,
    Admin
}

public class User
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Linked profile — only relevant if Role is Student or Instructor.
    // An Admin user has both of these set to null.
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
}