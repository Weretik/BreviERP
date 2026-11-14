using Reference.Api.Endpoints.GarmentParts.CreateGarmentPart;
using Reference.Api.Endpoints.GarmentParts.DeleteGarmentPart;
using Reference.Api.Endpoints.GarmentParts.GetGarmentParts;
using Reference.Api.Endpoints.GarmentParts.UpdateGarmentPart;

namespace Reference.Api.Endpoints.GarmentParts;

/// <summary>Garment parts, the groups of «Операції».</summary>
internal static class GarmentPartsEndpoints
{
    public static IEndpointRouteBuilder MapGarmentPartsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/garment-parts").RequireAuthorization("access:operations")
            .WithTags("GarmentParts");
        GetGarmentPartsEndpoint.Map(group);
        CreateGarmentPartEndpoint.Map(group);
        UpdateGarmentPartEndpoint.Map(group);
        DeleteGarmentPartEndpoint.Map(group);
        return app;
    }
}
