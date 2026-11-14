using Ai.Application.Abstractions;
using Ai.Domain.Settings;
using Ai.Application.Features.Settings.Shared;

namespace Ai.Application.Features.Settings.Commands.UpdateAiSettings;

public sealed class UpdateAiSettingsHandler(IAiDbContext db, AiSettingsStore store, TimeProvider clock)
    : ICommandHandler<UpdateAiSettingsCommand, Result<AiSettingsResponse>>
{
    public const string Stale = "Налаштування вже змінили. Відкрийте сторінку знову.";
    public const string KeyRequired = "Спочатку додайте ключ OpenAI, потім вмикайте ШІ.";

    public async ValueTask<Result<AiSettingsResponse>> Handle(UpdateAiSettingsCommand command, CancellationToken cancellationToken)
    {
        var settings = await store.LoadForUpdateAsync(cancellationToken);
        if (settings.Version != command.Version) return Result<AiSettingsResponse>.Conflict(Stale);
        if (command.Enabled && !settings.HasKey)
            return Result<AiSettingsResponse>.Invalid(new ValidationError(nameof(command.Enabled), KeyRequired));

        if (settings.Update(command.Enabled, command.FastModel, new AiModelPrice(command.FastInputPrice, command.FastOutputPrice),
                command.StrongModel, new AiModelPrice(command.StrongInputPrice, command.StrongOutputPrice),
                command.MonthlyLimitUsd, command.By, clock.GetUtcNow()))
            await db.SaveChangesAsync(cancellationToken);

        return Result.Success(await store.ResponseAsync(settings, cancellationToken));
    }
}
