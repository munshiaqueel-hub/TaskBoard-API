using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;

public class AddColumnRequestValidator : AbstractValidator<CreateColumnDto>
{
    public AddColumnRequestValidator(TaskBoardDbContext taskBoardDbContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 100)
            .MustAsync(async (name, cancellation) =>
                !await taskBoardDbContext.Columns.AnyAsync(x => x.Name == name))
            .WithMessage("Column name already exist!.");
    }
}
