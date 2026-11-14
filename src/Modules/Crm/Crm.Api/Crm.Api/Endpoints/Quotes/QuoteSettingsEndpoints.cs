using Crm.Api.Access;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>«Налаштування КП» (R-13): Director and Admin only (the Director also holds Admin).</summary>
internal static class QuoteSettingsEndpoints
{
    public static IEndpointRouteBuilder MapQuoteSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quote-settings").RequireAuthorization(CrmAccess.QuoteSettings)
            .WithTags("QuoteSettings");
        GetCrmQuoteSettingsEndpoint.Map(group);
        SaveCrmQuoteSettingsEndpoint.Map(group);
        return app;
    }
}
