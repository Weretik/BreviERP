using Media.Application.Features.Files.Shared.Images;

namespace Media.Application.Features.Files.Commands.BackfillMediaPreviews;

public sealed class BackfillMediaPreviewsHandler(MediaImageBackfill backfill)
    : ICommandHandler<BackfillMediaPreviewsCommand, Result<MediaImageBackfillReport>>
{
    public async ValueTask<Result<MediaImageBackfillReport>> Handle(BackfillMediaPreviewsCommand command,
        CancellationToken cancellationToken) =>
        Result.Success(await backfill.RunAsync(command.Limit, cancellationToken));
}
