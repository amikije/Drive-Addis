using DriveAddis.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DriveAddis.Infrastructure.Persistence;

public class DriveAddisDbContext : DbContext
{
    public DriveAddisDbContext(DbContextOptions<DriveAddisDbContext> options)
        : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriveAddisDbContext).Assembly);
    }
}