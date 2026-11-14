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
/// «Ціна вишивки» (EMB-001): Manager, Director and Admin calculate; only Director and Admin (the Director also holds
/// Admin) see the cost and change «Налаштування».
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
        GetCrmEmbroiderySettingsEndpoint.Map(group);
        SaveCrmEmbroiderySettingsEndpoint.Map(group);
        return app;
    }
}
