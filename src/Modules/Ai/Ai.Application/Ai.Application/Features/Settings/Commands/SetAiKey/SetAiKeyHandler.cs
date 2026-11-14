using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.SetAiKey;

public sealed class SetAiKeyHandler(IAiDbContext db, AiSettingsStore store, IAiKeyProtector protector, TimeProvider clock)
    : ICommandHandler<SetAiKeyCommand, Result<AiSettingsResponse>>
{
    public async ValueTask<Result<AiSettingsResponse>> Handle(SetAiKeyCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        settings.SetKey(protector.Protect(command.ApiKey), AiSettings.HintOf(command.ApiKey), command.By, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
