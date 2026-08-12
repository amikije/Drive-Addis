using DriveAddis.Application.Common;
using DriveAddis.Application.Dtos;
using DriveAddis.Application.Interfaces;
using DriveAddis.Domain.Entities;
using MediatR;

namespace DriveAddis.Application.Reviews.Commands;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, Result<ReviewResponseDto>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IReviewRepository _reviewRepository;

    public CreateReviewHandler(
        IBookingRepository bookingRepository,
        IInstructorRepository instructorRepository,
        IReviewRepository reviewRepository)
    {
        _bookingRepository = bookingRepository;
        _instructorRepository = instructorRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<Result<ReviewResponseDto>> Handle(CreateReviewCommand request, CancellationToken ct)
    {
        if (request.Rating < 1 || request.Rating > 5)
            return Result<ReviewResponseDto>.Failure("Rating must be between 1 and 5.");

        var booking = await _bookingRepository.GetByIdAsync(request.BookingId, ct);

        if (booking is null)
            return Result<ReviewResponseDto>.Failure("Booking not found.");

        if (booking.Status != BookingStatus.Completed)
            return Result<ReviewResponseDto>.Failure("Only completed bookings can be reviewed.");
        if (booking.StudentId != request.StudentId)
            return Result<ReviewResponseDto>.Failure("You can only review your own bookings.");
        var existingReview = await _reviewRepository.GetByBookingIdAsync(request.BookingId, ct);
        if (existingReview is not null)
            return Result<ReviewResponseDto>.Failure("This booking already has a review.");

        var review = new Review
        {
            BookingId = booking.Id,
            InstructorId = booking.InstructorId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review, ct);
        await _reviewRepository.SaveChangesAsync(ct);

        // Recalculate instructor's average rating now that a new review exists
        await _instructorRepository.UpdateAverageRatingAsync(booking.InstructorId, ct);

        return Result<ReviewResponseDto>.Success(new ReviewResponseDto
        {
            Id = review.Id,
            BookingId = review.BookingId,
            InstructorId = review.InstructorId,
            Rating = review.Rating,
            Comment = review.Comment
        });
    }
}