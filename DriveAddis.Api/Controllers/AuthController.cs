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

        if (result.IsSuccess)
        {
            SetTokenCookies(result.Value.Tokens);
            return Ok(result.Value);
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            SetTokenCookies(result.Value.Tokens);
            return Ok(result.Value);
        }
        return Unauthorized(new { error = result.Error });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.PhoneNumber), ct);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new VerifyResetOtpCommand(request.PhoneNumber, request.Code), ct);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { error = result.Error });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(request.ResetToken, request.NewPassword), ct);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { error = result.Error });
    }

    private void SetTokenCookies(DriveAddis.Application.Interfaces.AuthTokens tokens)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Set to false for local HTTP testing. Use true in production.
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddMinutes(15)
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("AccessToken", tokens.AccessToken, cookieOptions);
        Response.Cookies.Append("RefreshToken", tokens.RefreshToken, refreshCookieOptions);
    }
}