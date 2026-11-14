using Crm.Api.Access;
using Crm.Api.Endpoints.Quotes.CreateCrmQuoteIntroduction;
using Crm.Api.Endpoints.Quotes.SaveCrmQuoteIntroduction;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>«КП для знайомства»: a catalogue of season pages, 4 models each (R-16–R-18).
/// It is read, listed, sent and deleted through <c>api/crm/quotes</c>.</summary>
internal static class QuoteIntroductionsEndpoints
{
    public static IEndpointRouteBuilder MapQuoteIntroductionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quote-introductions").RequireAuthorization(CrmAccess.Quotes)
            .WithTags("QuoteIntroductions");
        CreateCrmQuoteIntroductionEndpoint.Map(group);
        SaveCrmQuoteIntroductionEndpoint.Map(group);
        return app;
    }
}
