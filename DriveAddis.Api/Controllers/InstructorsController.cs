using DriveAddis.Application.Dtos;
using DriveAddis.Application.Instructors.Commands;
using DriveAddis.Application.Instructors.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
      [FromQuery] double? lat,
      [FromQuery] double? lng,
      [FromQuery] decimal? maxPrice,
      [FromQuery] double? minRating,
      [FromQuery] string? vehicleType,
      [FromQuery] string? name,
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10,
      CancellationToken ct = default)
    {
        var query = new SearchInstructorsQuery(lat, lng, maxPrice, minRating, vehicleType, name, pageNumber, pageSize);
        var results = await _mediator.Send(query, ct);
        return Ok(results);
    }
    [HttpPatch("{id}/verify")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Verify(int id, CancellationToken ct)
    {
        var command = new VerifyInstructorCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { message = "Instructor verified." })
            : BadRequest(new { error = result.Error });
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