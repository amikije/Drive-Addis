using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Bookings.Queries;
using DriveAddis.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        // Extract studentId from JWT claims (set by JwtTokenGenerator)
        var studentIdClaim = User.FindFirst("studentId")?.Value;

        if (string.IsNullOrEmpty(studentIdClaim) || !int.TryParse(studentIdClaim, out var studentId))
            return StatusCode(403, new { error = "Only students can book lessons. Please log in as a student." });

        var command = new CreateBookingCommand(request.InstructorId, request.ScheduledAt)
        {
            StudentId = studentId
        };

        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken ct)
    {
        var command = new UpdateBookingStatusCommand(id, request.Status);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet]
    public async Task<IActionResult> GetBookings(
        [FromQuery] int? studentId,
        [FromQuery] int? instructorId,
        CancellationToken ct)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // Security / Data Isolation: Prevent students from seeing other students' bookings
        if (role == "Student")
        {
            var studentIdClaim = User.FindFirst("studentId")?.Value;
            if (int.TryParse(studentIdClaim, out var claimStudentId))
            {
                studentId = claimStudentId;
                instructorId = null; // Students cannot query by instructorId across other students
            }
        }
        // Prevent instructors from seeing other instructors' bookings
        else if (role == "Instructor")
        {
            var instructorIdClaim = User.FindFirst("instructorId")?.Value;
            if (int.TryParse(instructorIdClaim, out var claimInstructorId))
            {
                instructorId = claimInstructorId;
                studentId = null; // Instructors view all bookings assigned to them
            }
        }

        var query = new GetBookingsQuery(studentId, instructorId);
        var results = await _mediator.Send(query, ct);
        return Ok(results);
    }

    // Request DTOs — separate from MediatR commands
    public record CreateBookingRequest(int InstructorId, DateTime ScheduledAt);
    public record UpdateStatusRequest(BookingStatus Status);
}