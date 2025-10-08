namespace Company.Project.Application.Features.UploadFile;

public sealed class UploadValidator : AbstractValidator<UploadParameters>
{
    public UploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Length(5, 100)
            .WithMessage("File name must be between 5 and 100 characters");
    }
}
