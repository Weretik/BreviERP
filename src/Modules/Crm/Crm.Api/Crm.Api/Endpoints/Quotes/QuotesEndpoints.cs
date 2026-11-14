using Crm.Api.Access;
using Crm.Api.Endpoints.Quotes.AddCrmQuoteEmbroidery;
using Crm.Api.Endpoints.Quotes.CopyCrmQuote;
using Crm.Api.Endpoints.Quotes.CreateCrmQuote;
using Crm.Api.Endpoints.Quotes.DeleteCrmQuote;
using Crm.Api.Endpoints.Quotes.GetCrmLegacyArchivedQuotes;
using Crm.Api.Endpoints.Quotes.GetCrmQuote;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteHistory;
using Crm.Api.Endpoints.Quotes.GetCrmQuotePdf;
using Crm.Api.Endpoints.Quotes.GetCrmQuotePeriod;
using Crm.Api.Endpoints.Quotes.GetCrmQuoteSendDraft;
using Crm.Api.Endpoints.Quotes.GetCrmQuotes;
using Crm.Api.Endpoints.Quotes.PurgeCrmLegacyArchivedQuotes;
using Crm.Api.Endpoints.Quotes.RepriceCrmQuote;
using Crm.Api.Endpoints.Quotes.SaveCrmQuote;
using Crm.Api.Endpoints.Quotes.SendCrmQuote;
using Crm.Api.Endpoints.Quotes.SetCrmQuoteResponsible;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>КП (commercial proposals) of the «Менеджер сервісу» block. A Manager sees only own КП.</summary>
internal static class QuotesEndpoints
{
    public static IEndpointRouteBuilder MapQuotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quotes").RequireAuthorization(CrmAccess.Quotes)
            .WithTags("Quotes");
        GetCrmQuotesEndpoint.Map(group);
        GetCrmQuotePeriodEndpoint.Map(group);
        GetCrmQuoteEndpoint.Map(group);
        GetCrmQuotePdfEndpoint.Map(group);
        CreateCrmQuoteEndpoint.Map(group);
        SaveCrmQuoteEndpoint.Map(group);
        DeleteCrmQuoteEndpoint.Map(group);
        RepriceCrmQuoteEndpoint.Map(group);
        SetCrmQuoteResponsibleEndpoint.Map(group);
        GetCrmQuoteSendDraftEndpoint.Map(group);
        SendCrmQuoteEndpoint.Map(group);
        GetCrmQuoteHistoryEndpoint.Map(group);
        GetCrmLegacyArchivedQuotesEndpoint.Map(group);
        PurgeCrmLegacyArchivedQuotesEndpoint.Map(group);
        AddCrmQuoteEmbroideryEndpoint.Map(group);
        CopyCrmQuoteEndpoint.Map(group);
        return app;
    }
}
