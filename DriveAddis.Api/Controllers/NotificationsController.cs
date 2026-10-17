using DriveAddis.Application.Notifications.Commands;
using DriveAddis.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int? instructorId, CancellationToken ct)
    {
        // If instructorId not in query, try to get from token claims
        if (!instructorId.HasValue)
        {
            var claimVal = User.FindFirst("instructorId")?.Value;
            if (int.TryParse(claimVal, out var parsedInstructorId))
            {
                instructorId = parsedInstructorId;
            }
        }

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;

        var query = new GetNotificationsQuery(instructorId, userId);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var command = new MarkNotificationReadCommand(id);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = result.Error });
    }

    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] int? instructorId, CancellationToken ct)
    {
        if (!instructorId.HasValue)
        {
            var claimVal = User.FindFirst("instructorId")?.Value;
            if (int.TryParse(claimVal, out var parsedInstructorId))
            {
                instructorId = parsedInstructorId;
            }
        }

        if (!instructorId.HasValue)
            return BadRequest(new { error = "Instructor ID required." });

        var command = new MarkAllNotificationsReadCommand(instructorId.Value);
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { success = true })
            : BadRequest(new { error = result.Error });
    }
}
