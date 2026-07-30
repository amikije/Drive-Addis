using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using MediatR;

namespace DriveAddis.Application.Reviews.Commands;

public record CreateReviewCommand(
    int BookingId,
    int Rating,
    string? Comment
) : IRequest<Result<ReviewResponseDto>>;