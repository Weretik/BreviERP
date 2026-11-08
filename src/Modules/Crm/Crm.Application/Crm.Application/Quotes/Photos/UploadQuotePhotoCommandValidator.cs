namespace Crm.Application.Quotes.Photos;

public sealed class UploadQuotePhotoCommandValidator : AbstractValidator<UploadQuotePhotoCommand>
{
    public static readonly IReadOnlySet<string> ContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public UploadQuotePhotoCommandValidator()
    {
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).Must(x => x is not null && ContentTypes.Contains(x))
            .WithMessage("Завантажте фото JPG, PNG або WebP.");
        RuleFor(x => x.SizeInBytes).GreaterThan(0).WithMessage("Файл порожній.");
    }
}
