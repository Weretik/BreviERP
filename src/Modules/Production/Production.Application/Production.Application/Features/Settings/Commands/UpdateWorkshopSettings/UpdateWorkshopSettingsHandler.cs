using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Settings.Shared;
using BuildingBlocks.Application;

namespace Production.Application.Features.Settings.Commands.UpdateWorkshopSettings;

public sealed class UpdateWorkshopSettingsHandler(
    IProductionDbContext db, IProductionStaffReader staff, IProductionTelegram telegram, TimeProvider clock)
    : ICommandHandler<UpdateWorkshopSettingsCommand, Result<WorkshopSettingsResponse>>
{
    public async ValueTask<Result<WorkshopSettingsResponse>> Handle(UpdateWorkshopSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanManageSettings)
            return Result<WorkshopSettingsResponse>.Forbidden(SettingsMessages.CannotManage);
        var actorName = (await staff.FindAsync(command.Actor.UserId, cancellationToken))?.Name;
        if (actorName is null) return Result<WorkshopSettingsResponse>.Forbidden("Не вдалося визначити користувача.");
        var settings = await db.Settings.SingleOrDefaultAsync(x => x.Id == WorkshopSettings.SingletonId,
            cancellationToken);
        if (settings is null)
        {
            settings = new WorkshopSettings(null, null);
            db.Settings.Add(settings);
        }

        var input = command.Input;
        var updated = settings.Update(input.TelegramEnabled, input.TelegramChatId, input.TelegramThreadId, actorName,
            clock.GetUtcNow());
        if (updated.IsFailure) return updated.Error!.ToResult<WorkshopSettingsResponse>();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await WorkshopSettingsReading.ResponseAsync(telegram, settings, cancellationToken));
    }
}
