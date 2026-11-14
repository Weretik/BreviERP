using Production.Application.Abstractions;
using Production.Domain.Settings;
using Production.Application.Features.Settings.Shared;

namespace Production.Application.Features.Settings.Queries.GetWorkshopSettings;

public sealed class GetWorkshopSettingsHandler(IProductionDbContext db, IProductionTelegram telegram)
    : IQueryHandler<GetWorkshopSettingsQuery, Result<WorkshopSettingsResponse>>
{
    public async ValueTask<Result<WorkshopSettingsResponse>> Handle(GetWorkshopSettingsQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.CanManageSettings)
            return Result<WorkshopSettingsResponse>.Forbidden(SettingsMessages.CannotManage);
        return Result.Success(await WorkshopSettingsReading.ResponseAsync(telegram, await WorkshopSettingsReading.ReadAsync(db, cancellationToken), cancellationToken));
    }
}
