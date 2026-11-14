namespace Crm.Application.Features.Quotes.Commands.UploadQuotePhoto;

public sealed class UploadQuotePhotoValidator : AbstractValidator<UploadQuotePhotoCommand>
{
    public static readonly IReadOnlySet<string> ContentTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    public UploadQuotePhotoValidator()
    {
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ContentType).Must(x => x is not null && ContentTypes.Contains(x))
            .WithMessage("Завантажте фото JPG, PNG або WebP.");
        RuleFor(x => x.SizeInBytes).GreaterThan(0).WithMessage("Файл порожній.");
    }
}
