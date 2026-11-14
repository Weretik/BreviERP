using Production.Api.Access;

namespace Production.Api.Endpoints.Work;

/// <summary>«Журнал робіт» and «Робота» of an item: Admin and the head of the workshop (PRD-D37).</summary>
internal static class ProductionWorkEndpoints
{
    public static IEndpointRouteBuilder MapProductionWorkEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production").RequireAuthorization(ProductionAccess.WorkLog)
            .WithTags("ProductionWork");
        GetWorkLogEndpoint.Map(group);
        GetWorkChoicesEndpoint.Map(group);
        GetProductionItemWorkEndpoint.Map(group);
        CreateProductionWorkEndpoint.Map(group);
        VoidProductionWorkEndpoint.Map(group);
        ImportProductionWorkEndpoint.Map(group);
        return app;
    }
}
