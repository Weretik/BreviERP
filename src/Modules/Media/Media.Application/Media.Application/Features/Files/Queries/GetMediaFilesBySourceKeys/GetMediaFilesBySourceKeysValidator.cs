namespace Media.Application.Features.Files.Queries.GetMediaFilesBySourceKeys;

public sealed class GetMediaFilesBySourceKeysValidator : AbstractValidator<GetMediaFilesBySourceKeysQuery>
{
    public const int MaxKeys = 5000;

    public GetMediaFilesBySourceKeysValidator() =>
        RuleFor(x => x.SourceKeys).NotNull().Must(x => x.Count <= MaxKeys).WithMessage($"Не більше {MaxKeys} ключів.");
}
