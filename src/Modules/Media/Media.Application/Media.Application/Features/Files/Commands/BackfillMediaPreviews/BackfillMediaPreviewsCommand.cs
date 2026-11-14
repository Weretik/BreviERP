using Media.Application.Features.Files.Shared.Images;

namespace Media.Application.Features.Files.Commands.BackfillMediaPreviews;

/// <summary>The brevierp-sync step: makes previews for up to <paramref name="Limit"/> photos that have none yet.</summary>
public sealed record BackfillMediaPreviewsCommand(int Limit) : ICommand<Result<MediaImageBackfillReport>>;
