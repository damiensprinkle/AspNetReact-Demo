using FluentValidation;

namespace Application.Activities
{
    public class EditValidator : AbstractValidator<Edit.Command>
    {
        public EditValidator()
        {
            RuleFor(x => x.Activity).SetValidator(new ActivityValidator());
        }
    }
}
