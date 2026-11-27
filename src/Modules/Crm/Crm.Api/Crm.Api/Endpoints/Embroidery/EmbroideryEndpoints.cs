using Crm.Api.Access;
using Crm.Api.Endpoints.Embroidery.CalculateCrmEmbroidery;
using Crm.Api.Endpoints.Embroidery.DeleteCrmEmbroideryCalculation;
using Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryCalculation;
using Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryCalculations;
using Crm.Api.Endpoints.Embroidery.GetCrmEmbroideryForm;
using Crm.Api.Endpoints.Embroidery.GetCrmEmbroiderySettings;
using Crm.Api.Endpoints.Embroidery.ImportCrmEmbroideryArchive;
using Crm.Api.Endpoints.Embroidery.SaveCrmEmbroideryCalculation;
using Crm.Api.Endpoints.Embroidery.SaveCrmEmbroiderySettings;
using Crm.Api.Endpoints.Embroidery.SetCrmEmbroideryMockup;

namespace Crm.Api.Endpoints.Embroidery;

/// <summary>
/// «Ціна вишивки» (EMB-001): whoever has the page calculates, saves and deletes by its rights; the cost and «Налаштування»
/// are the page «Ціна вишивки: собівартість і налаштування» (ACC-002 R-02). The archive import stays Admin (R-04).
/// </summary>
internal static class EmbroideryEndpoints
{
    public static IEndpointRouteBuilder MapEmbroideryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/embroidery").RequireAuthorization(CrmAccess.Embroidery)
            .WithTags("Embroidery");
        GetCrmEmbroideryFormEndpoint.Map(group);
        CalculateCrmEmbroideryEndpoint.Map(group);
        GetCrmEmbroideryCalculationsEndpoint.Map(group);
        GetCrmEmbroideryCalculationEndpoint.Map(group);
        SaveCrmEmbroideryCalculationEndpoint.Map(group);
        SetCrmEmbroideryMockupEndpoint.Map(group);
        ImportCrmEmbroideryArchiveEndpoint.Map(group);
        DeleteCrmEmbroideryCalculationEndpoint.Map(group);

        var settings = app.MapGroup("api/crm/embroidery").RequireAuthorization(CrmAccess.EmbroiderySettings)
            .WithTags("Embroidery");
        GetCrmEmbroiderySettingsEndpoint.Map(settings);
        SaveCrmEmbroiderySettingsEndpoint.Map(settings);
        return app;
    }
}
