using Crm.Api.Access;
using Crm.Api.Endpoints.Quotes.CalculateCrmQuoteCustomSewing;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteCatalogProducts;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteFabricFacts;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteIntroductionSetup;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteModelComposition;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteStaff;
using Crm.Api.Endpoints.Quotes.SearchCrmQuoteCatalog;
using Crm.Api.Endpoints.Quotes.UploadCrmQuotePhoto;

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
