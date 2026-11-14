using Media.Domain;
using Media.Domain.Folders;

namespace Media.Application.Features.Folders.Commands.EnsureMediaFolderPath;

public sealed class EnsureMediaFolderPathValidator : AbstractValidator<EnsureMediaFolderPathCommand>
{
    public EnsureMediaFolderPathValidator()
    {
        RuleFor(x => x.Names).NotEmpty()
            .Must(x => x.Count <= MediaFolderTree.MaxDepth).WithMessage($"Не більше {MediaFolderTree.MaxDepth} рівнів папок.");
        RuleForEach(x => x.Names)
            .Must(x => MediaNames.IsValid(x, MediaFolder.NameMaxLength))
            .WithMessage($"Назва папки обов'язкова, до {MediaFolder.NameMaxLength} символів, без / і \\.");
    }
}
