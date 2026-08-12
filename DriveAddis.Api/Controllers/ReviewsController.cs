using System.Security.Claims;
using DriveAddis.Application.Reviews.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewCommand command, CancellationToken ct)
    {
        var studentIdClaim = User.FindFirst("studentId")?.Value;

        if (studentIdClaim is null)
            return Forbid();

        var fullCommand = command with { StudentId = int.Parse(studentIdClaim) };

        var result = await _mediator.Send(fullCommand, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
}