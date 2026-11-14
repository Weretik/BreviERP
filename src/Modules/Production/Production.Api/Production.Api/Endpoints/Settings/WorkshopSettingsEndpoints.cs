using Production.Api.Access;
using Production.Api.Endpoints.Settings.GetWorkshopSettings;
using Production.Api.Endpoints.Settings.SendWorkshopTelegramTest;
using Production.Api.Endpoints.Settings.UpdateWorkshopSettings;

namespace Production.Api.Endpoints.Settings;

/// <summary>«Налаштування цеху»: where the bot posts new orders.</summary>
internal static class WorkshopSettingsEndpoints
{
    public static IEndpointRouteBuilder MapWorkshopSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/settings").RequireAuthorization(ProductionAccess.Settings)
            .WithTags("WorkshopSettings");
        GetWorkshopSettingsEndpoint.Map(group);
        UpdateWorkshopSettingsEndpoint.Map(group);
        SendWorkshopTelegramTestEndpoint.Map(group);
        return app;
    }
}
