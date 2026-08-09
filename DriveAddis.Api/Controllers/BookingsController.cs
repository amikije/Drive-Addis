using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Bookings.Queries;
using DriveAddis.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DriveAddis.Application.Dtos;
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
    public async Task<IActionResult> Create([FromBody] CreateBookingCommand command, CancellationToken ct)
    {
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
        var query = new GetBookingsQuery(studentId, instructorId);
        var results = await _mediator.Send(query, ct);
        return Ok(results);
    }

    private static object MapToResponse(BookingListItemDto booking)
    {
        return new
        {
            booking.Id,
            booking.StudentName,
            booking.InstructorName,
            booking.ScheduledAt,
            booking.Status
        };
    }

    public record UpdateStatusRequest(BookingStatus Status);
}