using Media.Application.Contracts;

namespace Media.Application.Files.MoveFiles;

public sealed class MoveMediaFilesCommandValidator : AbstractValidator<MoveMediaFilesCommand>
{
    public MoveMediaFilesCommandValidator()
    {
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.FileIds).NotEmpty().Must(x => x.Count <= MoveMediaFilesCommand.MaxFiles)
                .WithMessage($"За раз можна перемістити до {MoveMediaFilesCommand.MaxFiles} фото.");
            RuleForEach(x => x.Input.FileIds).GreaterThan(0);
            RuleFor(x => x.Input.FolderId).GreaterThan(0).When(x => x.Input.FolderId is not null);
        });
    }
}
