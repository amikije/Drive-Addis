using DriveAddis.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DriveAddis.Infrastructure.Persistence.Configurations;

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.Property(i => i.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(i => i.PhoneNumber)
            .IsUnique();

        builder.Property(i => i.HourlyPrice)
            .HasPrecision(10, 2); // up to 99,999,999.99

        builder.Property(i => i.AverageRating)
            .HasPrecision(3, 2); // e.g. 4.75
    }
}