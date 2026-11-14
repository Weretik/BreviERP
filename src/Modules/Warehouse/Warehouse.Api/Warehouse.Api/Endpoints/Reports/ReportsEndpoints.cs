namespace Warehouse.Api.Endpoints.Reports;

/// <summary>
/// The four reports of «Склад тканин»: «План і факт», «Бронь і що замовити», «Вартість складу» and «Фурнітура до
/// замовлення». Each is a page of «Права доступу» with its own policy; each reads only and is calculated anew.
/// </summary>
internal static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/warehouse/reports").WithTags("StockReports");
        GetFabricPlanFactEndpoint.Map(group);
        GetFabricBookingEndpoint.Map(group);
        GetFabricValuationEndpoint.Map(group);
        GetHardwareOrderEndpoint.Map(group);
        return app;
    }
}
