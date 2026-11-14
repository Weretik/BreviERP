using Production.Api.Access;
using Production.Api.Endpoints.Forecast.GetSewingForecast;
using Production.Api.Endpoints.Forecast.SaveSewingForecastQueue;

namespace Production.Api.Endpoints.Forecast;

/// <summary>«Прогноз пошиття»: everyone with production orders sees it; the workshop chief and Admin reorder it.</summary>
internal static class ProductionForecastEndpoints
{
    public static IEndpointRouteBuilder MapProductionForecastEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/forecast").RequireAuthorization(ProductionAccess.Forecast)
            .WithTags("ProductionForecast");
        GetSewingForecastEndpoint.Map(group);
        SaveSewingForecastQueueEndpoint.Map(group);
        return app;
    }
}
