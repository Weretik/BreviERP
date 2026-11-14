using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.RemoveAiKey;

public sealed class RemoveAiKeyHandler(IAiDbContext db, AiSettingsStore store, TimeProvider clock)
    : ICommandHandler<RemoveAiKeyCommand, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(RemoveAiKeyCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        if (settings.RemoveKey(command.By, clock.GetUtcNow())) await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
