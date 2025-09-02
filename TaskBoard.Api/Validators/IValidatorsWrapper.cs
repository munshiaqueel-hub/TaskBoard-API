using FluentValidation;
namespace TaskBoard.Api.Validators;
public interface IValidatorWrapper<T>
{
    Task<List<FluentValidation.Results.ValidationFailure>?> ValidateAsync(T dto);
}

public class ValidatorWrapper<T> : IValidatorWrapper<T>
{
    private readonly IValidator<T> _validator;

    public ValidatorWrapper(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task<List<FluentValidation.Results.ValidationFailure>?> ValidateAsync(T dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid) return validationResult.Errors;
        return null; 
    }
}