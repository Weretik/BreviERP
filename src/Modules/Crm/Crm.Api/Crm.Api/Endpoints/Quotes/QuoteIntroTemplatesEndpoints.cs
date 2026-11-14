using Crm.Api.Access;
using Crm.Api.Endpoints.Quotes.CreateCrmQuoteIntroTemplate;
using Crm.Api.Endpoints.Quotes.DeleteCrmQuoteIntroTemplate;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteIntroTemplates;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>«Готові шаблони» of «КП для знайомства»: shared by all staff, no prices (R-19).</summary>
internal static class QuoteIntroTemplatesEndpoints
{
    public static IEndpointRouteBuilder MapQuoteIntroTemplatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quote-intro-templates").RequireAuthorization(CrmAccess.Quotes)
            .WithTags("QuoteIntroTemplates");
        GetCrmQuoteIntroTemplatesEndpoint.Map(group);
        CreateCrmQuoteIntroTemplateEndpoint.Map(group);
        DeleteCrmQuoteIntroTemplateEndpoint.Map(group);
        return app;
    }
}
