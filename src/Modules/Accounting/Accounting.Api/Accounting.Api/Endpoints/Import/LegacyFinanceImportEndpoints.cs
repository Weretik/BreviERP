using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Import;

/// <summary>
/// «Імпорт архіву Фінансів»: the finance.json export of the old Calculator, gzipped by the browser so it fits Cloud
/// Run's request limit. <c>dryRun=true</c> only checks and reports; <c>false</c> moves the records in one transaction.
/// </summary>
internal static class LegacyFinanceImportEndpoints
{
    public static IEndpointRouteBuilder MapLegacyFinanceImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("LegacyFinanceImport");
        ImportLegacyFinanceEndpoint.Map(group);
        return app;
    }
}
