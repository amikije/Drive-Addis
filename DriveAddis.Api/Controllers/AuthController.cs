using DriveAddis.Application.Auth.Commands;
using DriveAddis.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DriveAddis.Api.Controllers;


[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }
    public static object MapToResponse(AuthResponseDto authResponse)
    {
        return new
        {
            authResponse.Token,
            authResponse.Role,
            authResponse.UserId
        };
    }
}