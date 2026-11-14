using Media.Application.Abstractions;
using Media.Domain;
using Media.Domain.Files;

namespace Media.Application.Features.Files.Commands.UploadMediaFile;

public sealed class UploadMediaFileValidator : AbstractValidator<UploadMediaFileCommand>
{
    public UploadMediaFileValidator(MediaUploadSettings settings)
    {
        var maxMegabytes = settings.MaxFileSizeBytes / (1024 * 1024);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.DisplayName)
            .Must(x => MediaNames.IsValid(x, MediaFile.FileNameMaxLength))
            .WithMessage($"Назва файлу обов'язкова, до {MediaFile.FileNameMaxLength} символів.");
        RuleFor(x => x.ContentType)
            .Must(x => x is not null && UploadMediaFileCommand.Extensions.ContainsKey(x))
            .WithMessage("Можна завантажувати лише JPG, PNG або WebP.");
        RuleFor(x => x.SizeInBytes)
            .GreaterThan(0).WithMessage("Файл порожній.")
            .LessThanOrEqualTo(settings.MaxFileSizeBytes).WithMessage($"Фото має бути не більше {maxMegabytes} МБ.");
        RuleFor(x => x.SourceKey).MaximumLength(MediaFile.SourceKeyMaxLength);
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        RuleFor(x => x.Content)
            .MustAsync((command, stream, cancellationToken) => ImageFileSignature.IsValidAsync(stream, command.ContentType, cancellationToken))
            .WithMessage("Вміст файлу не відповідає типу зображення.")
            .When(x => x.Content is not null && x.ContentType is not null && UploadMediaFileCommand.Extensions.ContainsKey(x.ContentType));
    }
}
