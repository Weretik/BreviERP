using Reference.Api.Endpoints.FabricColors.CreateFabricColor;
using Reference.Api.Endpoints.FabricColors.DeleteFabricColor;
using Reference.Api.Endpoints.FabricColors.GetFabricColors;
using Reference.Api.Endpoints.FabricColors.UpdateFabricColor;

namespace Reference.Api.Endpoints.FabricColors;

/// <summary>«Кольори» of «Матеріали»: the sheet column «Загальний перелік кольорів».</summary>
internal static class FabricColorsEndpoints
{
    public static IEndpointRouteBuilder MapFabricColorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/reference/fabric-colors").WithTags("FabricColors");
        GetFabricColorsEndpoint.Map(group);
        CreateFabricColorEndpoint.Map(group);
        UpdateFabricColorEndpoint.Map(group);
        DeleteFabricColorEndpoint.Map(group);
        return app;
    }
}
