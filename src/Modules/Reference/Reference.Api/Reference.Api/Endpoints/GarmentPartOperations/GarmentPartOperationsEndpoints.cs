using Reference.Api.Endpoints.GarmentPartOperations.CreateGarmentPartOperation;
using Reference.Api.Endpoints.GarmentPartOperations.DeleteGarmentPartOperation;
using Reference.Api.Endpoints.GarmentPartOperations.GetGarmentPartOperations;
using Reference.Api.Endpoints.GarmentPartOperations.ImportGarmentPartOperations;
using Reference.Api.Endpoints.GarmentPartOperations.UpdateGarmentPartOperation;

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
