using DriveAddis.Application.Admin.Queries;
using DriveAddis.Application.Bookings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDashboardQuery(), ct);
        return Ok(result);
    }

    [HttpGet("instructors")]
    public async Task<IActionResult> GetInstructors([FromQuery] bool? unverifiedOnly, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInstructorsForAdminQuery(unverifiedOnly), ct);
        return Ok(result);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetAllBookings(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBookingsQuery(null, null), ct);
        return Ok(result);
    }
}