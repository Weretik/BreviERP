namespace Catalog.Application.Features.Media.Upload.Validators;

public sealed class UploadMediaFileCommandValidator : AbstractValidator<UploadMediaFileCommand>
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

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
            .MaximumLength(100)
            .Must(AllowedContentTypes.Contains)
            .WithMessage("Only image/jpeg, image/png, image/webp are allowed.");

        RuleFor(x => x.Request.BaseFolder)
            .NotEmpty()
            .MaximumLength(300);

        RuleFor(x => x.Request.SizeInBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(x => x.Request.MaxFileSizeBytes)
            .WithMessage(x => $"File size must not exceed {x.Request.MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.Request.MaxFileSizeBytes)
            .GreaterThan(0);

        RuleFor(x => x.Request.FileStream)
            .MustAsync(async (command, stream, context, cancellationToken) =>
                await ImageFileSignatureValidator.IsValidAsync(
                    stream,
                    command.Request.ContentType,
                    cancellationToken))
            .WithMessage("File content does not match the declared image type.")
            .When(x =>
                x.Request.FileStream is not null &&
                AllowedContentTypes.Contains(x.Request.ContentType));
    }
}
