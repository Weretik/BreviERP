using Crm.Api.Access;

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
