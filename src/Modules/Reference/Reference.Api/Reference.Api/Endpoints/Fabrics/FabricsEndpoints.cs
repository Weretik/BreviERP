using Reference.Api.Endpoints.Fabrics.CreateFabric;
using Reference.Api.Endpoints.Fabrics.DeleteFabric;
using Reference.Api.Endpoints.Fabrics.GetFabrics;
using Reference.Api.Endpoints.Fabrics.ImportFabrics;
using Reference.Api.Endpoints.Fabrics.UpdateFabric;

namespace Reference.Api.Endpoints.Fabrics;

/// <summary>«Тканини» of «Матеріали».</summary>
internal static class FabricsEndpoints
{
    public static IEndpointRouteBuilder MapFabricsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/fabrics").WithTags("Fabrics");
        GetFabricsEndpoint.Map(group);
        CreateFabricEndpoint.Map(group);
        ImportFabricsEndpoint.Map(group);
        UpdateFabricEndpoint.Map(group);
        DeleteFabricEndpoint.Map(group);
        return app;
    }
}
