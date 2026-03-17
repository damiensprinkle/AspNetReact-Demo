using FluentValidation;

namespace Application.Activities
{
    public class CreateValidator : AbstractValidator<Create.Command>
    {
        public CreateValidator()
        {
            RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
        }
    }
}
