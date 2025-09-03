using FluentValidation;
using TaskBoard.Api.Models;

public class AddColumnRequestValidator : AbstractValidator<CreateColumnDto>
{
    public AddColumnRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100);
    }
}
