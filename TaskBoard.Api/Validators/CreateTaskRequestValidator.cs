using FluentValidation;
using TaskBoard.Api.Models;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(3, 100);

        RuleFor(x => x.Description)
            .MaximumLength(3000);

        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("ColumnId is required");
    }
}
