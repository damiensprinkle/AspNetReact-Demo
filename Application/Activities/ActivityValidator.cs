using Application.DTOs;
using FluentValidation;

namespace Application.Activities
{
    public class ActivityValidator : AbstractValidator<ActivityFormDto>
    {
        public ActivityValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.MinValue);
            RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
            RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
            RuleFor(x => x.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
        }
    }
}
