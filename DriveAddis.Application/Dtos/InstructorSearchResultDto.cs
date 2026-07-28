namespace DriveAddis.Application.Dtos;

public class InstructorSearchResultDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal HourlyPrice { get; set; }
    public double AverageRating { get; set; }
    public double DistanceKm { get; set; }
    public List<string> VehicleTypes { get; set; } = new();
}