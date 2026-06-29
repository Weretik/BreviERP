namespace Catalog.Application.Features.Media.Upload.Validators;

public sealed class UploadMediaFileCommandValidator : AbstractValidator<UploadMediaFileCommand>
{
    public UploadMediaFileCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.FileStream)
            .NotNull();

        RuleFor(x => x.Request.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Request.ContentType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.BaseFolder)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Request.SizeInBytes)
            .GreaterThan(0);
    }
}
