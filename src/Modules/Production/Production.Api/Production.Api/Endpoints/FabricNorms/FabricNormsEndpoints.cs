using Production.Api.Access;

namespace Production.Api.Endpoints.FabricNorms;

/// <summary>«Норми витрати тканини» per Catalog sewing model, in metres.</summary>
internal static class FabricNormsEndpoints
{
    public static IEndpointRouteBuilder MapFabricNormsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/production/fabric-norms").RequireAuthorization(ProductionAccess.FabricNorms)
            .WithTags("FabricNorms");
        GetFabricNormsEndpoint.Map(group);
        GetModelCompositionsEndpoint.Map(group);
        SaveFabricNormEndpoint.Map(group);
        DeleteFabricNormEndpoint.Map(group);
        ImportFabricNormsEndpoint.Map(group);
        return app;
    }
}
