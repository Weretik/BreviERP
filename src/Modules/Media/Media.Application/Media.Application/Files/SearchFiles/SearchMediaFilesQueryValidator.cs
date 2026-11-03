using Media.Domain.Files;

namespace Media.Application.Files.SearchFiles;

public sealed class SearchMediaFilesQueryValidator : AbstractValidator<SearchMediaFilesQuery>
{
    public SearchMediaFilesQueryValidator()
    {
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        RuleFor(x => x.Search).MaximumLength(MediaFile.FileNameMaxLength);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, SearchMediaFilesQuery.MaxPageSize);
    }
}
