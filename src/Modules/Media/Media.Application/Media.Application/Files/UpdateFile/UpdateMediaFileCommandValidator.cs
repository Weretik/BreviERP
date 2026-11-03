using Media.Domain;
using Media.Domain.Files;

namespace Media.Application.Files.UpdateFile;

public sealed class UpdateMediaFileCommandValidator : AbstractValidator<UpdateMediaFileCommand>
{
    public UpdateMediaFileCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.FileName)
                .Must(x => MediaNames.IsValid(x, MediaFile.FileNameMaxLength))
                .WithMessage($"Назва фото обов'язкова, до {MediaFile.FileNameMaxLength} символів, без / і \\.");
            RuleFor(x => x.Input.Alt).MaximumLength(MediaFile.AltMaxLength);
            RuleFor(x => x.Input.FolderId).GreaterThan(0).When(x => x.Input.FolderId is not null);
        });
    }
}
