using DriveAddis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveAddis.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.HasOne(u => u.Student)
            .WithOne()
            .HasForeignKey<User>(u => u.StudentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Instructor)
            .WithOne()
            .HasForeignKey<User>(u => u.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}