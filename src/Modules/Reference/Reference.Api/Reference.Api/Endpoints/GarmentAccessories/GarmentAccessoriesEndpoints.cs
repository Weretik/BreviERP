using Reference.Api.Endpoints.GarmentAccessories.CreateGarmentAccessory;
using Reference.Api.Endpoints.GarmentAccessories.DeleteGarmentAccessory;
using Reference.Api.Endpoints.GarmentAccessories.GetGarmentAccessories;
using Reference.Api.Endpoints.GarmentAccessories.ImportGarmentAccessories;
using Reference.Api.Endpoints.GarmentAccessories.UpdateGarmentAccessory;

namespace Reference.Api.Endpoints.GarmentAccessories;

/// <summary>«Фурнітура» of «Матеріали».</summary>
internal static class GarmentAccessoriesEndpoints
{
    public static IEndpointRouteBuilder MapGarmentAccessoriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/garment-accessories").WithTags("GarmentAccessories");
        GetGarmentAccessoriesEndpoint.Map(group);
        CreateGarmentAccessoryEndpoint.Map(group);
        ImportGarmentAccessoriesEndpoint.Map(group);
        UpdateGarmentAccessoryEndpoint.Map(group);
        DeleteGarmentAccessoryEndpoint.Map(group);
        return app;
    }
}
