using Media.Application.Contracts;

namespace Media.Application.Files.GetFilesByIds;

public sealed class GetMediaFilesByIdsQueryValidator : AbstractValidator<GetMediaFilesByIdsQuery>
{
    public GetMediaFilesByIdsQueryValidator() => RuleFor(x => x.Ids).NotNull();
}
