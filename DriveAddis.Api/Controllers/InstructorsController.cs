using DriveAddis.Application.Dtos;
using DriveAddis.Application.Instructors.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstructorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
         [FromQuery] string? name,
       
        [FromQuery] decimal? maxPrice,
        [FromQuery] double? minRating,
        [FromQuery] string? vehicleType, 
        [FromQuery] double lat,
        [FromQuery] double lng,
        CancellationToken ct)
    {
        var query = new SearchInstructorsQuery(name,maxPrice, minRating, vehicleType, lat, lng);
        var results = await _mediator.Send(query, ct);
        return Ok(results);

    }
    public static object MapToResponse(InstructorSearchResultDto instructor)
    {
        return new
        {
            instructor.Id,
            instructor.FullName,
            instructor.HourlyPrice,
            instructor.AverageRating,
            instructor.DistanceKm,
            VehicleTypes = instructor.VehicleTypes
        };
    }
}