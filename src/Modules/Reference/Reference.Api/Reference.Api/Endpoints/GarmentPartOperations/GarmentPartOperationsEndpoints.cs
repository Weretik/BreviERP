namespace Reference.Api.Endpoints.GarmentPartOperations;

/// <summary>Sewing operations of «Операції».</summary>
internal static class GarmentPartOperationsEndpoints
{
    public static IEndpointRouteBuilder MapGarmentPartOperationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/garment-part-operations").WithTags("GarmentPartOperations");
        GetGarmentPartOperationsEndpoint.Map(group);
        CreateGarmentPartOperationEndpoint.Map(group);
        ImportGarmentPartOperationsEndpoint.Map(group);
        UpdateGarmentPartOperationEndpoint.Map(group);
        DeleteGarmentPartOperationEndpoint.Map(group);
        return app;
    }
}
