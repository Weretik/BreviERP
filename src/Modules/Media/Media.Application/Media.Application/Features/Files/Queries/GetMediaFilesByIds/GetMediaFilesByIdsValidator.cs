
namespace Media.Application.Features.Files.Queries.GetMediaFilesByIds;

public sealed class GetMediaFilesByIdsValidator : AbstractValidator<GetMediaFilesByIdsQuery>
{
    public GetMediaFilesByIdsValidator() => RuleFor(x => x.Ids).NotNull();
}
