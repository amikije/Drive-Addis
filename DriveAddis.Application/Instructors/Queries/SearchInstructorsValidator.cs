using FluentValidation;

namespace DriveAddis.Application.Instructors.Queries;

public class SearchInstructorsValidator : AbstractValidator<SearchInstructorsQuery>
{
    public SearchInstructorsValidator()
    {
        RuleFor(x => x.StudentLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.StudentLongitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.MinRating).InclusiveBetween(0, 5).When(x => x.MinRating.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThan(0).When(x => x.MaxPrice.HasValue);
    }
}