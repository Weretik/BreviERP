namespace Media.Application.Files.GetFilesBySourceKeys;

public sealed class GetMediaFilesBySourceKeysQueryValidator : AbstractValidator<GetMediaFilesBySourceKeysQuery>
{
    public const int MaxKeys = 5000;

    public GetMediaFilesBySourceKeysQueryValidator() =>
        RuleFor(x => x.SourceKeys).NotNull().Must(x => x.Count <= MaxKeys).WithMessage($"Не більше {MaxKeys} ключів.");
}
