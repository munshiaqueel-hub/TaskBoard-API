using FluentValidation;

public class FileCollectionValidator : AbstractValidator<IFormFileCollection>
{
     private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" }; // allowed types
    private const long _maxFileSize = 5 * 1024 * 1024; // 5 MB
    public FileCollectionValidator()
    {
        RuleFor(x => x)
            .NotEmpty().WithMessage("At least one file is required!");

        RuleForEach(x => x).ChildRules(file =>
        {
            file.RuleFor(f => f.Length)
                .LessThanOrEqualTo(_maxFileSize)
                .WithMessage(f => $"File '{f.FileName}' exceeds the maximum allowed size of 5 MB.");

            file.RuleFor(f => f.FileName)
                .Must(name => _allowedExtensions.Any(ext =>
                    name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage(f => $"File '{f.FileName}' has an invalid file type. Allowed types: {string.Join(", ", _allowedExtensions)}");
        });
    }
}
