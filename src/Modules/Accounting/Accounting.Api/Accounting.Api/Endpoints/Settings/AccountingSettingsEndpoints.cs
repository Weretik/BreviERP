using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Settings;

/// <summary>Fixed rules of the books for the forms: cutover date and today in Kyiv.</summary>
internal static class AccountingSettingsEndpoints
{
    public static IEndpointRouteBuilder MapAccountingSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/settings").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("AccountingSettings");
        GetAccountingSettingsEndpoint.Map(group);
        return app;
    }
}
