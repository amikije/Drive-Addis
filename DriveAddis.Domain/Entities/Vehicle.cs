namespace DriveAddis.Domain.Entities;

public enum VehicleType
{
    Manual,
    Automatic
}

public class Vehicle
{
    public int Id { get; set; }
    public int InstructorId { get; set; }
    public Instructor Instructor { get; set; } = null!;
    public VehicleType Type { get; set; }
    public string Model { get; set; } = string.Empty;
    public string PlateNumber { get; set; } = string.Empty;
}