using Accounting.Api.Access;

namespace Accounting.Api.Endpoints.Statements;

/// <summary>
/// «Виписки банку»: upload a statement into a preview, correct its lines and book it. Nothing reaches the books before
/// <c>book</c>; a booked statement changes only through <c>rollback</c>. 409 on a stale version.
/// </summary>
internal static class BankStatementsEndpoints
{
    public static IEndpointRouteBuilder MapBankStatementsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/accounting/statements").RequireAuthorization(AccountingAccess.Policy)
            .WithTags("BankStatements");
        GetBankStatementsEndpoint.Map(group);
        UploadBankStatementEndpoint.Map(group);
        GetBankStatementEndpoint.Map(group);
        UpdateBankStatementLineEndpoint.Map(group);
        BookBankStatementEndpoint.Map(group);
        RollbackBankStatementEndpoint.Map(group);
        return app;
    }
}
