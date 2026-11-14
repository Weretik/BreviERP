using Crm.Api.Endpoints.Quotes.ImportCrmQuotes;

namespace Crm.Api.Endpoints.Quotes;

/// <summary>
/// One-time transfer of КП from the old calculator (R-25): Director and Admin only. Run a dry run first; the real run
/// saves every new КП in one transaction and skips the ones transferred before.
/// </summary>
internal static class QuoteImportEndpoints
{
    public static IEndpointRouteBuilder MapQuoteImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/crm/quote-import")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .WithTags("QuoteImport");
        ImportCrmQuotesEndpoint.Map(group);
        return app;
    }
}
