using FluentValidation;
using TaskBoard.Api.Models;

public class EditTaskRequestValidator : AbstractValidator<EditTaskDto>
{
    public EditTaskRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100);

        RuleFor(x => x.Description)
            .MaximumLength(3000);
    }
}
