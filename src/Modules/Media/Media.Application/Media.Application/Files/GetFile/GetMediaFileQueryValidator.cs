using Media.Application.Contracts;

namespace Media.Application.Files.GetFile;

public sealed class GetMediaFileQueryValidator : AbstractValidator<GetMediaFileQuery>
{
    public GetMediaFileQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
