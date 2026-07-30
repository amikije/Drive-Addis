using DriveAddis.Application.Bookings.Commands;
using DriveAddis.Application.Bookings.Queries;
using DriveAddis.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/bookings")]
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

    public record UpdateStatusRequest(BookingStatus Status);
}