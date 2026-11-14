using Production.Api.Access;
using Production.Api.Endpoints.Orders.CancelProductionOrder;
using Production.Api.Endpoints.Orders.CancelProductionShipment;
using Production.Api.Endpoints.Orders.CloseProductionOrder;
using Production.Api.Endpoints.Orders.CreateProductionShipment;
using Production.Api.Endpoints.Orders.DeleteProductionOrderFile;
using Production.Api.Endpoints.Orders.ImportProductionProgress;
using Production.Api.Endpoints.Orders.ImportProductionReportPrices;
using Production.Api.Endpoints.Orders.ReopenProductionOrder;
using Production.Api.Endpoints.Orders.SendProductionOrderTelegram;
using Production.Api.Endpoints.Orders.SetProductionItemStages;
using Production.Api.Endpoints.Orders.UploadProductionOrderFile;

namespace Production.Api.Endpoints.Orders;

/// <summary>Work on an order after «Передати в цех»: stages, shipments, status, files and the Telegram message.</summary>
internal static class ProductionOrderTrackingEndpoints
{
    public static IEndpointRouteBuilder MapProductionOrderTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/orders").RequireAuthorization(ProductionAccess.Orders)
            .WithTags("ProductionOrderTracking");
        SetProductionItemStagesEndpoint.Map(group);
        CreateProductionShipmentEndpoint.Map(group);
        CancelProductionShipmentEndpoint.Map(group);
        CloseProductionOrderEndpoint.Map(group);
        CancelProductionOrderEndpoint.Map(group);
        ReopenProductionOrderEndpoint.Map(group);
        UploadProductionOrderFileEndpoint.Map(group);
        DeleteProductionOrderFileEndpoint.Map(group);
        SendProductionOrderTelegramEndpoint.Map(group);
        ImportProductionProgressEndpoint.Map(group);
        ImportProductionReportPricesEndpoint.Map(group);
        return app;
    }
}
