using FluentValidation;
using TaskBoard.Api.Models;

public class ColumnSwitchValidator : AbstractValidator<ColumnsSwitchDto>
{
    public ColumnSwitchValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task Id is required");

        RuleFor(x => x.ColumnId)
            .NotEmpty().WithMessage("Column Id is required");
    }
}
