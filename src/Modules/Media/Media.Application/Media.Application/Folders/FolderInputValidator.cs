using Media.Application.Contracts;
using Media.Domain;
using Media.Domain.Folders;

namespace Media.Application.Folders;

internal sealed class FolderInputValidator : AbstractValidator<MediaFolderInput>
{
    public FolderInputValidator()
    {
        RuleFor(x => x.Name)
            .Must(x => MediaNames.IsValid(x, MediaFolder.NameMaxLength))
            .WithMessage($"Назва папки обов'язкова, до {MediaFolder.NameMaxLength} символів, без / і \\.");
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
    }
}
