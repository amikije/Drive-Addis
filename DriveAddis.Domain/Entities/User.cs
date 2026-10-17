using Microsoft.AspNetCore.Identity;

namespace DriveAddis.Domain.Entities;

public enum UserRole
{
    Student,
    Instructor,
    Admin
}

public class User : IdentityUser<int>
{
    // Id, PhoneNumber, and PasswordHash are inherited from IdentityUser<int>
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Linked profile — only relevant if Role is Student or Instructor.
    // An Admin user has both of these set to null.
    public int? StudentId { get; set; }
    public Student? Student { get; set; }

    public int? InstructorId { get; set; }
    public Instructor? Instructor { get; set; }
}