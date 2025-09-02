
using FluentValidation;
using TaskBoard.Api.Models;

namespace TaskBoardAPI.TaskBoard.Api.Validators;

public class UserLoginValidator : AbstractValidator<LoginRequest>
{
    public UserLoginValidator()
    {
        RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Email is required")
               .EmailAddress().WithMessage("Invalid email format")
               .Length(5, 100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(20).WithMessage("Password cannot exceed 20 characters.")
            // must contain at least one uppercase
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            // must contain at least one lowercase
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            // must contain at least one digit
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            // only allow specific special characters
            .Matches(@"^[a-zA-Z0-9!@#$%^&*]+$")
            .WithMessage("Password can only contain letters, numbers, and the special characters ! @ # $ % ^ & *.")
            // must contain at least one allowed special char
            .Matches(@"[!@#$%^&*]").WithMessage("Password must contain at least one special character (!@#$%^&*).");
    }
}
