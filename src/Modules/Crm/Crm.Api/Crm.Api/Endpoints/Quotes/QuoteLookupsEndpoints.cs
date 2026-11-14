using Crm.Api.Access;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>What the КП editor picks from: Catalog products and managers.</summary>
internal static class QuoteLookupsEndpoints
{
    public static IEndpointRouteBuilder MapQuoteLookupsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quote-lookups").RequireAuthorization(CrmAccess.QuotesView)
            .WithTags("QuoteLookups");
        SearchCrmQuoteCatalogEndpoint.Map(group);
        GetCrmQuoteCatalogProductsEndpoint.Map(group);
        GetCrmQuoteFabricFactsEndpoint.Map(group);
        GetCrmQuoteIntroductionSetupEndpoint.Map(group);
        CalculateCrmQuoteCustomSewingEndpoint.Map(group);
        GetCrmQuoteModelCompositionEndpoint.Map(group);
        GetCrmQuoteStaffEndpoint.Map(group);
        UploadCrmQuotePhotoEndpoint.Map(group);
        return app;
    }
}
